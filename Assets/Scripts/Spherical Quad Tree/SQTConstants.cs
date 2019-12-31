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

    public SQTGlobal global;
    public SQTBranch branch;
    public SQTDepth[] depth;
}
