using UnityEngine;

public class SQTRoot : SQTTaxomy
{
    static Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

    SQTConstants.SQTGlobal global;
    SQTConstants[] constants;
    SQTNode[] branches;

    public SQTRoot(SQTConstants.SQTGlobal global, SQTConstants.SQTDepth[] depth)
    {
        this.global = global;
        branches = new SQTNode[directions.Length];
        constants = new SQTConstants[directions.Length];
        for (int i = 0; i < directions.Length; i++)
        {
            GameObject branchGameObject = new GameObject("SQT (" + i + ")");
            branchGameObject.transform.SetParent(global.gameObject.transform, false);
            SQTConstants.SQTBranch branch = new SQTConstants.SQTBranch(directions[i])
            {
                gameObject = branchGameObject
            };
            constants[i] = new SQTConstants
            {
                global = global,
                branch = branch,
                depth = depth
            };
            branches[i] = new SQTNode(this, constants[i], Vector2.zero, 0);
        }
    }

    public SQTNode FindNode(Camera camera)
    {
        Vector3 sphereToCameraVector = camera.transform.position - global.gameObject.transform.position;

        if (sphereToCameraVector.sqrMagnitude == 0f)
        {
            return null;
        }

        Vector3 sphereToCameraDirection = sphereToCameraVector.normalized;

        for (int i = 0; i < directions.Length; i++)
        {
            float denominator = Vector3.Dot(constants[i].branch.up, sphereToCameraDirection);
            if (denominator <= 0f)
            {
                // Camera is in opposite hemisphere.
                continue;
            }

            Vector3 pointOnPlane = sphereToCameraDirection / denominator;
            Vector2 pointInPlane = new Vector2(Vector3.Dot(constants[i].branch.forward, pointOnPlane), Vector3.Dot(constants[i].branch.right, pointOnPlane));

            if (pointInPlane.x < -1f || 1f < pointInPlane.x || pointInPlane.y < -1f || 1f < pointInPlane.y)
            {
                // Point is outside branch quad.
                continue;
            }

            return branches[i].FindNode(pointInPlane);
        }

        return null;
    }

    // public void Reconciliate(SQTReconciliationData reconciliationData)
    // {
    //     if (reconciliationData == null)
    //     {
    //         return;
    //     }
    //     child.Reconciliate(reconciliationData);
    // }

    public void Destroy()
    {
        foreach (SQTNode branch in branches)
        {
            branch.Destroy();
        }
    }
}
