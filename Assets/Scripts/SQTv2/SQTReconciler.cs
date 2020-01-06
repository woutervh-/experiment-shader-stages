using UnityEngine;

public class SQTReconciler
{
    SQTConstants[] constants;
    MeshedNode[] meshedBranches;

    public SQTReconciler(SQTConstants[] constants)
    {
        this.constants = constants;
        meshedBranches = new MeshedNode[constants.Length];
    }

    public void Destroy()
    {
        for (int i = 0; i < meshedBranches.Length; i++)
        {
            if (meshedBranches[i] != null)
            {
                meshedBranches[i].Destroy();
            }
        }
    }

    public void Reconcile(SQTBuilder.Node[] newBranches)
    {
        for (int i = 0; i < meshedBranches.Length; i++)
        {
            if (meshedBranches[i] == null)
            {
                meshedBranches[i] = new MeshedNode(null, constants[i], newBranches[i]);
            }
            if (newBranches[i].children != null && meshedBranches[i].children != null)
            {
                for (int j = 0; j < 4; j++)
                {
                    ReconcileNode(constants[i], newBranches[i].children[j], meshedBranches[i].children[j]);
                }
            }
            else if (newBranches[i].children != null && meshedBranches[i].children == null)
            {
                meshedBranches[i].meshRenderer.enabled = false;
                meshedBranches[i].children = new MeshedNode[4];
                for (int j = 0; j < 4; j++)
                {
                    meshedBranches[i].children[j] = new MeshedNode(meshedBranches[i], constants[i], newBranches[i].children[j]);
                    ReconcileNode(constants[i], newBranches[i].children[j], meshedBranches[i].children[j]);
                }
            }
            else if (newBranches[i].children == null && meshedBranches[i].children != null)
            {
                for (int j = 0; j < 4; j++)
                {
                    meshedBranches[i].children[j].Destroy();
                }
                meshedBranches[i].children = null;
                meshedBranches[i].meshRenderer.enabled = true;
            }
        }
    }

    void ReconcileNode(SQTConstants constants, SQTBuilder.Node newNode, MeshedNode meshedNode)
    {
        if (newNode.children != null && meshedNode.children != null)
        {
            for (int i = 0; i < 4; i++)
            {
                ReconcileNode(constants, newNode.children[i], meshedNode.children[i]);
            }
        }
        else if (newNode.children != null && meshedNode.children == null)
        {
            meshedNode.meshRenderer.enabled = false;
            meshedNode.children = new MeshedNode[4];
            for (int i = 0; i < 4; i++)
            {
                meshedNode.children[i] = new MeshedNode(meshedNode, constants, newNode.children[i]);
                ReconcileNode(constants, newNode.children[i], meshedNode.children[i]);
            }
        }
        else if (newNode.children == null && meshedNode.children != null)
        {
            for (int i = 0; i < 4; i++)
            {
                meshedNode.children[i].Destroy();
            }
            meshedNode.children = null;
            meshedNode.meshRenderer.enabled = true;
        }
    }

    public class MeshedNode
    {
        public MeshedNode[] children;
        public MeshRenderer meshRenderer;

        MeshedNode parent;
        SQTConstants constants;
        SQTBuilder.Node node;
        Mesh mesh;
        GameObject gameObject;
        MeshFilter meshFilter;

        public MeshedNode(MeshedNode parent, SQTConstants constants, SQTBuilder.Node node)
        {
            this.parent = parent;
            this.constants = constants;
            this.node = node;

            gameObject = new GameObject("Chunk " + string.Join("", node.path));
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
                foreach (MeshedNode child in children)
                {
                    child.Destroy();
                }
            }
            UnityEngine.Object.Destroy(meshRenderer);
            UnityEngine.Object.Destroy(meshFilter);
            UnityEngine.Object.Destroy(gameObject);
            UnityEngine.Object.Destroy(mesh);
        }

        Vector3 GetOrigin()
        {
            return constants.branch.up + node.offset.x * constants.branch.forward + node.offset.y * constants.branch.right;
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
                        + Mathf.Lerp(-1f, 1f, percent.x) * constants.depth[node.path.Length].scale * constants.branch.forward
                        + Mathf.Lerp(-1f, 1f, percent.y) * constants.depth[node.path.Length].scale * constants.branch.right;

                    Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;
                    // vertices[vertexIndex] = pointOnUnitSphere * constants.global.radius;
                    // normals[vertexIndex] = pointOnUnitSphere;
                    vertices[vertexIndex] = pointOnUnitCube;
                    normals[vertexIndex] = constants.branch.up;

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
}
