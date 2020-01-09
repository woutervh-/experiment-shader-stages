using System;
using System.Collections.Generic;
using UnityEngine;

namespace SQT.Core
{
    public interface Plugin
    {
        event EventHandler OnChange;
    }

    public interface VerticesPlugin
    {
        void ModifyVertices(SQT.Core.Constants constants, Vector3[] vertices, Vector3[] normals);
    }

    public interface MeshPlugin
    {
        void ModifyMesh(SQT.Core.Constants constants, Mesh mesh, SQT.Core.Builder.Node node);
    }

    public interface MaterialPlugin
    {
        void ModifyMaterial(ref Material material);
    }

    public interface ReconcilerFactoryPlugin
    {
        void ModifyReconcilerFactory(ref SQT.Core.ReconcilerFactory reconcilerFactory);
    }

    public interface BuilderLeavesPlugin
    {
        void ModifyBuilderLeaves(ref SQT.Core.Builder.Node[] leaves, SQT.Core.Builder.Node[] branches);
    }

    public interface ChainedPlugins : VerticesPlugin, MeshPlugin, MaterialPlugin, ReconcilerFactoryPlugin, BuilderLeavesPlugin { }

    class PluginsChain : SQT.Core.ChainedPlugins
    {
        VerticesPlugin[] verticesPlugins;
        MeshPlugin[] meshPlugins;
        MaterialPlugin[] materialPlugins;
        ReconcilerFactoryPlugin[] reconcilerFactoryPlugins;
        BuilderLeavesPlugin[] builderLeavesPlugins;

        public PluginsChain(SQT.Core.Plugin[] plugins)
        {
            List<VerticesPlugin> verticesPlugins = new List<VerticesPlugin>();
            List<MeshPlugin> meshPlugins = new List<MeshPlugin>();
            List<MaterialPlugin> materialPlugins = new List<MaterialPlugin>();
            List<ReconcilerFactoryPlugin> reconcilerFactoryPlugins = new List<ReconcilerFactoryPlugin>();
            List<BuilderLeavesPlugin> builderLeavesPlugins = new List<BuilderLeavesPlugin>();

            foreach (Plugin plugin in plugins)
            {
                if (plugin is MonoBehaviour monoBehaviour)
                {
                    if (!monoBehaviour.enabled)
                    {
                        continue;
                    }
                }
                if (plugin is VerticesPlugin verticesPlugin)
                {
                    verticesPlugins.Add(verticesPlugin);
                }
                if (plugin is MeshPlugin meshPlugin)
                {
                    meshPlugins.Add(meshPlugin);
                }
                if (plugin is MaterialPlugin materialModifier)
                {
                    materialPlugins.Add(materialModifier);
                }
                if (plugin is ReconcilerFactoryPlugin reconcilerFactoryPlugin)
                {
                    reconcilerFactoryPlugins.Add(reconcilerFactoryPlugin);
                }
                if (plugin is BuilderLeavesPlugin builderLeavesPlugin)
                {
                    builderLeavesPlugins.Add(builderLeavesPlugin);
                }
            }

            this.verticesPlugins = verticesPlugins.ToArray();
            this.meshPlugins = meshPlugins.ToArray();
            this.materialPlugins = materialPlugins.ToArray();
            this.reconcilerFactoryPlugins = reconcilerFactoryPlugins.ToArray();
            this.builderLeavesPlugins = builderLeavesPlugins.ToArray();
        }

        public void ModifyVertices(SQT.Core.Constants constants, Vector3[] vertices, Vector3[] normals)
        {
            foreach (VerticesPlugin plugin in verticesPlugins)
            {
                plugin.ModifyVertices(constants, vertices, normals);
            }
        }

        public void ModifyMesh(SQT.Core.Constants constants, Mesh mesh, SQT.Core.Builder.Node node)
        {
            foreach (MeshPlugin plugin in meshPlugins)
            {
                plugin.ModifyMesh(constants, mesh, node);
            }
        }

        public void ModifyMaterial(ref Material material)
        {
            foreach (MaterialPlugin plugin in materialPlugins)
            {
                plugin.ModifyMaterial(ref material);
            }
        }

        public void ModifyReconcilerFactory(ref SQT.Core.ReconcilerFactory reconcilerFactory)
        {
            foreach (ReconcilerFactoryPlugin plugin in reconcilerFactoryPlugins)
            {
                plugin.ModifyReconcilerFactory(ref reconcilerFactory);
            }
        }

        public void ModifyBuilderLeaves(ref SQT.Core.Builder.Node[] leaves, SQT.Core.Builder.Node[] branches)
        {
            foreach (BuilderLeavesPlugin plugin in builderLeavesPlugins)
            {
                plugin.ModifyBuilderLeaves(ref leaves, branches);
            }
        }
    }
}
