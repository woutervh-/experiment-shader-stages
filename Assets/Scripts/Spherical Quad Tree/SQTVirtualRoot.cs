using UnityEngine;

public class SQTVirtualRoot : SQTVirtualTaxonomy
{
    static Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

    SQTConstants.SQTGlobal global;
    SQTConstants[] constants;
    SQTVirtualNode[] branches;

    public SQTVirtualRoot(SQTConstants.SQTGlobal global, SQTConstants.SQTDepth[] depth)
    {
        this.global = global;
        branches = new SQTVirtualNode[directions.Length];
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
                depth = depth
            };
            branches[i] = new SQTVirtualNode(this, constants[i], Vector2.zero, 0, i);
        }
    }

    public void Reconciliate(Camera camera)
    {
        for (int i = 0; i < directions.Length; i++)
        {
            branches[i] = new SQTVirtualNode(this, constants[i], Vector2.zero, 0, i);
        }

        SQTReconciliationData reconciliationData = GetReconciliationData(camera);
        if (reconciliationData == null)
        {
            return;
        }

        SQTVirtualNode leaf = branches[reconciliationData.constants.branch.index].DeepSplit(reconciliationData);
        leaf.parent.EnsureChildNeighbor(leaf.ordinal, );
    }

    public SQTVirtualNode EnsureChild(int childOrdinal)
    {
        return branches[childOrdinal];
    }

    public SQTVirtualNode EnsureChildNeighbor(int childOrdinal, int direction)
    {
        return EnsureChild(neighborOrdinal[childOrdinal][direction]);
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

    static int[][] neighborOrdinal = new int[][] {
        new int[] { 2, 3, 5, 4 },
        new int[] { 3, 2, 4, 5 },
        new int[] { 5, 4, 1, 0 },
        new int[] { 4, 5, 0, 1 },
        new int[] { 3, 2, 1, 0 },
        new int[] { 2, 3, 0, 1 }
    };
}
