using System;
using UnityEngine;

namespace SQT2
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

        void OnEnable()
        {
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

        void DoUpdate()
        {
            Core.Context.Constants constants = new Core.Context.Constants
            {
                desiredScreenSpaceLength = desiredScreenSpaceLength,
                gameObject = gameObject,
                material = material,
                maxDepth = maxDepth,
                resolution = resolution * 2 - 1 // We can only use odd resolutions.
            };

            Core.Context.Branch[] branches = Core.Context.Branch.GetFromConstants(constants);
            Core.Context.Depth[] depths = Core.Context.Depth.GetFromConstants(constants);
            Core.Context.Triangles[] triangles = Core.Context.Triangles.GetFromConstants(constants);
            Core.Node[] roots = new Core.Node[6];
            for (int i = 0; i < 6; i++)
            {
                roots[i] = Core.Node.CreateRoot(context, context.branches[i]);
            }

            context = new Core.Context
            {
                constants = constants,
                branches = branches,
                depths = depths,
                triangles = triangles,
                roots = roots
            };

            Core.Reconciler.Initialize(context);
        }

        void DoCleanup()
        {
            for (int i = 0; i < 6; i++)
            {
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
