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

    public interface ChainedPlugins : MeshPlugin, MaterialPlugin, ReconcilerFactoryPlugin { }

    class PluginsChain : SQT.Core.ChainedPlugins
    {
        SQT.Core.MeshPlugin[] meshPlugins;
        SQT.Core.MaterialPlugin[] materialPlugins;
        SQT.Core.ReconcilerFactoryPlugin[] reconcilerFactoryPlugins;

        public PluginsChain(SQT.Core.Plugin[] plugins)
        {
            List<SQT.Core.MeshPlugin> meshPlugins = new List<SQT.Core.MeshPlugin>();
            List<SQT.Core.MaterialPlugin> materialPlugins = new List<SQT.Core.MaterialPlugin>();
            List<SQT.Core.ReconcilerFactoryPlugin> reconcilerFactoryPlugins = new List<SQT.Core.ReconcilerFactoryPlugin>();

            foreach (SQT.Core.Plugin plugin in plugins)
            {
                if (plugin is MonoBehaviour monoBehaviour)
                {
                    if (!monoBehaviour.enabled)
                    {
                        continue;
                    }
                }
                if (plugin is SQT.Core.MeshPlugin meshModifier)
                {
                    meshPlugins.Add(meshModifier);
                }
                if (plugin is SQT.Core.MaterialPlugin materialModifier)
                {
                    materialPlugins.Add(materialModifier);
                }
                if (plugin is SQT.Core.ReconcilerFactoryPlugin reconcilerFactoryPlugin)
                {
                    reconcilerFactoryPlugins.Add(reconcilerFactoryPlugin);
                }
            }

            this.meshPlugins = meshPlugins.ToArray();
            this.materialPlugins = materialPlugins.ToArray();
            this.reconcilerFactoryPlugins = reconcilerFactoryPlugins.ToArray();
        }

        public void ModifyMesh(Vector3[] vertices, Vector3[] normals)
        {
            foreach (SQT.Core.MeshPlugin plugin in meshPlugins)
            {
                plugin.ModifyMesh(vertices, normals);
            }
        }

        public void ModifyMaterial(ref Material material)
        {
            foreach (SQT.Core.MaterialPlugin plugin in materialPlugins)
            {
                plugin.ModifyMaterial(ref material);
            }
        }

        public void ModifyReconcilerFactory(ref SQT.Core.ReconcilerFactory reconcilerFactory)
        {
            foreach (SQT.Core.ReconcilerFactoryPlugin plugin in reconcilerFactoryPlugins)
            {
                plugin.ModifyReconcilerFactory(ref reconcilerFactory);
            }
        }
    }
}
