using UnityEngine;

public class SQTVirtualNode : SQTVirtualTaxonomy
{
    public SQTVirtualNode[] children;
    public SQTVirtualTaxonomy parent;

    SQTConstants constants;
    Vector2 offset;
    int depth;
    int ordinal;

    public SQTVirtualNode(SQTVirtualTaxonomy parent, SQTConstants constants, Vector2 offset, int depth, int ordinal)
    {
        this.parent = parent;
        this.constants = constants;
        this.offset = offset;
        this.depth = depth;
        this.ordinal = ordinal;
    }

    public SQTVirtualNode DeepSplit(SQTReconciliationData reconciliationData)
    {
        if (ShouldSplit(reconciliationData))
        {
            Split();
            int childIndex = GetChildIndex(reconciliationData);
            return children[childIndex].DeepSplit(reconciliationData);
        }
        else
        {
            return this;
        }
    }

    public SQTVirtualNode EnsureChild(int childOrdinal)
    {
        Split();
        return children[childOrdinal];
    }

    public SQTVirtualNode EnsureChildNeighbor(int childOrdinal, int direction)
    {
        if (neighborSameParent[childOrdinal][direction])
        {
            return EnsureChild(neighborOrdinal[childOrdinal][direction]);
        }
        else
        {
            return parent.EnsureChildNeighbor(ordinal, direction).EnsureChild(neighborOrdinal[childOrdinal][direction]);
        }
    }

    public void EnsureNeighbor(int direction)
    {
        parent.EnsureChildNeighbor(ordinal, direction);
    }

    public void EnsureBalanced()
    {
        for (int i = 0; i < 4; i++)
        {
            parent.EnsureNeighbor(i);
        }
        parent.EnsureBalanced();
    }

    // public SQTVirtualNode EnsureNeighbor(int direction)
    // {
    //     SQTTaxomy neighborParent;
    //     if (neighborSameParent[ordinal][direction])
    //     {
    //         neighborParent = parent;
    //     }
    //     else
    //     {
    //         neighborParent = parent.EnsureNeighbor(direction);
    //     }
    //     return neighborParent.EnsureChild(neighborOrdinal[ordinal][direction]);
    // }

    void Split()
    {
        if (children == null)
        {
            children = new SQTVirtualNode[4];
            for (int i = 0; i < 4; i++)
            {
                children[i] = new SQTVirtualNode(this, constants, offset + constants.depth[depth + 1].scale * childOffsetVectors[i], depth + 1, i);
            }
        }
    }

    bool ShouldSplit(SQTReconciliationData reconciliationData)
    {
        return depth < constants.global.maxDepth
            && constants.depth[depth].approximateSize > reconciliationData.desiredLength;
    }

    int GetChildIndex(SQTReconciliationData reconciliationData)
    {
        Vector2 t = (reconciliationData.pointInPlane - offset) / constants.depth[depth].scale;
        return (t.x < 0f ? 0 : 1) + (t.y < 0f ? 0 : 2);
    }

    static Vector2[] childOffsetVectors = {
        new Vector2(-1f, -1f),
        new Vector2(1f, -1f),
        new Vector2(-1f, 1f),
        new Vector2(1f, 1f),
     };

    static bool[][] neighborSameParent = new bool[][] {
        new bool[] { false, true, false, true },
        new bool[] { true, false, false, true },
        new bool[] { false, true, true, false },
        new bool[] { true, false, true, false }
    };

    static int[][] neighborOrdinal = new int[][] {
        new int[] { 1, 1, 2, 2 },
        new int[] { 0, 0, 3, 3 },
        new int[] { 3, 3, 0, 0 },
        new int[] { 2, 2, 1, 1 }
    };
}
