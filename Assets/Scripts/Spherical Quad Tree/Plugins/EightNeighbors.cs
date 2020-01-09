using System;
using UnityEngine;

namespace SQT.Plugins
{
    public class EightNeighbors : MonoBehaviour, SQT.Core.Plugin, SQT.Core.BuilderLeavesPlugin
    {
        public event EventHandler OnChange;

        void OnValidate()
        {
            if (OnChange != null)
            {
                OnChange.Invoke(this, EventArgs.Empty);
            }
        }

        // public void ModifyBuilderLeaves(ref SQT.Core.Builder.Node[] leaves, SQT.Core.Builder.Node[] branches)
        // {
        //     List<SQT.Core.Builder.Node> newLeaves = new List<SQT.Core.Builder.Node>();

        //     SQT.Core.Builder.Node current = leaves[0];
        //     while (current != null)
        //     {
        //         SQT.Core.Builder.Node n1 = SQT.Core.Builder.EnsureNeighbor(branches, current, 0);
        //         SQT.Core.Builder.Node n2 = SQT.Core.Builder.EnsureNeighbor(branches, current, 1);
        //         SQT.Core.Builder.Node n3 = SQT.Core.Builder.EnsureNeighbor(branches, current, 2);
        //         SQT.Core.Builder.Node n4 = SQT.Core.Builder.EnsureNeighbor(branches, current, 3);
        //         SQT.Core.Builder.Node n5 = SQT.Core.Builder.EnsureNeighbor(branches, n1, 3);
        //         SQT.Core.Builder.Node n6 = SQT.Core.Builder.EnsureNeighbor(branches, n2, 2);
        //         SQT.Core.Builder.Node n7 = SQT.Core.Builder.EnsureNeighbor(branches, n3, 0);
        //         SQT.Core.Builder.Node n8 = SQT.Core.Builder.EnsureNeighbor(branches, n4, 1);
        //         newLeaves.Add(n1);
        //         newLeaves.Add(n2);
        //         newLeaves.Add(n3);
        //         newLeaves.Add(n4);
        //         newLeaves.Add(n5);
        //         newLeaves.Add(n6);
        //         newLeaves.Add(n7);
        //         newLeaves.Add(n8);
        //         current = current.parent;
        //     }

        //     leaves = newLeaves.ToArray();
        // }

        public void ModifyBuilderLeaves(ref SQT.Core.Builder.Node[] leaves, SQT.Core.Builder.Node[] branches)
        {
            SQT.Core.Builder.Node[] newLeaves = new SQT.Core.Builder.Node[leaves.Length + 8];
            Array.Copy(leaves, newLeaves, leaves.Length);

            newLeaves[leaves.Length + 0] = SQT.Core.Builder.EnsureNeighbor(branches, leaves[0], 0);
            newLeaves[leaves.Length + 1] = SQT.Core.Builder.EnsureNeighbor(branches, leaves[0], 1);
            newLeaves[leaves.Length + 2] = SQT.Core.Builder.EnsureNeighbor(branches, leaves[0], 2);
            newLeaves[leaves.Length + 3] = SQT.Core.Builder.EnsureNeighbor(branches, leaves[0], 3);
            newLeaves[leaves.Length + 4] = SQT.Core.Builder.EnsureNeighbor(branches, newLeaves[leaves.Length + 0], 3);
            newLeaves[leaves.Length + 5] = SQT.Core.Builder.EnsureNeighbor(branches, newLeaves[leaves.Length + 1], 2);
            newLeaves[leaves.Length + 6] = SQT.Core.Builder.EnsureNeighbor(branches, newLeaves[leaves.Length + 2], 0);
            newLeaves[leaves.Length + 7] = SQT.Core.Builder.EnsureNeighbor(branches, newLeaves[leaves.Length + 3], 1);

            leaves = newLeaves;
        }
    }
}
