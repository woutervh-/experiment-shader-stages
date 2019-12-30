using UnityEngine;

public class SQTManager : MonoBehaviour
{
    public GameObject player;
    public Material material;

    SQTRoot root;
    SQTConstants constants;
    Camera playerCamera;

    void Start()
    {
        GameObject up = new GameObject("SQT Up");
        up.transform.SetParent(transform, false);

        SQTConstants.SQTGlobal global = new SQTConstants.SQTGlobal
        {
            maxDepth = 10,
            resolution = 16,
            radius = 1f,
            // radius = 1e6f,
            material = material
        };
        SQTConstants.SQTBranch branch = new SQTConstants.SQTBranch(Vector3.up)
        {
            gameObject = up
        };
        SQTConstants.SQTDepth[] depth = SQTConstants.SQTDepth.GetFromGlobal(global);
        constants = new SQTConstants
        {
            global = global,
            branch = branch,
            depth = depth
        };
        root = new SQTRoot(constants);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (root == null || playerCamera == null)
        {
            return;
        }

        SQTNode found = root.FindNode(playerCamera);
        if (found != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireMesh(found.mesh, found.gameObject.transform.position);
            Gizmos.DrawLine(found.gameObject.transform.position, playerCamera.transform.position);
        }
    }
#endif

    void Update()
    {
        if (playerCamera == null || playerCamera.gameObject != player)
        {
            playerCamera = player.GetComponent<Camera>();
        }

        SQTNode found = root.FindNode(playerCamera);
        SQTReconciliationSettings reconciliationSettings = SQTReconciliationSettings.GetSettings(constants, playerCamera, transform);
        if (found != null && reconciliationSettings != null)
        {
            found.Reconciliate(reconciliationSettings);
        }
    }

    void OnDestroy()
    {
        if (root != null)
        {
            root.Destroy();
        }
    }
}
