using UnityEngine;

public class SQTReconciliationSettings
{
    const float desiredScreenSpaceLength = 10f;

    public float desiredLength;

    public static SQTReconciliationSettings GetSettings(SQTConstants constants, Camera camera, Transform root)
    {
        float distanceToSphere = Vector3.Distance(camera.transform.position, root.position) - constants.global.radius;
        if (distanceToSphere < 0f)
        {
            // Inside the sphere, don't do anything.
            return null;
        }

        Vector3 aa = camera.transform.position + camera.transform.forward * distanceToSphere;
        Vector3 a = camera.WorldToScreenPoint(aa);
        Vector3 b = new Vector3(a.x, a.y + desiredScreenSpaceLength, a.z);
        Vector3 bb = camera.ScreenToWorldPoint(b);
        float desiredLength = (aa - bb).magnitude;

        return new SQTReconciliationSettings
        {
            desiredLength = desiredLength
        };
    }
}
