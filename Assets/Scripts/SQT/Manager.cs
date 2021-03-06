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
        public Material material;

#if UNITY_EDITOR
        public bool debug;
#endif

        bool dirty;
        Camera playerCamera;
        Core.Context context;
        Core.Plugin[] plugins;

        void OnEnable()
        {
            dirty = false;
            DoUpdate();
        }

        void OnDisable()
        {
            DoCleanup();
        }

        void OnValidate()
        {
            dirty = true;
        }

        void HandleChange(object sender, EventArgs e)
        {
            dirty = true;
        }

        void DoUpdate()
        {
            plugins = GetComponents<Core.Plugin>();

            Core.Context.Constants constants = new Core.Context.Constants
            {
                desiredScreenSpaceLength = desiredScreenSpaceLength,
                gameObject = gameObject,
                material = material,
                maxDepth = maxDepth,
                resolution = resolution * 2 - 1, // We can only use odd resolutions.
                plugins = new Core.Plugin.PluginChain(plugins)
            };

            Core.Context.Branch[] branches = Core.Context.Branch.GetFromConstants(constants);
            Core.Context.Depth[] depths = Core.Context.Depth.GetFromConstants(constants);
            Core.Context.Triangles[] triangles = Core.Context.Triangles.GetFromConstants(constants);
            Core.Node[] roots = new Core.Node[6];
            for (int i = 0; i < 6; i++)
            {
                roots[i] = Core.Node.CreateRoot(constants, depths[0], branches[i]);
            }

            context = new Core.Context
            {
                constants = constants,
                branches = branches,
                depths = depths,
                triangles = triangles,
                roots = roots
            };

            foreach (Core.Plugin plugin in plugins)
            {
                plugin.OnChange += HandleChange;
                plugin.OnPluginStart();
            }

            constants.plugins.ModifyMaterial(context, constants.material);

            Core.Reconciler.Initialize(context);
        }

        void DoCleanup()
        {
            foreach (Core.Plugin plugin in plugins)
            {
                plugin.OnPluginStop();
                plugin.OnChange -= HandleChange;
            }
            for (int i = 0; i < 6; i++)
            {
                UnityEngine.Object.Destroy(context.branches[i].gameObject);
                context.roots[i].Destroy();
            }
        }

        void Update()
        {
            if (dirty)
            {
                dirty = false;
                DoCleanup();
                DoUpdate();
            }

            if (playerCamera == null || playerCamera.gameObject != player)
            {
                playerCamera = player.GetComponent<Camera>();
            }

            Core.ReconciliationData reconciliationData = Core.ReconciliationData.GetData(context, playerCamera);
            if (reconciliationData == null)
            {
                return;
            }

            Core.Reconciler.Reconcile(context, reconciliationData);
        }
    }
}
