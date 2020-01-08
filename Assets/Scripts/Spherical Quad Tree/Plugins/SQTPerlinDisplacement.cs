using System;
using UnityEngine;

public class SQTPerlinDisplacement : MonoBehaviour, SQTPlugin, SQTApproximateEdgeLengthPlugin, SQTMeshPlugin, SQTDistanceToObjectPlugin
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

    Perlin.PerlinSample GetSample(Vector3 position, float frequency)
    {
        Perlin.PerlinSample sample = perlin.Sample(position * frequency);
        sample.derivative *= frequency;
        return sample;
    }

    Perlin.PerlinSample GetSample(Vector3 position)
    {
        float strength = 0.28f;
        float frequency = 1f;
        float lacunarity = 2.3f;
        float persistence = 0.4f;
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
            normals[i] = vertices[i].normalized;
            Perlin.PerlinSample sample = GetSample(normals[i]);
            vertices[i] = normals[i] * radius * sample.value;
            normals[i] = (normals[i] - sample.derivative).normalized;
        }
    }

    public void ModifyApproximateEdgeLength(ref float edgeLength)
    {
        edgeLength *= radius;
    }

    public void ModifyDistanceToObject(ref float distance)
    {
        distance = Mathf.Abs(distance - radius);
    }
}
