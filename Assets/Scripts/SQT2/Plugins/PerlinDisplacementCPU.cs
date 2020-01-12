using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SQT2.Plugins
{
    public class PerlinDisplacementCPU : SQT2.Core.Plugin, SQT2.Core.Plugin.VerticesPlugin
    {
        public int seed = 0;
        public float strength = 0.1f;
        public float frequency = 1f;
        public float lacunarity = 2f;
        public float persistence = 0.5f;
        public int octaves = 8;

        Perlin perlin;

        public override void OnPluginStart()
        {
            perlin = new Perlin(seed);
        }

        public Task ModifyVertices(SQT2.Core.Context context, SQT2.Core.Node node, CancellationTokenSource cancellation)
        {
            return Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < node.positions.Length; i++)
                {
                    Perlin.PerlinSample sample = GetSample(node.positions[i]);
                    node.positions[i] += node.normals[i] * sample.value;
                    node.normals[i] = (node.normals[i] - sample.derivative).normalized;
                }
            });
        }

        Perlin.PerlinSample GetSample(Vector3 position, float frequency)
        {
            Perlin.PerlinSample sample = perlin.Sample(position * frequency);
            sample.derivative *= frequency;
            return sample;
        }

        Perlin.PerlinSample GetSample(Vector3 position)
        {
            float strength = this.strength;
            float frequency = this.frequency;

            Perlin.PerlinSample sum = GetSample(position, frequency) * strength;
            for (int i = 1; i < octaves; i++)
            {
                strength *= persistence;
                frequency *= lacunarity;
                Perlin.PerlinSample sample = GetSample(position, frequency) * strength;
                sum += sample;
            }
            return sum;
        }
    }
}
