using System.Collections.Generic;
using UnityEngine;

namespace SQT2.Core
{
    public static class Reconciler
    {
        public static void Initialize(Context context)
        {
            // TODO: load meshes without depth.
        }

        public static void Reconcile(Context context, ReconciliationData reconciliationData)
        {
            // Mark-and-sweep.
            HashSet<Node> marked = new HashSet<Node>();

            // Add all roots to marked.
            foreach (Node node in context.roots)
            {
                marked.Add(node);
            }

            // Perform deep split.
            Node root = context.roots[reconciliationData.branch.index];
            Node leaf = DeepSplit(context, reconciliationData, root);
            MarkPathToNode(marked, leaf);

            // TODO: mark all children of nodes with some marked children.

            // Walk quad tree and sweep unmarked nodes.
        }

        static void Sweep(HashSet<Node> marked, Node[] nodes)
        {
            foreach (Node node in nodes)
            {
                if (node.children != null)
                {
                    Sweep(marked, node.children);
                }
                if (!marked.Contains(node))
                {

                }
            }
        }

        static void MarkPathToNode(HashSet<Node> marked, Node node)
        {
            while (node != null)
            {
                marked.Add(node);
                node = node.parent;
            }
        }

        static Node EnsureChild(Context context, Node node, int ordinal)
        {
            if (node.children == null)
            {
                node.children = Node.CreateChildren(context, node);
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
