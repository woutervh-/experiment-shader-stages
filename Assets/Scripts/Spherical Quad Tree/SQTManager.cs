using UnityEngine;

public class SQTManager : MonoBehaviour
{
    public GameObject player;
    public Material material;

    SQTBranches branches;
    Camera playerCamera;

    void Start()
    {
        SQTConstants.SQTGlobal global = new SQTConstants.SQTGlobal
        {
            maxDepth = 10,
            // resolution = 16,
            resolution = 4,
            radius = 1f,
            // radius = 1e6f,
            material = material,
            gameObject = gameObject
        };
        SQTConstants.SQTDepth[] depth = SQTConstants.SQTDepth.GetFromGlobal(global);
        branches = new SQTBranches(global, depth);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (playerCamera == null)
        {
            return;
        }

        if (branches != null)
        {
            branches.DrawBranches(playerCamera);
        }
    }
#endif

    void Update()
    {
        if (playerCamera == null || playerCamera.gameObject != player)
        {
            playerCamera = player.GetComponent<Camera>();
        }

        if (branches != null)
        {
            branches.Reconciliate(playerCamera);
        }
    }

    void OnDestroy()
    {
        branches.Destroy();
    }
}
