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

    public SQTNode FindNode(SQTReconciliationSettings reconciliationSettings)
    {
        if (reconciliationSettings == null)
        {
            return null;
        }
        return child.FindNode(reconciliationSettings);
    }

    public void Reconciliate(SQTReconciliationSettings reconciliationSettings)
    {
        // Do nothing.
    }

    public void Destroy()
    {
        child.Destroy();
    }
}
