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
            SQTConstants.SQTBranch branch = new SQTConstants.SQTBranch(directions[i])
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
        for (int i = 0; i < directions.Length; i++)
        {
            SQTReconciliationData reconciliationData = SQTReconciliationData.GetData(constants[i], camera);
            if (reconciliationData == null)
            {
                continue;
            }
            if (reconciliationData.pointInPlane.x < -1f || 1f < reconciliationData.pointInPlane.x || reconciliationData.pointInPlane.y < -1f || 1f < reconciliationData.pointInPlane.y)
            {
                // Point is outside branch quad.
                continue;
            }
            return branches[i].FindNode(reconciliationData.pointInPlane);
        }

        return null;
    }

    public void Reconciliate(Camera camera)
    {
    }

    public void Destroy()
    {
        foreach (SQTNode branch in branches)
        {
            branch.Destroy();
        }
    }
}
