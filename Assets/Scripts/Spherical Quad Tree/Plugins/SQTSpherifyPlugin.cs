using System;
using UnityEngine;

public class SQTSpherifyPlugin : MonoBehaviour, SQTPlugin, SQTApproximateEdgeLengthPlugin, SQTMeshPlugin, SQTDistanceToObjectPlugin
{
    [Range(0f, 1e6f)]
    public float radius = 1f;

    public event EventHandler OnChange;

    void OnValidate()
    {
        if (OnChange != null)
        {
            OnChange.Invoke(this, EventArgs.Empty);
        }
    }

    public void ModifyMesh(Vector3[] vertices, Vector3[] normals)
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].Normalize();
            normals[i] = vertices[i];
            vertices[i] *= radius;
        }
    }

    public void ModifyApproximateEdgeLength(ref float edgeLength)
    {
        edgeLength *= radius;
    }

    public void ModifyDistanceToObject(ref float distance)
    {
        distance = Mathf.Abs(distance - radius);
    }
}
