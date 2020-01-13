using UnityEngine;

namespace SQT2.Plugins
{
    public class PerlinMaterialProperties : SQT2.Core.Plugin, SQT2.Core.Plugin.MaterialPlugin
    {
        public int seed = 0;
        public float strength = 0.1f;
        public float frequency = 1f;
        public float lacunarity = 2f;
        public float persistence = 0.5f;
        public int octaves = 8;

        Perlin perlin;
        Texture2D gradientsTexture;
        Texture2D permutationTexture;

        public override void OnPluginStart()
        {
            perlin = new Perlin(seed);
            gradientsTexture = PerlinTextureGenerator.CreateGradientsTexture(perlin);
            permutationTexture = PerlinTextureGenerator.CreatePermutationTexture(perlin);
        }

        public override void OnPluginStop()
        {
            UnityEngine.Object.Destroy(gradientsTexture);
            UnityEngine.Object.Destroy(permutationTexture);
        }

        public void ModifyMaterial(SQT2.Core.Context context, Material material)
        {
            material.SetTexture("_Gradients2D", gradientsTexture);
            material.SetTexture("_Permutation2D", permutationTexture);
            material.SetFloat("_Strength", strength);
            material.SetFloat("_Frequency", frequency);
            material.SetFloat("_Lacunarity", lacunarity);
            material.SetFloat("_Persistence", persistence);
            material.SetInt("_Octaves", octaves);
        }
    }
}
