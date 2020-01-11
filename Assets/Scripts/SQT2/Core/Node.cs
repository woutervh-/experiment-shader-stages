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

            // Dummy delay. TODO: remove it and replace with plugins.
            await Task.Delay(500, meshRequestCancellation.Token);

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

        static Vector2[] childOffsetVectors = new Vector2[] {
            new Vector2(-1f, -1f),
            new Vector2(1f, -1f),
            new Vector2(-1f, 1f),
            new Vector2(1f, 1f),
        };
    }
}
