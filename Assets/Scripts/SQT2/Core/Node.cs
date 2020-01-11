using System;
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
                meshRenderer = meshRenderer
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
                meshRenderer = meshRenderer
            };
        }

        public static int[] GetChildPath(int[] path, int ordinal)
        {
            int[] childPath = new int[path.Length + 1];
            Array.Copy(path, childPath, path.Length);
            childPath[path.Length] = ordinal;
            return childPath;
        }

        public void Destroy()
        {
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
