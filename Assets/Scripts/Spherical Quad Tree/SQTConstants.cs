using UnityEngine;

public class SQTConstants
{
    public class SQTDepth
    {
        public float scale;
        public float approximateSize;

        public static SQTDepth[] GetFromGlobal(SQTGlobal global)
        {
            SQTDepth[] depth = new SQTDepth[global.maxDepth + 1];
            for (int i = 0; i <= global.maxDepth; i++)
            {
                float scale = 1f / Mathf.Pow(2f, i);
                depth[i] = new SQTDepth
                {
                    scale = scale,
                    approximateSize = scale / global.resolution
                };
            }
            return depth;
        }
    }

    public class SQTGlobal
    {
        public int maxDepth;
        public int resolution;
        public float radius;
        public Material material;
        public GameObject gameObject;
    }

    public class SQTBranch
    {
        public int index;
        public Vector3 up;
        public Vector3 forward;
        public Vector3 right;
        public GameObject gameObject;

        public SQTBranch(int index, Vector3 up)
        {
            this.index = index;
            this.up = up;
            forward = new Vector3(up.y, up.z, up.x);
            right = Vector3.Cross(up, forward);
        }
    }

    public class SQTMesh
    {
        public int[] triangles;

        public static SQTMesh[] GetFromGlobal(SQTGlobal global)
        {
            SQTMesh[] meshes = new SQTMesh[17];
            for (int i = 0; i < 17; i++)
            {
                meshes[i] = GetFromGlobal(global, i);
            }
            return meshes;
        }

        public static SQTMesh GetFromGlobal(SQTGlobal global, int neighborMask)
        {
            int innerTrianglesCount = (global.resolution - 3) * (global.resolution - 3) * 2;
            int outerTrianglesCount = (global.resolution / 2 + 2 * (global.resolution / 2 - 1)) * 4;
            int[] triangles = new int[(innerTrianglesCount + outerTrianglesCount) * 3];
            int triangleIndex = 0;

            // Inner triangles.
            for (int y = 0; y < global.resolution; y++)
            {
                for (int x = 0; x < global.resolution; x++)
                {
                    int vertexIndex = x + global.resolution * y;
                    if (1 <= x && x < global.resolution - 2 && 1 <= y && y < global.resolution - 2)
                    {
                        triangles[triangleIndex] = vertexIndex;
                        triangles[triangleIndex + 1] = vertexIndex + global.resolution + 1;
                        triangles[triangleIndex + 2] = vertexIndex + global.resolution;
                        triangles[triangleIndex + 3] = vertexIndex;
                        triangles[triangleIndex + 4] = vertexIndex + 1;
                        triangles[triangleIndex + 5] = vertexIndex + global.resolution + 1;
                        triangleIndex += 6;
                    }
                }
            }

            // South.
            for (int x = 0; x < global.resolution - 2; x++)
            {
                int vertexIndex = x;
                if (x % 2 == 0)
                {
                    triangles[triangleIndex] = vertexIndex;
                    triangles[triangleIndex + 1] = vertexIndex + 2;
                    triangles[triangleIndex + 2] = vertexIndex + global.resolution + 1;
                    triangleIndex += 3;
                }
                else
                {
                    triangles[triangleIndex] = vertexIndex + global.resolution;
                    triangles[triangleIndex + 1] = vertexIndex + 1;
                    triangles[triangleIndex + 2] = vertexIndex + global.resolution + 1;
                    triangles[triangleIndex + 3] = vertexIndex + 1;
                    triangles[triangleIndex + 4] = vertexIndex + global.resolution + 2;
                    triangles[triangleIndex + 5] = vertexIndex + global.resolution + 1;
                    triangleIndex += 6;
                }
            }

            // East.
            for (int y = 0; y < global.resolution - 2; y++)
            {
                int vertexIndex = y * global.resolution + global.resolution - 2;
                if (y % 2 == 0)
                {
                    triangles[triangleIndex] = vertexIndex + 1;
                    triangles[triangleIndex + 1] = vertexIndex + global.resolution + global.resolution + 1;
                    triangles[triangleIndex + 2] = vertexIndex + global.resolution;
                    triangleIndex += 3;
                }
                else
                {
                    triangles[triangleIndex] = vertexIndex;
                    triangles[triangleIndex + 1] = vertexIndex + global.resolution + 1;
                    triangles[triangleIndex + 2] = vertexIndex + global.resolution;
                    triangles[triangleIndex + 3] = vertexIndex + global.resolution;
                    triangles[triangleIndex + 4] = vertexIndex + global.resolution + 1;
                    triangles[triangleIndex + 5] = vertexIndex + global.resolution + global.resolution;
                    triangleIndex += 6;
                }
            }

            // North.
            for (int x = 0; x < global.resolution - 2; x++)
            {
                int vertexIndex = x + global.resolution * (global.resolution - 2);
                if (x % 2 == 0)
                {
                    triangles[triangleIndex] = vertexIndex + global.resolution;
                    triangles[triangleIndex + 1] = vertexIndex + 1;
                    triangles[triangleIndex + 2] = vertexIndex + global.resolution + 2;
                    triangleIndex += 3;
                }
                else
                {
                    triangles[triangleIndex] = vertexIndex;
                    triangles[triangleIndex + 1] = vertexIndex + 1;
                    triangles[triangleIndex + 2] = vertexIndex + global.resolution + 1;
                    triangles[triangleIndex + 3] = vertexIndex + 1;
                    triangles[triangleIndex + 4] = vertexIndex + 2;
                    triangles[triangleIndex + 5] = vertexIndex + global.resolution + 1;
                    triangleIndex += 6;
                }
            }

            // West.
            for (int y = 0; y < global.resolution - 2; y++)
            {
                int vertexIndex = y * global.resolution;
                if (y % 2 == 0)
                {
                    triangles[triangleIndex] = vertexIndex;
                    triangles[triangleIndex + 1] = vertexIndex + global.resolution + 1;
                    triangles[triangleIndex + 2] = vertexIndex + global.resolution + global.resolution;
                    triangleIndex += 3;
                }
                else
                {
                    triangles[triangleIndex] = vertexIndex + 1;
                    triangles[triangleIndex + 1] = vertexIndex + global.resolution + 1;
                    triangles[triangleIndex + 2] = vertexIndex + global.resolution;
                    triangles[triangleIndex + 3] = vertexIndex + global.resolution;
                    triangles[triangleIndex + 4] = vertexIndex + global.resolution + 1;
                    triangles[triangleIndex + 5] = vertexIndex + global.resolution + global.resolution + 1;
                    triangleIndex += 6;
                }
            }

            return new SQTMesh
            {
                triangles = triangles
            };
        }
    }

    public SQTGlobal global;
    public SQTBranch branch;
    public SQTDepth[] depth;
    public SQTMesh[] meshes;
}
