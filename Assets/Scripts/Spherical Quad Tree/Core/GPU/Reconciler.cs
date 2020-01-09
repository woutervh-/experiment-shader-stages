namespace SQT.Core.GPU
{
    public class Reconciler : SQT.Core.Reconciler
    {
        Constants[] constants;
        InstancedMesh instancedMesh;
        MeshedNode[] instancedBranches;

        public Reconciler(Constants[] constants)
        {
            this.constants = constants;
            instancedMesh = new InstancedMesh(constants[0].global, constants[0].meshes); // TODO: make proper fix for meshes location.
            instancedBranches = new MeshedNode[constants.Length];
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

        public void Reconcile(Builder.Node[] newBranches)
        {
            for (int i = 0; i < instancedBranches.Length; i++)
            {
                if (instancedBranches[i] == null)
                {
                    instancedBranches[i] = new MeshedNode(null, constants[i], instancedMesh, newBranches[i]);
                }
                Reconcile(constants[i], newBranches[i], instancedBranches, i);
            }
        }

        void Reconcile(Constants constants, Builder.Node newNode, MeshedNode[] siblings, int index)
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
                siblings[index].children = new MeshedNode[4];
                for (int i = 0; i < 4; i++)
                {
                    siblings[index].children[i] = new MeshedNode(siblings[index], constants, instancedMesh, newNode.children[i]);
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

    public class Factory : SQT.Core.ReconcilerFactory
    {
        public SQT.Core.Reconciler FromConstants(SQT.Core.Constants[] constants)
        {
            return new Reconciler(constants);
        }
    }
}
