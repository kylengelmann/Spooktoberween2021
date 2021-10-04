using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SpriteLightRendering
{
    public class EditorNormalsDiffusePass : ScriptableRenderPass
    {
#if UNITY_EDITOR
        readonly string m_PassName;
        ProfilingSampler m_ProfilingSampler;
        List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();
        readonly FilteringSettings m_FilterSettings;
        RenderStateBlock m_RenderStateBlock;
        bool m_bIsTransparent;

        RenderTargetIdentifier[] m_ColorIdentifiers;
        RenderTargetIdentifier m_DepthIdentifier;

        Material m_EditorNormalMaterial;

        static readonly string NormalsPassTag = "NormalsPass";
        static readonly string NormalsDiffuseShaderName = "Hidden/SpriteLight/EditorNormalsDiffuse";
#endif // UNITY_EDITOR

        public EditorNormalsDiffusePass(string PassName, RenderQueueRange renderQueueRange, RenderPassEvent passEvent) : base()
        {
#if UNITY_EDITOR
            renderPassEvent = passEvent;

            m_PassName = PassName;
            m_ProfilingSampler = new ProfilingSampler(m_PassName);

            m_ShaderTagIdList.Add(new ShaderTagId(NormalsPassTag));

            m_FilterSettings = new FilteringSettings(renderQueueRange);
            m_FilterSettings.renderingLayerMask = 0xFFFFFFFF;
            m_FilterSettings.sortingLayerRange = SortingLayerRange.all;

            m_RenderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);

            m_bIsTransparent = renderQueueRange == RenderQueueRange.transparent;

            Shader EditorNormalsDiffuseShader = Shader.Find(NormalsDiffuseShaderName);
            if (EditorNormalsDiffuseShader)
            {
                m_EditorNormalMaterial = new Material(EditorNormalsDiffuseShader);
            }
#endif // UNITY_EDITOR
        }

        public void Setup(RenderTargetIdentifier[] ColorTargets, RenderTargetIdentifier DepthTarget)
        {
#if UNITY_EDITOR
            m_ColorIdentifiers = ColorTargets;
            m_DepthIdentifier = DepthTarget;
#endif // UNITY_EDITOR
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
#if UNITY_EDITOR
            ConfigureTarget(m_ColorIdentifiers, m_DepthIdentifier);
            ConfigureClear(ClearFlag.None, Color.clear);
#endif // UNITY_EDITOR
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
#if UNITY_EDITOR
            CommandBuffer commandBuffer = CommandBufferPool.Get(m_PassName);
            using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
            {
                context.ExecuteCommandBuffer(commandBuffer);
                commandBuffer.Clear();

                SortingCriteria SortFlags = m_bIsTransparent ? SortingCriteria.CommonTransparent : SortingCriteria.CommonOpaque;
                DrawingSettings drawSettings = CreateDrawingSettings(m_ShaderTagIdList, ref renderingData, SortFlags);
                drawSettings.overrideMaterial = m_EditorNormalMaterial;
                FilteringSettings filterSettings = m_FilterSettings;

                context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filterSettings, ref m_RenderStateBlock);
            }
            context.ExecuteCommandBuffer(commandBuffer);
            CommandBufferPool.Release(commandBuffer);
#endif // UNITY_EDITOR
        }
    }
}