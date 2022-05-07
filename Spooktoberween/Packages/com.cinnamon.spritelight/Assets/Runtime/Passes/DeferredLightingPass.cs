using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SpriteLightRendering
{
    public class DeferredLightingPass : ScriptableRenderPass
    {
        const byte DOUBLE_SIDED_STENCIL = 2;
        const byte NO_SHADOW_STENCIL = 1;

        readonly string m_PassName;
        ProfilingSampler m_ProfilingSampler;
        List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();

        DeferredLightMaterials m_DirectionalLightMaterials;
        DeferredLightMaterials m_PointLightMaterials;
        DeferredLightMaterials m_SpotLightMaterials;

        MaterialPropertyBlock m_MPB;

        static readonly string DeferredShaderName = "Hidden/SpriteLight/DeferredLight";
        static readonly string DeferredLightingTag = "DeferredLighting";
        static readonly int LightPositionID = Shader.PropertyToID("LightPosition");
        static readonly int LightColorID = Shader.PropertyToID("LightColor");
        static readonly int ShadowStrengthID = Shader.PropertyToID("ShadowStrength");
        static readonly int SpotlightDirectionID = Shader.PropertyToID("SpotlightDirection");
        static readonly int SpotlightDirDotLRangeID = Shader.PropertyToID("SpotlightDirDotLRange");
        static readonly int StencilRef = Shader.PropertyToID("_StencilRef");
        static readonly int StencilReadMask = Shader.PropertyToID("_StencilReadMask");

        List<int> ShadowPointLightIndices = new List<int>();
        int spotLightIndex = -1;

        RenderTargetHandle ColorTarget;
        RenderTargetHandle DepthTarget;

        class DeferredLightMaterials
        {
            string m_lightTypeShaderKeyword;

            public Material noShadows;
            public Material shadows;
            public Material noReceiveShadows;

            public Material noShadowsDoubleSided;
            public Material shadowsDoubleSided;
            public Material noReceiveShadowsDoubleSided;

            public DeferredLightMaterials(string lightTypeShaderKeyword)
            {
                m_lightTypeShaderKeyword = lightTypeShaderKeyword;

                //Shader shader = Shader.Find(DeferredShaderName);

                GetMaterial(false, false, false);
                GetMaterial(false, false, true);
                GetMaterial(false, true, false);
                GetMaterial(false, true, true);
                GetMaterial(true, false, false);
                GetMaterial(true, false, true);

                //noShadows = new Material(shader);
                //noShadows.EnableKeyword(m_lightTypeShaderKeyword);
                //noShadows.EnableKeyword("SHADOWS_OFF");
                //noShadows.EnableKeyword("SINGLE_SIDED");
                //noShadows.SetInteger(StencilRef, 0);
                //noShadows.SetInteger(StencilReadMask, DOUBLE_SIDED_STENCIL);

                //shadows = new Material(shader);
                //shadows.EnableKeyword(m_lightTypeShaderKeyword);
                //shadows.EnableKeyword("SHADOWS_ON");
                //shadows.EnableKeyword("SINGLE_SIDED");
                //shadows.SetInteger(StencilRef, 0);
                //shadows.SetInteger(StencilReadMask, DOUBLE_SIDED_STENCIL | NO_SHADOW_STENCIL);

                //noReceiveShadows = new Material(shader);
                //noReceiveShadows.EnableKeyword(m_lightTypeShaderKeyword);
                //noReceiveShadows.EnableKeyword("SHADOWS_OFF");
                //noReceiveShadows.EnableKeyword("SINGLE_SIDED");
                //noReceiveShadows.SetInteger(StencilRef, NO_SHADOW_STENCIL);
                //noReceiveShadows.SetInteger(StencilReadMask, DOUBLE_SIDED_STENCIL | NO_SHADOW_STENCIL);

                //noShadowsDoubleSided = new Material(shader);
                //noShadowsDoubleSided.EnableKeyword(m_lightTypeShaderKeyword);
                //noShadowsDoubleSided.EnableKeyword("SHADOWS_OFF");
                //noShadowsDoubleSided.EnableKeyword("DOUBLE_SIDED");
                //noShadowsDoubleSided.SetInteger(StencilRef, DOUBLE_SIDED_STENCIL);
                //noShadowsDoubleSided.SetInteger(StencilReadMask, DOUBLE_SIDED_STENCIL);

                //shadowsDoubleSided = new Material(shader);
                //shadowsDoubleSided.EnableKeyword(m_lightTypeShaderKeyword);
                //shadowsDoubleSided.EnableKeyword("SHADOWS_ON");
                //shadowsDoubleSided.EnableKeyword("DOUBLE_SIDED");
                //shadowsDoubleSided.SetInteger(StencilRef, DOUBLE_SIDED_STENCIL);
                //shadowsDoubleSided.SetInteger(StencilReadMask, DOUBLE_SIDED_STENCIL | NO_SHADOW_STENCIL);

                //noReceiveShadowsDoubleSided = new Material(shader);
                //noReceiveShadowsDoubleSided.EnableKeyword(m_lightTypeShaderKeyword);
                //noReceiveShadowsDoubleSided.EnableKeyword("SHADOWS_OFF");
                //noReceiveShadowsDoubleSided.EnableKeyword("DOUBLE_SIDED");
                //noReceiveShadowsDoubleSided.SetInteger(StencilRef, DOUBLE_SIDED_STENCIL | NO_SHADOW_STENCIL);
                //noReceiveShadowsDoubleSided.SetInteger(StencilReadMask, DOUBLE_SIDED_STENCIL | NO_SHADOW_STENCIL);
            }

            public Material GetMaterial(bool bCastsShadows, bool bReceiveShadows, bool bDoubleSided)
            {
                if(bCastsShadows)
                {
                    if(bDoubleSided)
                    { 
                        if(shadowsDoubleSided == null)
                        {
                            Shader shader = Shader.Find(DeferredShaderName);

                            shadowsDoubleSided = new Material(shader);
                            shadowsDoubleSided.EnableKeyword(m_lightTypeShaderKeyword);
                            shadowsDoubleSided.EnableKeyword("SHADOWS_ON");
                            shadowsDoubleSided.EnableKeyword("DOUBLE_SIDED");
                            shadowsDoubleSided.SetInteger(StencilRef, DOUBLE_SIDED_STENCIL);
                            shadowsDoubleSided.SetInteger(StencilReadMask, DOUBLE_SIDED_STENCIL | NO_SHADOW_STENCIL);
                        }
                        return shadowsDoubleSided;
                    }
                    else
                    {
                        if(shadows == null)
                        {
                            Shader shader = Shader.Find(DeferredShaderName);

                            shadows = new Material(shader);
                            shadows.EnableKeyword(m_lightTypeShaderKeyword);
                            shadows.EnableKeyword("SHADOWS_ON");
                            shadows.EnableKeyword("SINGLE_SIDED");
                            shadows.SetInteger(StencilRef, 0);
                            shadows.SetInteger(StencilReadMask, DOUBLE_SIDED_STENCIL | NO_SHADOW_STENCIL);
                        }
                        return shadows;
                    }
                }
                else if(bReceiveShadows)
                {
                    if(bDoubleSided)
                    {
                        if(noShadowsDoubleSided == null)
                        {
                            Shader shader = Shader.Find(DeferredShaderName);

                            noShadowsDoubleSided = new Material(shader);
                            noShadowsDoubleSided.EnableKeyword(m_lightTypeShaderKeyword);
                            noShadowsDoubleSided.EnableKeyword("SHADOWS_OFF");
                            noShadowsDoubleSided.EnableKeyword("DOUBLE_SIDED");
                            noShadowsDoubleSided.SetInteger(StencilRef, DOUBLE_SIDED_STENCIL);
                            noShadowsDoubleSided.SetInteger(StencilReadMask, DOUBLE_SIDED_STENCIL);
                        }
                        return noShadowsDoubleSided;
                    }
                    else
                    {
                        if(noShadows == null)
                        {
                            Shader shader = Shader.Find(DeferredShaderName);

                            noShadows = new Material(shader);
                            noShadows.EnableKeyword(m_lightTypeShaderKeyword);
                            noShadows.EnableKeyword("SHADOWS_OFF");
                            noShadows.EnableKeyword("SINGLE_SIDED");
                            noShadows.SetInteger(StencilRef, 0);
                            noShadows.SetInteger(StencilReadMask, DOUBLE_SIDED_STENCIL);
                        }
                        return noShadows;
                    }
                }
                else
                {
                    if(bDoubleSided)
                    {
                        if(noReceiveShadowsDoubleSided == null)
                        {
                            Shader shader = Shader.Find(DeferredShaderName);

                            noReceiveShadowsDoubleSided = new Material(shader);
                            noReceiveShadowsDoubleSided.EnableKeyword(m_lightTypeShaderKeyword);
                            noReceiveShadowsDoubleSided.EnableKeyword("SHADOWS_OFF");
                            noReceiveShadowsDoubleSided.EnableKeyword("DOUBLE_SIDED");
                            noReceiveShadowsDoubleSided.SetInteger(StencilRef, DOUBLE_SIDED_STENCIL | NO_SHADOW_STENCIL);
                            noReceiveShadowsDoubleSided.SetInteger(StencilReadMask, DOUBLE_SIDED_STENCIL | NO_SHADOW_STENCIL);
                        }
                        return noReceiveShadowsDoubleSided;
                    }
                    else
                    {
                        if(noReceiveShadows == null)
                        {
                            Shader shader = Shader.Find(DeferredShaderName);

                            noReceiveShadows = new Material(shader);
                            noReceiveShadows.EnableKeyword(m_lightTypeShaderKeyword);
                            noReceiveShadows.EnableKeyword("SHADOWS_OFF");
                            noReceiveShadows.EnableKeyword("SINGLE_SIDED");
                            noReceiveShadows.SetInteger(StencilRef, NO_SHADOW_STENCIL);
                            noReceiveShadows.SetInteger(StencilReadMask, DOUBLE_SIDED_STENCIL | NO_SHADOW_STENCIL);
                        }
                        return noReceiveShadows;
                    }
                }
            }
        }

        public DeferredLightingPass(string PassName, RenderPassEvent passEvent) : base()
        {
            renderPassEvent = passEvent;

            m_PassName = PassName;
            m_ProfilingSampler = new ProfilingSampler(m_PassName);

            m_ShaderTagIdList.Add(new ShaderTagId(DeferredLightingTag));

            m_DirectionalLightMaterials = new DeferredLightMaterials("DIRECTIONAL_LIGHTING");
            m_PointLightMaterials = new DeferredLightMaterials("POINT_LIGHTING");
            m_SpotLightMaterials = new DeferredLightMaterials("SPOT_LIGHTING");

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
                    bool bHasShadows = renderingData.lightData.visibleLights[spotLightIndex].light.shadows != LightShadows.None;
                    DrawSpotLight(renderingData.lightData.visibleLights[spotLightIndex], context, commandBuffer, camera, bHasShadows, bHasShadows, false);
                    DrawSpotLight(renderingData.lightData.visibleLights[spotLightIndex], context, commandBuffer, camera, bHasShadows, bHasShadows, true);

                    if(bHasShadows) 
                    {
                        DrawSpotLight(renderingData.lightData.visibleLights[spotLightIndex], context, commandBuffer, camera, false, true, false);
                        DrawSpotLight(renderingData.lightData.visibleLights[spotLightIndex], context, commandBuffer, camera, false, true, true);
                    }
                }

                if (bIsPerfectPixel)
                {
                    //commandBuffer.SetViewport(camera.pixelRect);
                }
            }
            context.ExecuteCommandBuffer(commandBuffer);
            CommandBufferPool.Release(commandBuffer);
        }

        void DrawDirectionalLights(ScriptableRenderContext context, CommandBuffer commandBuffer, RenderingData renderingData, bool bDoubleSided = false)
        {
            Camera camera = renderingData.cameraData.camera;

            int MainLightIndex = renderingData.lightData.mainLightIndex;
            if (MainLightIndex >= 0)
            {
                VisibleLight mainLight = renderingData.lightData.visibleLights[MainLightIndex];
                bool bHasShadows = mainLight.light.shadows != LightShadows.None;
                DrawDirectionalLight(mainLight, context, commandBuffer, camera, bHasShadows, bHasShadows, bDoubleSided);

                if(bHasShadows) DrawDirectionalLight(mainLight, context, commandBuffer, camera, false, true, bDoubleSided);
            }

            for (int i = 0; i < renderingData.lightData.visibleLights.Length; ++i)
            {
                VisibleLight light = renderingData.lightData.visibleLights[i];
                if(i == MainLightIndex || light.lightType != LightType.Directional)
                {
                    continue;
                }

                bool bHasShadows = false;
                DrawDirectionalLight(light, context, commandBuffer, camera, bHasShadows, bHasShadows, bDoubleSided);
            }

            if(!bDoubleSided) DrawDirectionalLights(context, commandBuffer, renderingData, true);
        }

        void DrawDirectionalLight(VisibleLight light, ScriptableRenderContext context, CommandBuffer commandBuffer, Camera camera, bool bHasShadows, bool bCheckRecievesShadows = false, bool bDoubleSided = false)
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

            Material LightMaterial = m_DirectionalLightMaterials.GetMaterial(bHasShadows, !bCheckRecievesShadows, bDoubleSided);
            commandBuffer.DrawMesh(DeferredUtils.FullscreenQuadDoubleSided, Matrix4x4.identity, LightMaterial, 0, -1, m_MPB);

            context.ExecuteCommandBuffer(commandBuffer);
            commandBuffer.Clear();
        }

        void DrawPointLights(ScriptableRenderContext context, CommandBuffer commandBuffer, RenderingData renderingData, bool bDoubleSided = false)
        {
            Camera camera = renderingData.cameraData.camera;
            for (int lightIndex = 0; lightIndex < ShadowPointLightIndices.Count; ++lightIndex)
            {
                VisibleLight light = renderingData.lightData.visibleLights[ShadowPointLightIndices[lightIndex]];
                commandBuffer.SetGlobalInt("_LightIndex", lightIndex);

                bool bHasShadows = true;
                DrawPointLight(light, context, commandBuffer, camera, bHasShadows, bHasShadows, bDoubleSided);
            }

            for (int i = 0; i < renderingData.lightData.visibleLights.Length; ++i)
            {
                VisibleLight light = renderingData.lightData.visibleLights[i];
                if (light.lightType != LightType.Point || light.light.CompareTag(ShadowUtils.GetVisibilityLightTag()))
                {
                    continue;
                }

                bool bHasShadows = false;
                DrawPointLight(light, context, commandBuffer, camera, bHasShadows, ShadowPointLightIndices.Contains(i), bDoubleSided);
            }

            if(!bDoubleSided) DrawPointLights(context, commandBuffer, renderingData, true);
        }

        void DrawPointLight(VisibleLight light, ScriptableRenderContext context, CommandBuffer commandBuffer, Camera camera, bool bHasShadows, bool bCheckRecievesShadows = false, bool bDoubleSided = false)
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

            Material LightMaterial = m_PointLightMaterials.GetMaterial(bHasShadows, !bCheckRecievesShadows, bDoubleSided);
            commandBuffer.DrawMesh(DeferredUtils.SphereMesh, lightMatrix, LightMaterial, 0, -1, m_MPB);

            context.ExecuteCommandBuffer(commandBuffer);
            commandBuffer.Clear();
        }

        void DrawSpotLight(VisibleLight light, ScriptableRenderContext context, CommandBuffer commandBuffer, Camera camera, bool bHasShadows, bool bCheckRecievesShadows = false, bool bDoubleSided = false)
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

            Material LightMaterial = m_SpotLightMaterials.GetMaterial(bHasShadows, !bCheckRecievesShadows, bDoubleSided);

            commandBuffer.DrawMesh(DeferredUtils.SphereMesh, lightMatrix, LightMaterial, 0, -1, m_MPB);

            context.ExecuteCommandBuffer(commandBuffer);
            commandBuffer.Clear();
        }
    }
}
