using UnityEngine;

namespace SQT.Core.GPU
{
    public class SQTInstancedNode
    {
        public SQTInstancedNode[] children;
        public MeshRenderer meshRenderer;
        public int neighborMask;

        SQTInstancedMesh instancedMesh;
        SQTInstancedNode parent;
        SQTConstants constants;
        SQTBuilder.Node node;
        GameObject gameObject;
        MeshFilter meshFilter;

        public SQTInstancedNode(SQTInstancedNode parent, SQTConstants constants, SQTInstancedMesh instancedMesh, SQTBuilder.Node node)
        {
            this.instancedMesh = instancedMesh;
            this.parent = parent;
            this.constants = constants;
            this.node = node;

            neighborMask = -1;
            gameObject = new GameObject("Chunk " + string.Join("", node.path));
            gameObject.transform.SetParent(constants.branch.gameObject.transform, false);
            gameObject.transform.SetPositionAndRotation(Vector3.zero, GetRotation());
            meshFilter = gameObject.AddComponent<MeshFilter>();
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = constants.global.material;
        }

        public void Destroy()
        {
            if (children != null)
            {
                foreach (SQTInstancedNode child in children)
                {
                    child.Destroy();
                }
            }
            UnityEngine.Object.Destroy(meshRenderer);
            UnityEngine.Object.Destroy(meshFilter);
            UnityEngine.Object.Destroy(gameObject);
        }

        public void SetMeshTriangles(int neighborMask)
        {

            meshFilter.sharedMesh = instancedMesh.GetMesh(node.path.Length, neighborMask);
        }

        Vector3 GetOrigin()
        {
            return constants.branch.up + node.offset.x * constants.branch.forward + node.offset.y * constants.branch.right;
        }

        Quaternion GetRotation()
        {
            return Quaternion.FromToRotation(Vector3.up, GetOrigin());
        }
    }
}
