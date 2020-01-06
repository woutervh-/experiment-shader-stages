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
            if (newBranches[i].children != null && meshedBranches[i].children != null)
            {
                for (int j = 0; j < 4; j++)
                {
                    ReconcileNode(constants[i], newBranches[i].children[j], meshedBranches[i].children[j]);
                }
            }
            else if (newBranches[i].children != null && meshedBranches[i].children == null)
            {
                meshedBranches[i].meshRenderer.enabled = false;
                meshedBranches[i].children = new MeshedNode[4];
                for (int j = 0; j < 4; j++)
                {
                    meshedBranches[i].children[j] = new MeshedNode(meshedBranches[i], constants[i], newBranches[i].children[j]);
                    ReconcileNode(constants[i], newBranches[i].children[j], meshedBranches[i].children[j]);
                }
            }
            else if (newBranches[i].children == null && meshedBranches[i].children != null)
            {
                for (int j = 0; j < 4; j++)
                {
                    meshedBranches[i].children[j].Destroy();
                }
                meshedBranches[i].children = null;
                meshedBranches[i].meshRenderer.enabled = true;
            }
        }
    }

    void ReconcileNode(SQTConstants constants, SQTBuilder.Node newNode, MeshedNode meshedNode)
    {
        if (newNode.children != null && meshedNode.children != null)
        {
            for (int i = 0; i < 4; i++)
            {
                ReconcileNode(constants, newNode.children[i], meshedNode.children[i]);
            }
        }
        else if (newNode.children != null && meshedNode.children == null)
        {
            meshedNode.meshRenderer.enabled = false;
            meshedNode.children = new MeshedNode[4];
            for (int i = 0; i < 4; i++)
            {
                meshedNode.children[i] = new MeshedNode(meshedNode, constants, newNode.children[i]);
                ReconcileNode(constants, newNode.children[i], meshedNode.children[i]);
            }
        }
        else if (newNode.children == null && meshedNode.children != null)
        {
            for (int i = 0; i < 4; i++)
            {
                meshedNode.children[i].Destroy();
            }
            meshedNode.children = null;
            meshedNode.meshRenderer.enabled = true;
        }
    }
}
