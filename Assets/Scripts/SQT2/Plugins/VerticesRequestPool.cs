using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SQT2.Plugins
{
    public class VerticesRequestPool : SQT2.Core.Plugin, SQT2.Core.Plugin.VerticesPlugin
    {
        [Range(0f, 2000f)]
        public int delay = 250;

        Task current;

        public async Task ModifyVertices(SQT2.Core.Context context, SQT2.Core.Node node, CancellationTokenSource cancellation)
        {
            if (current == null)
            {
                current = Task.Delay(delay, cancellation.Token);
            }
            else
            {
                Task temp = current;
                current = current.ContinueWith((task) =>
                {
                    Task.Delay(delay, cancellation.Token).Wait();
                });
                await temp;
            }
        }
    }
}
