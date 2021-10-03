using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SpriteLightRendering
{

    public class NormalsPass : ScriptableRenderPass
    {
        readonly string m_PassName;
        ProfilingSampler m_ProfilingSampler;
        List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();
        readonly FilteringSettings m_FilterSettings;
        RenderStateBlock m_RenderStateBlock;
        Material m_LightMat;

        RenderTargetHandle m_RenderTargetHandle;
        RenderTargetHandle m_DepthHandle;

        static readonly string NormalsTag = "NormalsPass";

        public NormalsPass(string PassName, RenderQueueRange renderQueueRange, RenderPassEvent passEvent, LayerMask layerMask) : base()
        {
            m_PassName = PassName;
            m_ProfilingSampler = new ProfilingSampler(m_PassName);

            m_ShaderTagIdList.Add(new ShaderTagId(NormalsTag));

            m_FilterSettings = new FilteringSettings(renderQueueRange);

            m_RenderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
        }

        public void Setup(RenderTargetHandle ColorHandle, RenderTargetHandle DepthHandle)
        {
            m_RenderTargetHandle = ColorHandle;
            m_DepthHandle = DepthHandle;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            ConfigureTarget(m_RenderTargetHandle.Identifier(), m_DepthHandle.Identifier());
            ConfigureClear(ClearFlag.All, Color.clear);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer commandBuffer = CommandBufferPool.Get(m_PassName);
            using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
            {
                context.ExecuteCommandBuffer(commandBuffer);
                commandBuffer.Clear();

                SortingCriteria sortFlags = SortingCriteria.CommonOpaque;
                DrawingSettings drawSettings = CreateDrawingSettings(m_ShaderTagIdList, ref renderingData, sortFlags);
                FilteringSettings filterSettings = m_FilterSettings;

                context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filterSettings, ref m_RenderStateBlock);
            }
            context.ExecuteCommandBuffer(commandBuffer);
            CommandBufferPool.Release(commandBuffer);
        }
    }
}
