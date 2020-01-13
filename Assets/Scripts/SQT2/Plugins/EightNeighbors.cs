using System.Collections.Generic;

namespace SQT2.Plugins
{
    public class EightNeighbors : SQT2.Core.Plugin, SQT2.Core.Plugin.MarkedSetPlugin
    {
        public void ModifyMarkedSet(SQT2.Core.Context context, HashSet<SQT2.Core.Node> marked, SQT2.Core.Node leaf)
        {
            if (leaf.parent == null)
            {
                return;
            }

            SQT2.Core.Node n1 = SQT2.Core.Reconciler.EnsureNeighbor(context, leaf.parent, 0);
            SQT2.Core.Node n2 = SQT2.Core.Reconciler.EnsureNeighbor(context, leaf.parent, 1);
            SQT2.Core.Node n3 = SQT2.Core.Reconciler.EnsureNeighbor(context, leaf.parent, 2);
            SQT2.Core.Node n4 = SQT2.Core.Reconciler.EnsureNeighbor(context, leaf.parent, 3);
            SQT2.Core.Node n5 = SQT2.Core.Reconciler.EnsureNeighbor(context, n1, 3);
            SQT2.Core.Node n6 = SQT2.Core.Reconciler.EnsureNeighbor(context, n2, 2);
            SQT2.Core.Node n7 = SQT2.Core.Reconciler.EnsureNeighbor(context, n3, 0);
            SQT2.Core.Node n8 = SQT2.Core.Reconciler.EnsureNeighbor(context, n4, 1);
            marked.Add(n1);
            marked.Add(n2);
            marked.Add(n3);
            marked.Add(n4);
            marked.Add(n5);
            marked.Add(n6);
            marked.Add(n7);
            marked.Add(n8);
        }
    }
}
