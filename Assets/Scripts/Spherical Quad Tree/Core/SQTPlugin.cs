using System;
using UnityEngine;

public interface SQTPlugin
{
    event EventHandler OnChange;
}

public interface SQTMeshPlugin
{
    void ModifyMesh(Vector3[] vertices, Vector3[] normals);
}

public interface SQTMaterialPlugin
{
    void ModifyMaterial(Material material);
}

public interface SQTChainedPlugins : SQTMeshPlugin, SQTMaterialPlugin { }
