using System;
using UnityEngine;

// IDEA: virtual quad tree.
// Every update:
// 1. Start with empty virtual quad tree.
// 2. Ensure required quads are present.
// 3. Compare old virtual quad tree with new one.
// 4. Prune and grow actual (non-virtual) quad tree nodes.

public class SQTNode : SQTTaxomy
{
    public Mesh mesh;
    public GameObject gameObject;
    public int[] path;

    SQTTaxomy parent;
    SQTConstants constants;
    SQTNode[] children;
    Vector2 offset;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    public SQTNode(SQTTaxomy parent, SQTConstants constants, Vector2 offset, int[] path)
    {
        this.parent = parent;
        this.constants = constants;
        this.offset = offset;
        this.path = path;

        gameObject = new GameObject("Chunk " + string.Join("", path));
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
        UnityEngine.Object.Destroy(meshRenderer);
        UnityEngine.Object.Destroy(meshFilter);
        UnityEngine.Object.Destroy(gameObject);
        UnityEngine.Object.Destroy(mesh);
    }

    public Vector3 GetOrigin()
    {
        return constants.branch.up + offset.x * constants.branch.forward + offset.y * constants.branch.right;
    }

    int GetChildIndex(Vector2 pointInPlane)
    {
        Vector2 t = (pointInPlane - offset) / constants.depth[path.Length].scale;
        return (t.x < 0f ? 0 : 1) + (t.y < 0f ? 0 : 2);
    }

    public SQTNode FindNode(Vector2 pointInPlane)
    {
        if (children == null)
        {
            // There are no children, this is as far as the search goes.
            return this;
        }
        int childIndex = GetChildIndex(pointInPlane);
        return children[childIndex].FindNode(pointInPlane);
    }

    bool ShouldSplit(SQTReconciliationData reconciliationData)
    {
        return path.Length < constants.global.maxDepth
            && constants.depth[path.Length].approximateSize > reconciliationData.desiredLength;
    }

    void Split()
    {
        if (children == null)
        {
            meshRenderer.enabled = false;
            children = new SQTNode[4];
            for (int i = 0; i < 4; i++)
            {
                int[] childPath = new int[path.Length + 1];
                Array.Copy(path, childPath, path.Length);
                childPath[path.Length] = i;
                children[i] = new SQTNode(this, constants, offset + constants.depth[path.Length + 1].scale * childOffsetVectors[i], childPath);
            }
        }
    }

    void Merge()
    {
        if (children != null)
        {
            foreach (SQTNode child in children)
            {
                child.Destroy();
            }
            meshRenderer.enabled = true;
            children = null;
        }
    }

    public SQTNode DeepSplit(SQTReconciliationData reconciliationData)
    {
        if (ShouldSplit(reconciliationData))
        {
            Split();
            int childIndex = GetChildIndex(reconciliationData.pointInPlane);
            return children[childIndex].DeepSplit(reconciliationData);
        }
        else
        {
            return this;
        }
    }

    public void Reconciliate(int[] targetPath)
    {
        if (path.Length >= targetPath.Length)
        {
            Merge();
        }
        else
        {
            if (children != null)
            {
                foreach (SQTNode child in children)
                {
                    child.Reconciliate(path);
                }
            }

            // TODO:
            // parent.EnsureMaximumDepth(...?);
            // parent.GetNeighbor(0).EnsureMaximumDepth(...?)
            // parent.GetNeighbor(1).EnsureMaximumDepth(...?)
            // parent.GetNeighbor(2).EnsureMaximumDepth(...?)
            // parent.GetNeighbor(3).EnsureMaximumDepth(...?)
        }
    }

    public void EnsureMaximumDepth(int depth)
    {
        if (depth == 0)
        {
            Merge();
        }
        else
        {
            if (children != null)
            {
                foreach (SQTNode child in children)
                {
                    child.EnsureMaximumDepth(depth - 1);
                }
            }
        }
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
                    + Mathf.Lerp(-1f, 1f, percent.x) * constants.depth[path.Length].scale * constants.branch.forward
                    + Mathf.Lerp(-1f, 1f, percent.y) * constants.depth[path.Length].scale * constants.branch.right;

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

    static int[] GetNeighborPath(int[] path, int direction)
    {
        // direction: west=0, east=1, south=2, north=3

        int[] neighborPath = new int[path.Length];
        Array.Copy(path, neighborPath, path.Length);
        for (int i = neighborPath.Length - 1; i >= 0; i--)
        {
            bool carry;
            if (direction == 0 || direction == 1)
            {
                carry = ((path[i] ^ (direction << 0)) & 1) == 0;
                neighborPath[i] = path[i] ^ 1;
            }
            else
            {
                carry = ((path[i] ^ (direction << 1)) & 2) == 0;
                neighborPath[i] = path[i] ^ 2;
            }
            if (!carry)
            {
                break;
            }
        }
        return neighborPath;
    }

    static Vector2[] childOffsetVectors = {
        new Vector2(-1f, -1f),
        new Vector2(1f, -1f),
        new Vector2(-1f, 1f),
        new Vector2(1f, 1f),
     };

    // static int[][] childVisitOrder = {
    //     new int[] { 0, 1, 2, 3 },
    //     new int[] { 1, 0, 3, 2 },
    //     new int[] { 2, 0, 3, 1 },
    //     new int[] { 3, 1, 2, 0 }
    //  };
}
