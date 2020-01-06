using System;
using System.Collections.Generic;
using UnityEngine;

public class SQTBuilder
{
    const int MAX_PATH_LENGTH = 5;

    static Vector2[] childOffsetVectors = new Vector2[] {
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

    static int[][] rootOrdinalRotation = new int[][] {
        new int[] { 2, 3, 4, 5 },
        new int[] { 3, 2, 4, 5 },
        new int[] { 4, 5, 0, 1 },
        new int[] { 5, 4, 0, 1 },
        new int[] { 1, 0, 3, 2 },
        new int[] { 0, 1, 3, 2 }
    };

    static int[][][] neighborOrdinalRotation = new int[][][] {
        new int[][] {
            new int[] { 0, 1, 2, 3 },
            new int[] { 0, 1, 2, 3 },
            new int[] { 0, 2, 1, 3 },
            new int[] { 3, 1, 2, 0 },
            new int[] { 3, 1, 2, 0 },
            new int[] { 3, 1, 2, 0 }
        },
        new int[][] {
            new int[] { 0, 1, 2, 3 },
            new int[] { 0, 1, 2, 3 },
            new int[] { 0, 2, 1, 3 },
            new int[] { 3, 1, 2, 0 },
            new int[] { 0, 2, 1, 3 },
            new int[] { 0, 2, 1, 3 }
        },
        new int[][] {
            new int[] { 0, 2, 1, 3 },
            new int[] { 0, 2, 1, 3 },
            new int[] { 0, 1, 2, 3 },
            new int[] { 0, 1, 2, 3 },
            new int[] { 3, 1, 2, 0 },
            new int[] { 0, 2, 1, 3 }
        },
        new int[][] {
            new int[] { 3, 1, 2, 0 },
            new int[] { 3, 1, 2, 0 },
            new int[] { 0, 1, 2, 3 },
            new int[] { 0, 1, 2, 3 },
            new int[] { 3, 1, 2, 0 },
            new int[] { 0, 2, 1, 3 }
        },
        new int[][] {
            new int[] { 3, 1, 2, 0 },
            new int[] { 0, 2, 1, 3 },
            new int[] { 3, 1, 2, 0 },
            new int[] { 3, 1, 2, 0 },
            new int[] { 0, 1, 2, 3 },
            new int[] { 0, 1, 2, 3 }
        },
        new int[][] {
            new int[] { 3, 1, 2, 0 },
            new int[] { 0, 2, 1, 3 },
            new int[] { 0, 2, 1, 3 },
            new int[] { 0, 2, 1, 3 },
            new int[] { 0, 1, 2, 3 },
            new int[] { 0, 1, 2, 3 }
        }
    };

    public Node[] branches;

    public void CalculatePaths(SQTReconciliationData reconciliationData)
    {
        branches = new Node[6];
        for (int i = 0; i < 6; i++)
        {
            branches[i] = new Node
            {
                branch = i,
                children = null,
                parent = null,
                path = new int[0],
                offset = Vector2.zero,
                scale = 1f
            };
        }

        Node leaf = DeepSplit(branches[reconciliationData.constants.branch.index], reconciliationData.pointInPlane);
        BuildBalancedNodes(leaf);
    }

    void BuildBalancedNodes(Node node)
    {
        HashSet<Node> seen = new HashSet<Node>();
        Stack<Node> remaining = new Stack<Node>();
        seen.Add(node);
        remaining.Push(node);
        while (remaining.Count >= 1)
        {
            Node current = remaining.Pop();

            if (current.path.Length <= 1)
            {
                continue;
            }

            Node[] parentNeighbors = new Node[] {
                GetNeighbor(node.parent, 0),
                GetNeighbor(node.parent, 1),
                GetNeighbor(node.parent, 2),
                GetNeighbor(node.parent, 3)
            };

            for (int i = 0; i < 4; i++)
            {
                if (!seen.Contains(parentNeighbors[i]))
                {
                    seen.Add(parentNeighbors[i]);
                    remaining.Push(parentNeighbors[i]);
                }
            }
        }
    }

    Node GetNeighbor(Node node, int direction)
    {
        int[] neighborPath = new int[node.path.Length];
        Array.Copy(node.path, neighborPath, node.path.Length);

        for (int i = node.path.Length - 1; i >= 0; i--)
        {
            neighborPath[i] = neighborOrdinal[node.path[i]][direction];
            if (neighborSameParent[node.path[i]][direction])
            {
                break;
            }
        }

        return GetRelativePath(branches[node.branch], neighborPath);

        // int commonAncestorDistance = 1;
        // for (int i = node.path.Length - 1; i >= 0; i--)
        // {
        //     if (neighborSameParent[node.path[i]][direction])
        //     {
        //         break;
        //     }
        //     commonAncestorDistance += 1;
        // }

        // int[] neighborPath = new int[node.path.Length];
        // // if (commonAncestorDistance <= node.path.Length)
        // {
        //     for (int i = 0; i < node.path.Length; i++)
        //     {
        //         if (i < commonAncestorDistance)
        //         {
        //             neighborPath[node.path.Length - i - 1] = neighborOrdinal[node.path[node.path.Length - i - 1]][direction];
        //         }
        //         else
        //         {
        //             neighborPath[node.path.Length - i - 1] = node.path[node.path.Length - i - 1];
        //         }
        //     }
        //     return GetRelativePath(branches[node.branch], neighborPath);
        // }
        // // else
        // // {
        // //     int fromOrdinal = node.branch;
        // //     int toOrdinal = rootOrdinalRotation[fromOrdinal][direction];
        // //     for (int i = 0; i < node.path.Length; i++)
        // //     {
        // //         neighborPath[i] = neighborOrdinalRotation[fromOrdinal][toOrdinal][node.path[i]];
        // //     }
        // //     return GetRelativePath(branches[toOrdinal], neighborPath);
        // // }
    }

    static Node GetRelativePath(Node node, int[] path)
    {
        Node current = node;
        for (int i = 0; i < path.Length; i++)
        {
            current = GetChild(current, path[i]);
        }
        return current;
    }

    static Node GetChild(Node node, int ordinal)
    {
        if (node.children == null)
        {
            node.children = new Node[] { null, null, null, null };
        }
        if (node.children[ordinal] == null)
        {
            node.children[ordinal] = new Node
            {
                branch = node.branch,
                children = null,
                parent = node,
                path = GetChildPath(node.path, ordinal),
                offset = node.offset + childOffsetVectors[ordinal] * node.scale / 2f,
                scale = node.scale / 2f
            };
        }
        return node.children[ordinal];
    }

    static int GetChildOrdinal(Vector2 pointInPlane, Vector2 offset, float scale)
    {
        Vector2 t = (pointInPlane - offset) / scale;
        return (t.x < 0 ? 0 : 1) + (t.y < 0 ? 0 : 2);
    }

    static Node DeepSplit(Node node, Vector2 pointInPlane)
    {
        if (node.path.Length >= MAX_PATH_LENGTH)
        {
            return node;
        }
        else
        {
            int ordinal = GetChildOrdinal(pointInPlane, node.offset, node.scale);
            return DeepSplit(GetChild(node, ordinal), pointInPlane);
        }
    }

    static int[] GetChildPath(int[] path, int ordinal)
    {
        int[] childPath = new int[path.Length + 1];
        Array.Copy(path, childPath, path.Length);
        childPath[path.Length] = ordinal;
        return childPath;
    }

    public class Node
    {
        public Node parent;
        public Node[] children;
        public int branch;
        public int[] path;
        public Vector2 offset;
        public float scale;

        // public override bool Equals(object obj)
        // {
        //     if (obj == this)
        //     {
        //         return true;
        //     }
        //     if (obj is Node node)
        //     {
        //         if (path == node.path)
        //         {
        //             return true;
        //         }
        //         if (path.Length != node.path.Length)
        //         {
        //             return false;
        //         }
        //         for (int i = 0; i < path.Length; i++)
        //         {
        //             if (path[i] != node.path[i])
        //             {
        //                 return false;
        //             }
        //         }
        //         return true;
        //     }
        //     return false;
        // }

        // public override int GetHashCode()
        // {
        //     int hash = 7 * 31 + path.Length;
        //     for (int i = 0; i < path.Length; i++)
        //     {
        //         hash = hash * 31 + path[i];
        //     }
        //     return hash;
        // }
    }
}
