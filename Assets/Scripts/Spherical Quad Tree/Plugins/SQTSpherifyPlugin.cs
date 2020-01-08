using System;
using UnityEngine;

public class SQTSpherifyPlugin : MonoBehaviour, SQTPlugin, SQTPlugin.ApproximateEdgeLengthModifier, SQTPlugin.MeshModifier
{
    [Range(0f, 1e6f)]
    public float radius = 1f;

    public event EventHandler OnChange;

    public void ModifyMesh(Mesh mesh)
    {
        // TODO:
    }

    public void ModifyApproximateEdgeLength(ref float edgeLength)
    {
        edgeLength *= radius;
    }
}
