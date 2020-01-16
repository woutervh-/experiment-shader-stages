using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class Atmosphere : MonoBehaviour
{
    CommandBuffer buffer;
    Camera primaryCamera;

    void Start()
    {
        buffer = new CommandBuffer();
        primaryCamera = GetComponent<Camera>();
        primaryCamera.AddCommandBuffer(CameraEvent.BeforeLighting, buffer);
    }
}
