using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class ManyCameras : MonoBehaviour
{
    [SerializeField]
    [HideInInspector]
    Camera primaryCamera;
    [SerializeField]
    [HideInInspector]
    CameraClearFlags originalClearFlags;
    [SerializeField]
    [HideInInspector]
    Camera secondaryCamera;
    [SerializeField]
    [HideInInspector]
    Camera tertiaryCamera;
    [SerializeField]
    [HideInInspector]
    GameObject secondaryGameObject;
    [SerializeField]
    [HideInInspector]
    GameObject tertiaryGameObject;
    RenderTexture secondaryRenderTexture;
    RenderTexture tertiaryRenderTexture;

    public RenderTexture GetSecondaryRenderTexture()
    {
        return secondaryRenderTexture;
    }

    public RenderTexture GetTertiaryRenderTexture()
    {
        return tertiaryRenderTexture;
    }

    void Start()
    {
        if (primaryCamera == null)
        {
            primaryCamera = GetComponent<Camera>();
            originalClearFlags = primaryCamera.clearFlags;
            primaryCamera.clearFlags = CameraClearFlags.Nothing;
        }
        if (secondaryGameObject == null)
        {
            secondaryGameObject = new GameObject(primaryCamera.name + " (secondary)");
            secondaryGameObject.transform.parent = transform;
        }
        if (tertiaryGameObject == null)
        {
            tertiaryGameObject = new GameObject(primaryCamera.name + " (tertiary)");
            tertiaryGameObject.transform.parent = transform;
        }
        if (secondaryCamera == null)
        {
            secondaryCamera = secondaryGameObject.AddComponent<Camera>();
            secondaryCamera.CopyFrom(primaryCamera);
            secondaryCamera.depth = primaryCamera.depth - 1;
            secondaryCamera.clearFlags = CameraClearFlags.Nothing;
        }
        if (tertiaryCamera == null)
        {
            tertiaryCamera = tertiaryGameObject.AddComponent<Camera>();
            tertiaryCamera.CopyFrom(primaryCamera);
            tertiaryCamera.depth = secondaryCamera.depth - 1;
            tertiaryCamera.clearFlags = originalClearFlags;
        }
        if (secondaryRenderTexture != null)
        {
            secondaryRenderTexture.Release();
        }
        secondaryRenderTexture = new RenderTexture(Screen.width, Screen.height, 16);
        secondaryCamera.targetTexture = secondaryRenderTexture;
        if (tertiaryRenderTexture != null)
        {
            tertiaryRenderTexture.Release();
        }
        tertiaryRenderTexture = new RenderTexture(Screen.width, Screen.height, 16);
        tertiaryCamera.targetTexture = tertiaryRenderTexture;
    }

    void Update()
    {
        if (secondaryRenderTexture.width != Screen.width || secondaryRenderTexture.height != Screen.height)
        {
            secondaryRenderTexture.Release();
            secondaryRenderTexture = new RenderTexture(Screen.width, Screen.height, 16);
            secondaryCamera.targetTexture = secondaryRenderTexture;
        }
        if (tertiaryRenderTexture.width != Screen.width || tertiaryRenderTexture.height != Screen.height)
        {
            tertiaryRenderTexture.Release();
            tertiaryRenderTexture = new RenderTexture(Screen.width, Screen.height, 16);
            tertiaryCamera.targetTexture = tertiaryRenderTexture;
        }

        secondaryCamera.fieldOfView = primaryCamera.fieldOfView;
        tertiaryCamera.fieldOfView = primaryCamera.fieldOfView;

        secondaryCamera.nearClipPlane = primaryCamera.farClipPlane;
        secondaryCamera.farClipPlane = primaryCamera.farClipPlane * primaryCamera.farClipPlane / primaryCamera.nearClipPlane;

        tertiaryCamera.nearClipPlane = secondaryCamera.farClipPlane;
        tertiaryCamera.farClipPlane = secondaryCamera.farClipPlane * secondaryCamera.farClipPlane / secondaryCamera.nearClipPlane;
    }

    void OnDestroy()
    {
        if (secondaryRenderTexture != null)
        {
            secondaryRenderTexture.Release();
        }
        if (tertiaryRenderTexture != null)
        {
            tertiaryRenderTexture.Release();
        }
        primaryCamera.clearFlags = originalClearFlags;
        DestroyImmediate(secondaryGameObject);
        DestroyImmediate(tertiaryGameObject);
    }
}
