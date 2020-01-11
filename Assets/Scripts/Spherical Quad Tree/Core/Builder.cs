using System;
using System.Collections.Generic;
using UnityEngine;

namespace SQT.Core
{
    public class Builder
    {
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

        public static Node[] BuildBranches(ReconciliationData reconciliationData)
        {
            Node[] branches = new Node[6];
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

            Node[] leaves = new Node[] { DeepSplit(branches[reconciliationData.constants.branch.index], reconciliationData) };
            reconciliationData.constants.global.plugins.ModifyBuilderLeaves(ref leaves, branches);

            foreach (Node leaf in leaves)
            {
                BuildBalancedNodes(branches, leaf);
            }

            FillMissingSiblings(branches);
            DetermineNeighborMasks(branches);
            return branches;
        }

        static void DetermineNeighborMasks(Node[] branches)
        {
            foreach (Node branch in branches)
            {
                DetermineNeighborMasks(branches, branch);
            }
        }

        static void DetermineNeighborMasks(Node[] branches, Node node)
        {
            for (int i = 0; i < 4; i++)
            {
                if (!HasNeighbor(branches, node, i))
                {
                    node.neighborMask |= Node.NEIGHBOR_MASKS[i];
                }
            }
            if (node.children != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    DetermineNeighborMasks(branches, node.children[i]);
                }
            }
        }

        static void FillMissingSiblings(Node[] branches)
        {
            foreach (Node branch in branches)
            {
                FillMissingSiblings(branch);
            }
        }

        static void FillMissingSiblings(Node node)
        {
            if (node.children != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    FillMissingSiblings(EnsureChild(node, i));
                }
            }
        }

        static void BuildBalancedNodes(Node[] branches, Node node)
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
                EnsureNeighbor(branches, current.parent, 0),
                EnsureNeighbor(branches, current.parent, 1),
                EnsureNeighbor(branches, current.parent, 2),
                EnsureNeighbor(branches, current.parent, 3)
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

        public static int GetCommonAncestorDistance(Node node, int direction)
        {
            int commonAncestorDistance = 1;
            for (int i = node.path.Length - 1; i >= 0; i--)
            {
                if (neighborSameParent[node.path[i]][direction])
                {
                    break;
                }
                commonAncestorDistance += 1;
            }
            return commonAncestorDistance;
        }

        public static int[] GetNeighborPath(Node node, int direction, int commonAncestorDistance)
        {
            int[] neighborPath = new int[node.path.Length];
            if (commonAncestorDistance <= node.path.Length)
            {
                for (int i = 0; i < node.path.Length; i++)
                {
                    if (i < commonAncestorDistance)
                    {
                        neighborPath[node.path.Length - i - 1] = neighborOrdinal[node.path[node.path.Length - i - 1]][direction];
                    }
                    else
                    {
                        neighborPath[node.path.Length - i - 1] = node.path[node.path.Length - i - 1];
                    }
                }
            }
            else
            {
                int fromOrdinal = node.branch;
                int toOrdinal = rootOrdinalRotation[fromOrdinal][direction];
                for (int i = 0; i < node.path.Length; i++)
                {
                    neighborPath[i] = neighborOrdinalRotation[fromOrdinal][toOrdinal][node.path[i]];
                }
            }
            return neighborPath;
        }

        public static int GetNeighborBranch(Node node, int direction, int commonAncestorDistance)
        {
            if (commonAncestorDistance <= node.path.Length)
            {
                return node.branch;
            }
            else
            {
                return rootOrdinalRotation[node.branch][direction];
            }
        }

        public static bool HasNeighbor(Node[] branches, Node node, int direction)
        {
            int commonAncestorDistance = GetCommonAncestorDistance(node, direction);
            int[] neighborPath = GetNeighborPath(node, direction, commonAncestorDistance);
            int neighborBranch = GetNeighborBranch(node, direction, commonAncestorDistance);
            return GetRelativePath(branches[neighborBranch], neighborPath) != null;
        }

        public static Node EnsureNeighbor(Node[] branches, Node node, int direction)
        {
            int commonAncestorDistance = GetCommonAncestorDistance(node, direction);
            int[] neighborPath = GetNeighborPath(node, direction, commonAncestorDistance);
            int neighborBranch = GetNeighborBranch(node, direction, commonAncestorDistance);
            return EnsureRelativePath(branches[neighborBranch], neighborPath);
        }

        public static Node GetRelativePath(Node node, int[] path)
        {
            Node current = node;
            for (int i = 0; i < path.Length && current != null; i++)
            {
                current = GetChild(current, path[i]);
            }
            return current;
        }

        public static Node EnsureRelativePath(Node node, int[] path)
        {
            Node current = node;
            for (int i = 0; i < path.Length; i++)
            {
                current = EnsureChild(current, path[i]);
            }
            return current;
        }

        public static Node GetChild(Node node, int ordinal)
        {
            if (node.children == null)
            {
                return null;
            }
            if (node.children[ordinal] == null)
            {
                return null;
            }
            return node.children[ordinal];
        }

        public static Node EnsureChild(Node node, int ordinal)
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

        static bool ShouldSplit(Node node, ReconciliationData reconciliationData)
        {
            return node.path.Length < reconciliationData.constants.global.maxDepth
                && reconciliationData.constants.depth[node.path.Length].approximateSize > reconciliationData.desiredLength;
        }

        static Node DeepSplit(Node node, ReconciliationData reconciliationData)
        {
            if (ShouldSplit(node, reconciliationData))
            {
                int ordinal = GetChildOrdinal(reconciliationData.pointInPlane, node.offset, node.scale);
                return DeepSplit(EnsureChild(node, ordinal), reconciliationData);
            }
            else
            {
                return node;
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
            public static int[] NEIGHBOR_MASKS = new int[] { 1, 2, 4, 8 };

            public Node parent;
            public Node[] children;
            public int branch;
            public int[] path;
            public Vector2 offset;
            public float scale;
            public int neighborMask;
        }
    }
}
