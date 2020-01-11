using System.Collections.Generic;

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
            // Core.Node root = context.roots[reconciliationData.branch.index];

            // Mark-and-sweep.
            HashSet<Node> marked = new HashSet<Node>();

            // Add all roots to marked.
            foreach (Node root in context.roots)
            {
                marked.Add(root);
            }

            // Perform deep split.
            // TODO:

            // Walk quad tree and sweep unmarked nodes.
        }
    }
}
