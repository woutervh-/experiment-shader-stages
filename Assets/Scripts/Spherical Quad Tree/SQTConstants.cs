using UnityEngine;

public class SQTConstants
{
    public class SQTGlobal
    {
        public int resolution;
        public float radius;
    }

    public class SQTBranch
    {
        public Vector3 up;
        public Vector3 forward;
        public Vector3 right;
        public GameObject gameObject;

        public SQTBranch(Vector3 up)
        {
            this.up = up;
            forward = new Vector3(up.y, up.z, up.x);
            right = Vector3.Cross(up, forward);
        }
    }

    public SQTGlobal global;
    public SQTBranch branch;
}
