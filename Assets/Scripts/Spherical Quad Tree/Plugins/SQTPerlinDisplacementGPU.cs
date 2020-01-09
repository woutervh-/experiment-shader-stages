using System;
using UnityEngine;

public class SQTPerlinDisplacementGPU : MonoBehaviour, SQTPlugin, SQTMaterialPlugin
{
    public int seed = 0;
    [Range(1f, 1e6f)]
    public float radius = 1f;

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
