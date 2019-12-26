using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(BlurRenderer), PostProcessEvent.AfterStack, "Custom/Blur")]
public sealed class Blur : PostProcessEffectSettings {
    
}

public sealed class BlurRenderer : PostProcessEffectRenderer<Blur>
{
    public override void Render(PostProcessRenderContext context)
    {
        RenderTexture intermediate = RenderTexture.GetTemporary(context.width / 8, context.height / 8, 0, context.sourceFormat);
        context.command.BlitFullscreenTriangle(context.source, intermediate);
        context.command.BlitFullscreenTriangle(intermediate, context.destination);
    }
}
