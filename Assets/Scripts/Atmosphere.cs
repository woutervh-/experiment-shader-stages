using UnityEngine;
using UnityEngine.Rendering;

public class Atmosphere : MonoBehaviour
{
    public Material material;
    public float radius = 1f;

    void Start()
    {

    }

    void Update()
    {
        Vector3 p1W = transform.position;
        Vector3 p1V = Camera.main.WorldToViewportPoint(p1W);
        Vector3 p2V = p1V + Vector3.up;
        Vector3 p2W = Camera.main.ViewportToWorldPoint(p2V);
        Vector3 p3W = p1W + (p2W - p1W).normalized * radius;
        Vector3 p3V = Camera.main.WorldToViewportPoint(p3W);

        float d = Vector3.Distance(p1V, p3V);
        Vector3 p00V = new Vector3(p1V.x - d, p1V.y - d, 1f);
        Vector3 p01V = new Vector3(p1V.x + d, p1V.y - d, 1f);
        Vector3 p10V = new Vector3(p1V.x - d, p1V.y + d, 1f);
        Vector3 p11V = new Vector3(p1V.x + d, p1V.y + d, 1f);

        Vector3 p00W = Camera.main.ViewportToWorldPoint(p00V);
        Vector3 p01W = Camera.main.ViewportToWorldPoint(p01V);
        Vector3 p10W = Camera.main.ViewportToWorldPoint(p10V);
        Vector3 p11W = Camera.main.ViewportToWorldPoint(p11V);

        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[] { p00W, p01W, p10W, p11W };
        mesh.triangles = new int[] { 0, 2, 1, 1, 2, 3 };
        Graphics.DrawMesh(mesh, Vector3.zero, Quaternion.identity, material, 0);
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
