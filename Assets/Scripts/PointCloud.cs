using UnityEngine;

[ExecuteInEditMode]
public class PointCloud : MonoBehaviour
{
    public Material material;

    Bounds bounds;

    void Start()
    {
        bounds = new Bounds(Vector3.zero, Vector3.one);
    }

    void Update()
    {
        Graphics.DrawProcedural(material, bounds, MeshTopology.Points, 8);
    }
}
