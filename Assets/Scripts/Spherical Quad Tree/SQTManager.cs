using UnityEngine;

public class SQTManager : MonoBehaviour
{
    public GameObject player;
    public Material material;

    SQTRoot root;

    void Start()
    {
        GameObject up = new GameObject("SQT Up");
        up.transform.SetParent(transform, false);
        SQTConstants constants = new SQTConstants(10)
        {
            global = new SQTConstants.SQTGlobal
            {
                resolution = 16,
                radius = 1f,
                // radius = 1e6f,
                material = material
            },
            branch = new SQTConstants.SQTBranch(Vector3.up)
            {
                gameObject = up
            }
        };

        root = new SQTRoot(constants);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (root == null)
        {
            return;
        }

        SQTNode found = root.FindNode(player);
        if (found != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireMesh(found.mesh, found.gameObject.transform.position);
            Gizmos.DrawLine(found.gameObject.transform.position, player.transform.position);
        }
    }
#endif

    void Update()
    {

    }

    void OnDestroy()
    {
        if (root != null)
        {
            root.Destroy();
        }
    }
}
