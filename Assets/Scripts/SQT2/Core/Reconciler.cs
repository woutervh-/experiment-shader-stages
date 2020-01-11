using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace SQT2.Core
{
    public static class Reconciler
    {
        public static void Initialize(Context context)
        {
            HashSet<Node> marked = new HashSet<Node>();
            MarkRoots(context, marked);
            PerformMarkAndSweep(context, marked);
        }

        public static void Reconcile(Context context, ReconciliationData reconciliationData)
        {
            HashSet<Node> marked = new HashSet<Node>();
            MarkRoots(context, marked);

            // Perform deep split.
            Node root = context.roots[reconciliationData.branch.index];
            Node leaf = DeepSplit(context, reconciliationData, root);
            marked.Add(leaf);

            // Mark nodes to create a balanced tree (max 2:1 split between neighbors).
            MarkBalancedNodes(context, marked, leaf);

            PerformMarkAndSweep(context, marked);
        }

        static void PerformMarkAndSweep(Context context, HashSet<Node> marked)
        {
            // Ensure parents and siblings of marked nodes are marked as well.
            MarkRequiredNodes(marked, context.roots);

            // Walk quad tree and sweep unmarked nodes.
            Sweep(marked, context.roots);

            // Request meshes.
            MakeMeshRequests(context, context.roots);

            // Set mesh visibilities.
            DetermineVisibleMeshes(context, context.roots);
        }

        static void DetermineVisibleMeshes(Context context, Node[] nodes)
        {
            foreach (Node node in nodes)
            {
                // TODO: also consider neighbor nodes. If they are not yet loaded, then there could be misalignment...

                if (node.mesh == null)
                {
                    continue;
                }

                if (node.children != null && AreChildMeshesLoaded(node))
                {
                    node.meshRenderer.enabled = false;
                    DetermineVisibleMeshes(context, node.children);
                }
                else
                {
                    // TODO: determine actual neighbor mask.
                    int neighborMask = 0;
                    if (node.mesh.triangles != context.triangles[neighborMask].triangles)
                    {
                        node.mesh.triangles = context.triangles[neighborMask].triangles;
                    }
                    node.meshRenderer.enabled = true;
                }
            }
        }

        static bool AreChildMeshesLoaded(Node parent)
        {
            foreach (Node child in parent.children)
            {
                if (child.mesh == null)
                {
                    return false;
                }
            }
            return true;
        }

        static void MakeMeshRequests(Context context, Node[] nodes)
        {
            foreach (Node node in nodes)
            {
                if (node.meshRequest == null)
                {
                    node.meshRequestCancellation = new CancellationTokenSource();
                    node.meshRequest = node.RequestMesh(context);
                }
                if (node.children != null)
                {
                    MakeMeshRequests(context, node.children);
                }
            }
        }

        static void Sweep(HashSet<Node> marked, Node[] nodes)
        {
            foreach (Node node in nodes)
            {
                if (node.children != null)
                {
                    Sweep(marked, node.children);
                    if (!HasMarkedChild(marked, node))
                    {
                        Node.RemoveChildren(node);
                    }
                }
                if (!marked.Contains(node))
                {
                    node.Destroy();
                }
            }
        }

        static void MarkRoots(Context context, HashSet<Node> marked)
        {
            foreach (Node root in context.roots)
            {
                marked.Add(root);
            }
        }

        static void MarkBalancedNodes(Context context, HashSet<Node> marked, Node node)
        {
            HashSet<Node> seen = new HashSet<Node>();
            Stack<Node> remaining = new Stack<Node>();
            remaining.Push(node);
            while (remaining.Count >= 1)
            {
                Node current = remaining.Pop();
                if (current.path.Length <= 1)
                {
                    continue;
                }

                Node[] parentNeighbors = new Node[] {
                    EnsureNeighbor(context, current.parent, 0),
                    EnsureNeighbor(context, current.parent, 1),
                    EnsureNeighbor(context, current.parent, 2),
                    EnsureNeighbor(context, current.parent, 3)
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
            marked.UnionWith(seen);
        }

        static void MarkRequiredNodes(HashSet<Node> marked, Node[] nodes)
        {
            foreach (Node node in nodes)
            {
                if (node.children != null)
                {
                    MarkRequiredNodes(marked, node.children);
                    if (HasMarkedChild(marked, node))
                    {
                        marked.Add(node);
                        foreach (Node child in node.children)
                        {
                            marked.Add(child);
                        }
                    }
                }
            }
        }

        static bool HasMarkedChild(HashSet<Node> marked, Node node)
        {
            foreach (Node child in node.children)
            {
                if (marked.Contains(child))
                {
                    return true;
                }
            }
            return false;
        }

        static Node EnsureChild(Context context, Node node, int ordinal)
        {
            if (node.children == null)
            {
                Node.CreateChildren(context, node);
            }
            return node.children[ordinal];
        }

        static Node EnsureRelativePath(Context context, Node node, int[] path)
        {
            Node current = node;
            for (int i = 0; i < path.Length; i++)
            {
                current = EnsureChild(context, current, path[i]);
            }
            return current;
        }

        static int GetNeighborCommonAncestorDistance(Node node, int direction)
        {
            int commonAncestorDistance = 1;
            for (int i = node.path.Length - 1; i >= 0; i--)
            {
                if (Node.neighborSameParent[node.path[i]][direction])
                {
                    break;
                }
                commonAncestorDistance += 1;
            }
            return commonAncestorDistance;
        }

        static int[] GetNeighborPath(Node node, int direction, int commonAncestorDistance)
        {
            int[] neighborPath = new int[node.path.Length];
            if (commonAncestorDistance <= node.path.Length)
            {
                for (int i = 0; i < node.path.Length; i++)
                {
                    if (i < commonAncestorDistance)
                    {
                        neighborPath[node.path.Length - i - 1] = Node.neighborOrdinal[node.path[node.path.Length - i - 1]][direction];
                    }
                    else
                    {
                        neighborPath[node.path.Length - i - 1] = node.path[node.path.Length - i - 1];
                    }
                }
            }
            else
            {
                int fromOrdinal = node.branch.index;
                int toOrdinal = Node.rootOrdinalRotation[fromOrdinal][direction];
                for (int i = 0; i < node.path.Length; i++)
                {
                    neighborPath[i] = Node.neighborOrdinalRotation[fromOrdinal][toOrdinal][node.path[i]];
                }
            }
            return neighborPath;
        }

        static int GetNeighborBranch(Node node, int direction, int commonAncestorDistance)
        {
            if (commonAncestorDistance <= node.path.Length)
            {
                return node.branch.index;
            }
            else
            {
                return Node.rootOrdinalRotation[node.branch.index][direction];
            }
        }

        static Node EnsureNeighbor(Context context, Node node, int direction)
        {
            int commonAncestorDistance = GetNeighborCommonAncestorDistance(node, direction);
            int[] neighborPath = GetNeighborPath(node, direction, commonAncestorDistance);
            int neighborBranch = GetNeighborBranch(node, direction, commonAncestorDistance);
            return EnsureRelativePath(context, context.roots[neighborBranch], neighborPath);
        }

        static int GetChildOrdinal(Vector2 pointInPlane, Vector2 offset, float scale)
        {
            Vector2 t = (pointInPlane - offset) / scale;
            return (t.x < 0 ? 0 : 1) + (t.y < 0 ? 0 : 2);
        }

        static bool ShouldSplit(Context context, ReconciliationData reconciliationData, Node node)
        {
            return node.path.Length < context.constants.maxDepth
                && node.depth.approximateSize > reconciliationData.desiredLength;
        }

        static Node DeepSplit(Context context, ReconciliationData reconciliationData, Node node)
        {
            if (ShouldSplit(context, reconciliationData, node))
            {
                int ordinal = GetChildOrdinal(reconciliationData.pointInPlane, node.offset, node.depth.scale);
                return DeepSplit(context, reconciliationData, EnsureChild(context, node, ordinal));
            }
            else
            {
                return node;
            }
        }
    }
}
