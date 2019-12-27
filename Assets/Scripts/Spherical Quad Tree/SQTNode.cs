using System.Threading.Tasks;
using UnityEngine;

public class SQTNode : SQTTaxomy
{
    SQTTaxomy parent;
    SQTConstants constants;
    SQTNode[] children;
    Mesh mesh;

    public SQTNode(SQTTaxomy parent, SQTConstants constants)
    {
        this.parent = parent;
        this.constants = constants;
    }

    public void Destroy()
    {
        // TODO: clean up
    }

    public int GetResolution()
    {
        return parent.GetResolution() + 1;
    }

    public Task<Mesh> GenerateMesh()
    {
        return new Task<Mesh>(() =>
        {
            Mesh mesh = new Mesh();
            int resolution = GetResolution();
            Vector3[] vertices = new Vector3[resolution * resolution];
            Vector3[] normals = new Vector3[resolution * resolution];
            int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];

            int triangleIndex = 0;
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    int vertexIndex = x + resolution * y;
                    Vector2 percent = new Vector2(x, y) / (resolution - 1);
                    Vector3 pointOnUnitCube = constants.branch.up
                        + Mathf.Lerp(-1f, 1f, percent.x) * constants.branch.forward
                        + Mathf.Lerp(-1f, 1f, percent.y) * constants.branch.right;

                    Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;
                    vertices[vertexIndex] = pointOnUnitSphere * constants.global.radius;
                    normals[vertexIndex] = pointOnUnitSphere;

                    if (x != resolution - 1 && y != resolution - 1)
                    {
                        triangles[triangleIndex] = vertexIndex;
                        triangles[triangleIndex + 1] = vertexIndex + resolution + 1;
                        triangles[triangleIndex + 2] = vertexIndex + resolution;
                        triangles[triangleIndex + 3] = vertexIndex;
                        triangles[triangleIndex + 4] = vertexIndex + 1;
                        triangles[triangleIndex + 5] = vertexIndex + resolution + 1;
                        triangleIndex += 6;
                    }
                }
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.normals = normals;
            mesh.RecalculateBounds();

            return mesh;
        });
    }
}
