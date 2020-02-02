using UnityEngine;

namespace Atmosphere
{
    [ExecuteInEditMode]
    public class AtmospherePrecomputed : MonoBehaviour
    {
        public Material material;

        Mesh mesh;

        void OnEnable()
        {
            mesh = new Mesh();
            mesh.vertices = new Vector3[4];
            mesh.triangles = new int[] { 0, 2, 1, 1, 2, 3 };
        }

        void OnDisable()
        {
            UnityEngine.Object.DestroyImmediate(mesh);
        }

        void LateUpdate()
        {
            Vector3 p00V = new Vector3(0f, 0f, 1f);
            Vector3 p01V = new Vector3(1f, 0f, 1f);
            Vector3 p10V = new Vector3(0f, 1f, 1f);
            Vector3 p11V = new Vector3(1f, 1f, 1f);
            Vector3 p00W = Camera.main.ViewportToWorldPoint(p00V) - (Camera.main.transform.position + Camera.main.transform.forward);
            Vector3 p01W = Camera.main.ViewportToWorldPoint(p01V) - (Camera.main.transform.position + Camera.main.transform.forward);
            Vector3 p10W = Camera.main.ViewportToWorldPoint(p10V) - (Camera.main.transform.position + Camera.main.transform.forward);
            Vector3 p11W = Camera.main.ViewportToWorldPoint(p11V) - (Camera.main.transform.position + Camera.main.transform.forward);

            mesh.vertices = new Vector3[] { p00W, p01W, p10W, p11W };

            if (material != null)
            {
                Graphics.DrawMesh(mesh, Camera.main.transform.position + Camera.main.transform.forward, Quaternion.identity, material, 0);
            }
        }
    }
}
