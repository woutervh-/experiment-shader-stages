using System;
using UnityEngine;

namespace SQT.Plugins
{
    public class PerlinDisplacement : MonoBehaviour, SQT.Core.Plugin, SQT.Core.VerticesPlugin, SQT.Core.MeshPlugin, SQT.Core.MaterialPlugin
    {
        public int seed = 0;
        public float strength = 0.1f;
        public float frequency = 1f;
        public float lacunarity = 2f;
        public float persistence = 0.5f;
        public int octaves = 8;
        public bool displaceOnGPU = false;

        public event EventHandler OnChange;

        Perlin perlin;

        public void StartPlugin()
        {
            perlin = new Perlin(seed);
        }

        public void StopPlugin() { }

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
            // return sum + 1f;
            return sum;
        }

        public void ModifyVertices(SQT.Core.Constants constants, Vector3[] vertices, Vector3[] normals)
        {
            if (displaceOnGPU)
            {
                return;
            }

            for (int i = 0; i < vertices.Length; i++)
            {
                Perlin.PerlinSample sample = GetSample(vertices[i]);
                vertices[i] += normals[i] * sample.value;
                normals[i] = (normals[i] - sample.derivative).normalized;
            }
        }

        public void ModifyMesh(SQT.Core.Constants constants, Mesh mesh, SQT.Core.Builder.Node node)
        {
            if (!displaceOnGPU)
            {
                return;
            }

            float strength = this.strength;
            float maxOffset = strength;
            for (int i = 1; i < octaves; i++)
            {
                strength *= persistence;
                maxOffset += strength;
            }

            float scale = constants.depth[node.path.Length].scale;
            Vector3 origin = constants.branch.up + constants.branch.forward * node.offset.x + constants.branch.right * node.offset.y;
            Vector3 p00 = (origin - scale * constants.branch.forward - scale * constants.branch.right).normalized;
            Vector3 p01 = (origin + scale * constants.branch.forward - scale * constants.branch.right).normalized;
            Vector3 p10 = (origin - scale * constants.branch.forward + scale * constants.branch.right).normalized;
            Vector3 p11 = (origin + scale * constants.branch.forward + scale * constants.branch.right).normalized;
            Vector3 b000 = p00 * (1f - maxOffset);
            Vector3 b001 = p01 * (1f - maxOffset);
            Vector3 b010 = p10 * (1f - maxOffset);
            Vector3 b011 = p11 * (1f - maxOffset);
            Vector3 b100 = p00 * (1f + maxOffset);
            Vector3 b101 = p01 * (1f + maxOffset);
            Vector3 b110 = p10 * (1f + maxOffset);
            Vector3 b111 = p11 * (1f + maxOffset);
            Bounds bounds = new Bounds(b000, Vector3.zero);
            bounds.Encapsulate(b001);
            bounds.Encapsulate(b010);
            bounds.Encapsulate(b011);
            bounds.Encapsulate(b100);
            bounds.Encapsulate(b101);
            bounds.Encapsulate(b110);
            bounds.Encapsulate(b111);

            mesh.bounds = bounds;
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
