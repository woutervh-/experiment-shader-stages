using System;
using UnityEngine;

public interface SQTPlugin
{
    event EventHandler OnChange;
}

public interface SQTApproximateEdgeLengthPlugin
{
    void ModifyApproximateEdgeLength(ref float edgeLength);
}

public interface SQTMeshPlugin
{
    void ModifyMesh(Vector3[] vertices, Vector3[] normals);
}

public interface SQTDistanceToObjectPlugin
{
    void ModifyDistanceToObject(ref float distance);
}

public interface SQTMaterialPlugin
{
    void ModifyMaterial(Material material);
}

public interface SQTChainedPlugins : SQTApproximateEdgeLengthPlugin, SQTMeshPlugin, SQTDistanceToObjectPlugin, SQTMaterialPlugin { }
