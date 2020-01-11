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
            MarkRoots(marked, context);
            PerformMarkAndSweep(context, marked);
        }

        public static void Reconcile(Context context, ReconciliationData reconciliationData)
        {
            HashSet<Node> marked = new HashSet<Node>();
            MarkRoots(marked, context);

            // Perform deep split.
            Node root = context.roots[reconciliationData.branch.index];
            Node leaf = DeepSplit(context, reconciliationData, root);
            marked.Add(leaf);

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

        static void MarkRoots(HashSet<Node> marked, Context context)
        {
            foreach (Node root in context.roots)
            {
                marked.Add(root);
            }
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
