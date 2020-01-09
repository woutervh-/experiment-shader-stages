using System;
using UnityEngine;

namespace SQT.Plugins
{
    public class PerlinDisplacement : MonoBehaviour, SQT.Core.Plugin, SQT.Core.MeshPlugin, SQT.Core.MaterialPlugin
    {
        public int seed = 0;
        public float strength = 1f;
        public float frequency = 1f;
        public float lacunarity = 2f;
        public float persistence = 0.5f;
        public int octaves = 8;
        public bool displaceOnGPU = false;

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
            return sum + 1f;
        }

        public void ModifyMesh(SQT.Core.Constants constants, Vector3[] vertices, Vector3[] normals)
        {
            if (displaceOnGPU)
            {
                return;
            }

            for (int i = 0; i < vertices.Length; i++)
            {
                Perlin.PerlinSample sample = GetSample(normals[i]);
                vertices[i] = normals[i] * sample.value;
                normals[i] = (normals[i] - sample.derivative).normalized;
            }
        }

        public void ModifyMaterial(ref Material material)
        {
            Texture2D gradientsTexture = PerlinTextureGenerator.CreateGradientsTexture(perlin);
            Texture2D permutationTexture = PerlinTextureGenerator.CreatePermutationTexture(perlin);

            SetKeyword(material, "_VERTEX_DISPLACEMENT", displaceOnGPU);
            material.SetFloat("_VertexDisplacement", displaceOnGPU ? 1f : 0f);
            material.SetTexture("_Gradients2D", gradientsTexture);
            material.SetTexture("_Permutation2D", permutationTexture);
            material.SetFloat("_Strength", strength);
            material.SetFloat("_Frequency", frequency);
            material.SetFloat("_Lacunarity", lacunarity);
            material.SetFloat("_Persistence", persistence);
            material.SetInt("_Octaves", octaves);
        }

        void SetKeyword(Material material, string keyword, bool enabled)
        {
            if (enabled)
            {
                material.EnableKeyword(keyword);
            }
            else
            {
                material.DisableKeyword(keyword);
            }
        }
    }
}
