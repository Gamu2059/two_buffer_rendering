using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Gamu2059.two_buffer_rendering
{
    public class LowDpiRenderPass : ScriptableRenderPass
    {
        private readonly ShaderTagId renderId = new ShaderTagId("UniversalForwardLowDpi");
        private readonly int lowDpiId = Shader.PropertyToID("_LowDpi");
        private readonly int lowDpiDepthId = Shader.PropertyToID("_LowDpiDepth");
        
        private LowDpiRenderFeature renderFeature;
        private Material copyMaterial;
        
        public LowDpiRenderPass(LowDpiRenderFeature renderFeature)
        {
            this.renderFeature = renderFeature;
            renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
            copyMaterial = CoreUtils.CreateEngineMaterial("Hidden/Universal Render Pipeline/Custom/CopyLowDpi");
        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get("LowDpiRender");

            var colorTarget = renderingData.cameraData.renderer.cameraColorTarget;
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.width = (int) (desc.width * renderFeature.ResolutionRate);
            desc.height = (int) (desc.height * renderFeature.ResolutionRate);
            
            cmd.GetTemporaryRT(lowDpiId, desc);
            cmd.GetTemporaryRT(lowDpiDepthId, desc.width, desc.height, desc.depthBufferBits, FilterMode.Point, RenderTextureFormat.Depth);
            cmd.SetRenderTarget(color: lowDpiId, depth: lowDpiDepthId);
            cmd.ClearRenderTarget(true, false, Color.black, 1f);
            context.ExecuteCommandBuffer(cmd);

            var cullingResults = renderingData.cullResults;
            var drawSettings = CreateDrawingSettings(renderId, ref renderingData, SortingCriteria.CommonOpaque);
            var renderQueueRange = new RenderQueueRange(0, (int) RenderQueue.GeometryLast);
            var filterSettings = new FilteringSettings(renderQueueRange, renderingData.cameraData.camera.cullingMask);
            context.DrawRenderers(cullingResults, ref drawSettings, ref filterSettings);
            
            cmd.Clear();
            cmd.SetGlobalTexture(lowDpiId, lowDpiId);
            cmd.SetGlobalTexture(lowDpiDepthId, lowDpiDepthId);
            cmd.Blit(null, colorTarget, copyMaterial);
            cmd.SetRenderTarget(colorTarget);
            cmd.ReleaseTemporaryRT(lowDpiId);
            context.ExecuteCommandBuffer(cmd);
            
            CommandBufferPool.Release(cmd);
        }
    }
}