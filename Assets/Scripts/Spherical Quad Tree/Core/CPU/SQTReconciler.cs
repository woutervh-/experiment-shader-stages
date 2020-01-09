namespace SQT.Core.CPU
{
    public class SQTReconciler : SQT.Core.Reconciler
    {
        SQTMeshedNode[] meshedBranches;

        public SQTReconciler(SQTConstants[] constants)
        {
            meshedBranches = new SQTMeshedNode[constants.Length];
        }

        public void Destroy()
        {
            for (int i = 0; i < meshedBranches.Length; i++)
            {
                if (meshedBranches[i] != null)
                {
                    meshedBranches[i].Destroy();
                }
            }
        }

        public void Reconcile(SQTConstants[] constants, SQTBuilder.Node[] newBranches)
        {
            for (int i = 0; i < meshedBranches.Length; i++)
            {
                if (meshedBranches[i] == null)
                {
                    meshedBranches[i] = new SQTMeshedNode(null, constants[i], newBranches[i]);
                }
                Reconcile(constants[i], newBranches[i], meshedBranches, i);
            }
        }

        void Reconcile(SQTConstants constants, SQTBuilder.Node newNode, SQTMeshedNode[] siblings, int index)
        {
            if (siblings[index].neighborMask != newNode.neighborMask)
            {
                siblings[index].neighborMask = newNode.neighborMask;
                siblings[index].SetMeshTriangles(newNode.neighborMask);
            }

            if (newNode.children != null && siblings[index].children != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    Reconcile(constants, newNode.children[i], siblings[index].children, i);
                }
            }
            else if (newNode.children != null && siblings[index].children == null)
            {
                siblings[index].meshRenderer.enabled = false;
                siblings[index].children = new SQTMeshedNode[4];
                for (int i = 0; i < 4; i++)
                {
                    siblings[index].children[i] = new SQTMeshedNode(siblings[index], constants, newNode.children[i]);
                    Reconcile(constants, newNode.children[i], siblings[index].children, i);
                }
            }
            else if (newNode.children == null && siblings[index].children != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    siblings[index].children[i].Destroy();
                }
                siblings[index].children = null;
                siblings[index].meshRenderer.enabled = true;
            }
        }
    }
}
