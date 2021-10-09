using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SpriteLightRendering
{
    public class VisibilityPass : ScriptableRenderPass
    {
        const int NUM_CUBE_SIDES = 6;
        const int MAP_RES = 2048;
        const int SLICE_RES = 512;

        struct PointLightShadowData
        {
            public int lightIndex;
            public ShadowSliceData[] shadowSlices;
            public ShadowSplitData[] shadowSplits;
        }

        PointLightShadowData m_VisibilityShadowData;
        PointLightShadowData m_ShadowShadowData;

        Matrix4x4[] m_VisibilityTransforms = new Matrix4x4[NUM_CUBE_SIDES];
        Matrix4x4[] m_ShadowTransforms = new Matrix4x4[NUM_CUBE_SIDES];
        Vector4 m_LightPosition;

        RenderTexture m_ShadowTexture;
        RenderTargetHandle m_ShadowHandle;

        bool bFoundVisibilityLight = false;
        ProfilingSampler m_ProfilingSampler;

        static readonly int VisibilityWorldToShadowID = Shader.PropertyToID("_VisibilityWorldToShadow");
        static readonly int VisibilityShadowWorldToShadowID = Shader.PropertyToID("_VisibilityShadowWorldToShadow");
        static readonly string PassName = "VisibilityShadows";

        public VisibilityPass(RenderPassEvent evt) : base()
        {
            renderPassEvent = evt;

            m_ProfilingSampler = new ProfilingSampler(PassName);

            m_ShadowHandle.Init("_VisibilityShadowTexture");

            m_VisibilityShadowData.shadowSlices = new ShadowSliceData[NUM_CUBE_SIDES];
            m_VisibilityShadowData.shadowSplits = new ShadowSplitData[NUM_CUBE_SIDES];

            m_ShadowShadowData.shadowSlices = new ShadowSliceData[NUM_CUBE_SIDES];
            m_ShadowShadowData.shadowSplits = new ShadowSplitData[NUM_CUBE_SIDES];
        }

        public void Setup(ref RenderingData renderingData, out int LightIndex)
        {
            int numVisibleLights = renderingData.lightData.visibleLights.Length;

            bFoundVisibilityLight = false;
            for (int i = 0; i < numVisibleLights; ++i)
            {
                if (i == renderingData.lightData.mainLightIndex)
                {
                    continue;
                }

                VisibleLight light = renderingData.lightData.visibleLights[i];

                if (light.lightType != LightType.Point || light.light.shadows == LightShadows.None)
                {
                    continue;
                }

                if(light.light.CompareTag(ShadowUtils.GetVisibilityLightTag()))
                {
                    if (renderingData.cullResults.GetShadowCasterBounds(i, out var bounds))
                    {
                        m_VisibilityShadowData.lightIndex = i;

                        for (int j = 0; j < NUM_CUBE_SIDES; ++j)
                        {
                            ShadowUtils.GetPointLightShadowParams(ref renderingData.cullResults, ref renderingData.shadowData, i, 0, (CubemapFace)j, MAP_RES, MAP_RES, SLICE_RES, .2f,
                                out m_VisibilityShadowData.shadowSlices[j], out m_VisibilityShadowData.shadowSplits[j]);
                            m_VisibilityTransforms[j] = m_VisibilityShadowData.shadowSlices[j].shadowTransform;
                        }

                        m_ShadowShadowData.lightIndex = i;

                        for (int j = 0; j < NUM_CUBE_SIDES; ++j)
                        {
                            ShadowUtils.GetPointLightShadowParams(ref renderingData.cullResults, ref renderingData.shadowData, i, 1, (CubemapFace)j, MAP_RES, MAP_RES, SLICE_RES, .2f,
                                out m_ShadowShadowData.shadowSlices[j], out m_ShadowShadowData.shadowSplits[j]);
                            m_ShadowTransforms[j] = m_ShadowShadowData.shadowSlices[j].shadowTransform;
                        }

                        bFoundVisibilityLight = true;
                        LightIndex = i;
                        return;
                    }
                }
            }

            LightIndex = -1;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            m_ShadowTexture = UnityEngine.Rendering.Universal.ShadowUtils.GetTemporaryShadowTexture(MAP_RES, MAP_RES, 16);
            ConfigureTarget(new RenderTargetIdentifier(m_ShadowTexture));
            ConfigureClear(ClearFlag.All, Color.black);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer commandBuffer = CommandBufferPool.Get(PassName);
            using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
            {
                context.ExecuteCommandBuffer(commandBuffer);
                commandBuffer.Clear();
                //context.ExecuteCommandBuffer(commandBuffer);
                if(bFoundVisibilityLight)
                {
                    int lightIdx = m_VisibilityShadowData.lightIndex;
                    VisibleLight visibleLight = renderingData.lightData.visibleLights[lightIdx];

                    for (int j = 0; j < NUM_CUBE_SIDES; ++j)
                    {
                        ShadowDrawingSettings settings = new ShadowDrawingSettings(renderingData.cullResults, lightIdx);

                        settings.useRenderingLayerMaskTest = true;
                        settings.splitData = m_VisibilityShadowData.shadowSplits[j];
                        Vector4 shadowBias = ShadowUtils.GetShadowBias(ref visibleLight, lightIdx, ref renderingData.shadowData, m_VisibilityShadowData.shadowSlices[j].projectionMatrix, SLICE_RES);
                        ShadowUtils.SetupPointLightShadowCasterConstantBuffer(commandBuffer, (CubemapFace)j, shadowBias);
                        UnityEngine.Rendering.Universal.ShadowUtils.RenderShadowSlice(commandBuffer, ref context, ref m_VisibilityShadowData.shadowSlices[j], ref settings);
                    }
                }

                SetGlobalVariables(commandBuffer);
                context.ExecuteCommandBuffer(commandBuffer);
            }

            context.ExecuteCommandBuffer(commandBuffer);
            commandBuffer.Clear();

            commandBuffer = CommandBufferPool.Get("VisShadows");
            using (new ProfilingScope(commandBuffer, new ProfilingSampler("VisShadows")))
            {
                context.ExecuteCommandBuffer(commandBuffer);
                commandBuffer.Clear();
                //context.ExecuteCommandBuffer(commandBuffer);
                if (bFoundVisibilityLight)
                {
                    int lightIdx = m_ShadowShadowData.lightIndex;
                    VisibleLight visibleLight = renderingData.lightData.visibleLights[lightIdx];

                    for (int j = 0; j < NUM_CUBE_SIDES; ++j)
                    {
                        ShadowDrawingSettings settings = new ShadowDrawingSettings(renderingData.cullResults, lightIdx);

                        settings.useRenderingLayerMaskTest = false;
                        settings.splitData = m_ShadowShadowData.shadowSplits[j];
                        Vector4 shadowBias = ShadowUtils.GetShadowBias(ref visibleLight, lightIdx, ref renderingData.shadowData, m_ShadowShadowData.shadowSlices[j].projectionMatrix, SLICE_RES);
                        ShadowUtils.SetupPointLightShadowCasterConstantBuffer(commandBuffer, (CubemapFace)j, shadowBias);
                        UnityEngine.Rendering.Universal.ShadowUtils.RenderShadowSlice(commandBuffer, ref context, ref m_ShadowShadowData.shadowSlices[j], ref settings);
                    }
                }
            }

            context.ExecuteCommandBuffer(commandBuffer);
            commandBuffer.Clear();
        }

        void SetGlobalVariables(CommandBuffer commandBuffer)
        {
            commandBuffer.SetGlobalMatrixArray(VisibilityWorldToShadowID, m_VisibilityTransforms);
            commandBuffer.SetGlobalMatrixArray(VisibilityShadowWorldToShadowID, m_ShadowTransforms);
            commandBuffer.SetGlobalTexture(m_ShadowHandle.id, m_ShadowTexture);
            commandBuffer.SetGlobalVector("_VisibilityLightWorldPosition", m_LightPosition);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            if (m_ShadowTexture != null)
            {
                RenderTexture.ReleaseTemporary(m_ShadowTexture);
                m_ShadowTexture = null;
            }
        }
    }
}
