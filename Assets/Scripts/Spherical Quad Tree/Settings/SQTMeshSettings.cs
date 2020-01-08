using System;
using UnityEngine;

[CreateAssetMenu(menuName = "SQT/Mesh settings")]
public class SQTMeshSettings : ScriptableObject
{
    [Range(0f, 16f)]
    public int maxDepth = 10;
    [Range(2f, 16f)]
    public int resolution = 7;
    [Range(1f, 100f)]
    public float desiredScreenSpaceLength = 10f;
    public bool sphere = false;

    public event EventHandler OnChange;

    void OnValidate()
    {
        if (OnChange != null)
        {
            OnChange.Invoke(this, EventArgs.Empty);
        }
    }
}
