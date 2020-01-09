using UnityEngine;

public class SQTReconciliationData
{
    public SQTConstants constants;
    public float desiredLength;
    public Vector2 pointInPlane;

    public static SQTReconciliationData GetData(SQTConstants.SQTGlobal global, SQTConstants[] constants, Camera camera)
    {
        Vector3 sphereToCamera = global.gameObject.transform.InverseTransformPoint(camera.transform.position);
        float distanceToSphere = Mathf.Abs(Mathf.Sqrt(Vector3.Dot(sphereToCamera, sphereToCamera)) - 1f);
        Vector3 aa = camera.transform.position + camera.transform.forward * distanceToSphere;
        Vector3 a = camera.WorldToScreenPoint(aa);
        Vector3 b = new Vector3(a.x, a.y + global.desiredScreenSpaceLength, a.z);
        Vector3 bb = camera.ScreenToWorldPoint(b);
        float desiredLength = (aa - bb).magnitude;

        for (int i = 0; i < constants.Length; i++)
        {
            Vector2? pointInPlane = GetPointInPlane(constants[i], camera, sphereToCamera);
            if (pointInPlane != null)
            {
                return new SQTReconciliationData
                {
                    constants = constants[i],
                    desiredLength = desiredLength,
                    pointInPlane = pointInPlane.Value
                };
            }
        }

        return null;
    }

    static Vector2? GetPointInPlane(SQTConstants constants, Camera camera, Vector3 sphereToCamera)
    {
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

        Vector3 pointOnPlane = direction / denominator;
        Vector2 pointInPlane = new Vector2(Vector3.Dot(constants.branch.forward, pointOnPlane), Vector3.Dot(constants.branch.right, pointOnPlane));

        if (pointInPlane.x < -1f || 1f < pointInPlane.x || pointInPlane.y < -1f || 1f < pointInPlane.y)
        {
            return null;
        }

        return pointInPlane;
    }
}
