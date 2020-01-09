using System;
using UnityEngine;

namespace SQT
{
    public class Manager : MonoBehaviour
    {
        public GameObject player;
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
        SQT.Core.Plugin[] plugins;
        SQT.Core.PluginsChain pluginsChain;
        SQT.Core.Branches branches;
        Material material;
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
                foreach (SQT.Core.Plugin plugin in plugins)
                {
                    plugin.OnChange -= HandleChange;
                }
            }
            plugins = GetComponents<SQT.Core.Plugin>();
            pluginsChain = new SQT.Core.PluginsChain(plugins);
            foreach (SQT.Core.Plugin plugin in plugins)
            {
                plugin.OnChange += HandleChange;
            }

            pluginsChain.ModifyMaterial(ref material);
        }

        void DoUpdateSettings()
        {
            dirty = false;

            if (branches != null)
            {
                branches.Destroy();
            }

            SQT.Core.Constants.SQTGlobal global = new SQT.Core.Constants.SQTGlobal
            {
                maxDepth = maxDepth,
                resolution = resolution * 2 - 1,
                desiredScreenSpaceLength = desiredScreenSpaceLength,
                material = material,
                gameObject = gameObject,
                plugins = pluginsChain
            };
            SQT.Core.Constants.SQTDepth[] depth = SQT.Core.Constants.SQTDepth.GetFromGlobal(global);
            SQT.Core.Constants.SQTMesh[] meshes = SQT.Core.Constants.SQTMesh.GetFromGlobal(global);
            branches = new SQT.Core.Branches(global, depth, meshes, );
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
    }
}
