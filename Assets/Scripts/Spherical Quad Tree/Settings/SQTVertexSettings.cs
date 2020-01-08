using System;
using UnityEngine;

[CreateAssetMenu()]
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
