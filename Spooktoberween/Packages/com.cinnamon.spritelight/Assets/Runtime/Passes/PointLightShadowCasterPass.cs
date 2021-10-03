using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SpriteLightRendering
{
    public class PointLightShadowCasterPass : ScriptableRenderPass
    {
        const int MAX_POINT_LIGHT_SHADOWS = 2;
        const int NUM_CUBE_SIDES = 6;
        const int SLICE_RES = 256;

        struct PointLightShadowData
        {
            public int lightIndex;
            public ShadowSliceData[] shadowSlices;
            public ShadowSplitData[] shadowSplits;
        }

        PointLightShadowData[] m_PointLightShadowData = new PointLightShadowData[MAX_POINT_LIGHT_SHADOWS];
        
        Matrix4x4[] m_ShadowTransforms = new Matrix4x4[MAX_POINT_LIGHT_SHADOWS*NUM_CUBE_SIDES];
        Vector4[] m_LightPositions = new Vector4[MAX_POINT_LIGHT_SHADOWS];

        RenderTexture m_ShadowTexture;
        RenderTargetHandle m_ShadowHandle;

        int m_NumLightsToRender = 0;
        ProfilingSampler m_ProfilingSampler;

        static readonly int PointLightWorldToShadowID = Shader.PropertyToID("_PointLightWorldToShadow");
        static readonly string PassName = "PointLightShadows";

        public PointLightShadowCasterPass(RenderPassEvent evt) : base()
        {
            renderPassEvent = evt;

            m_ProfilingSampler = new ProfilingSampler(PassName);

            m_ShadowHandle.Init("_PointLightShadowTexture");

            for(int i = 0; i < MAX_POINT_LIGHT_SHADOWS; ++i)
            {
                m_PointLightShadowData[i].shadowSlices = new ShadowSliceData[NUM_CUBE_SIDES];
                m_PointLightShadowData[i].shadowSplits = new ShadowSplitData[NUM_CUBE_SIDES];
            }
        }

        public void Setup(ref RenderingData renderingData, ref List<int> LightIndices)
        {
            int numVisibleLights = renderingData.lightData.visibleLights.Length;
            m_NumLightsToRender = 0;
            LightIndices.Clear();
            for(int i = 0; i < numVisibleLights && m_NumLightsToRender < MAX_POINT_LIGHT_SHADOWS; ++i)
            {
                if(i == renderingData.lightData.mainLightIndex)
                {
                    continue;
                }

                VisibleLight light = renderingData.lightData.visibleLights[i];

                if(light.lightType != LightType.Point || light.light.shadows == LightShadows.None)
                {
                    continue;
                }

                if (renderingData.cullResults.GetShadowCasterBounds(i, out var bounds))
                {
                    m_PointLightShadowData[m_NumLightsToRender].lightIndex = i;
                    m_LightPositions[m_NumLightsToRender] = light.light.transform.position;
                    for(int j = 0; j < NUM_CUBE_SIDES; ++j)
                    {
                        ShadowUtils.GetPointLightShadowParams(ref renderingData.cullResults, ref renderingData.shadowData, i, m_NumLightsToRender, (CubemapFace)j, 1024, 1024, SLICE_RES, .2f,
                            out m_PointLightShadowData[m_NumLightsToRender].shadowSlices[j], out m_PointLightShadowData[m_NumLightsToRender].shadowSplits[j]);
                        m_ShadowTransforms[m_NumLightsToRender*NUM_CUBE_SIDES + j] = m_PointLightShadowData[m_NumLightsToRender].shadowSlices[j].shadowTransform;
                    }

                    LightIndices.Add(i);

                    ++m_NumLightsToRender;
                }
            }
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            m_ShadowTexture = UnityEngine.Rendering.Universal.ShadowUtils.GetTemporaryShadowTexture(1024, 1024, 16);
            ConfigureTarget(new RenderTargetIdentifier(m_ShadowTexture));
            ConfigureClear(ClearFlag.All, Color.black);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer commandBuffer = CommandBufferPool.Get(PassName);
            using(new ProfilingScope(commandBuffer, m_ProfilingSampler))
            {
                context.ExecuteCommandBuffer(commandBuffer);
                commandBuffer.Clear();
                context.ExecuteCommandBuffer(commandBuffer);
                for(int i = 0; i < m_NumLightsToRender; ++i)
                {
                    int lightIdx = m_PointLightShadowData[i].lightIndex;
                    VisibleLight visibleLight = renderingData.lightData.visibleLights[lightIdx];
                    for(int j = 0; j < NUM_CUBE_SIDES; ++j)
                    {
                        ShadowDrawingSettings settings = new ShadowDrawingSettings(renderingData.cullResults, lightIdx);
                        settings.useRenderingLayerMaskTest = false;
                        settings.splitData = m_PointLightShadowData[i].shadowSplits[j];
                        Vector4 shadowBias = ShadowUtils.GetShadowBias(ref visibleLight, lightIdx, ref renderingData.shadowData, m_PointLightShadowData[i].shadowSlices[j].projectionMatrix, SLICE_RES);
                        ShadowUtils.SetupPointLightShadowCasterConstantBuffer(commandBuffer, (CubemapFace)j, shadowBias);
                        UnityEngine.Rendering.Universal.ShadowUtils.RenderShadowSlice(commandBuffer, ref context, ref m_PointLightShadowData[i].shadowSlices[j], ref settings);
                    }
                }

                SetGlobalVariables(commandBuffer);
                context.ExecuteCommandBuffer(commandBuffer);
            }

            context.ExecuteCommandBuffer(commandBuffer);
            commandBuffer.Clear();
        }

        void SetGlobalVariables(CommandBuffer commandBuffer)
        {
            commandBuffer.SetGlobalMatrixArray(PointLightWorldToShadowID, m_ShadowTransforms);
            commandBuffer.SetGlobalTexture(m_ShadowHandle.id, m_ShadowTexture);
            commandBuffer.SetGlobalVectorArray("_PointLightWorldPosition", m_LightPositions);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            if(m_ShadowTexture != null)
            {
                RenderTexture.ReleaseTemporary(m_ShadowTexture);
                m_ShadowTexture = null;
            }
        }
    }
}
