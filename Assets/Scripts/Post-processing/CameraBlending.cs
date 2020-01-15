using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(CameraBlendingRenderer), PostProcessEvent.AfterStack, "Custom/Camera blending")]
public sealed class CameraBlending : PostProcessEffectSettings { }

public sealed class CameraBlendingRenderer : PostProcessEffectRenderer<CameraBlending>
{
    public override DepthTextureMode GetCameraFlags()
    {
        return DepthTextureMode.Depth;
    }

    public override void Render(PostProcessRenderContext context)
    {
        ManyCameras manyCameras = context.camera.GetComponent<ManyCameras>();
        if (manyCameras == null || manyCameras.GetSecondaryRenderTexture() == null || manyCameras.GetTertiaryRenderTexture() == null)
        {
            return;
        }

        PropertySheet sheet = context.propertySheets.Get(Shader.Find("Hidden/Custom/Blend Transparency"));

        // context.command.SetGlobalTexture("_SecondaryCameraTexture", manyCameras.GetSecondaryRenderTexture());
        // context.command.SetGlobalTexture("_TertiaryCameraTexture", manyCameras.GetTertiaryRenderTexture());

        context.command.BlitFullscreenTriangle(manyCameras.GetTertiaryRenderTexture(), context.destination, sheet, 0);
        context.command.BlitFullscreenTriangle(manyCameras.GetSecondaryRenderTexture(), context.destination, sheet, 0);

        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}
