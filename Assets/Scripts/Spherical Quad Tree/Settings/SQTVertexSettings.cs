using System;
using UnityEngine;

[CreateAssetMenu(menuName = "SQT/Vertex settings")]
public class SQTVertexSettings : ScriptableObject
{
    public int seed;
    [Range(0f, 100f)]
    public float height = 0f;

    public event EventHandler OnChange;

    void OnValidate()
    {
        if (OnChange != null)
        {
            OnChange.Invoke(this, EventArgs.Empty);
        }
    }
}
