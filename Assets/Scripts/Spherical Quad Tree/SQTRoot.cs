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
        Vector3 direction = (camera.transform.position - constants.branch.gameObject.transform.position).normalized;
        float denominator = Vector3.Dot(constants.branch.up, direction);

        if (denominator <= 0f)
        {
            return null;
        }

        Vector3 pointOnPlane = direction / denominator;
        Vector2 pointInPlane = new Vector2(Vector3.Dot(constants.branch.forward, pointOnPlane), Vector3.Dot(constants.branch.right, pointOnPlane));

        return child.FindNode(pointInPlane);
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
