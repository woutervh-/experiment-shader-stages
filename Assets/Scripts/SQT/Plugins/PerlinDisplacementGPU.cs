using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace SQT.Plugins
{
    public class PerlinDisplacementGPU : SQT.Core.Plugin, SQT.Core.Plugin.VerticesPlugin
    {
        public int seed = 0;
        public float strength = 0.1f;
        public float frequency = 1f;
        public float lacunarity = 2f;
        public float persistence = 0.5f;
        public int octaves = 8;
        public ComputeShader computeShader;

        Perlin perlin;
        Texture2D gradientsTexture;
        Texture2D permutationTexture;
        ComputeBuffer positionBuffer;
        ComputeBuffer normalBuffer;
        int computeKernel;

        public override void OnPluginStart()
        {
            perlin = new Perlin(seed);
            gradientsTexture = PerlinTextureGenerator.CreateGradientsTexture(perlin);
            permutationTexture = PerlinTextureGenerator.CreatePermutationTexture(perlin);
            computeKernel = computeShader.FindKernel("GenerateMesh");
        }

        public override void OnPluginStop()
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

        public async Task ModifyVertices(SQT.Core.Context context, SQT.Core.Node node, CancellationTokenSource cancellation)
        {
            if (positionBuffer == null || positionBuffer.count != node.positions.Length)
            {
                if (positionBuffer != null)
                {
                    positionBuffer.Release();
                }
                positionBuffer = new ComputeBuffer(node.positions.Length, 3 * 4);
            }
            if (normalBuffer == null || normalBuffer.count != node.normals.Length)
            {
                if (normalBuffer != null)
                {
                    normalBuffer.Release();
                }
                normalBuffer = new ComputeBuffer(node.normals.Length, 3 * 4);
            }

            positionBuffer.SetData(node.positions);
            normalBuffer.SetData(node.normals);

            computeShader.SetBuffer(computeKernel, "positionBuffer", positionBuffer);
            computeShader.SetBuffer(computeKernel, "normalBuffer", normalBuffer);
            computeShader.SetTexture(computeKernel, "_Gradients2D", gradientsTexture);
            computeShader.SetTexture(computeKernel, "_Permutation2D", permutationTexture);
            computeShader.SetFloat("_Strength", strength);
            computeShader.SetFloat("_Frequency", frequency);
            computeShader.SetFloat("_Lacunarity", lacunarity);
            computeShader.SetFloat("_Persistence", persistence);
            computeShader.SetInt("_Octaves", octaves);

            uint x, y, z;
            computeShader.GetKernelThreadGroupSizes(computeKernel, out x, out y, out z);
            float total = x * y * z;
            computeShader.Dispatch(computeKernel, Mathf.CeilToInt(node.positions.Length / total), 1, 1);

            Task positionsTask = new Task(() => { });
            Task normalsTask = new Task(() => { });
            Action<AsyncGPUReadbackRequest> positionsAction = new Action<AsyncGPUReadbackRequest>((request) =>
            {
                node.positions = request.GetData<Vector3>().ToArray();
                positionsTask.Start();
            });
            Action<AsyncGPUReadbackRequest> normalsAction = new Action<AsyncGPUReadbackRequest>((request) =>
            {
                node.normals = request.GetData<Vector3>().ToArray();
                normalsTask.Start();
            });

            AsyncGPUReadback.Request(positionBuffer, positionsAction);
            AsyncGPUReadback.Request(normalBuffer, normalsAction);

            await positionsTask;
            await normalsTask;
        }
    }
}
