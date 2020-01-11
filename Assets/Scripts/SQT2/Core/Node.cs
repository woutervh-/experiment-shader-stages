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
        // public int neighborMask;

        public static Node CreateRoot(Context context, Context.Branch branch)
        {
            return new Node
            {
                parent = null,
                children = null,
                branch = branch,
                depth = context.depths[0],
                offset = Vector2.zero,
                path = new int[0]
            };
        }

        public void Destroy()
        {
            //
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
