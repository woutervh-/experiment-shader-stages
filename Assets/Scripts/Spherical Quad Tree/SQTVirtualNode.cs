using System;
using UnityEngine;

public class SQTVirtualNode : SQTTaxomy
{
    public SQTVirtualNode[] children;

    SQTTaxomy parent;
    SQTConstants constants;
    Vector2 offset;
    int depth;
    int ordinal;

    public SQTVirtualNode(SQTTaxomy parent, SQTConstants constants, Vector2 offset, int depth, int ordinal)
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
            int childIndex = GetChildIndex(reconciliationData.pointInPlane);
            return children[childIndex].DeepSplit(reconciliationData);
        }
        else
        {
            return this;
        }
    }

    public SQTVirtualNode GetChild(int ordinal)
    {
        return children[ordinal];
    }

    public SQTVirtualNode EnsureNeighbor(int direction)
    {
        // direction: west=0, east=1, south=2, north=3
        // ordinal: south west=0, south east=1, north west=2, north east=3

        SQTTaxomy neighborParent;
        if (direction == (ordinal & 1) || direction == ((ordinal >> 1) | 2))
        {
            neighborParent = parent.EnsureNeighbor(direction);
        }
        else
        {
            neighborParent = parent;
        }
        return neighborParent.GetChild(((direction ^ (ordinal & 2)) & 2) | ((direction ^ (((ordinal << 1) ^ 2) & 2)) >> 1));

        // if (
        //     ordinal == 0 && (direction == 0 || direction == 2)
        //     || ordinal == 1 && (direction == 1 || direction == 2)
        //     || ordinal == 2 && (direction == 0 || direction == 3)
        //     || ordinal == 3 && (direction == 1 || direction == 3)
        // )
        // {
        //     neighborParent = parent.EnsureNeighbor(direction);
        // }
        // else
        // {
        //     neighborParent = parent;
        // }

        // if (ordinal == 0)
        // {
        //     if (direction == 0 || direction == 1)
        //     {
        //         return neighborParent.GetChild(1);
        //     }
        //     else
        //     {
        //         return neighborParent.GetChild(2);
        //     }
        // }
        // else if (ordinal == 1)
        // {
        //     if (direction == 0 || direction == 1)
        //     {
        //         return neighborParent.GetChild(0);
        //     }
        //     else
        //     {
        //         return neighborParent.GetChild(3);
        //     }
        // }
        // else if (ordinal == 2)
        // {
        //     if (direction == 0 || direction == 1)
        //     {
        //         return neighborParent.GetChild(3);
        //     }
        //     else
        //     {
        //         return neighborParent.GetChild(0);
        //     }
        // }
        // else
        // {
        //     if (direction == 1 || direction == 3)
        //     {
        //         if (direction == 1)
        //         {
        //             return neighborParent.GetChild(2);
        //         }
        //         else
        //         {
        //             return neighborParent.GetChild(1);
        //         }
        //     }
        //     else
        //     {
        //         if (direction == 0)
        //         {
        //             return neighborParent.GetChild(2);
        //         }
        //         else
        //         {
        //             return neighborParent.GetChild(1);
        //         }
        //     }
        // }
    }

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

    int GetChildIndex(Vector2 pointInPlane)
    {
        Vector2 t = (pointInPlane - offset) / constants.depth[depth].scale;
        return (t.x < 0f ? 0 : 1) + (t.y < 0f ? 0 : 2);
    }

    static Vector2[] childOffsetVectors = {
        new Vector2(-1f, -1f),
        new Vector2(1f, -1f),
        new Vector2(-1f, 1f),
        new Vector2(1f, 1f),
     };
}
