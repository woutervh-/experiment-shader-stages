using UnityEngine;

public partial class SQTBranches
{
    static Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

    SQTConstants[] constants;
    SQTReconciler reconciler;

    public SQTBranches(SQTConstants.SQTGlobal global, SQTConstants.SQTDepth[] depth, SQTConstants.SQTMesh[] meshes)
    {
        constants = new SQTConstants[directions.Length];
        int[] branchRootPath = new int[0];
        for (int i = 0; i < directions.Length; i++)
        {
            GameObject branchGameObject = new GameObject("SQT (" + i + ")");
            branchGameObject.transform.SetParent(global.gameObject.transform, false);
            SQTConstants.SQTBranch branch = new SQTConstants.SQTBranch(i, directions[i])
            {
                gameObject = branchGameObject
            };
            constants[i] = new SQTConstants
            {
                global = global,
                branch = branch,
                depth = depth,
                meshes = meshes
            };
        }

        reconciler = new SQTReconciler(constants);
    }

    public void Reconcile(Camera camera)
    {
        SQTReconciliationData reconciliationData = GetReconciliationData(camera);
        SQTBuilder.Node[] branches = SQTBuilder.BuildBranches(reconciliationData);
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

    SQTReconciliationData GetReconciliationData(Camera camera)
    {
        for (int i = 0; i < directions.Length; i++)
        {
            SQTReconciliationData reconciliationData = SQTReconciliationData.GetData(constants[i], camera);
            if (reconciliationData != null)
            {
                return reconciliationData;
            }
        }
        return null;
    }
}
