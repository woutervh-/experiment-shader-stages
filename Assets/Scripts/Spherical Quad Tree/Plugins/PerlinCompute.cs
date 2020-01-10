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
        Texture2D gradientsTexture;
        Texture2D permutationTexture;
        ComputeBuffer positionBuffer;
        ComputeBuffer normalBuffer;
        int generateMeshKernel;

        public void StartPlugin()
        {
            generateMeshKernel = meshGeneratorShader.FindKernel("GenerateMesh");

            perlin = new Perlin(seed);
            gradientsTexture = PerlinTextureGenerator.CreateGradientsTexture(perlin);
            permutationTexture = PerlinTextureGenerator.CreatePermutationTexture(perlin);
        }

        public void StopPlugin()
        {
            if (positionBuffer != null)
            {
                positionBuffer.Release();
                positionBuffer = null;
            }
            if (normalBuffer != null)
            {
                normalBuffer.Release();
                normalBuffer = null;
            }
            UnityEngine.Object.Destroy(gradientsTexture);
            UnityEngine.Object.Destroy(permutationTexture);
        }

        void OnValidate()
        {
            if (OnChange != null)
            {
                OnChange.Invoke(this, EventArgs.Empty);
            }
        }

        public void ModifyVertices(SQT.Core.Constants constants, Vector3[] vertices, Vector3[] normals)
        {
            if (positionBuffer == null || positionBuffer.count != vertices.Length)
            {
                if (positionBuffer != null)
                {
                    positionBuffer.Release();
                }
                positionBuffer = new ComputeBuffer(vertices.Length, 3 * 4);
            }
            if (normalBuffer == null || normalBuffer.count != normals.Length)
            {
                if (normalBuffer != null)
                {
                    normalBuffer.Release();
                }
                normalBuffer = new ComputeBuffer(normals.Length, 3 * 4);
            }

            positionBuffer.SetData(vertices);

            meshGeneratorShader.SetBuffer(generateMeshKernel, "positionBuffer", positionBuffer);
            meshGeneratorShader.SetBuffer(generateMeshKernel, "normalBuffer", normalBuffer);
            meshGeneratorShader.SetTexture(generateMeshKernel, "_Gradients2D", gradientsTexture);
            meshGeneratorShader.SetTexture(generateMeshKernel, "_Permutation2D", permutationTexture);
            meshGeneratorShader.SetFloat("_Strength", strength);
            meshGeneratorShader.SetFloat("_Frequency", frequency);
            meshGeneratorShader.SetFloat("_Lacunarity", lacunarity);
            meshGeneratorShader.SetFloat("_Persistence", persistence);
            meshGeneratorShader.SetInt("_Octaves", octaves);

            meshGeneratorShader.Dispatch(generateMeshKernel, Mathf.CeilToInt(vertices.Length / 64f), 1, 1);

            positionBuffer.GetData(vertices);
            normalBuffer.GetData(normals);
        }

        public void ModifyMaterial(ref Material material)
        {
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
