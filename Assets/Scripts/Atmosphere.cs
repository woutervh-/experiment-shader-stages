using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.LWRP;

[ExecuteInEditMode]
public class Atmosphere : MonoBehaviour
{
    public Material material;
    public float radius = 1f;

    Mesh mesh;

    void OnEnable()
    {
        mesh = new Mesh();
        mesh.vertices = new Vector3[4];
        mesh.triangles = new int[] { 0, 2, 1, 1, 2, 3 };
    }

    void OnDisable()
    {
        UnityEngine.Object.DestroyImmediate(mesh);
    }

    void LateUpdate()
    {
        // Vector3 min = new Vector3(transform.position.x - radius, transform.position.y - radius, transform.position.z - radius);
        // Vector3 max = new Vector3(transform.position.x + radius, transform.position.y + radius, transform.position.z + radius);
        // Vector3 p000W = new Vector3(min.x, min.y, min.z);
        // Vector3 p001W = new Vector3(max.x, min.y, min.z);
        // Vector3 p010W = new Vector3(min.x, max.y, min.z);
        // Vector3 p011W = new Vector3(max.x, max.y, min.z);
        // Vector3 p100W = new Vector3(min.x, min.y, max.z);
        // Vector3 p101W = new Vector3(max.x, min.y, max.z);
        // Vector3 p110W = new Vector3(min.x, max.y, max.z);
        // Vector3 p111W = new Vector3(max.x, max.y, max.z);
        // Vector3 p000V = Camera.main.WorldToViewportPoint(p000W);
        // Vector3 p001V = Camera.main.WorldToViewportPoint(p001W);
        // Vector3 p010V = Camera.main.WorldToViewportPoint(p010W);
        // Vector3 p011V = Camera.main.WorldToViewportPoint(p011W);
        // Vector3 p100V = Camera.main.WorldToViewportPoint(p100W);
        // Vector3 p101V = Camera.main.WorldToViewportPoint(p101W);
        // Vector3 p110V = Camera.main.WorldToViewportPoint(p110W);
        // Vector3 p111V = Camera.main.WorldToViewportPoint(p111W);
        // float minX = Mathf.Max(0, Mathf.Min(p000V.x, p001V.x, p010V.x, p011V.x, p100V.x, p101V.x, p110V.x, p111V.x));
        // float maxX = Mathf.Min(1, Mathf.Max(p000V.x, p001V.x, p010V.x, p011V.x, p100V.x, p101V.x, p110V.x, p111V.x));
        // float minY = Mathf.Max(0, Mathf.Min(p000V.y, p001V.y, p010V.y, p011V.y, p100V.y, p101V.y, p110V.y, p111V.y));
        // float maxY = Mathf.Min(1, Mathf.Max(p000V.y, p001V.y, p010V.y, p011V.y, p100V.y, p101V.y, p110V.y, p111V.y));
        float minX = 0f;
        float maxX = 1f;
        float minY = 0f;
        float maxY = 1f;
        Vector3 q00V = new Vector3(minX, minY, 1f);
        Vector3 q01V = new Vector3(maxX, minY, 1f);
        Vector3 q10V = new Vector3(minX, maxY, 1f);
        Vector3 q11V = new Vector3(maxX, maxY, 1f);
        // Debug.Log(maxY); when camera is at (0, 0, ±1) there is a visual glitch.
        Vector3 q00W = Camera.main.ViewportToWorldPoint(q00V) - (Camera.main.transform.position + Camera.main.transform.forward);
        Vector3 q01W = Camera.main.ViewportToWorldPoint(q01V) - (Camera.main.transform.position + Camera.main.transform.forward);
        Vector3 q10W = Camera.main.ViewportToWorldPoint(q10V) - (Camera.main.transform.position + Camera.main.transform.forward);
        Vector3 q11W = Camera.main.ViewportToWorldPoint(q11V) - (Camera.main.transform.position + Camera.main.transform.forward);

        mesh.vertices = new Vector3[] { q00W, q01W, q10W, q11W };
        if (material != null)
        {
            material.SetVector("_PlanetPosition", transform.position);
            Graphics.DrawMesh(mesh, Camera.main.transform.position + Camera.main.transform.forward, Quaternion.identity, material, 0);
        }
    }

    void OnDrawGizmos()
    {
        Vector3 p1W = transform.position;
        Vector3 p1V = Camera.main.WorldToViewportPoint(p1W);
        Vector3 p2V = p1V + Vector3.up;
        Vector3 p2W = Camera.main.ViewportToWorldPoint(p2V);
        Vector3 p3W = p1W + (p2W - p1W).normalized * radius;
        Vector3 p3V = Camera.main.WorldToViewportPoint(p3W);

        float d = Vector3.Distance(p1V, p3V);
        Vector3 p00V = new Vector3(p1V.x - d, p1V.y - d, p1V.z);
        Vector3 p01V = new Vector3(p1V.x + d, p1V.y - d, p1V.z);
        Vector3 p10V = new Vector3(p1V.x - d, p1V.y + d, p1V.z);
        Vector3 p11V = new Vector3(p1V.x + d, p1V.y + d, p1V.z);

        Vector3 p00W = Camera.main.ViewportToWorldPoint(p00V);
        Vector3 p01W = Camera.main.ViewportToWorldPoint(p01V);
        Vector3 p10W = Camera.main.ViewportToWorldPoint(p10V);
        Vector3 p11W = Camera.main.ViewportToWorldPoint(p11V);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(p00W, p01W);
        Gizmos.DrawLine(p01W, p11W);
        Gizmos.DrawLine(p11W, p10W);
        Gizmos.DrawLine(p10W, p00W);
    }
}
