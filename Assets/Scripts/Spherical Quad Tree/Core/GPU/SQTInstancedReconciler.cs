namespace SQT.Core.GPU
{
    public class SQTInstancedReconciler : SQT.Core.Reconciler
    {
        SQTInstancedMesh instancedMesh;
        SQTInstancedNode[] instancedBranches;

        public SQTInstancedReconciler(SQTConstants[] constants)
        {
            instancedMesh = new SQTInstancedMesh(constants[0].global, constants[0].meshes); // TODO: make proper fix for meshes location.
            instancedBranches = new SQTInstancedNode[constants.Length];
        }

        public void Destroy()
        {
            for (int i = 0; i < instancedBranches.Length; i++)
            {
                if (instancedBranches[i] != null)
                {
                    instancedBranches[i].Destroy();
                }
            }
        }

        public void Reconcile(SQTConstants[] constants, SQTBuilder.Node[] newBranches)
        {
            for (int i = 0; i < instancedBranches.Length; i++)
            {
                if (instancedBranches[i] == null)
                {
                    instancedBranches[i] = new SQTInstancedNode(null, constants[i], instancedMesh, newBranches[i]);
                }
                Reconcile(constants[i], newBranches[i], instancedBranches, i);
            }
        }

        void Reconcile(SQTConstants constants, SQTBuilder.Node newNode, SQTInstancedNode[] siblings, int index)
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
                siblings[index].children = new SQTInstancedNode[4];
                for (int i = 0; i < 4; i++)
                {
                    siblings[index].children[i] = new SQTInstancedNode(siblings[index], constants, instancedMesh, newNode.children[i]);
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
