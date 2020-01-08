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
        DoUpdateSettings();
    }

    void OnValidate()
    {
        dirty = true;
    }

    void OnEnable()
    {
        plugins = GetComponents<SQTPlugin>();
        pluginsChain = new PluginsChain(plugins);
        foreach (SQTPlugin plugin in plugins)
        {
            plugin.OnChange += HandleChange;
        }
    }

    void OnDisable()
    {
        foreach (SQTPlugin plugin in plugins)
        {
            plugin.OnChange -= HandleChange;
        }
    }

    void HandleChange(object sender, EventArgs e)
    {
        dirty = true;
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

    class PluginsChain : SQTPlugin.ChainedPlugins
    {
        SQTPlugin.ApproximateEdgeLengthModifier[] approximateEdgeLengthModifiers;
        SQTPlugin.MeshModifier[] meshModifiers;

        public PluginsChain(SQTPlugin[] plugins)
        {
            List<SQTPlugin.ApproximateEdgeLengthModifier> approximateEdgeLengthModifiers = new List<SQTPlugin.ApproximateEdgeLengthModifier>();
            List<SQTPlugin.MeshModifier> meshModifiers = new List<SQTPlugin.MeshModifier>();

            foreach (SQTPlugin plugin in plugins)
            {
                if (plugin is SQTPlugin.ApproximateEdgeLengthModifier approximateEdgeLengthModifier)
                {
                    approximateEdgeLengthModifiers.Add(approximateEdgeLengthModifier);
                }
                if (plugin is SQTPlugin.MeshModifier meshModifier)
                {
                    meshModifiers.Add(meshModifier);
                }
            }

            this.approximateEdgeLengthModifiers = approximateEdgeLengthModifiers.ToArray();
            this.meshModifiers = meshModifiers.ToArray();
        }

        public void ModifyApproximateEdgeLength(ref float edgeLength)
        {
            foreach (SQTPlugin.ApproximateEdgeLengthModifier modifier in approximateEdgeLengthModifiers)
            {
                modifier.ModifyApproximateEdgeLength(ref edgeLength);
            }
        }

        public void ModifyMesh(Mesh mesh)
        {
            foreach (SQTPlugin.MeshModifier modifier in meshModifiers)
            {
                modifier.ModifyMesh(mesh);
            }
        }
    }
}
