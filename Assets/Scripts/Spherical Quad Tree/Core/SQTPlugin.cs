using System;
using UnityEngine;

public interface SQTPlugin
{
    event EventHandler OnChange;

    public interface ApproximateEdgeLengthModifier
    {
        void ModifyApproximateEdgeLength(ref float edgeLength);
    }

    public interface MeshModifier
    {
        void ModifyMesh(Mesh mesh);
    }

    public interface ChainedPlugins : ApproximateEdgeLengthModifier, MeshModifier { }
}
