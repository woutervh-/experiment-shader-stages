using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.LWRP;

public class AtmosphereRenderPass : ScriptableRendererFeature
{
    private RenderPass renderPass;

    public override void Create()
    {
        renderPass = new RenderPass(RenderTargetHandle.CameraTarget);
        renderPass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(renderPass);
    }

    public class RenderPass : ScriptableRenderPass
    {

        public RenderPass(RenderTargetHandle colorHandle)
        {
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer commandBuffer = CommandBufferPool.Get("Atmosphere");

            using (new ProfilingSample(commandBuffer, commandBuffer.name))
            {
            }

            context.ExecuteCommandBuffer(commandBuffer);
            CommandBufferPool.Release(commandBuffer);
        }

    }
}
