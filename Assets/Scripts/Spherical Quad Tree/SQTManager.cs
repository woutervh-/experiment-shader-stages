using UnityEngine;

public class SQTManager : MonoBehaviour
{
    SQTRoot root;

    void Start()
    {
        SQTConstants constants = new SQTConstants
        {
            global = new SQTConstants.SQTGlobal
            {
                resolution = 16,
                radius = 1e6f
            },
            branch = new SQTConstants.SQTBranch(Vector3.up)
            {
                gameObject = new GameObject("SQT Up")
            }
        };

        root = new SQTRoot(constants);
    }

    void OnDestroy()
    {
        root.Destroy();
    }
}
