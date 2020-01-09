using System;
using UnityEngine;

namespace SQT.Plugins
{
    public class SQTPerlinDisplacementGPU : MonoBehaviour, SQT.Core.SQTPlugin, SQT.Core.SQTMaterialPlugin
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

        public void ModifyMaterial(Material material)
        {
            Texture2D gradientsTexture = PerlinTextureGenerator.CreateGradientsTexture(perlin);
            Texture2D permutationTexture = PerlinTextureGenerator.CreatePermutationTexture(perlin);
            material.SetTexture("_Gradients2D", gradientsTexture);
            material.SetTexture("_Permutation2D", permutationTexture);
        }
    }
}
