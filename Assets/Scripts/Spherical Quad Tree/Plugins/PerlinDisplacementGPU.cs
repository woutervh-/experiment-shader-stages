using System;
using UnityEngine;

namespace SQT.Plugins
{
    public class PerlinDisplacementGPU : MonoBehaviour, SQT.Core.Plugin, SQT.Core.MaterialPlugin, SQT.Core.ReconcilerFactoryPlugin
    {
        public Material material;
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

        public void ModifyMaterial(ref Material material)
        {
            Texture2D gradientsTexture = PerlinTextureGenerator.CreateGradientsTexture(perlin);
            Texture2D permutationTexture = PerlinTextureGenerator.CreatePermutationTexture(perlin);
            material = this.material;
            material.SetTexture("_Gradients2D", gradientsTexture);
            material.SetTexture("_Permutation2D", permutationTexture);
        }

        public void ModifyReconcilerFactory(ref SQT.Core.ReconcilerFactory reconcilerFactory)
        {
            reconcilerFactory = new SQT.Core.GPU.Reconciler.Factory();
        }
    }
}
