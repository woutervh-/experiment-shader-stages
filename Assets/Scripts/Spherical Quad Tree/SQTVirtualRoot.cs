using UnityEngine;

public class SQTVirtualRoot : SQTVirtualTaxonomy
{
    static Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

    static int[][] neighborOrdinal = new int[][] {
        new int[] { 2, 3, 4, 5 },
        new int[] { 3, 2, 4, 5 },
        new int[] { 4, 5, 0, 1 },
        new int[] { 5, 4, 0, 1 },
        new int[] { 1, 0, 3, 2 },
        new int[] { 0, 1, 3, 2 }
    };

    SQTConstants.SQTGlobal global;
    SQTConstants[] constants;
    public SQTVirtualNode[] branches;

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
            branches[i] = new SQTVirtualNode(this, constants[i], Vector2.zero, new int[] { i });
        }
    }

    public void Reconciliate(Camera camera)
    {
        for (int i = 0; i < directions.Length; i++)
        {
            branches[i] = new SQTVirtualNode(this, constants[i], Vector2.zero, new int[] { i });
        }

        SQTReconciliationData reconciliationData = GetReconciliationData(camera);
        if (reconciliationData == null)
        {
            return;
        }

        SQTVirtualNode leaf = branches[reconciliationData.constants.branch.index].DeepSplit(reconciliationData);
        leaf.EnsureBalanced();
    }

    public SQTVirtualNode EnsureChild(int childOrdinal)
    {
        return branches[childOrdinal];
    }

    public SQTVirtualNode EnsureChildNeighbor(int childOrdinal, int direction)
    {
        return EnsureChild(neighborOrdinal[childOrdinal][direction]);
    }

    public void EnsureNeighbor(int direction)
    {
        // Root does not have a neighbor.
    }

    public void EnsureBalanced()
    {
        // Root does not need to be balanced.
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
