using System;
using System.Collections.Generic;
using UnityEngine;

namespace SQT
{
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
        SQT.Core.SQTPlugin[] plugins;
        PluginsChain pluginsChain;
        SQT.Core.SQTBranches branches;
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
                foreach (SQT.Core.SQTPlugin plugin in plugins)
                {
                    plugin.OnChange -= HandleChange;
                }
            }
            plugins = GetComponents<SQT.Core.SQTPlugin>();
            pluginsChain = new PluginsChain(plugins);
            foreach (SQT.Core.SQTPlugin plugin in plugins)
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

            SQT.Core.SQTConstants.SQTGlobal global = new SQT.Core.SQTConstants.SQTGlobal
            {
                maxDepth = maxDepth,
                resolution = resolution * 2 - 1,
                desiredScreenSpaceLength = desiredScreenSpaceLength,
                material = material,
                gameObject = gameObject,
                plugins = pluginsChain
            };
            SQT.Core.SQTConstants.SQTDepth[] depth = SQT.Core.SQTConstants.SQTDepth.GetFromGlobal(global);
            SQT.Core.SQTConstants.SQTMesh[] meshes = SQT.Core.SQTConstants.SQTMesh.GetFromGlobal(global);
            SQT.Core.CPU.SQTReconciler reconciler = new SQT.Core.CPU.SQTReconciler(new SQT.Core.SQTConstants[6]);
            branches = new SQT.Core.SQTBranches(global, depth, meshes, reconciler);
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

        class PluginsChain : SQT.Core.SQTChainedPlugins
        {
            SQT.Core.SQTMeshPlugin[] meshPlugins;
            SQT.Core.SQTMaterialPlugin[] materialPlugins;

            public PluginsChain(SQT.Core.SQTPlugin[] plugins)
            {
                List<SQT.Core.SQTMeshPlugin> meshPlugins = new List<SQT.Core.SQTMeshPlugin>();
                List<SQT.Core.SQTMaterialPlugin> materialPlugins = new List<SQT.Core.SQTMaterialPlugin>();

                foreach (SQT.Core.SQTPlugin plugin in plugins)
                {
                    if (plugin is MonoBehaviour monoBehaviour)
                    {
                        if (!monoBehaviour.enabled)
                        {
                            continue;
                        }
                    }
                    if (plugin is SQT.Core.SQTMeshPlugin meshModifier)
                    {
                        meshPlugins.Add(meshModifier);
                    }
                    if (plugin is SQT.Core.SQTMaterialPlugin materialModifier)
                    {
                        materialPlugins.Add(materialModifier);
                    }
                }

                this.meshPlugins = meshPlugins.ToArray();
                this.materialPlugins = materialPlugins.ToArray();
            }

            public void ModifyMesh(Vector3[] vertices, Vector3[] normals)
            {
                foreach (SQT.Core.SQTMeshPlugin plugin in meshPlugins)
                {
                    plugin.ModifyMesh(vertices, normals);
                }
            }

            public void ModifyMaterial(Material material)
            {
                foreach (SQT.Core.SQTMaterialPlugin plugin in materialPlugins)
                {
                    plugin.ModifyMaterial(material);
                }
            }
        }
    }
}
