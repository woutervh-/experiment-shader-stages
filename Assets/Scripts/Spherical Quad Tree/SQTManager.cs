using System;
using System.Collections.Generic;
using UnityEngine;

public class SQTManager : MonoBehaviour
{
    public GameObject player;
    public Material material;
    [Range(0f, 16f)]
    public int maxDepth = 10;
    [Range(2f, 16f)]
    public int resolution = 7;
    [Range(1f, 100f)]
    public float desiredScreenSpaceLength = 10f;
    // [Range(0f, 1e6f)]
    // public float radius = 1f;
    // public bool sphere;

#if UNITY_EDITOR
    public bool debug;
#endif

    bool dirty;
    SQTPlugin[] plugins;
    PluginsChain pluginsChain;
    SQTBranches branches;
    Camera playerCamera;

    void Start()
    {
        DoUpdatePlugins();
        DoUpdateSettings();
    }

    void OnValidate()
    {
        dirty = true;
    }

    void HandleChange(object sender, EventArgs e)
    {
        dirty = true;
    }

    void DoUpdatePlugins()
    {
        if (plugins != null)
        {
            foreach (SQTPlugin plugin in plugins)
            {
                plugin.OnChange -= HandleChange;
            }
        }
        plugins = GetComponents<SQTPlugin>();
        pluginsChain = new PluginsChain(plugins);
        foreach (SQTPlugin plugin in plugins)
        {
            plugin.OnChange += HandleChange;
        }
    }

    void DoUpdateSettings()
    {
        dirty = false;

        if (branches != null)
        {
            branches.Destroy();
        }

        SQTConstants.SQTGlobal global = new SQTConstants.SQTGlobal
        {
            maxDepth = maxDepth,
            resolution = resolution * 2 - 1,
            desiredScreenSpaceLength = desiredScreenSpaceLength,
            material = material,
            gameObject = gameObject,
            plugins = pluginsChain
        };
        SQTConstants.SQTDepth[] depth = SQTConstants.SQTDepth.GetFromGlobal(global);
        SQTConstants.SQTMesh[] meshes = SQTConstants.SQTMesh.GetFromGlobal(global);
        branches = new SQTBranches(global, depth, meshes);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!debug)
        {
            return;
        }

        if (playerCamera == null)
        {
            return;
        }

        if (branches != null)
        {
            branches.DrawBranches(playerCamera);
        }
    }
#endif

    void Update()
    {
        if (dirty)
        {
            DoUpdatePlugins();
            DoUpdateSettings();
        }

        if (playerCamera == null || playerCamera.gameObject != player)
        {
            playerCamera = player.GetComponent<Camera>();
        }

        if (branches != null)
        {
            branches.Reconcile(playerCamera);
        }
    }

    void OnDestroy()
    {
        branches.Destroy();
    }

    class PluginsChain : SQTChainedPlugins
    {
        SQTApproximateEdgeLengthPlugin[] approximateEdgeLengthModifiers;
        SQTMeshPlugin[] meshModifiers;
        SQTDistanceToObjectPlugin[] distanceToObjectModifiers;

        public PluginsChain(SQTPlugin[] plugins)
        {
            List<SQTApproximateEdgeLengthPlugin> approximateEdgeLengthModifiers = new List<SQTApproximateEdgeLengthPlugin>();
            List<SQTMeshPlugin> meshModifiers = new List<SQTMeshPlugin>();
            List<SQTDistanceToObjectPlugin> distanceToObjectModifiers = new List<SQTDistanceToObjectPlugin>();

            foreach (SQTPlugin plugin in plugins)
            {
                if (plugin is SQTApproximateEdgeLengthPlugin approximateEdgeLengthModifier)
                {
                    approximateEdgeLengthModifiers.Add(approximateEdgeLengthModifier);
                }
                if (plugin is SQTMeshPlugin meshModifier)
                {
                    meshModifiers.Add(meshModifier);
                }
                if (plugin is SQTDistanceToObjectPlugin distanceToObjectModifier)
                {
                    distanceToObjectModifiers.Add(distanceToObjectModifier);
                }
            }

            this.approximateEdgeLengthModifiers = approximateEdgeLengthModifiers.ToArray();
            this.meshModifiers = meshModifiers.ToArray();
            this.distanceToObjectModifiers = distanceToObjectModifiers.ToArray();
        }

        public void ModifyApproximateEdgeLength(ref float edgeLength)
        {
            foreach (SQTApproximateEdgeLengthPlugin modifier in approximateEdgeLengthModifiers)
            {
                modifier.ModifyApproximateEdgeLength(ref edgeLength);
            }
        }

        public void ModifyMesh(Vector3[] vertices, Vector3[] normals)
        {
            foreach (SQTMeshPlugin modifier in meshModifiers)
            {
                modifier.ModifyMesh(vertices, normals);
            }
        }

        public void ModifyDistanceToObject(ref float distance)
        {
            foreach (SQTDistanceToObjectPlugin modifier in distanceToObjectModifiers)
            {
                modifier.ModifyDistanceToObject(ref distance);
            }
        }
    }
}
