using UnityEngine;

namespace SQT.Core
{
    public class Constants
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
                    float scale = GetScale(i);
                    float approximateSize = scale / global.resolution;

                    depth[i] = new SQTDepth
                    {
                        scale = scale,
                        approximateSize = approximateSize
                    };
                }
                return depth;
            }

            public static float GetScale(int depth)
            {
                return 1f / Mathf.Pow(2f, depth);
            }
        }

        public class SQTGlobal
        {
            public ChainedPlugins plugins;
            public int maxDepth;
            public int resolution;
            public float desiredScreenSpaceLength;
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
                forward = GetForward(up);
                right = GetRight(up);
            }

            public static Vector3 GetForward(Vector3 up)
            {
                return new Vector3(up.y, up.z, up.x);
            }

            public static Vector3 GetRight(Vector3 up)
            {
                return Vector3.Cross(up, GetForward(up));
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
                int lerpWest = (neighborMask & Builder.Node.NEIGHBOR_MASKS[0]) == 0 ? 0 : 1;
                int lerpEast = (neighborMask & Builder.Node.NEIGHBOR_MASKS[1]) == 0 ? 0 : 1;
                int lerpSouth = (neighborMask & Builder.Node.NEIGHBOR_MASKS[2]) == 0 ? 0 : 1;
                int lerpNorth = (neighborMask & Builder.Node.NEIGHBOR_MASKS[3]) == 0 ? 0 : 1;

                int westTriangles = lerpWest == 0 ? 0 : (global.resolution / 2 + 2 * (global.resolution / 2 - 1));
                int eastTriangles = lerpEast == 0 ? 0 : (global.resolution / 2 + 2 * (global.resolution / 2 - 1));
                int southTriangles = lerpSouth == 0 ? 0 : (global.resolution / 2 + 2 * (global.resolution / 2 - 1));
                int northTriangles = lerpNorth == 0 ? 0 : (global.resolution / 2 + 2 * (global.resolution / 2 - 1));

                if (lerpWest == 1)
                {
                    westTriangles += 2;
                    westTriangles -= lerpSouth;
                    westTriangles -= lerpNorth;
                }
                if (lerpEast == 1)
                {
                    eastTriangles += 2;
                    eastTriangles -= lerpSouth;
                    eastTriangles -= lerpNorth;
                }
                if (lerpSouth == 1)
                {
                    southTriangles += 2;
                    southTriangles -= lerpEast;
                    southTriangles -= lerpWest;
                }
                if (lerpNorth == 1)
                {
                    northTriangles += 2;
                    northTriangles -= lerpEast;
                    northTriangles -= lerpWest;
                }

                int innerTrianglesCount = (global.resolution - lerpWest - lerpEast - 1) * (global.resolution - lerpSouth - lerpNorth - 1) * 2;
                int outerTrianglesCount = westTriangles + eastTriangles + southTriangles + northTriangles;
                int[] triangles = new int[(innerTrianglesCount + outerTrianglesCount) * 3];
                int triangleIndex = 0;

                // Inner triangles.
                for (int y = 0; y < global.resolution; y++)
                {
                    for (int x = 0; x < global.resolution; x++)
                    {
                        int vertexIndex = x + global.resolution * y;
                        if (lerpWest <= x && x < global.resolution - lerpEast - 1 && lerpSouth <= y && y < global.resolution - lerpNorth - 1)
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

                if (lerpWest == 1)
                {
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
                    if (lerpSouth == 0)
                    {
                        int vertexIndex = 0;
                        triangles[triangleIndex] = vertexIndex;
                        triangles[triangleIndex + 1] = vertexIndex + 1;
                        triangles[triangleIndex + 2] = vertexIndex + global.resolution + 1;
                        triangleIndex += 3;
                    }
                    if (lerpNorth == 0)
                    {
                        int vertexIndex = global.resolution * (global.resolution - 2);
                        triangles[triangleIndex] = vertexIndex + 1;
                        triangles[triangleIndex + 1] = vertexIndex + global.resolution + 1;
                        triangles[triangleIndex + 2] = vertexIndex + global.resolution;
                        triangleIndex += 3;
                    }
                }

                if (lerpEast == 1)
                {
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
                    if (lerpSouth == 0)
                    {
                        int vertexIndex = global.resolution - 2;
                        triangles[triangleIndex] = vertexIndex;
                        triangles[triangleIndex + 1] = vertexIndex + 1;
                        triangles[triangleIndex + 2] = vertexIndex + global.resolution;
                        triangleIndex += 3;
                    }
                    if (lerpNorth == 0)
                    {
                        int vertexIndex = global.resolution * (global.resolution - 2) + global.resolution - 2;
                        triangles[triangleIndex] = vertexIndex;
                        triangles[triangleIndex + 1] = vertexIndex + global.resolution + 1;
                        triangles[triangleIndex + 2] = vertexIndex + global.resolution;
                        triangleIndex += 3;
                    }
                }

                if (lerpSouth == 1)
                {
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
                    if (lerpWest == 0)
                    {
                        int vertexIndex = 0;
                        triangles[triangleIndex] = vertexIndex;
                        triangles[triangleIndex + 1] = vertexIndex + global.resolution + 1;
                        triangles[triangleIndex + 2] = vertexIndex + global.resolution;
                        triangleIndex += 3;
                    }
                    if (lerpEast == 0)
                    {
                        int vertexIndex = global.resolution - 2;
                        triangles[triangleIndex] = vertexIndex + 1;
                        triangles[triangleIndex + 1] = vertexIndex + global.resolution + 1;
                        triangles[triangleIndex + 2] = vertexIndex + global.resolution;
                        triangleIndex += 3;
                    }
                }

                if (lerpNorth == 1)
                {
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
                    if (lerpWest == 0)
                    {
                        int vertexIndex = global.resolution * (global.resolution - 2);
                        triangles[triangleIndex] = vertexIndex;
                        triangles[triangleIndex + 1] = vertexIndex + 1;
                        triangles[triangleIndex + 2] = vertexIndex + global.resolution;
                        triangleIndex += 3;
                    }
                    if (lerpEast == 0)
                    {
                        int vertexIndex = global.resolution * (global.resolution - 2) + global.resolution - 2;
                        triangles[triangleIndex] = vertexIndex;
                        triangles[triangleIndex + 1] = vertexIndex + 1;
                        triangles[triangleIndex + 2] = vertexIndex + global.resolution + 1;
                        triangleIndex += 3;
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
}