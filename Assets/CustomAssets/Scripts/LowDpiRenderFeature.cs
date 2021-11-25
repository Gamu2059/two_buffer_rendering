using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Gamu2059.two_buffer_rendering
{
    public class LowDpiRenderFeature :ScriptableRendererFeature
    {
        [SerializeField] private float resolutionRate;

        public float ResolutionRate => resolutionRate;

        [SerializeField] private Shader shader;

        public Shader Shader => shader;

        private LowDpiRenderPass pass;
        
        public override void Create()
        {
            if (pass == null)
            {
                pass = new LowDpiRenderPass(this);
            }
        }
        
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing && pass != null)
            {
                pass = null;
            }
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(pass);
        }
    }
}