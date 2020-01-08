public class SQTReconciler
{
    SQTConstants[] constants;
    MeshedNode[] meshedBranches;

    public SQTReconciler(SQTConstants[] constants)
    {
        this.constants = constants;
        meshedBranches = new MeshedNode[constants.Length];
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

    public void Reconcile(SQTBuilder.Node[] newBranches)
    {
        for (int i = 0; i < meshedBranches.Length; i++)
        {
            if (meshedBranches[i] == null)
            {
                meshedBranches[i] = new MeshedNode(null, constants[i], newBranches[i]);
            }
            Reconcile(constants[i], newBranches[i], meshedBranches, i);
        }
    }

    void Reconcile(SQTConstants constants, SQTBuilder.Node newNode, MeshedNode[] siblings, int index)
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
                siblings[index].children[i] = new MeshedNode(siblings[index], constants, newNode.children[i]);
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
