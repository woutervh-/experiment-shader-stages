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

    Node[] branches;

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
    }

    Node[] BuildBalancedNodes(Node node)
    {
        HashSet<Node> remainingSet = new HashSet<Node>();
        List<Node> remaining = new List<Node>();
        HashSet<Node> done = new HashSet<Node>();
        remaining.Add(node);
        remainingSet.Add(node);
        while (remaining.Count >= 1)
        {
            Node current = remaining[remaining.Count - 1];
            remaining.RemoveAt(remaining.Count - 1);
            remainingSet.Remove(current);

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
            // TODO:

            done.Add(current);
        }
        Node[] result = new Node[done.Count];
        done.CopyTo(result);
        return result;
    }

    Node GetNeighbor(Node node, int direction)
    {
        if (node.path.Length <= 0)
        {
            return branches[rootOrdinalRotation[node.branch][direction]];
        }

        int commonAncestorDistance = 0;
        for (int i = node.path.Length - 1; i >= 0; i--)
        {
            if (neighborSameParent[node.path[i]][direction])
            {
                break;
            }
            commonAncestorDistance += 1;
        }

        if (commonAncestorDistance < node.path.Length)
        {
            int[]
            for (int i = commonAncestorDistance; i >= 0; i++)
            {

            }
        }
        else
        {

        }

    }

    // start with node, direction, path=node.path, offset=path.Length-1
    Node GetNeighborRelativePath(Node node, int direction, int[] path, int offset)
    {
        if (offset <= 0)
        {
            // TODO: rotate entire path.
            return GetRelativePath(branches[rootOrdinalRotation[node.branch][direction]], path, offset);
        }
        else
        {
            if (neighborSameParent[node.path[node.path.Length - 1]][direction])
            {
                // TODO: use neighborOrdinal on all ordinals in path from offset to end.
                return GetRelativePath(node.parent, path, offset);
            }
            else
            {
                return GetNeighborRelativePath(node.parent, direction, path, offset - 1);
            }
        }
    }

    Node GetRelativePath(Node node, int[] path, int offset)
    {
        Node current = node;
        for (int i = offset; i < path.Length; i++)
        {
            current = GetChild(current, path[i]);
        }
        return current;
    }

    // static Node GetNeighbor(Node node, int direction)
    // {
    //     int commonAncestorDistance = 0;
    //     for (int i = node.path.Length - 1; i >= 0; i--)
    //     {
    //         if (neighborSameParent[node.path[i]][direction])
    //         {
    //             break;
    //         }
    //         commonAncestorDistance += 1;
    //     }

    //     if (commonAncestorDistance < node.path.Length)
    //     {
    //         int[] 
    //         for (int i = commonAncestorDistance; i >= 0; i++)
    //         {

    //         }
    //     }
    //     else
    //     {

    //     }
    // }

    // static Node GetParent(Node node)
    // {
    //     if (node.parent == null)
    //     {
    //         node.parent = new Node
    //         {
    //             branch = node.branch,
    //             children = new Node[] { null, null, null, null },
    //             parent = null,
    //             path = GetParentPath(node.path),
    //             offset = 
    //         };
    //         node.parent.children[node.path[node.path.Length - 1]] = node;
    //     }
    //     return node.parent;
    // }

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
            return GetChild(node, ordinal);
        }
    }

    // static int[] DeepSplit(Vector2 pointInPlane)
    // {
    //     List<int> path = new List<int>(MAX_PATH_LENGTH);
    //     Vector2 offset = Vector2.zero;
    //     float scale = 1f;
    //     while (path.Count < MAX_PATH_LENGTH)
    //     {
    //         int childIndex = GetChildIndex(pointInPlane, offset, scale);
    //         Vector2 childOffset = childOffsetVectors[childIndex];
    //         scale /= 2;
    //         offset = offset + childOffset * scale;
    //         path.Add(childIndex);
    //     }
    //     return path.ToArray();
    // }

    static int[] GetParentPath(int[] path)
    {
        int[] parentPath = new int[path.Length - 1];
        Array.Copy(path, parentPath, path.Length - 1);
        return parentPath;
    }

    static int[] GetChildPath(int[] path, int ordinal)
    {
        int[] childPath = new int[path.Length + 1];
        Array.Copy(path, childPath, path.Length);
        childPath[path.Length] = ordinal;
        return childPath;
    }

    class Node
    {
        public Node parent;
        public Node[] children;
        public int branch;
        public int[] path;
        public Vector2 offset;
        public float scale;

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            if (obj is Node node)
            {
                if (path == node.path)
                {
                    return true;
                }
                if (path.Length != node.path.Length)
                {
                    return false;
                }
                for (int i = 0; i < path.Length; i++)
                {
                    if (path[i] != node.path[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = 7 * 31 + path.Length;
            for (int i = 0; i < path.Length; i++)
            {
                hash = hash * 31 + path[i];
            }
            return hash;
        }
    }
}
