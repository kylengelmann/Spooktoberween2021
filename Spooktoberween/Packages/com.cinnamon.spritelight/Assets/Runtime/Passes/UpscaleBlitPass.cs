using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SpriteLightRendering
{
    public class UpscaleBlitPass : ScriptableRenderPass
    {
        const string mProfilerTag = "Upscale Blit Pass";
        ProfilingSampler m_ProfilingSampler;
        Material mBlitMaterial;

        RenderTargetIdentifier m_BlitSource;

        static readonly int s_BlitSourceID = Shader.PropertyToID("_BlitTex");
        static readonly int s_SrcBlendModeID = Shader.PropertyToID("_SrcBlendMode");
        static readonly int s_DestBlendModeID = Shader.PropertyToID("_DestBlendMode");

        public UpscaleBlitPass(RenderPassEvent evt, BlendMode SrcBlendMode, BlendMode DestBlendMode)
        {
            mBlitMaterial = new Material(Shader.Find("SpriteLight/UpscaleBlit"));
            mBlitMaterial.SetInt(s_SrcBlendModeID, (int)SrcBlendMode);
            mBlitMaterial.SetInt(s_DestBlendModeID, (int)DestBlendMode);

            renderPassEvent = evt;

            m_ProfilingSampler = new ProfilingSampler(mProfilerTag);
        }

        public void Setup(RenderTargetHandle BlitSource)
        {
            m_BlitSource = BlitSource.Identifier();
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(mProfilerTag);
            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                cmd.SetGlobalTexture(s_BlitSourceID , m_BlitSource);
                cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, mBlitMaterial);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}
