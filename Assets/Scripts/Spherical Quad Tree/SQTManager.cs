using UnityEngine;

public class SQTManager : MonoBehaviour
{
    public GameObject player;
    public Material material;
    [Range(0f, 16f)]
    public int maxDepth = 10;
    [Range(2f, 16f)]
    public int resolution = 7;
    [Range(0f, 1e6f)]
    public float radius = 1f;
    [Range(1f, 100f)]
    public float desiredScreenSpaceLength = 10f;
    public bool sphere = false;
    public SQTHeightSettings heightSettings;

#if UNITY_EDITOR
    public bool debug;
#endif

    int currentMaxDepth;
    int currentResolution;
    float currentRadius;
    float currentDesiredScreenSpaceLength;
    bool currentSphere;
    SQTBranches branches;
    Camera playerCamera;

    void Start()
    {
        DoUpdateSettings();
    }

    bool ShouldUpdateSettings()
    {
        return currentMaxDepth != maxDepth || currentResolution != resolution || currentRadius != radius || currentDesiredScreenSpaceLength != desiredScreenSpaceLength || currentSphere != sphere;
    }

    void DoUpdateSettings()
    {
        if (branches != null)
        {
            branches.Destroy();
        }

        currentMaxDepth = maxDepth;
        currentResolution = resolution;
        currentRadius = radius;
        currentDesiredScreenSpaceLength = desiredScreenSpaceLength;
        currentSphere = sphere;

        SQTConstants.SQTGlobal global = new SQTConstants.SQTGlobal
        {
            maxDepth = maxDepth,
            resolution = resolution * 2 - 1,
            radius = radius,
            desiredScreenSpaceLength = desiredScreenSpaceLength,
            material = material,
            gameObject = gameObject,
            sphere = sphere
        };
        SQTConstants.SQTDepth[] depth = SQTConstants.SQTDepth.GetFromGlobal(global);
        SQTConstants.SQTMesh[] meshes = SQTConstants.SQTMesh.GetFromGlobal(global);
        branches = new SQTBranches(global, depth, meshes);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!debug)
        {
            return;
        }

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
        if (ShouldUpdateSettings())
        {
            DoUpdateSettings();
        }

        if (playerCamera == null || playerCamera.gameObject != player)
        {
            playerCamera = player.GetComponent<Camera>();
        }

        if (branches != null)
        {
            branches.Reconcile(playerCamera);
        }
    }

    void OnDestroy()
    {
        branches.Destroy();
    }
}
