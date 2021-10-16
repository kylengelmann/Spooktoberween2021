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
        Material m_DirectionalLightMat;
        Material m_DirectionalLightShadowMat;
        Material m_PointLightMat;
        Material m_PointLightShadowMat;
        Material m_SpotLightMat;
        Material m_SpotLightShadowMat;
        MaterialPropertyBlock m_MPB;

        static readonly string DeferredShaderName = "Hidden/SpriteLight/DeferredLight";
        static readonly string DeferredLightingTag = "DeferredLighting";
        static readonly int LightPositionID = Shader.PropertyToID("LightPosition");
        static readonly int LightColorID = Shader.PropertyToID("LightColor");
        static readonly int ShadowStrengthID = Shader.PropertyToID("ShadowStrength");
        static readonly int SpotlightDirectionID = Shader.PropertyToID("SpotlightDirection");
        static readonly int SpotlightDirDotLRangeID = Shader.PropertyToID("SpotlightDirDotLRange");

        List<int> ShadowPointLightIndices = new List<int>();
        int spotLightIndex = -1;

        RenderTargetHandle ColorTarget;
        RenderTargetHandle DepthTarget;

        public DeferredLightingPass(string PassName, RenderPassEvent passEvent) : base()
        {
            renderPassEvent = passEvent;

            m_PassName = PassName;
            m_ProfilingSampler = new ProfilingSampler(m_PassName);

            m_ShaderTagIdList.Add(new ShaderTagId(DeferredLightingTag));

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

            m_SpotLightMat = new Material(Shader.Find(DeferredShaderName));
            m_SpotLightMat.EnableKeyword("SPOT_LIGHTING");
            m_SpotLightMat.EnableKeyword("SHADOWS_OFF");

            m_SpotLightShadowMat = new Material(Shader.Find(DeferredShaderName));
            m_SpotLightShadowMat.EnableKeyword("SPOT_LIGHTING");
            m_SpotLightShadowMat.EnableKeyword("SHADOWS_ON");

            m_MPB = new MaterialPropertyBlock();
        }

        public void Setup(RenderTargetHandle colorTarget, RenderTargetHandle depthTarget, in List<int> ShadowCastingPointLightIndices, int SpotLightIndex)
        {
            ColorTarget = colorTarget;
            DepthTarget = depthTarget;
            ShadowPointLightIndices.Clear();
            ShadowPointLightIndices.AddRange(ShadowCastingPointLightIndices);
            spotLightIndex = SpotLightIndex;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            ConfigureTarget(new RenderTargetIdentifier[] {ColorTarget.Identifier()}, DepthTarget.Identifier());
            ConfigureClear(ClearFlag.None, Color.black);
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

                if(spotLightIndex >= 0)
                {
                    DrawSpotLight(renderingData.lightData.visibleLights[spotLightIndex], context, commandBuffer, camera, renderingData.lightData.visibleLights[spotLightIndex].light.shadows != LightShadows.None);
                }

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
                if (ShadowPointLightIndices.Contains(i) || light.lightType != LightType.Point || light.light.CompareTag(ShadowUtils.GetVisibilityLightTag()))
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

        void DrawSpotLight(VisibleLight light, ScriptableRenderContext context, CommandBuffer commandBuffer, Camera camera, bool bHasShadows)
        {
            Vector4 LightPosition = light.localToWorldMatrix.GetColumn(3);
            LightPosition = camera.transform.InverseTransformPoint(LightPosition);

            LightPosition.z = -LightPosition.z;

            LightPosition.w = light.range;

            Vector4 LightDirection = light.light.transform.forward;
            LightDirection = camera.transform.InverseTransformDirection(LightDirection);

            LightDirection.z = -LightDirection.z;

            Vector4 LightDirDotLRange = new Vector4(light.light.innerSpotAngle * Mathf.Deg2Rad * .5f, light.light.spotAngle * Mathf.Deg2Rad * .5f, 0f, 0f);

            m_MPB.SetVector(LightPositionID, LightPosition);
            m_MPB.SetVector(LightColorID, light.finalColor);
            m_MPB.SetVector(SpotlightDirectionID, LightDirection);
            m_MPB.SetVector(SpotlightDirDotLRangeID, LightDirDotLRange);
            if (bHasShadows)
            {
                m_MPB.SetFloat(ShadowStrengthID, light.light.shadowStrength);
            }

            Matrix4x4 lightMatrix = light.localToWorldMatrix;
            lightMatrix = lightMatrix * Matrix4x4.Scale(light.range * Vector3.one);
            commandBuffer.DrawMesh(DeferredUtils.SphereMesh, lightMatrix, bHasShadows ? m_SpotLightShadowMat : m_SpotLightMat, 0, -1, m_MPB);

            context.ExecuteCommandBuffer(commandBuffer);
            commandBuffer.Clear();
        }
    }
}
