using System;
using System.Collections.Generic;
using UnityEngine;

namespace SQT.Core
{
    public interface Plugin
    {
        event EventHandler OnChange;
    }

    public interface MeshPlugin
    {
        void ModifyMesh(Vector3[] vertices, Vector3[] normals);
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

    public interface ChainedPlugins : MeshPlugin, MaterialPlugin, ReconcilerFactoryPlugin, BuilderLeavesPlugin { }

    class PluginsChain : SQT.Core.ChainedPlugins
    {
        MeshPlugin[] meshPlugins;
        MaterialPlugin[] materialPlugins;
        ReconcilerFactoryPlugin[] reconcilerFactoryPlugins;
        BuilderLeavesPlugin[] builderLeavesPlugins;

        public PluginsChain(SQT.Core.Plugin[] plugins)
        {
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
                if (plugin is MeshPlugin meshModifier)
                {
                    meshPlugins.Add(meshModifier);
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

            this.meshPlugins = meshPlugins.ToArray();
            this.materialPlugins = materialPlugins.ToArray();
            this.reconcilerFactoryPlugins = reconcilerFactoryPlugins.ToArray();
            this.builderLeavesPlugins = builderLeavesPlugins.ToArray();
        }

        public void ModifyMesh(Vector3[] vertices, Vector3[] normals)
        {
            foreach (MeshPlugin plugin in meshPlugins)
            {
                plugin.ModifyMesh(vertices, normals);
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
