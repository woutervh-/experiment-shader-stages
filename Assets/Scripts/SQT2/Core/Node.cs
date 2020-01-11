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
        public Task<Mesh> meshRequest;
        public CancellationTokenSource meshRequestCancellation;

        public static Node CreateRoot(Context context, Context.Branch branch)
        {
            GameObject gameObject = new GameObject("SQT (" + branch.index + ")");
            gameObject.transform.SetParent(context.constants.gameObject.transform, false);
            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = context.constants.material;

            return new Node
            {
                parent = null,
                children = null,
                path = new int[0],
                branch = branch,
                depth = context.depths[0],
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
            gameObject.transform.SetParent(context.roots[parent.branch.index].gameObject.transform, false);
            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
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

        public static Node[] CreateChildren(Context context, Node parent)
        {
            Node[] children = new Node[4];
            for (int i = 0; i < 4; i++)
            {
                children[i] = CreateChild(context, parent, i);
            }
            return children;
        }

        public static int[] GetChildPath(int[] path, int ordinal)
        {
            int[] childPath = new int[path.Length + 1];
            Array.Copy(path, childPath, path.Length);
            childPath[path.Length] = ordinal;
            return childPath;
        }

        public async void RequestMesh(Context context)
        {
            Vector3[] positions;
            Vector3[] normals;
            MeshHelper.GenerateVertices(context, branch, depth, offset, out positions, out normals);

            // Dummy delay. TODO: remove it and replace with plugins.
            await Task.Delay(500, meshRequestCancellation.Token);

            mesh = new Mesh();
            mesh.vertices = positions;
            mesh.normals = normals;
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
