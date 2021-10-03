using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SpriteLightRendering
{
    public class DeferredLightingPass : ScriptableRenderPass
    {
        readonly string m_PassName;
        ProfilingSampler m_ProfilingSampler;
        List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();
        readonly FilteringSettings m_FilterSettings;
        RenderStateBlock m_RenderStateBlock;
        Material m_DirectionalLightMat;
        Material m_DirectionalLightShadowMat;
        Material m_PointLightMat;
        Material m_PointLightShadowMat;
        MaterialPropertyBlock m_MPB;

        static readonly string DeferredShaderName = "SpriteLight/DeferredLight";
        static readonly string DeferredLightingTag = "DeferredLighting";
        static readonly int LightPositionID = Shader.PropertyToID("LightPosition");
        static readonly int LightColorID = Shader.PropertyToID("LightColor");
        static readonly int ShadowStrengthID = Shader.PropertyToID("ShadowStrength");

        List<int> ShadowPointLightIndices = new List<int>();

        public DeferredLightingPass(string PassName, RenderQueueRange renderQueueRange, RenderPassEvent passEvent) : base()
        {
            renderPassEvent = passEvent;

            m_PassName = PassName;
            m_ProfilingSampler = new ProfilingSampler(m_PassName);

            m_ShaderTagIdList.Add(new ShaderTagId(DeferredLightingTag));

            m_FilterSettings = new FilteringSettings(renderQueueRange);

            m_RenderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);

            m_DirectionalLightMat = new Material(Shader.Find(DeferredShaderName));
            m_DirectionalLightMat.EnableKeyword("DIRECTIONAL_LIGHTING");
            m_DirectionalLightMat.EnableKeyword("SHADOWS_OFF");

            m_DirectionalLightShadowMat = new Material(Shader.Find(DeferredShaderName));
            m_DirectionalLightShadowMat.EnableKeyword("DIRECTIONAL_LIGHTING");
            m_DirectionalLightShadowMat.EnableKeyword("SHADOWS_ON");

            m_PointLightMat = new Material(Shader.Find(DeferredShaderName));
            m_PointLightMat.EnableKeyword("POINT_LIGHTING");
            m_PointLightMat.EnableKeyword("SHADOWS_OFF");

            m_PointLightShadowMat = new Material(Shader.Find(DeferredShaderName));
            m_PointLightShadowMat.EnableKeyword("POINT_LIGHTING");
            m_PointLightShadowMat.EnableKeyword("SHADOWS_ON");

            m_MPB = new MaterialPropertyBlock();
        }

        public void Setup(RenderTargetIdentifier colorTarget, RenderTargetIdentifier depthTarget, in List<int> ShadowCastingPointLightIndices)
        {
            ConfigureTarget(colorTarget, depthTarget);
            ConfigureClear(ClearFlag.Color, Color.black);
            ShadowPointLightIndices.Clear();
            ShadowPointLightIndices.AddRange(ShadowCastingPointLightIndices);
        }


        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer commandBuffer = CommandBufferPool.Get(m_PassName);

            using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
            {
                context.ExecuteCommandBuffer(commandBuffer);
                commandBuffer.Clear();

                Camera camera = renderingData.cameraData.camera;
                PixelPerfectCamera pixelPerfectCamera = null;
                bool bIsPerfectPixel = camera.TryGetComponent(out pixelPerfectCamera) && pixelPerfectCamera.enabled;

#if UNITY_EDITOR
                if (bIsPerfectPixel)
                {
                    bIsPerfectPixel &= (Application.isPlaying || pixelPerfectCamera.runInEditMode);
                }
#endif

                if (bIsPerfectPixel)
                {
                    //commandBuffer.SetViewport(new Rect(Vector2.zero, new Vector2(pixelPerfectCamera.refResolutionX, pixelPerfectCamera.refResolutionY)));
                    //context.ExecuteCommandBuffer(commandBuffer);
                }

                DrawDirectionalLights(context, commandBuffer, renderingData);
                DrawPointLights(context, commandBuffer, renderingData);

                if (bIsPerfectPixel)
                {
                    //commandBuffer.SetViewport(camera.pixelRect);
                }
            }
            context.ExecuteCommandBuffer(commandBuffer);
            CommandBufferPool.Release(commandBuffer);
        }

        void DrawDirectionalLights(ScriptableRenderContext context, CommandBuffer commandBuffer, RenderingData renderingData)
        {
            Camera camera = renderingData.cameraData.camera;

            int MainLightIndex = renderingData.lightData.mainLightIndex;
            if (MainLightIndex >= 0)
            {
                VisibleLight mainLight = renderingData.lightData.visibleLights[MainLightIndex];
                bool bHasShadows = mainLight.light.shadows != LightShadows.None;
                DrawDirectionalLight(mainLight, context, commandBuffer, camera, bHasShadows);
            }

            for (int i = 0; i < renderingData.lightData.visibleLights.Length; ++i)
            {
                VisibleLight light = renderingData.lightData.visibleLights[i];
                if(i == MainLightIndex || light.lightType != LightType.Directional)
                {
                    continue;
                }

                bool bHasShadows = false;
                DrawDirectionalLight(light, context, commandBuffer, camera, bHasShadows);
            }
        }

        void DrawDirectionalLight(VisibleLight light, ScriptableRenderContext context, CommandBuffer commandBuffer, Camera camera, bool bHasShadows)
        {
            Vector4 LightDirection = light.localToWorldMatrix.MultiplyVector(Vector3.forward);
            LightDirection = camera.transform.InverseTransformDirection(LightDirection);
            LightDirection.z = -LightDirection.z;

            m_MPB.SetVector(LightPositionID, LightDirection);
            m_MPB.SetVector(LightColorID, light.finalColor);
            if (bHasShadows)
            {
                m_MPB.SetFloat(ShadowStrengthID, light.light.shadowStrength);
            }

            commandBuffer.DrawMesh(DeferredUtils.FullscreenQuadDoubleSided, Matrix4x4.identity, bHasShadows ? m_DirectionalLightShadowMat : m_DirectionalLightMat, 0, -1, m_MPB);

            context.ExecuteCommandBuffer(commandBuffer);
            commandBuffer.Clear();
        }

        void DrawPointLights(ScriptableRenderContext context, CommandBuffer commandBuffer, RenderingData renderingData)
        {
            Camera camera = renderingData.cameraData.camera;
            for (int lightIndex = 0; lightIndex < ShadowPointLightIndices.Count; ++lightIndex)
            {
                VisibleLight light = renderingData.lightData.visibleLights[ShadowPointLightIndices[lightIndex]];
                commandBuffer.SetGlobalInt("_LightIndex", lightIndex);

                bool bHasShadows = true;
                DrawPointLight(light, context, commandBuffer, camera, bHasShadows);
            }

            for (int i = 0; i < renderingData.lightData.visibleLights.Length; ++i)
            {
                VisibleLight light = renderingData.lightData.visibleLights[i];
                if (ShadowPointLightIndices.Contains(i) || light.lightType != LightType.Point)
                {
                    continue;
                }

                bool bHasShadows = false;
                DrawPointLight(light, context, commandBuffer, camera, bHasShadows);
            }
        }

        void DrawPointLight(VisibleLight light, ScriptableRenderContext context, CommandBuffer commandBuffer, Camera camera, bool bHasShadows)
        {
            Vector4 LightPosition = light.localToWorldMatrix.GetColumn(3);
            LightPosition = camera.transform.InverseTransformPoint(LightPosition);

            LightPosition.z = -LightPosition.z;

            LightPosition.w = light.range;

            m_MPB.SetVector(LightPositionID, LightPosition);
            m_MPB.SetVector(LightColorID, light.finalColor);
            if(bHasShadows)
            {
                m_MPB.SetFloat(ShadowStrengthID, light.light.shadowStrength);
            }

            Matrix4x4 lightMatrix = light.localToWorldMatrix;
            lightMatrix = lightMatrix * Matrix4x4.Scale(light.range * Vector3.one);
            commandBuffer.DrawMesh(DeferredUtils.SphereMesh, lightMatrix, bHasShadows ? m_PointLightShadowMat : m_PointLightMat, 0, -1, m_MPB);

            context.ExecuteCommandBuffer(commandBuffer);
            commandBuffer.Clear();
        }
    }
}
