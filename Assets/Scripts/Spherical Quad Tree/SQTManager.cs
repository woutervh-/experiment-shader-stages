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

        pluginsChain.ModifyMaterial(material);
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
        SQTApproximateEdgeLengthPlugin[] approximateEdgeLengthPlugins;
        SQTMeshPlugin[] meshPlugins;
        SQTDistanceToObjectPlugin[] distanceToObjectPlugins;
        SQTMaterialPlugin[] materialPlugins;

        public PluginsChain(SQTPlugin[] plugins)
        {
            List<SQTApproximateEdgeLengthPlugin> approximateEdgeLengthPlugins = new List<SQTApproximateEdgeLengthPlugin>();
            List<SQTMeshPlugin> meshPlugins = new List<SQTMeshPlugin>();
            List<SQTDistanceToObjectPlugin> distanceToObjectPlugins = new List<SQTDistanceToObjectPlugin>();
            List<SQTMaterialPlugin> materialPlugins = new List<SQTMaterialPlugin>();

            foreach (SQTPlugin plugin in plugins)
            {
                if (plugin is MonoBehaviour monoBehaviour)
                {
                    if (!monoBehaviour.enabled)
                    {
                        continue;
                    }
                }
                if (plugin is SQTApproximateEdgeLengthPlugin approximateEdgeLengthModifier)
                {
                    approximateEdgeLengthPlugins.Add(approximateEdgeLengthModifier);
                }
                if (plugin is SQTMeshPlugin meshModifier)
                {
                    meshPlugins.Add(meshModifier);
                }
                if (plugin is SQTDistanceToObjectPlugin distanceToObjectModifier)
                {
                    distanceToObjectPlugins.Add(distanceToObjectModifier);
                }
                if (plugin is SQTMaterialPlugin materialModifier)
                {
                    materialPlugins.Add(materialModifier);
                }
            }

            this.approximateEdgeLengthPlugins = approximateEdgeLengthPlugins.ToArray();
            this.meshPlugins = meshPlugins.ToArray();
            this.distanceToObjectPlugins = distanceToObjectPlugins.ToArray();
            this.materialPlugins = materialPlugins.ToArray();
        }

        public void ModifyApproximateEdgeLength(ref float edgeLength)
        {
            foreach (SQTApproximateEdgeLengthPlugin plugin in approximateEdgeLengthPlugins)
            {
                plugin.ModifyApproximateEdgeLength(ref edgeLength);
            }
        }

        public void ModifyMesh(Vector3[] vertices, Vector3[] normals)
        {
            foreach (SQTMeshPlugin plugin in meshPlugins)
            {
                plugin.ModifyMesh(vertices, normals);
            }
        }

        public void ModifyDistanceToObject(ref float distance)
        {
            foreach (SQTDistanceToObjectPlugin plugin in distanceToObjectPlugins)
            {
                plugin.ModifyDistanceToObject(ref distance);
            }
        }

        public void ModifyMaterial(Material material)
        {
            foreach (SQTMaterialPlugin plugin in materialPlugins)
            {
                plugin.ModifyMaterial(material);
            }
        }
    }
}
