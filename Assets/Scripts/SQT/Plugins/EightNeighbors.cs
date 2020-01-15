using System.Collections.Generic;

namespace SQT.Plugins
{
    public class EightNeighbors : SQT.Core.Plugin, SQT.Core.Plugin.MarkedSetPlugin
    {
        public void ModifyMarkedSet(SQT.Core.Context context, HashSet<SQT.Core.Node> marked, SQT.Core.Node leaf)
        {
            SQT.Core.Node n1 = SQT.Core.Reconciler.EnsureNeighbor(context, leaf, 0);
            SQT.Core.Node n2 = SQT.Core.Reconciler.EnsureNeighbor(context, leaf, 1);
            SQT.Core.Node n3 = SQT.Core.Reconciler.EnsureNeighbor(context, leaf, 2);
            SQT.Core.Node n4 = SQT.Core.Reconciler.EnsureNeighbor(context, leaf, 3);
            SQT.Core.Node n5 = SQT.Core.Reconciler.EnsureNeighbor(context, n1, 3);
            SQT.Core.Node n6 = SQT.Core.Reconciler.EnsureNeighbor(context, n2, 2);
            SQT.Core.Node n7 = SQT.Core.Reconciler.EnsureNeighbor(context, n3, 0);
            SQT.Core.Node n8 = SQT.Core.Reconciler.EnsureNeighbor(context, n4, 1);
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
