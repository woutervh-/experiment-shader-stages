using UnityEngine;

public class SQTManager : MonoBehaviour
{
    public GameObject player;
    public Material material;

    SQTRoot root;
    SQTVirtualRoot virtualRoot;
    Camera playerCamera;

    void Start()
    {
        SQTConstants.SQTGlobal global = new SQTConstants.SQTGlobal
        {
            maxDepth = 10,
            resolution = 16,
            radius = 1f,
            // radius = 1e6f,
            material = material,
            gameObject = gameObject
        };
        SQTConstants.SQTDepth[] depth = SQTConstants.SQTDepth.GetFromGlobal(global);
        root = new SQTRoot(global, depth);
        virtualRoot = new SQTVirtualRoot(global, depth);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (root == null || playerCamera == null)
        {
            return;
        }

        SQTNode found = root.FindNode(playerCamera);

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

        root.Reconciliate(playerCamera);
        virtualRoot.Reconciliate(playerCamera);
        new SQTVirtualRootTester(virtualRoot).Render();

        // foreach (SQTRoot root in roots)
        // {
        //     SQTReconciliationData reconciliationData = SQTReconciliationData.GetSettings(root.constants, playerCamera, transform);
        //     root.Reconciliate(reconciliationData);
        // }
    }

    void OnDestroy()
    {
        if (root != null)
        {
            root.Destroy();
        }
    }
}
