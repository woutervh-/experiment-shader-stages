using UnityEngine;

[CreateAssetMenu()]
public class SQTHeightSettings : ScriptableObject
{
    [Range(0f, 100f)]
    public float height = 0f;

    public float GetHeight(Vector3 position)
    {
        return height;
    }
}
