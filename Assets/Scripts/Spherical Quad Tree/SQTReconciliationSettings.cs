using UnityEngine;

public class SQTReconciliationSettings
{
    const float desiredScreenSpaceLength = 10f;

    public float desiredLength;
    public Vector2 pointInPlane;

    public static SQTReconciliationSettings GetSettings(SQTConstants constants, Camera camera, Transform root)
    {
        Vector3 sphereToCamera = camera.transform.position - root.position;
        float distanceToSphere = Mathf.Sqrt(Vector3.Dot(sphereToCamera, sphereToCamera)) - constants.global.radius;
        Vector3 direction = sphereToCamera.normalized;
        float denominator = Vector3.Dot(constants.branch.up, direction);

        if (distanceToSphere < 0f || denominator <= 0f)
        {
            // Inside the sphere or on the wrong side of the plane, don't do anything.
            return null;
        }

        Vector3 aa = camera.transform.position + camera.transform.forward * distanceToSphere;
        Vector3 a = camera.WorldToScreenPoint(aa);
        Vector3 b = new Vector3(a.x, a.y + desiredScreenSpaceLength, a.z);
        Vector3 bb = camera.ScreenToWorldPoint(b);
        float desiredLength = (aa - bb).magnitude;

        Vector3 pointOnPlane = direction / denominator;
        Vector2 pointInPlane = new Vector2(Vector3.Dot(constants.branch.forward, pointOnPlane), Vector3.Dot(constants.branch.right, pointOnPlane));

        return new SQTReconciliationSettings
        {
            desiredLength = desiredLength,
            pointInPlane = pointInPlane
        };
    }
}


// Vector3 direction = sphereToCamera.normalized;
// float denominator = Vector3.Dot(constants.branch.up, direction);

// if (denominator <= 0f)
// {
//     return null;
// }

// Vector3 pointOnPlane = direction / denominator;
// Vector2 pointInPlane = new Vector2(Vector3.Dot(constants.branch.forward, pointOnPlane), Vector3.Dot(constants.branch.right, pointOnPlane));

// return child.FindNode(pointInPlane);
