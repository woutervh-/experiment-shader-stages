using UnityEngine;

namespace SQT.Core.GPU
{
    public class SQTInstancedMesh
    {
        Mesh[,] meshInstances;

        public SQTInstancedMesh(SQTConstants.SQTGlobal global, SQTConstants.SQTMesh[] meshes)
        {
            meshInstances = new Mesh[global.maxDepth + 1, meshes.Length];

            Vector3 up = Vector3.up;
            Vector3 forward = SQTConstants.SQTBranch.GetForward(up);
            Vector3 right = SQTConstants.SQTBranch.GetRight(up);

            for (int i = 0; i <= global.maxDepth; i++)
            {
                Vector3[] vertices = new Vector3[global.resolution * global.resolution];
                Vector3[] normals = new Vector3[global.resolution * global.resolution];
                GenerateVertices(global, vertices, normals, i, up, forward, right);
                for (int j = 0; j < meshes.Length; j++)
                {
                    Mesh mesh = new Mesh();
                    mesh.vertices = vertices;
                    mesh.normals = normals;
                    mesh.triangles = meshes[j].triangles;
                    mesh.RecalculateBounds();
                    meshInstances[i, j] = mesh;
                }
            }
        }

        public Mesh GetMesh(int depth, int neighborMask)
        {
            return meshInstances[depth, neighborMask];
        }

        public void Destroy()
        {
            for (int i = 0; i < meshInstances.GetLength(0); i++)
            {
                for (int j = 0; j < meshInstances.GetLength(1); j++)
                {
                    UnityEngine.Object.Destroy(meshInstances[i, j]);
                }
            }
        }

        public static void GenerateVertices(SQTConstants.SQTGlobal global, Vector3[] vertices, Vector3[] normals, int depth, Vector3 up, Vector3 forward, Vector3 right)
        {
            for (int y = 0; y < global.resolution; y++)
            {
                for (int x = 0; x < global.resolution; x++)
                {
                    int vertexIndex = x + global.resolution * y;
                    Vector2 percent = new Vector2(x, y) / (global.resolution - 1);
                    Vector3 pointOnUnitCube = Vector3.up
                        + Mathf.Lerp(-1f, 1f, percent.x) * SQTConstants.SQTDepth.GetScale(depth) * forward
                        + Mathf.Lerp(-1f, 1f, percent.y) * SQTConstants.SQTDepth.GetScale(depth) * right;

                    Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;
                    vertices[vertexIndex] = pointOnUnitSphere;
                    normals[vertexIndex] = pointOnUnitSphere;
                }
            }
        }
    }
}
