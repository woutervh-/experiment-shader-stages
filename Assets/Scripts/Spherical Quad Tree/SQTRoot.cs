using UnityEngine;

public class SQTRoot : SQTTaxomy
{
    static Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

    SQTConstants.SQTGlobal global;
    SQTConstants[] constants;
    SQTNode[] branches;

    public SQTRoot(SQTConstants.SQTGlobal global, SQTConstants.SQTDepth[] depth)
    {
        this.global = global;
        branches = new SQTNode[directions.Length];
        constants = new SQTConstants[directions.Length];
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
                depth = depth
            };
            branches[i] = new SQTNode(this, constants[i], Vector2.zero, 0);
        }
    }

    public SQTNode FindNode(Camera camera)
    {
        SQTReconciliationData reconciliationData = GetReconciliationData(camera);
        if (reconciliationData == null)
        {
            return null;
        }
        return branches[reconciliationData.constants.branch.index].FindNode(reconciliationData.pointInPlane);
    }

    public void Reconciliate(Camera camera)
    {
        SQTReconciliationData reconciliationData = GetReconciliationData(camera);
        if (reconciliationData == null)
        {
            return;
        }
        branches[reconciliationData.constants.branch.index].Reconciliate(reconciliationData);
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

    public void Destroy()
    {
        foreach (SQTNode branch in branches)
        {
            branch.Destroy();
        }
    }
}
