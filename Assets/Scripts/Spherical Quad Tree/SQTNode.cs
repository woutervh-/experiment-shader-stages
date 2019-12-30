using UnityEngine;

public class SQTNode : SQTTaxomy
{
    public Mesh mesh;
    public GameObject gameObject;

    SQTTaxomy parent;
    SQTConstants constants;
    SQTNode[] children;
    int depth;
    Vector2 offset;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    public SQTNode(SQTTaxomy parent, SQTConstants constants, Vector2 offset, int depth)
    {
        this.parent = parent;
        this.constants = constants;
        this.offset = offset;
        this.depth = depth;

        gameObject = new GameObject("Chunk " + depth);
        gameObject.transform.SetParent(constants.branch.gameObject.transform, false);
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        mesh = GenerateMesh();
        meshFilter.mesh = mesh;
        meshRenderer.sharedMaterial = constants.global.material;
    }

    public void Destroy()
    {
        if (children != null)
        {
            foreach (SQTNode child in children)
            {
                child.Destroy();
            }
        }
        Object.Destroy(meshRenderer);
        Object.Destroy(meshFilter);
        Object.Destroy(gameObject);
        Object.Destroy(mesh);
    }

    public Vector3 GetOrigin()
    {
        return constants.branch.up + offset.x * constants.branch.forward + offset.y * constants.branch.right;
    }

    public SQTNode FindNode(GameObject player)
    {
        Vector3 direction = (player.transform.position - gameObject.transform.position).normalized;
        float denominator = Vector3.Dot(constants.branch.up, direction);

        if (denominator <= 0f)
        {
            return null;
        }

        Vector3 pointOnPlane = direction / denominator;
        float tx = (Vector3.Dot(constants.branch.forward, pointOnPlane) - offset.x) * constants.depth[depth].scale;
        float ty = (Vector3.Dot(constants.branch.right, pointOnPlane) - offset.y) * constants.depth[depth].scale;
        if (tx < -1f || 1f < tx || ty < -1f || 1f < ty)
        {
            return null;
        }

        if (children != null)
        {
            foreach (SQTNode child in children)
            {
                SQTNode found = child.FindNode(player);
                if (found != null)
                {
                    return found;
                }
            }
        }

        return this;
    }

    Mesh GenerateMesh()
    {
        Vector3[] vertices = new Vector3[constants.global.resolution * constants.global.resolution];
        Vector3[] normals = new Vector3[constants.global.resolution * constants.global.resolution];
        int[] triangles = new int[(constants.global.resolution - 1) * (constants.global.resolution - 1) * 6];

        Vector3 origin = GetOrigin();
        int triangleIndex = 0;
        for (int y = 0; y < constants.global.resolution; y++)
        {
            for (int x = 0; x < constants.global.resolution; x++)
            {
                int vertexIndex = x + constants.global.resolution * y;
                Vector2 percent = new Vector2(x, y) / (constants.global.resolution - 1);
                Vector3 pointOnUnitCube = origin
                    + Mathf.Lerp(-1f, 1f, percent.x) * constants.depth[depth].scale * constants.branch.forward
                    + Mathf.Lerp(-1f, 1f, percent.y) * constants.depth[depth].scale * constants.branch.right;

                Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;
                vertices[vertexIndex] = pointOnUnitSphere * constants.global.radius;
                normals[vertexIndex] = pointOnUnitSphere;

                if (x != constants.global.resolution - 1 && y != constants.global.resolution - 1)
                {
                    triangles[triangleIndex] = vertexIndex;
                    triangles[triangleIndex + 1] = vertexIndex + constants.global.resolution + 1;
                    triangles[triangleIndex + 2] = vertexIndex + constants.global.resolution;
                    triangles[triangleIndex + 3] = vertexIndex;
                    triangles[triangleIndex + 4] = vertexIndex + 1;
                    triangles[triangleIndex + 5] = vertexIndex + constants.global.resolution + 1;
                    triangleIndex += 6;
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.RecalculateBounds();
        return mesh;
    }
}
