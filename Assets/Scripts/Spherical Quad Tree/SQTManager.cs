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

        SQTReconciliationSettings reconciliationSettings = SQTReconciliationSettings.GetSettings(constants, playerCamera, transform);
        SQTNode found = root.FindNode(reconciliationSettings);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, playerCamera.transform.position);
        if (found != null)
        {
            Gizmos.DrawWireMesh(found.mesh, found.gameObject.transform.position);
        }
    }
#endif

    void Update()
    {
        if (playerCamera == null || playerCamera.gameObject != player)
        {
            playerCamera = player.GetComponent<Camera>();
        }

        SQTReconciliationSettings reconciliationSettings = SQTReconciliationSettings.GetSettings(constants, playerCamera, transform);
        SQTNode found = root.FindNode(reconciliationSettings);
        if (found != null)
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
