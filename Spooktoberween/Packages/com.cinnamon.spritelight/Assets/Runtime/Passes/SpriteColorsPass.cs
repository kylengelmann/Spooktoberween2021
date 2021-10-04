using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SpriteLightRendering
{
    public class SpriteColorPass : ScriptableRenderPass
    {
        readonly string m_PassName;
        ProfilingSampler m_ProfilingSampler;
        List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();
        readonly FilteringSettings m_FilterSettings;
        RenderStateBlock m_RenderStateBlock;
        bool m_bIsTransparent;

        RenderTargetIdentifier[] m_ColorIdentifiers;
        RenderTargetIdentifier m_DepthIdentifier;

        static readonly string BaseColorTag = "LitSpriteColor";

        public SpriteColorPass(string PassName, RenderQueueRange renderQueueRange, RenderPassEvent passEvent) : base()
        {
            renderPassEvent = passEvent;

            m_PassName = PassName;
            m_ProfilingSampler = new ProfilingSampler(m_PassName);

            m_ShaderTagIdList.Add(new ShaderTagId(BaseColorTag));

            m_FilterSettings = new FilteringSettings(renderQueueRange);
            m_FilterSettings.renderingLayerMask = 0xFFFFFFFF;
            m_FilterSettings.sortingLayerRange = SortingLayerRange.all;

            m_RenderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);

            m_bIsTransparent = renderQueueRange == RenderQueueRange.transparent;
        }

        public void Setup(RenderTargetIdentifier[] ColorTargets, RenderTargetIdentifier DepthTarget)
        {
            m_ColorIdentifiers = ColorTargets;
            m_DepthIdentifier = DepthTarget;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            ConfigureTarget(m_ColorIdentifiers, m_DepthIdentifier);
            ConfigureClear(m_bIsTransparent ? ClearFlag.None : ClearFlag.All, Color.clear);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer commandBuffer = CommandBufferPool.Get(m_PassName);
            using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
            {
                context.ExecuteCommandBuffer(commandBuffer);
                commandBuffer.Clear();

                SortingCriteria SortFlags = m_bIsTransparent ? SortingCriteria.CommonTransparent : SortingCriteria.CommonOpaque;
                DrawingSettings drawSettings = CreateDrawingSettings(m_ShaderTagIdList, ref renderingData, SortFlags);
                FilteringSettings filterSettings = m_FilterSettings;

                context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filterSettings, ref m_RenderStateBlock);
            }
            context.ExecuteCommandBuffer(commandBuffer);
            CommandBufferPool.Release(commandBuffer);
        }
    }
}
