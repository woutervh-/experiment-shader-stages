using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SQT2.Core
{
    public abstract class Plugin : MonoBehaviour
    {
        public event EventHandler OnChange;

        void OnValidate()
        {
            if (OnChange != null)
            {
                OnChange.Invoke(this, EventArgs.Empty);
            }
        }

        public void OnPluginStart()
        {
            //
        }

        public void OnPluginStop()
        {
            //
        }

        public interface VerticesPlugin
        {
            Task ModifyVertices(Context context, Node node, CancellationTokenSource cancellation);
        }

        public class PluginChain
        {
            VerticesPlugin[] verticesPlugins;

            public PluginChain(Plugin[] plugins)
            {
                verticesPlugins = GetPlugins<VerticesPlugin>(plugins);
            }

            static T[] GetPlugins<T>(Plugin[] plugins)
            {
                List<T> matches = new List<T>();
                foreach (Plugin plugin in plugins)
                {
                    if (plugin is T p)
                    {
                        matches.Add(p);
                    }
                }
                return matches.ToArray();
            }

            public async Task ModifyVertices(Context context, Node node, CancellationTokenSource cancellation)
            {
                foreach (VerticesPlugin plugin in verticesPlugins)
                {
                    await plugin.ModifyVertices(context, node, cancellation);
                }
            }
        }
    }
}
