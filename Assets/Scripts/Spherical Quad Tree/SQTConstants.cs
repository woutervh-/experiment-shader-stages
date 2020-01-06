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

    public class SQTMeshes
    {
        public int[] triangles;

        public static SQTMeshes GetFromGlobal(SQTGlobal global)
        {
            int[] triangles = new int[(global.resolution - 1) * (global.resolution - 1) * 6];
            int triangleIndex = 0;
            for (int y = 0; y < global.resolution; y++)
            {
                for (int x = 0; x < global.resolution; x++)
                {
                    int vertexIndex = x + global.resolution * y;
                    if (x != global.resolution - 1 && y != global.resolution - 1)
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
            return new SQTMeshes
            {
                triangles = triangles
            };
        }
    }

    public SQTGlobal global;
    public SQTBranch branch;
    public SQTDepth[] depth;
    public SQTMeshes meshes;
}
