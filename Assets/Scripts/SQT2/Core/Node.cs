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
        GameObject gameObject;
        Mesh mesh;
        MeshFilter meshFilter;
        MeshRenderer meshRenderer;

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

        public enum Cardinal
        {
            WEST = 0,
            EAST = 1,
            SOUTH = 2,
            NORTH = 3
        }

        public enum CardinalMask
        {
            WEST = 1,
            EAST = 2,
            SOUTH = 4,
            NORTH = 8
        }

        public enum Ordinal
        {
            SOUTH_WEST = 0,
            SOUTH_EAST = 1,
            NORTH_WEST = 2,
            NORTH_EAST = 3
        }
    }
}
