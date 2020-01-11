using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SQT2.Core
{
    public class Node
    {
        public Node parent;
        public Node[] children;
        public int[] path;
        public Context.Branch branch;
        public Context.Depth depth;
        public Vector2 offset;
        public GameObject gameObject;
        public Mesh mesh;
        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;
        public Task meshRequest;
        public CancellationTokenSource meshRequestCancellation;

        public static Node CreateRoot(Context.Constants constants, Context.Depth depth, Context.Branch branch)
        {
            GameObject gameObject = new GameObject("Chunk");
            gameObject.transform.SetParent(branch.gameObject.transform, false);
            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.enabled = false;
            meshRenderer.sharedMaterial = constants.material;

            return new Node
            {
                parent = null,
                children = null,
                path = new int[0],
                branch = branch,
                depth = depth,
                offset = Vector2.zero,
                gameObject = gameObject,
                mesh = null,
                meshFilter = meshFilter,
                meshRenderer = meshRenderer,
                meshRequest = null,
                meshRequestCancellation = null
            };
        }

        public static Node CreateChild(Context context, Node parent, int ordinal)
        {
            int[] path = GetChildPath(parent.path, ordinal);
            GameObject gameObject = new GameObject("Chunk " + string.Join("", path));
            gameObject.transform.SetParent(parent.branch.gameObject.transform, false);
            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.enabled = false;
            meshRenderer.sharedMaterial = context.constants.material;
            Context.Depth depth = context.depths[parent.depth.index + 1];

            return new Node
            {
                parent = parent,
                children = null,
                path = path,
                branch = parent.branch,
                depth = depth,
                offset = parent.offset + childOffsetVectors[ordinal] * depth.scale,
                gameObject = gameObject,
                mesh = null,
                meshFilter = meshFilter,
                meshRenderer = meshRenderer,
                meshRequest = null,
                meshRequestCancellation = null
            };
        }

        public static void CreateChildren(Context context, Node parent)
        {
            parent.children = new Node[4];
            for (int i = 0; i < 4; i++)
            {
                parent.children[i] = CreateChild(context, parent, i);
            }
        }

        public static void RemoveChildren(Node parent)
        {
            for (int i = 0; i < 4; i++)
            {
                parent.children[i].Destroy();
            }
            parent.children = null;
        }

        public static int[] GetChildPath(int[] path, int ordinal)
        {
            int[] childPath = new int[path.Length + 1];
            Array.Copy(path, childPath, path.Length);
            childPath[path.Length] = ordinal;
            return childPath;
        }

        public async Task RequestMesh(Context context)
        {
            Vector3[] positions;
            Vector3[] normals;
            MeshHelper.GenerateVertices(context, branch, depth, offset, out positions, out normals);

            // Artificial delay. TODO: remove it and replace with plugins.
            await Task.Delay((int)UnityEngine.Random.Range(250f, 750f), meshRequestCancellation.Token);

            if (!meshRequestCancellation.Token.IsCancellationRequested)
            {
                mesh = new Mesh();
                mesh.vertices = positions;
                mesh.normals = normals;
                meshFilter.sharedMesh = mesh;
            }
        }

        public void Destroy()
        {
            if (meshRequestCancellation != null)
            {
                meshRequestCancellation.Cancel();
            }
            if (mesh != null)
            {
                UnityEngine.Object.Destroy(mesh);
            }
            if (meshFilter != null)
            {
                UnityEngine.Object.Destroy(meshFilter);
            }
            if (meshRenderer != null)
            {
                UnityEngine.Object.Destroy(meshRenderer);
            }
            if (gameObject != null)
            {
                UnityEngine.Object.Destroy(gameObject);
            }
        }

        public static Vector2[] childOffsetVectors = new Vector2[] {
            new Vector2(-1f, -1f),
            new Vector2(1f, -1f),
            new Vector2(-1f, 1f),
            new Vector2(1f, 1f),
        };

        public static bool[][] neighborSameParent = new bool[][] {
            new bool[] { false, true, false, true },
            new bool[] { true, false, false, true },
            new bool[] { false, true, true, false },
            new bool[] { true, false, true, false }
        };

        public static int[][] neighborOrdinal = new int[][] {
            new int[] { 1, 1, 2, 2 },
            new int[] { 0, 0, 3, 3 },
            new int[] { 3, 3, 0, 0 },
            new int[] { 2, 2, 1, 1 }
        };

        public static int[][] rootOrdinalRotation = new int[][] {
            new int[] { 2, 3, 4, 5 },
            new int[] { 3, 2, 4, 5 },
            new int[] { 4, 5, 0, 1 },
            new int[] { 5, 4, 0, 1 },
            new int[] { 1, 0, 3, 2 },
            new int[] { 0, 1, 3, 2 }
        };

        public static int[][][] neighborOrdinalRotation = new int[][][] {
            new int[][] {
                new int[] { 0, 1, 2, 3 },
                new int[] { 0, 1, 2, 3 },
                new int[] { 0, 2, 1, 3 },
                new int[] { 3, 1, 2, 0 },
                new int[] { 3, 1, 2, 0 },
                new int[] { 3, 1, 2, 0 }
            },
            new int[][] {
                new int[] { 0, 1, 2, 3 },
                new int[] { 0, 1, 2, 3 },
                new int[] { 0, 2, 1, 3 },
                new int[] { 3, 1, 2, 0 },
                new int[] { 0, 2, 1, 3 },
                new int[] { 0, 2, 1, 3 }
            },
            new int[][] {
                new int[] { 0, 2, 1, 3 },
                new int[] { 0, 2, 1, 3 },
                new int[] { 0, 1, 2, 3 },
                new int[] { 0, 1, 2, 3 },
                new int[] { 3, 1, 2, 0 },
                new int[] { 0, 2, 1, 3 }
            },
            new int[][] {
                new int[] { 3, 1, 2, 0 },
                new int[] { 3, 1, 2, 0 },
                new int[] { 0, 1, 2, 3 },
                new int[] { 0, 1, 2, 3 },
                new int[] { 3, 1, 2, 0 },
                new int[] { 0, 2, 1, 3 }
            },
            new int[][] {
                new int[] { 3, 1, 2, 0 },
                new int[] { 0, 2, 1, 3 },
                new int[] { 3, 1, 2, 0 },
                new int[] { 3, 1, 2, 0 },
                new int[] { 0, 1, 2, 3 },
                new int[] { 0, 1, 2, 3 }
            },
            new int[][] {
                new int[] { 3, 1, 2, 0 },
                new int[] { 0, 2, 1, 3 },
                new int[] { 0, 2, 1, 3 },
                new int[] { 0, 2, 1, 3 },
                new int[] { 0, 1, 2, 3 },
                new int[] { 0, 1, 2, 3 }
            }
        };
    }
}
