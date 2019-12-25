using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class ManyCameras : MonoBehaviour
{
    Camera primaryCamera;
    Camera secondaryCamera;
    Camera tertiaryCamera;
    GameObject secondaryGameObject;
    GameObject tertiaryGameObject;
    // RenderTexture secondaryRenderTexture;
    // RenderTexture tertiaryRenderTexture;

    void Start()
    {
        primaryCamera = GetComponent<Camera>();
        secondaryGameObject = new GameObject(primaryCamera.name + " (secondary)");
        tertiaryGameObject = new GameObject(primaryCamera.name + " (tertiary)");
        secondaryGameObject.transform.parent = transform;
        tertiaryGameObject.transform.parent = transform;
        secondaryCamera = secondaryGameObject.AddComponent<Camera>();
        tertiaryCamera = tertiaryGameObject.AddComponent<Camera>();
        secondaryCamera.CopyFrom(primaryCamera);
        tertiaryCamera.CopyFrom(primaryCamera);
        secondaryCamera.depth = secondaryCamera.depth - 1;
        tertiaryCamera.depth = primaryCamera.depth - 1;

        primaryCamera.clearFlags = CameraClearFlags.Nothing;
        secondaryCamera.clearFlags = CameraClearFlags.Nothing;
        tertiaryCamera.clearFlags = CameraClearFlags.Skybox;
        // secondaryRenderTexture = new RenderTexture(Screen.width, Screen.height, 16);
        // tertiaryRenderTexture = new RenderTexture(Screen.width, Screen.height, 16);
        // secondaryCamera.targetTexture = secondaryRenderTexture;
        // tertiaryCamera.targetTexture = tertiaryRenderTexture;
    }

    void Update()
    {
        secondaryCamera.fieldOfView = primaryCamera.fieldOfView;
        tertiaryCamera.fieldOfView = primaryCamera.fieldOfView;

        secondaryCamera.nearClipPlane = primaryCamera.farClipPlane;
        secondaryCamera.farClipPlane = primaryCamera.farClipPlane * primaryCamera.farClipPlane / primaryCamera.nearClipPlane;

        tertiaryCamera.nearClipPlane = secondaryCamera.farClipPlane;
        tertiaryCamera.farClipPlane = secondaryCamera.farClipPlane * secondaryCamera.farClipPlane / secondaryCamera.nearClipPlane;
    }

    // void OnRenderImage(RenderTexture source, RenderTexture destination)
    // {
    //     Graphics.Blit(tertiaryRenderTexture, destination);
    //     Graphics.Blit(secondaryRenderTexture, destination);
    //     Graphics.Blit(source, destination);
    // }

    void OnDestroy()
    {
        DestroyImmediate(secondaryGameObject);
        DestroyImmediate(tertiaryGameObject);
        // DestroyImmediate(secondaryRenderTexture);
        // DestroyImmediate(tertiaryRenderTexture);
    }
}
