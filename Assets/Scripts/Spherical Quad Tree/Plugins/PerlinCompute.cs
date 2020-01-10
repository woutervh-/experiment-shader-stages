using System;
using UnityEngine;

namespace SQT.Plugins
{
    public class PerlinCompute : MonoBehaviour, SQT.Core.Plugin, SQT.Core.VerticesPlugin, SQT.Core.MaterialPlugin
    {
        public int seed = 0;
        public float strength = 1f;
        public float frequency = 1f;
        public float lacunarity = 2f;
        public float persistence = 0.5f;
        public int octaves = 8;
        public ComputeShader meshGeneratorShader;

        public event EventHandler OnChange;

        Perlin perlin;
        ComputeBuffer positionBuffer;
        ComputeBuffer normalBuffer;

        void Start()
        {
            positionBuffer = new ComputeBuffer()
            perlin = new Perlin(seed);
        }

        void OnDestroy()
        {
            positionBuffer.Release();
            normalBuffer.Release();
        }

        void OnValidate()
        {
            perlin = new Perlin(seed);

            if (OnChange != null)
            {
                OnChange.Invoke(this, EventArgs.Empty);
            }
        }

        public void ModifyVertices(SQT.Core.Constants constants, Vector3[] vertices, Vector3[] normals)
        {

        }

        public void ModifyMaterial(ref Material material)
        {
            Texture2D gradientsTexture = PerlinTextureGenerator.CreateGradientsTexture(perlin);
            Texture2D permutationTexture = PerlinTextureGenerator.CreatePermutationTexture(perlin);

            SetKeyword(material, "_VERTEX_DISPLACEMENT", false);
            material.SetFloat("_VertexDisplacement", 0f);
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
