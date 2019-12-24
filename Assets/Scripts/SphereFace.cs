using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class SphereFace : MonoBehaviour
{
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    public Material material;
    public int resolution = 16;
    public Direction direction = Direction.Up;

    int currentResolution;
    Direction currentDirection;
    Material currentMaterial;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        currentResolution = resolution;
        currentDirection = direction;
        currentMaterial = material;

        UpdateMaterial();
        UpdateMesh();
    }

    void Update()
    {
        if (resolution != currentResolution || direction != currentDirection)
        {
            UpdateMesh();
            currentResolution = resolution;
            currentDirection = direction;
        }
        if (material != currentMaterial)
        {
            UpdateMaterial();
            currentMaterial = material;
        }
    }

    void UpdateMaterial()
    {
        meshRenderer.material = material;
    }

    void UpdateMesh()
    {
        Vector3 up = GetDirection(direction);
        Vector3 front = new Vector3(up.y, up.z, up.x);
        Vector3 right = Vector3.Cross(up, front);

        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[resolution * resolution];
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];

        int triangleIndex = 0;
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int vertexIndex = x + resolution * y;
                Vector2 percent = new Vector2(x, y) / (resolution - 1);
                Vector3 pointOnUnitCube = up + (percent.x - 0.5f) * 2f * front + (percent.y - 0.5f) * 2f * right;
                Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;
                vertices[vertexIndex] = pointOnUnitSphere;

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
        mesh.normals = vertices;
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;
    }

    public enum Direction
    {
        Up, Down, Left, Right, Foward, Back
    }

    static Vector3[] directions = new Vector3[] {
        Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back
    };

    static Vector3 GetDirection(Direction direction)
    {
        return directions[(int)direction];
    }
}
