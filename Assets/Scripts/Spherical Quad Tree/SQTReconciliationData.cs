using UnityEngine;

public class SQTReconciliationData
{
    const float desiredScreenSpaceLength = 10f;

    public SQTConstants constants;
    public float desiredLength;
    public Vector2 pointInPlane;

    public static SQTReconciliationData GetData(SQTConstants constants, Camera camera)
    {
        Vector3 sphereToCamera = constants.global.gameObject.transform.InverseTransformPoint(camera.transform.position);

        Vector3 direction;
        float denominator;
        if (sphereToCamera.sqrMagnitude == 0f)
        {
            // Camera is at the center of the sphere.
            direction = constants.branch.up;
            denominator = 1f;
        }
        else
        {
            direction = sphereToCamera.normalized;
            denominator = Vector3.Dot(constants.branch.up, direction);

            if (denominator <= 0f)
            {
                // Camera is in opposite hemisphere.
                return null;
            }
        }

        float distanceToSphere = Mathf.Abs(Mathf.Sqrt(Vector3.Dot(sphereToCamera, sphereToCamera)) - constants.global.radius);
        Vector3 aa = camera.transform.position + camera.transform.forward * distanceToSphere;
        Vector3 a = camera.WorldToScreenPoint(aa);
        Vector3 b = new Vector3(a.x, a.y + desiredScreenSpaceLength, a.z);
        Vector3 bb = camera.ScreenToWorldPoint(b);
        float desiredLength = (aa - bb).magnitude;

        Vector3 pointOnPlane = direction / denominator;
        Vector2 pointInPlane = new Vector2(Vector3.Dot(constants.branch.forward, pointOnPlane), Vector3.Dot(constants.branch.right, pointOnPlane));

        if (pointInPlane.x < -1f || 1f < pointInPlane.x || pointInPlane.y < -1f || 1f < pointInPlane.y)
        {
            return null;
        }

        return new SQTReconciliationData
        {
            constants = constants,
            desiredLength = desiredLength,
            pointInPlane = pointInPlane
        };
    }
}
