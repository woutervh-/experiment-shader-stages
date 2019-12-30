using UnityEngine;

public class SQTRoot : SQTTaxomy
{
    SQTConstants constants;
    SQTNode child;

    public SQTRoot(SQTConstants constants)
    {
        this.constants = constants;
        child = new SQTNode(this, constants, Vector2.zero, 0);
    }

    public SQTNode FindNode(Camera camera)
    {
        return child.FindNode(camera);
    }

    public void Reconciliate(SQTReconciliationSettings reconciliationSettings)
    {
        // Do nothing, let nodes handle reconciliation entirely.
    }

    public void Destroy()
    {
        child.Destroy();
    }
}
