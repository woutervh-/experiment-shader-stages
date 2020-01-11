namespace SQT2.Core
{
    public static class MeshHelper
    {
        public static int[] GetTriangles(int resolution, int neighborMask)
        {
            int lerpWest = (neighborMask & 1) == 0 ? 0 : 1;
            int lerpEast = (neighborMask & 2) == 0 ? 0 : 1;
            int lerpSouth = (neighborMask & 4) == 0 ? 0 : 1;
            int lerpNorth = (neighborMask & 8) == 0 ? 0 : 1;

            int outerTrianglesCount = 0;
            if (lerpWest == 1)
            {
                outerTrianglesCount += (resolution / 2 + 2 * (resolution / 2 - 1));
                outerTrianglesCount += 2;
                outerTrianglesCount -= lerpSouth;
                outerTrianglesCount -= lerpNorth;
            }
            if (lerpEast == 1)
            {
                outerTrianglesCount += (resolution / 2 + 2 * (resolution / 2 - 1));
                outerTrianglesCount += 2;
                outerTrianglesCount -= lerpSouth;
                outerTrianglesCount -= lerpNorth;
            }
            if (lerpSouth == 1)
            {
                outerTrianglesCount += (resolution / 2 + 2 * (resolution / 2 - 1));
                outerTrianglesCount += 2;
                outerTrianglesCount -= lerpEast;
                outerTrianglesCount -= lerpWest;
            }
            if (lerpNorth == 1)
            {
                outerTrianglesCount += (resolution / 2 + 2 * (resolution / 2 - 1));
                outerTrianglesCount += 2;
                outerTrianglesCount -= lerpEast;
                outerTrianglesCount -= lerpWest;
            }

            int innerTrianglesCount = (resolution - lerpWest - lerpEast - 1) * (resolution - lerpSouth - lerpNorth - 1) * 2;
            int[] triangles = new int[(innerTrianglesCount + outerTrianglesCount) * 3];
            int triangleIndex = 0;

            // Inner triangles.
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    int vertexIndex = x + resolution * y;
                    if (lerpWest <= x && x < resolution - lerpEast - 1 && lerpSouth <= y && y < resolution - lerpNorth - 1)
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

            if (lerpWest == 1)
            {
                for (int y = 0; y < resolution - 2; y++)
                {
                    int vertexIndex = y * resolution;
                    if (y % 2 == 0)
                    {
                        triangles[triangleIndex] = vertexIndex;
                        triangles[triangleIndex + 1] = vertexIndex + resolution + 1;
                        triangles[triangleIndex + 2] = vertexIndex + resolution + resolution;
                        triangleIndex += 3;
                    }
                    else
                    {
                        triangles[triangleIndex] = vertexIndex + 1;
                        triangles[triangleIndex + 1] = vertexIndex + resolution + 1;
                        triangles[triangleIndex + 2] = vertexIndex + resolution;
                        triangles[triangleIndex + 3] = vertexIndex + resolution;
                        triangles[triangleIndex + 4] = vertexIndex + resolution + 1;
                        triangles[triangleIndex + 5] = vertexIndex + resolution + resolution + 1;
                        triangleIndex += 6;
                    }
                }
                if (lerpSouth == 0)
                {
                    int vertexIndex = 0;
                    triangles[triangleIndex] = vertexIndex;
                    triangles[triangleIndex + 1] = vertexIndex + 1;
                    triangles[triangleIndex + 2] = vertexIndex + resolution + 1;
                    triangleIndex += 3;
                }
                if (lerpNorth == 0)
                {
                    int vertexIndex = resolution * (resolution - 2);
                    triangles[triangleIndex] = vertexIndex + 1;
                    triangles[triangleIndex + 1] = vertexIndex + resolution + 1;
                    triangles[triangleIndex + 2] = vertexIndex + resolution;
                    triangleIndex += 3;
                }
            }

            if (lerpEast == 1)
            {
                for (int y = 0; y < resolution - 2; y++)
                {
                    int vertexIndex = y * resolution + resolution - 2;
                    if (y % 2 == 0)
                    {
                        triangles[triangleIndex] = vertexIndex + 1;
                        triangles[triangleIndex + 1] = vertexIndex + resolution + resolution + 1;
                        triangles[triangleIndex + 2] = vertexIndex + resolution;
                        triangleIndex += 3;
                    }
                    else
                    {
                        triangles[triangleIndex] = vertexIndex;
                        triangles[triangleIndex + 1] = vertexIndex + resolution + 1;
                        triangles[triangleIndex + 2] = vertexIndex + resolution;
                        triangles[triangleIndex + 3] = vertexIndex + resolution;
                        triangles[triangleIndex + 4] = vertexIndex + resolution + 1;
                        triangles[triangleIndex + 5] = vertexIndex + resolution + resolution;
                        triangleIndex += 6;
                    }
                }
                if (lerpSouth == 0)
                {
                    int vertexIndex = resolution - 2;
                    triangles[triangleIndex] = vertexIndex;
                    triangles[triangleIndex + 1] = vertexIndex + 1;
                    triangles[triangleIndex + 2] = vertexIndex + resolution;
                    triangleIndex += 3;
                }
                if (lerpNorth == 0)
                {
                    int vertexIndex = resolution * (resolution - 2) + resolution - 2;
                    triangles[triangleIndex] = vertexIndex;
                    triangles[triangleIndex + 1] = vertexIndex + resolution + 1;
                    triangles[triangleIndex + 2] = vertexIndex + resolution;
                    triangleIndex += 3;
                }
            }

            if (lerpSouth == 1)
            {
                for (int x = 0; x < resolution - 2; x++)
                {
                    int vertexIndex = x;
                    if (x % 2 == 0)
                    {
                        triangles[triangleIndex] = vertexIndex;
                        triangles[triangleIndex + 1] = vertexIndex + 2;
                        triangles[triangleIndex + 2] = vertexIndex + resolution + 1;
                        triangleIndex += 3;
                    }
                    else
                    {
                        triangles[triangleIndex] = vertexIndex + resolution;
                        triangles[triangleIndex + 1] = vertexIndex + 1;
                        triangles[triangleIndex + 2] = vertexIndex + resolution + 1;
                        triangles[triangleIndex + 3] = vertexIndex + 1;
                        triangles[triangleIndex + 4] = vertexIndex + resolution + 2;
                        triangles[triangleIndex + 5] = vertexIndex + resolution + 1;
                        triangleIndex += 6;
                    }
                }
                if (lerpWest == 0)
                {
                    int vertexIndex = 0;
                    triangles[triangleIndex] = vertexIndex;
                    triangles[triangleIndex + 1] = vertexIndex + resolution + 1;
                    triangles[triangleIndex + 2] = vertexIndex + resolution;
                    triangleIndex += 3;
                }
                if (lerpEast == 0)
                {
                    int vertexIndex = resolution - 2;
                    triangles[triangleIndex] = vertexIndex + 1;
                    triangles[triangleIndex + 1] = vertexIndex + resolution + 1;
                    triangles[triangleIndex + 2] = vertexIndex + resolution;
                    triangleIndex += 3;
                }
            }

            if (lerpNorth == 1)
            {
                for (int x = 0; x < resolution - 2; x++)
                {
                    int vertexIndex = x + resolution * (resolution - 2);
                    if (x % 2 == 0)
                    {
                        triangles[triangleIndex] = vertexIndex + resolution;
                        triangles[triangleIndex + 1] = vertexIndex + 1;
                        triangles[triangleIndex + 2] = vertexIndex + resolution + 2;
                        triangleIndex += 3;
                    }
                    else
                    {
                        triangles[triangleIndex] = vertexIndex;
                        triangles[triangleIndex + 1] = vertexIndex + 1;
                        triangles[triangleIndex + 2] = vertexIndex + resolution + 1;
                        triangles[triangleIndex + 3] = vertexIndex + 1;
                        triangles[triangleIndex + 4] = vertexIndex + 2;
                        triangles[triangleIndex + 5] = vertexIndex + resolution + 1;
                        triangleIndex += 6;
                    }
                }
                if (lerpWest == 0)
                {
                    int vertexIndex = resolution * (resolution - 2);
                    triangles[triangleIndex] = vertexIndex;
                    triangles[triangleIndex + 1] = vertexIndex + 1;
                    triangles[triangleIndex + 2] = vertexIndex + resolution;
                    triangleIndex += 3;
                }
                if (lerpEast == 0)
                {
                    int vertexIndex = resolution * (resolution - 2) + resolution - 2;
                    triangles[triangleIndex] = vertexIndex;
                    triangles[triangleIndex + 1] = vertexIndex + 1;
                    triangles[triangleIndex + 2] = vertexIndex + resolution + 1;
                    triangleIndex += 3;
                }
            }

            return triangles;
        }
    }
}
