using System;
using UnityEngine;

public class SQTVirtualNode : SQTVirtualTaxonomy
{
    public SQTVirtualNode[] children;
    public SQTVirtualTaxonomy parent;
    public Vector2 offset;
    public int[] path;

    SQTConstants constants;

    public SQTVirtualNode(SQTVirtualTaxonomy parent, SQTConstants constants, Vector2 offset, int[] path)
    {
        this.parent = parent;
        this.constants = constants;
        this.offset = offset;
        this.path = path;
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
            return parent.EnsureChildNeighbor(GetOrdinal(), direction).EnsureChild(neighborOrdinal[childOrdinal][direction]);
        }
    }

    public void EnsureNeighbor(int direction)
    {
        parent.EnsureChildNeighbor(GetOrdinal(), direction);
    }

    public void EnsureBalanced()
    {
        for (int i = 0; i < 4; i++)
        {
            parent.EnsureNeighbor(i);
        }
        parent.EnsureBalanced();
    }

    void Split()
    {
        if (children == null)
        {
            children = new SQTVirtualNode[4];
            for (int i = 0; i < 4; i++)
            {
                int[] childPath = new int[path.Length + 1];
                Array.Copy(path, childPath, path.Length);
                childPath[path.Length] = i;
                children[i] = new SQTVirtualNode(this, constants, offset + constants.depth[GetDepth() + 1].scale * childOffsetVectors[i], childPath);
            }
        }
    }

    public int GetDepth()
    {
        return path.Length - 1;
    }

    public int GetOrdinal()
    {
        return path[path.Length - 1];
    }

    bool ShouldSplit(SQTReconciliationData reconciliationData)
    {
        return GetDepth() < constants.global.maxDepth
            && constants.depth[GetDepth()].approximateSize > reconciliationData.desiredLength;
    }

    int GetChildIndex(SQTReconciliationData reconciliationData)
    {
        Vector2 t = (reconciliationData.pointInPlane - offset) / constants.depth[GetDepth()].scale;
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
