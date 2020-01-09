using System;
using UnityEngine;

namespace SQT.Plugins
{
    public class SQTPerlinDisplacementCPU : MonoBehaviour, SQT.Core.SQTPlugin, SQT.Core.SQTMeshPlugin
    {
        public int seed = 0;

        public event EventHandler OnChange;

        Perlin perlin;

        void Start()
        {
            perlin = new Perlin(seed);
        }

        void OnValidate()
        {
            perlin = new Perlin(seed);

            if (OnChange != null)
            {
                OnChange.Invoke(this, EventArgs.Empty);
            }
        }

        Perlin.PerlinSample GetSample(Vector3 position, float frequency)
        {
            Perlin.PerlinSample sample = perlin.Sample(position * frequency);
            sample.derivative *= frequency;
            return sample;
        }

        Perlin.PerlinSample GetSample(Vector3 position)
        {
            float strength = 1f;
            float frequency = 1f;
            float lacunarity = 2f;
            float persistence = 0.5f;
            int octaves = 8;

            Perlin.PerlinSample sum = GetSample(position, frequency) * strength;
            for (int i = 1; i < octaves; i++)
            {
                strength *= persistence;
                frequency *= lacunarity;
                Perlin.PerlinSample sample = GetSample(position, frequency) * strength;
                sum += sample;
            }
            return sum + 1f;
        }

        public void ModifyMesh(Vector3[] vertices, Vector3[] normals)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                Perlin.PerlinSample sample = GetSample(normals[i]);
                vertices[i] = normals[i] * sample.value;
                normals[i] = (normals[i] - sample.derivative).normalized;
            }
        }
    }
}
