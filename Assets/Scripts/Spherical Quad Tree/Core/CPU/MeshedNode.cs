using UnityEngine;

namespace SQT.Core.CPU
{
    public class MeshedNode
    {
        public MeshedNode[] children;
        public MeshRenderer meshRenderer;
        public int neighborMask;
        public Mesh mesh;

        MeshedNode parent;
        Constants constants;
        Builder.Node node;
        GameObject gameObject;
        MeshFilter meshFilter;

        public MeshedNode(MeshedNode parent, Constants constants, Builder.Node node)
        {
            this.parent = parent;
            this.constants = constants;
            this.node = node;

            neighborMask = -1;
            gameObject = new GameObject("Chunk " + string.Join("", node.path));
            gameObject.transform.SetParent(constants.branch.gameObject.transform, false);
            meshFilter = gameObject.AddComponent<MeshFilter>();
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.enabled = false;
            // mesh = GenerateMesh();
            // meshFilter.sharedMesh = mesh;
            meshRenderer.sharedMaterial = constants.global.material;

            RequestMesh();
        }

        void NotifyChildEvent()
        {
            if (meshRenderer == null)
            {
                return;
            }
            bool enable = false;
            if (children == null)
            {
                enable = true;
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    if (children[i].mesh == null)
                    {
                        enable = true;
                        break;
                    }
                }
            }
            meshRenderer.enabled = enable;
            // TODO: on enable, make sure children are disabled
        }

        async void RequestMesh()
        {
            Vector3[] vertices = new Vector3[constants.global.resolution * constants.global.resolution];
            Vector3[] normals = new Vector3[constants.global.resolution * constants.global.resolution];

            Vector3 origin = GetOrigin();
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
                    vertices[vertexIndex] = pointOnUnitSphere;
                    normals[vertexIndex] = pointOnUnitSphere;
                }
            }

            await constants.global.plugins.ModifyVertices(constants, vertices, normals);

            if (meshFilter != null)
            {
                mesh = new Mesh();
                mesh.vertices = vertices;
                mesh.normals = normals;
                SetMeshTriangles(neighborMask);
                meshFilter.sharedMesh = mesh;
            }

            if (parent != null)
            {
                parent.NotifyChildEvent();
            }
        }

        public void Destroy()
        {
            if (children != null)
            {
                foreach (MeshedNode child in children)
                {
                    child.Destroy();
                }
                children = null;
            }
            UnityEngine.Object.Destroy(meshRenderer);
            UnityEngine.Object.Destroy(meshFilter);
            UnityEngine.Object.Destroy(gameObject);
            UnityEngine.Object.Destroy(mesh);
            meshRenderer = null;
            meshFilter = null;
            gameObject = null;
            mesh = null;

            if (parent != null)
            {
                parent.NotifyChildEvent();
            }
        }

        public void SetMeshTriangles(int neighborMask)
        {
            if (mesh != null && neighborMask != -1)
            {
                mesh.triangles = constants.meshes[neighborMask].triangles;
                mesh.RecalculateBounds();
                constants.global.plugins.ModifyMesh(constants, mesh, node);
            }
        }

        Vector3 GetOrigin()
        {
            return constants.branch.up + node.offset.x * constants.branch.forward + node.offset.y * constants.branch.right;
        }

        // Mesh GenerateMesh()
        // {
        //     Vector3[] vertices = new Vector3[constants.global.resolution * constants.global.resolution];
        //     Vector3[] normals = new Vector3[constants.global.resolution * constants.global.resolution];

        //     Vector3 origin = GetOrigin();
        //     for (int y = 0; y < constants.global.resolution; y++)
        //     {
        //         for (int x = 0; x < constants.global.resolution; x++)
        //         {
        //             int vertexIndex = x + constants.global.resolution * y;
        //             Vector2 percent = new Vector2(x, y) / (constants.global.resolution - 1);
        //             Vector3 pointOnUnitCube = origin
        //                 + Mathf.Lerp(-1f, 1f, percent.x) * constants.depth[node.path.Length].scale * constants.branch.forward
        //                 + Mathf.Lerp(-1f, 1f, percent.y) * constants.depth[node.path.Length].scale * constants.branch.right;

        //             Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;
        //             vertices[vertexIndex] = pointOnUnitSphere;
        //             normals[vertexIndex] = pointOnUnitSphere;
        //         }
        //     }

        //     constants.global.plugins.ModifyVertices(constants, vertices, normals);

        //     Mesh mesh = new Mesh();
        //     mesh.vertices = vertices;
        //     mesh.normals = normals;
        //     return mesh;
        // }
    }
}
