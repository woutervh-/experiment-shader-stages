using UnityEngine;

namespace SQT.Core
{
    public partial class Branches
    {
        static Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        Constants.SQTGlobal global;
        Constants[] constants;
        Reconciler reconciler;

        public Branches(Constants.SQTGlobal global, Constants.SQTDepth[] depth, Constants.SQTMesh[] meshes, ReconcilerFactory reconcilerFactory)
        {
            this.global = global;
            constants = new Constants[directions.Length];
            int[] branchRootPath = new int[0];
            for (int i = 0; i < directions.Length; i++)
            {
                GameObject branchGameObject = new GameObject("SQT (" + i + ")");
                branchGameObject.transform.SetParent(global.gameObject.transform, false);
                Constants.SQTBranch branch = new Constants.SQTBranch(i, directions[i])
                {
                    gameObject = branchGameObject
                };
                constants[i] = new Constants
                {
                    global = global,
                    branch = branch,
                    depth = depth,
                    meshes = meshes
                };
            }
            reconciler = reconcilerFactory.FromConstants(constants);
        }

        public void Reconcile(Camera camera)
        {
            ReconciliationData reconciliationData = ReconciliationData.GetData(global, constants, camera);
            Builder.Node[] branches = Builder.BuildBranches(reconciliationData);
            reconciler.Reconcile(branches);
            // Debug.Log(StringifyNode(branches[0]));
        }

        public void Destroy()
        {
            for (int i = 0; i < directions.Length; i++)
            {
                UnityEngine.Object.Destroy(constants[i].branch.gameObject);
            }
            reconciler.Destroy();
        }
    }
}
