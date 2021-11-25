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
        private Mesh copyMesh;
        private Material copyMaterial;

        public LowDpiRenderPass(LowDpiRenderFeature renderFeature)
        {
            this.renderFeature = renderFeature;
            renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
            CreateMesh();
            CreateMaterial();
        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CreateMesh();
            CreateMaterial();
            
            var cmd = CommandBufferPool.Get("LowDpiRender");

            var camera = renderingData.cameraData.camera;
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
            cmd.SetRenderTarget(colorTarget);
            cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
            cmd.DrawMesh(copyMesh, Matrix4x4.identity, copyMaterial);
            cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);
            cmd.ReleaseTemporaryRT(lowDpiId);
            context.ExecuteCommandBuffer(cmd);
            
            CommandBufferPool.Release(cmd);
        }

        private void CreateMesh()
        {
            if (copyMesh == null)
            {
                copyMesh = new Mesh();
                copyMesh.vertices = new[]
                {
                    new Vector3(-1, -1, 0),
                    new Vector3(-1, 1, 0),
                    new Vector3(1, 1, 0),
                    new Vector3(1, -1, 0),
                };
                copyMesh.uv = new[]
                {
                    new Vector2(0, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 1),
                    new Vector2(1, 0),
                };
                copyMesh.triangles = new[]
                {
                    0, 1, 2, 0, 2, 3,
                };
            }
        }

        private void CreateMaterial()
        {
            if (copyMaterial == null)
            {
                copyMaterial = CoreUtils.CreateEngineMaterial(renderFeature.Shader);
            }
        }
    }
}