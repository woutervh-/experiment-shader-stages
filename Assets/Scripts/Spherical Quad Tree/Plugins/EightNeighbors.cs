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

        public void StartPlugin() { }

        public void StopPlugin() { }

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
