using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

namespace SpriteLightRendering
{
    public sealed class SpriteLightRenderer : ScriptableRenderer
    {
#region VARIABLE_DEFS
        const int k_DepthStencilBufferBits = 32;
        const string k_CreateCameraTextures = "Create Camera Texture";

        // Shadows
        MainLightShadowCasterPass m_MainLightShadowCasterPass;
        AdditionalLightsShadowCasterPass m_AdditionalLightsShadowCasterPass;
        PointLightShadowCasterPass m_PointLightShadowCasterPass;
        VisibilityPass m_VisibilityPass;

        // Prepasses
        ColorGradingLutPass m_ColorGradingLutPass;
        DepthOnlyPass m_DepthPrepass;
        VisibilityTexturePass m_VisibilityTexturePass;

        // Opaque
        NormalsPass m_NormalsPass;
        SpriteColorPass m_SpriteColorPass;
#if UNITY_EDITOR
        EditorNormalsDiffusePass m_EditorNormalsDiffusePass;
#endif // UNITY_EDITOR
        DeferredLightingPass m_DeferredLightingPass;
        UpscaleBlitPass m_UpscaleBasePass;

        // Skybox
        DrawSkyboxPass m_DrawSkyboxPass;

        // Copy Passes
        CopyDepthPass m_CopyDepthPass;
        CopyColorPass m_CopyColorPass;

        // Transparent
        ClearTargetsPass m_ClearTargetsPass;
        NormalsPass m_TransparentNormalsPass;
        SpriteColorPass m_TransparentColorPass;
#if UNITY_EDITOR
        EditorNormalsDiffusePass m_EditorTransparentNormalsDiffusePass;
#endif // UNITY_EDITOR
        DeferredLightingPass m_TransparentLightingPass;
        UpscaleBlitPass m_UpscaleTransparentBasePass;

        // Post processing
        InvokeOnRenderObjectCallbackPass m_OnRenderObjectCallbackPass;
        PostProcessPass m_PostProcessPass;
        PostProcessPass m_FinalPostProcessPass;
        FinalBlitPass m_FinalBlitPass;
        CapturePass m_CapturePass;

    #if POST_PROCESSING_STACK_2_0_0_OR_NEWER
            PostProcessPassCompat m_OpaquePostProcessPassCompat;
            PostProcessPassCompat m_PostProcessPassCompat;
    #endif

    #if UNITY_EDITOR
        SceneViewDepthCopyPass m_SceneViewDepthCopyPass;
    #endif

        RenderTargetHandle m_ActiveCameraColorAttachment;
        RenderTargetHandle m_ActiveCameraDepthAttachment;
        RenderTargetHandle m_CameraColorAttachment;
        RenderTargetHandle m_CameraDepthAttachment;
        RenderTargetHandle m_DepthTexture;
        RenderTargetHandle m_NormalsTexture;
        RenderTargetHandle m_NormalsDepthTexture;
        RenderTargetHandle m_BaseColor;
        RenderTargetHandle m_BaseDepth;
        RenderTargetHandle m_OpaqueColor;
        RenderTargetHandle m_DiffuseColor;
        RenderTargetHandle m_SpecularColor;
        RenderTargetHandle m_VisibilityTexture;
        RenderTargetHandle m_AfterPostProcessColor;
        RenderTargetHandle m_ColorGradingLut;

        StencilState m_DefaultStencilState;

        Material m_BlitMaterial;
        Material m_CopyDepthMaterial;
        Material m_SamplingMaterial;
        Material m_ScreenspaceShadowsMaterial;

        List<int> ShadowPointLightIndices = new List<int>();
#endregion

        public SpriteLightRenderer(SpriteLightRendererData data) : base(data)
        {
            m_BlitMaterial = CoreUtils.CreateEngineMaterial(data.shaders.blitPS);
            m_CopyDepthMaterial = CoreUtils.CreateEngineMaterial(data.shaders.copyDepthPS);
            m_SamplingMaterial = CoreUtils.CreateEngineMaterial(data.shaders.samplingPS);
            m_ScreenspaceShadowsMaterial = CoreUtils.CreateEngineMaterial(data.shaders.screenSpaceShadowPS);

            StencilStateData stencilData = data.defaultStencilState;
            m_DefaultStencilState = StencilState.defaultValue;
            m_DefaultStencilState.enabled = stencilData.overrideStencilState;
            m_DefaultStencilState.SetCompareFunction(stencilData.stencilCompareFunction);
            m_DefaultStencilState.SetPassOperation(stencilData.passOperation);
            m_DefaultStencilState.SetFailOperation(stencilData.failOperation);
            m_DefaultStencilState.SetZFailOperation(stencilData.zFailOperation);

            // Note: Since all custom render passes inject first and we have stable sort,
            // we inject the builtin passes in the before events.

            // Shadows
            m_MainLightShadowCasterPass = new MainLightShadowCasterPass(RenderPassEvent.BeforeRenderingShadows);
            m_AdditionalLightsShadowCasterPass = new AdditionalLightsShadowCasterPass(RenderPassEvent.BeforeRenderingShadows);
            m_PointLightShadowCasterPass = new PointLightShadowCasterPass(RenderPassEvent.BeforeRenderingShadows);
            m_VisibilityPass = new VisibilityPass(RenderPassEvent.BeforeRenderingShadows);

            // Prepasses
            m_DepthPrepass = new DepthOnlyPass(RenderPassEvent.BeforeRenderingPrePasses, RenderQueueRange.opaque, data.opaqueLayerMask);
            m_ColorGradingLutPass = new ColorGradingLutPass(RenderPassEvent.BeforeRenderingPrePasses, data.postProcessData);

            // Opaque
            m_NormalsPass = new NormalsPass("Normals", RenderQueueRange.opaque, RenderPassEvent.AfterRenderingOpaques - 2, data.opaqueLayerMask);
            m_VisibilityTexturePass = new VisibilityTexturePass("VisibilityTexture", RenderPassEvent.AfterRenderingOpaques - 1);
            m_SpriteColorPass = new SpriteColorPass("Sprite Color", RenderQueueRange.opaque, RenderPassEvent.BeforeRenderingOpaques);
#if UNITY_EDITOR
            m_EditorNormalsDiffusePass = new EditorNormalsDiffusePass("Editor Normals Diffuse", RenderQueueRange.opaque, RenderPassEvent.BeforeRenderingOpaques - 1);
#endif // UNITY_EDITOR
            m_DeferredLightingPass = new DeferredLightingPass("Deferred Lighting", RenderPassEvent.AfterRenderingOpaques);
            m_UpscaleBasePass = new UpscaleBlitPass(RenderPassEvent.AfterRenderingOpaques + 1, BlendMode.SrcAlpha, BlendMode.OneMinusSrcAlpha);

            // Skybox
            m_DrawSkyboxPass = new DrawSkyboxPass(RenderPassEvent.BeforeRenderingSkybox);

            // Copy Passes
            m_CopyDepthPass = new CopyDepthPass(RenderPassEvent.AfterRenderingTransparents + 2, m_CopyDepthMaterial);
            m_CopyColorPass = new CopyColorPass(RenderPassEvent.AfterRenderingTransparents + 2, m_SamplingMaterial);

            // Transparent
            m_ClearTargetsPass = new ClearTargetsPass("Clear Opaque Textures", RenderPassEvent.BeforeRenderingTransparents - 1, ClearFlag.Color, Color.clear);
            m_TransparentNormalsPass = new NormalsPass("TransparentNormals", RenderQueueRange.opaque, RenderPassEvent.BeforeRenderingTransparents, data.transparentLayerMask);
            m_TransparentColorPass = new SpriteColorPass("Transparent Sprite Color", RenderQueueRange.transparent, RenderPassEvent.BeforeRenderingTransparents);
#if UNITY_EDITOR
            m_EditorTransparentNormalsDiffusePass = new EditorNormalsDiffusePass("Editor Transparent Normals Diffuse", RenderQueueRange.transparent, RenderPassEvent.BeforeRenderingTransparents + 1);
#endif // UNITY_EDITOR
            m_TransparentLightingPass = new DeferredLightingPass("Transparent Lighting", RenderPassEvent.AfterRenderingTransparents);
            m_UpscaleTransparentBasePass = new UpscaleBlitPass(RenderPassEvent.AfterRenderingTransparents + 1, BlendMode.One, BlendMode.OneMinusSrcAlpha);

            // Post processing
            m_OnRenderObjectCallbackPass = new InvokeOnRenderObjectCallbackPass(RenderPassEvent.BeforeRenderingPostProcessing);
            m_PostProcessPass = new PostProcessPass(RenderPassEvent.BeforeRenderingPostProcessing, data.postProcessData, m_BlitMaterial);
            m_FinalPostProcessPass = new PostProcessPass(RenderPassEvent.AfterRendering + 1, data.postProcessData, m_BlitMaterial);
            m_CapturePass = new CapturePass(RenderPassEvent.AfterRendering);
            m_FinalBlitPass = new FinalBlitPass(RenderPassEvent.AfterRendering + 1, m_BlitMaterial);

    #if POST_PROCESSING_STACK_2_0_0_OR_NEWER
                m_OpaquePostProcessPassCompat = new PostProcessPassCompat(RenderPassEvent.BeforeRenderingOpaques, true);
                m_PostProcessPassCompat = new PostProcessPassCompat(RenderPassEvent.BeforeRenderingPostProcessing);
    #endif

    #if UNITY_EDITOR
            m_SceneViewDepthCopyPass = new SceneViewDepthCopyPass(RenderPassEvent.AfterRendering + 9, m_CopyDepthMaterial);
    #endif

            // RenderTexture format depends on camera and pipeline (HDR, non HDR, etc)
            // Samples (MSAA) depend on camera and pipeline
            m_CameraColorAttachment.Init("_CameraColorTexture");
            m_CameraDepthAttachment.Init("_CameraDepthAttachment");
            m_DepthTexture.Init("_CameraDepthTexture");
            m_NormalsTexture.Init("_NormalsTexture");
            m_NormalsDepthTexture.Init("_NormalsDepth");
            m_OpaqueColor.Init("_CameraOpaqueTexture");
            m_AfterPostProcessColor.Init("_AfterPostProcessTexture");
            m_ColorGradingLut.Init("_InternalGradingLut");
            m_BaseColor.Init("_BaseColor");
            m_BaseDepth.Init("_BaseDepth");
            m_DiffuseColor.Init("_DiffuseTexture");
            m_SpecularColor.Init("_SpecularTexture");
            m_VisibilityTexture.Init("_VisibilityTexture");

            supportedRenderingFeatures = new RenderingFeatures()
            {
                cameraStacking = true,
            };
        }

        protected override void Dispose(bool disposing)
        {
            // always dispose unmanaged resources
            m_PostProcessPass.Cleanup();
            CoreUtils.Destroy(m_BlitMaterial);
            CoreUtils.Destroy(m_CopyDepthMaterial);
            CoreUtils.Destroy(m_SamplingMaterial);
            CoreUtils.Destroy(m_ScreenspaceShadowsMaterial);
        }

        public override void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            Camera camera = renderingData.cameraData.camera;
            ref CameraData cameraData = ref renderingData.cameraData;
            RenderTextureDescriptor cameraTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;

            bool bIsPixelPerfectCamera = false;
            float pixelRatio = 1f;
            PixelPerfectCamera PixelPerfectComponent = null;
            if (camera)
            {
                PixelPerfectComponent = camera.GetComponent<PixelPerfectCamera>();
#if UNITY_EDITOR
                bIsPixelPerfectCamera = PixelPerfectComponent && (Application.isPlaying || PixelPerfectComponent.runInEditMode) && PixelPerfectComponent.enabled;
#else
                bIsPixelPerfectCamera = PixelPerfectComponent && PixelPerfectComponent.enabled;
#endif
                if (bIsPixelPerfectCamera)
                {
                    pixelRatio = PixelPerfectComponent.pixelRatio;
                }
            }

            // Special path for depth only offscreen cameras. Only write opaques + transparents.
            bool isOffscreenDepthTexture = cameraData.targetTexture != null && cameraData.targetTexture.format == RenderTextureFormat.Depth;
            if (isOffscreenDepthTexture || cameraData.isPreviewCamera)
            {
                return;
            }
            

            // Should apply post-processing after rendering this camera?
            bool applyPostProcessing = cameraData.postProcessEnabled;
            // There's at least a camera in the camera stack that applies post-processing
            bool anyPostProcessing = renderingData.postProcessingEnabled;

            // We generate color LUT in the base camera only. This allows us to not break render pass execution for overlay cameras.
            bool generateColorGradingLUT = anyPostProcessing && cameraData.renderType == CameraRenderType.Base;

            bool isSceneViewCamera = cameraData.isSceneViewCamera;

            CommandBuffer commandBuffer = CommandBufferPool.Get("SetShaderParameters");

            commandBuffer.SetGlobalFloat("_PixelRatio", pixelRatio);
            Vector4 UVOffset = Vector4.one;
            if (bIsPixelPerfectCamera)
            {
                commandBuffer.EnableShaderKeyword("UNITY_PIXEL_PERFECT");

                Vector3 pixelCamPos = PixelPerfectComponent.RoundToPixel(camera.transform.position);
                Vector3 pixelCamPosOffset = pixelCamPos - camera.transform.position;

                UVOffset.x = -pixelCamPosOffset.x / PixelPerfectComponent.refResolutionX * PixelPerfectComponent.pixelRatio;
                UVOffset.y = -pixelCamPosOffset.y / PixelPerfectComponent.refResolutionY * PixelPerfectComponent.pixelRatio;
            }
            else
            {
                commandBuffer.DisableShaderKeyword("UNITY_PIXEL_PERFECT");

                UVOffset.x = 0f;
                UVOffset.y = 0f;
            }
            commandBuffer.SetGlobalVector("UVOffset", UVOffset);

            commandBuffer.SetGlobalVector("_AmbientLightColor", RenderSettings.ambientLight);

            //if (isSceneViewCamera || (!cameraData.resolveFinalTarget))
            {
                commandBuffer.EnableShaderKeyword("RENDERING_TO_TEMP_TARGET");
            }
            //else
            //{
            //    commandBuffer.DisableShaderKeyword("RENDERING_TO_TEMP_TARGET");
            //}

            context.ExecuteCommandBuffer(commandBuffer);
            CommandBufferPool.Release(commandBuffer);

            bool isPreviewCamera = cameraData.isPreviewCamera;
            bool requiresDepthTexture = cameraData.requiresDepthTexture;
            bool isStereoEnabled = false;

            bool mainLightShadows = m_MainLightShadowCasterPass.Setup(ref renderingData);
            //bool additionalLightShadows = m_AdditionalLightsShadowCasterPass.Setup(ref renderingData);
            bool additionalLightShadows = false;

            // Depth prepass is generated in the following cases:
            // - If game or offscreen camera requires it we check if we can copy the depth from the rendering opaques pass and use that instead.
            // - Scene or preview cameras always require a depth texture. We do a depth pre-pass to simplify it and it shouldn't matter much for editor.
            bool requiresDepthPrepass = requiresDepthTexture && !CanCopyDepth(ref renderingData.cameraData);
            requiresDepthPrepass |= isSceneViewCamera;
            requiresDepthPrepass |= isPreviewCamera;

            // The copying of depth should normally happen after rendering opaques.
            // But if we only require it for post processing or the scene camera then we do it after rendering transparent objects
            m_CopyDepthPass.renderPassEvent = (!requiresDepthTexture && (applyPostProcessing || isSceneViewCamera)) ? RenderPassEvent.AfterRenderingTransparents + 2: RenderPassEvent.AfterRenderingOpaques;

            // TODO: CopyDepth pass is disabled in XR due to required work to handle camera matrices in URP.
            // IF this condition is removed make sure the CopyDepthPass.cs is working properly on all XR modes. This requires PureXR SDK integration.
            if (isStereoEnabled && requiresDepthTexture)
                requiresDepthPrepass = true;

            bool isRunningHololens = false;
    #if ENABLE_VR && ENABLE_VR_MODULE
                isRunningHololens = UniversalRenderPipeline.IsRunningHololens(camera);
    #endif
            bool createColorTexture = RequiresIntermediateColorTexture(ref cameraData);
            createColorTexture |= (rendererFeatures.Count != 0 && !isRunningHololens);
            createColorTexture &= !isPreviewCamera;

            // If camera requires depth and there's no depth pre-pass we create a depth texture that can be read later by effect requiring it.
            bool createDepthTexture = cameraData.requiresDepthTexture && !requiresDepthPrepass;
            createDepthTexture |= (cameraData.renderType == CameraRenderType.Base && !cameraData.resolveFinalTarget);

#if UNITY_ANDROID
                if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.Vulkan)
                {
                    // GLES can not use render texture's depth buffer with the color buffer of the backbuffer
                    // in such case we create a color texture for it too.
                    createColorTexture |= createDepthTexture;
                }
#endif

            // Configure all settings require to start a new camera stack (base camera only)
            if (cameraData.renderType == CameraRenderType.Base)
            {
                m_ActiveCameraColorAttachment = (createColorTexture) ? m_CameraColorAttachment : RenderTargetHandle.CameraTarget;
                m_ActiveCameraDepthAttachment = (createDepthTexture) ? m_CameraDepthAttachment : RenderTargetHandle.CameraTarget;

                bool intermediateRenderTexture = createColorTexture || createDepthTexture;

                // Doesn't create texture for Overlay cameras as they are already overlaying on top of created textures.
                CreateCameraRenderTarget(context, ref renderingData.cameraData);

                // if rendering to intermediate render texture we don't have to create msaa backbuffer
                int backbufferMsaaSamples = (intermediateRenderTexture) ? 1 : cameraTargetDescriptor.msaaSamples;

                if (Camera.main == camera && camera.cameraType == CameraType.Game && cameraData.targetTexture == null)
                    SetupBackbufferFormat(backbufferMsaaSamples, isStereoEnabled);
            }
            else
            {
                m_ActiveCameraColorAttachment = m_CameraColorAttachment;
                m_ActiveCameraDepthAttachment = m_CameraDepthAttachment;
            }

            ConfigureCameraTarget(m_ActiveCameraColorAttachment.Identifier(), m_ActiveCameraDepthAttachment.Identifier());

            for (int i = 0; i < rendererFeatures.Count; ++i)
            {
                if (rendererFeatures[i].isActive)
                    rendererFeatures[i].AddRenderPasses(this, ref renderingData);
            }

            int count = activeRenderPassQueue.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                if (activeRenderPassQueue[i] == null)
                    activeRenderPassQueue.RemoveAt(i);
            }
            bool hasPassesAfterPostProcessing = activeRenderPassQueue.Find(x => x.renderPassEvent == RenderPassEvent.AfterRendering) != null;

            if (mainLightShadows)
                EnqueuePass(m_MainLightShadowCasterPass);

            if (additionalLightShadows)
                EnqueuePass(m_AdditionalLightsShadowCasterPass);

            m_PointLightShadowCasterPass.Setup(ref renderingData, ref ShadowPointLightIndices);
            EnqueuePass(m_PointLightShadowCasterPass);

            int VisibilityLightIndex, FlashlightIndex;
            m_VisibilityPass.Setup(ref renderingData, out VisibilityLightIndex, out FlashlightIndex);
            EnqueuePass(m_VisibilityPass);

            if (requiresDepthPrepass)
            {
                m_DepthPrepass.Setup(cameraTargetDescriptor, m_DepthTexture);
                EnqueuePass(m_DepthPrepass);
            }

            if (generateColorGradingLUT)
            {
                m_ColorGradingLutPass.Setup(m_ColorGradingLut);
                EnqueuePass(m_ColorGradingLutPass);
            }

            // Opaque
            m_NormalsPass.Setup(m_NormalsTexture, m_NormalsDepthTexture);
            EnqueuePass(m_NormalsPass);

            m_VisibilityTexturePass.Setup(m_VisibilityTexture.Identifier(), m_BaseDepth.Identifier(), VisibilityLightIndex);
            EnqueuePass(m_VisibilityTexturePass);

            m_SpriteColorPass.Setup(new RenderTargetIdentifier[] { m_BaseColor.Identifier(), m_DiffuseColor.Identifier(), m_SpecularColor.Identifier() }, m_BaseDepth.Identifier());
            EnqueuePass(m_SpriteColorPass);

#if UNITY_EDITOR
            if(isSceneViewCamera)
            {
                m_EditorNormalsDiffusePass.Setup(new RenderTargetIdentifier[] { m_BaseColor.Identifier(), m_DiffuseColor.Identifier(), m_SpecularColor.Identifier() }, m_BaseDepth.Identifier());
                EnqueuePass(m_EditorNormalsDiffusePass);
            }
#endif // UNITY_EDITOR

            m_DeferredLightingPass.Setup(m_BaseColor, m_BaseDepth, in ShadowPointLightIndices, FlashlightIndex);
            EnqueuePass(m_DeferredLightingPass);

            //m_UpscaleBasePass.Setup(m_BaseColor);
            //m_UpscaleBasePass.ConfigureTarget(m_ActiveCameraColorAttachment.Identifier());
            //EnqueuePass(m_UpscaleBasePass);

            // Skybox and copy color
            bool isOverlayCamera = cameraData.renderType == CameraRenderType.Overlay;
            if (camera.clearFlags == CameraClearFlags.Skybox && RenderSettings.skybox != null && !isOverlayCamera)
                EnqueuePass(m_DrawSkyboxPass);

            if (renderingData.cameraData.requiresOpaqueTexture)
            {
                // TODO: Downsampling method should be store in the renderer instead of in the asset.
                // We need to migrate this data to renderer. For now, we query the method in the active asset.
                Downsampling downsamplingMethod = UniversalRenderPipeline.asset.opaqueDownsampling;
                m_CopyColorPass.Setup(m_ActiveCameraColorAttachment.Identifier(), m_OpaqueColor, downsamplingMethod);
                EnqueuePass(m_CopyColorPass);
            }

            // Transparent
//            m_ClearTargetsPass.ConfigureTarget(new RenderTargetIdentifier[] { m_DiffuseColor.Identifier(), m_SpecularColor.Identifier() }, m_BaseDepth.Identifier());
//            EnqueuePass(m_ClearTargetsPass);

//            m_TransparentNormalsPass.Setup(m_NormalsTexture, m_NormalsDepthTexture);
//            EnqueuePass(m_TransparentNormalsPass);

//            m_TransparentColorPass.Setup(new RenderTargetIdentifier[] { m_BaseColor.Identifier(), m_DiffuseColor.Identifier(), m_SpecularColor.Identifier() }, m_BaseDepth.Identifier());
//            EnqueuePass(m_TransparentColorPass);

//#if UNITY_EDITOR
//            if (isSceneViewCamera)
//            {
//                m_EditorTransparentNormalsDiffusePass.Setup(new RenderTargetIdentifier[] { m_BaseColor.Identifier(), m_DiffuseColor.Identifier(), m_SpecularColor.Identifier() }, m_BaseDepth.Identifier());
//                EnqueuePass(m_EditorNormalsDiffusePass);
//            }
//#endif // UNITY_EDITOR

//            m_TransparentLightingPass.Setup(m_BaseColor, m_BaseDepth, in ShadowPointLightIndices, FlashlightIndex);
//            EnqueuePass(m_TransparentLightingPass);

            m_UpscaleTransparentBasePass.Setup(m_BaseColor);
            m_UpscaleTransparentBasePass.ConfigureTarget(m_ActiveCameraColorAttachment.Identifier());
            EnqueuePass(m_UpscaleTransparentBasePass);

            // Post processing
            EnqueuePass(m_OnRenderObjectCallbackPass);

            bool lastCameraInTheStack = cameraData.resolveFinalTarget;
            bool hasCaptureActions = renderingData.cameraData.captureActions != null && lastCameraInTheStack;
            bool applyFinalPostProcessing = anyPostProcessing && lastCameraInTheStack &&
                                     renderingData.cameraData.antialiasing == AntialiasingMode.FastApproximateAntialiasing;

            // When post-processing is enabled we can use the stack to resolve rendering to camera target (screen or RT).
            // However when there are render passes executing after post we avoid resolving to screen so rendering continues (before sRGBConvertion etc)
            bool resolvePostProcessingToCameraTarget = !hasCaptureActions && !hasPassesAfterPostProcessing && !applyFinalPostProcessing;

            {

                if (lastCameraInTheStack)
                {
                    // Post-processing will resolve to final target. No need for final blit pass.
                    if (applyPostProcessing)
                    {
                        var destination = resolvePostProcessingToCameraTarget ? RenderTargetHandle.CameraTarget : m_AfterPostProcessColor;

                        // if resolving to screen we need to be able to perform sRGBConvertion in post-processing if necessary
                        bool doSRGBConvertion = resolvePostProcessingToCameraTarget;
                        m_PostProcessPass.Setup(cameraTargetDescriptor, m_ActiveCameraColorAttachment, destination, m_ActiveCameraDepthAttachment, m_ColorGradingLut, applyFinalPostProcessing, doSRGBConvertion);
                        EnqueuePass(m_PostProcessPass);
                    }

                    if (renderingData.cameraData.captureActions != null)
                    {
                        m_CapturePass.Setup(m_ActiveCameraColorAttachment);
                        EnqueuePass(m_CapturePass);
                    }

                    // if we applied post-processing for this camera it means current active texture is m_AfterPostProcessColor
                    var sourceForFinalPass = (applyPostProcessing) ? m_AfterPostProcessColor : m_ActiveCameraColorAttachment;

                    // Do FXAA or any other final post-processing effect that might need to run after AA.
                    if (applyFinalPostProcessing)
                    {
                        m_FinalPostProcessPass.SetupFinalPass(sourceForFinalPass);
                        EnqueuePass(m_FinalPostProcessPass);
                    }

                    // if post-processing then we already resolved to camera target while doing post.
                    // Also only do final blit if camera is not rendering to RT.
                    bool cameraTargetResolved =
                        // final PP always blit to camera target
                        (applyFinalPostProcessing ||
                        // no final PP but we have PP stack. In that case it blit unless there are render pass after PP
                        (applyPostProcessing && !hasPassesAfterPostProcessing) ||
                        // offscreen camera rendering to a texture, we don't need a blit pass to resolve to screen
                        m_ActiveCameraColorAttachment == RenderTargetHandle.CameraTarget);

                    // We need final blit to resolve to screen
                    if (!cameraTargetResolved)
                    {
                        m_FinalBlitPass.Setup(cameraTargetDescriptor, sourceForFinalPass);
                        EnqueuePass(m_FinalBlitPass);
                    }
                }

                // stay in RT so we resume rendering on stack after post-processing
                else if (applyPostProcessing)
                {
                    m_PostProcessPass.Setup(cameraTargetDescriptor, m_ActiveCameraColorAttachment, m_AfterPostProcessColor, m_ActiveCameraDepthAttachment, m_ColorGradingLut, false, false);
                    EnqueuePass(m_PostProcessPass);
                }

            }

    #if UNITY_EDITOR
            if (isSceneViewCamera)
            {
                // Scene view camera should always resolve target (not stacked)
                UnityEngine.Assertions.Assert.IsTrue(lastCameraInTheStack, "Editor camera must resolve target upon finish rendering.");
                m_SceneViewDepthCopyPass.Setup(m_DepthTexture);
                EnqueuePass(m_SceneViewDepthCopyPass);
            }
    #endif
        }

        /// <inheritdoc />
        public override void SetupLights(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            //m_ForwardLights.Setup(context, ref renderingData);
        }

        /// <inheritdoc />
        public override void SetupCullingParameters(ref ScriptableCullingParameters cullingParameters,
            ref CameraData cameraData)
        {
            // TODO: PerObjectCulling also affect reflection probes. Enabling it for now.
            // if (asset.additionalLightsRenderingMode == LightRenderingMode.Disabled ||
            //     asset.maxAdditionalLightsCount == 0)
            // {
            //     cullingParameters.cullingOptions |= CullingOptions.DisablePerObjectCulling;
            // }

            // We disable shadow casters if both shadow casting modes are turned off
            // or the shadow distance has been turned down to zero
            bool isShadowCastingDisabled = !UniversalRenderPipeline.asset.supportsMainLightShadows && !UniversalRenderPipeline.asset.supportsAdditionalLightShadows;
            bool isShadowDistanceZero = Mathf.Approximately(cameraData.maxShadowDistance, 0.0f);
            if (isShadowCastingDisabled || isShadowDistanceZero)
            {
                cullingParameters.cullingOptions &= ~CullingOptions.ShadowCasters;
            }

            // We set the number of maximum visible lights allowed and we add one for the mainlight...
            cullingParameters.maximumVisibleLights = UniversalRenderPipeline.maxVisibleAdditionalLights + 1;
            cullingParameters.shadowDistance = cameraData.maxShadowDistance;
        }

        /// <inheritdoc />
        public override void FinishRendering(CommandBuffer cmd)
        {
            if (m_ActiveCameraColorAttachment != RenderTargetHandle.CameraTarget)
            {
                cmd.ReleaseTemporaryRT(m_ActiveCameraColorAttachment.id);
                m_ActiveCameraColorAttachment = RenderTargetHandle.CameraTarget;
            }

            if (m_ActiveCameraDepthAttachment != RenderTargetHandle.CameraTarget)
            {
                cmd.ReleaseTemporaryRT(m_ActiveCameraDepthAttachment.id);
                m_ActiveCameraDepthAttachment = RenderTargetHandle.CameraTarget;
            }

            cmd.ReleaseTemporaryRT(m_DiffuseColor.id);
            cmd.ReleaseTemporaryRT(m_SpecularColor.id);
            cmd.ReleaseTemporaryRT(m_NormalsTexture.id);
            cmd.ReleaseTemporaryRT(m_BaseColor.id);
            cmd.ReleaseTemporaryRT(m_BaseDepth.id);
            cmd.ReleaseTemporaryRT(m_VisibilityTexture.id);

            cmd.ReleaseTemporaryRT(m_NormalsDepthTexture.id);
        }

        void CreateCameraRenderTarget(ScriptableRenderContext context, ref CameraData cameraData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(k_CreateCameraTextures);
            RenderTextureDescriptor descriptor = cameraData.cameraTargetDescriptor;

            int msaaSamples = descriptor.msaaSamples;
            if (m_ActiveCameraColorAttachment != RenderTargetHandle.CameraTarget)
            {
                bool useDepthRenderBuffer = m_ActiveCameraDepthAttachment == RenderTargetHandle.CameraTarget;
                var colorDescriptor = descriptor;
                colorDescriptor.depthBufferBits = (useDepthRenderBuffer) ? k_DepthStencilBufferBits : 0;
                cmd.GetTemporaryRT(m_ActiveCameraColorAttachment.id, colorDescriptor, FilterMode.Point);
            }

            if (m_ActiveCameraDepthAttachment != RenderTargetHandle.CameraTarget)
            {
                var depthDescriptor = descriptor;
                depthDescriptor.colorFormat = RenderTextureFormat.Depth;
                depthDescriptor.depthBufferBits = k_DepthStencilBufferBits;
                depthDescriptor.bindMS = msaaSamples > 1 && !SystemInfo.supportsMultisampleAutoResolve && (SystemInfo.supportsMultisampledTextures != 0);
                cmd.GetTemporaryRT(m_ActiveCameraDepthAttachment.id, depthDescriptor, FilterMode.Point);
            }

            Camera camera = cameraData.camera;
            if (camera)
            {
                PixelPerfectCamera PixelPerfectComponent = camera.GetComponent<PixelPerfectCamera>();
#if UNITY_EDITOR
                bool bIsPixelPerfectCamera = PixelPerfectComponent && (Application.isPlaying || PixelPerfectComponent.runInEditMode) && PixelPerfectComponent.enabled;
#else
                bIsPixelPerfectCamera = PixelPerfectComponent && PixelPerfectComponent.enabled;
#endif

                if (bIsPixelPerfectCamera)
                {
                    descriptor.width = PixelPerfectComponent.refResolutionX;
                    descriptor.height = PixelPerfectComponent.refResolutionY;

                    cmd.ClearRenderTarget(false, true, Color.black);
                }
            }

            RenderTextureDescriptor NormalsDepthDescriptor = descriptor;
            NormalsDepthDescriptor.depthBufferBits = 32;
            NormalsDepthDescriptor.colorFormat = RenderTextureFormat.Depth;
            cmd.GetTemporaryRT(m_NormalsDepthTexture.id, NormalsDepthDescriptor, FilterMode.Point);

            cmd.GetTemporaryRT(m_BaseDepth.id, NormalsDepthDescriptor, FilterMode.Point);

            RenderTextureDescriptor NormalsDescriptor = descriptor;
            NormalsDescriptor.depthBufferBits = 0;
            cmd.GetTemporaryRT(m_NormalsTexture.id, NormalsDescriptor, FilterMode.Point);

            RenderTextureDescriptor SpriteColorDescriptor = descriptor;
            SpriteColorDescriptor.depthBufferBits = 0;
            cmd.GetTemporaryRT(m_DiffuseColor.id, SpriteColorDescriptor, FilterMode.Point);
            cmd.GetTemporaryRT(m_BaseColor.id, SpriteColorDescriptor, FilterMode.Point);

            cmd.GetTemporaryRT(m_VisibilityTexture.id, SpriteColorDescriptor, FilterMode.Point);

            SpriteColorDescriptor.colorFormat = RenderTextureFormat.ARGB32;
            cmd.GetTemporaryRT(m_SpecularColor.id, SpriteColorDescriptor, FilterMode.Point);


            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        void SetupBackbufferFormat(int msaaSamples, bool stereo)
        {
    #if ENABLE_VR && ENABLE_VR_MODULE
                if (!stereo)
                    return;
            
                bool msaaSampleCountHasChanged = false;
                int currentQualitySettingsSampleCount = QualitySettings.antiAliasing;
                if (currentQualitySettingsSampleCount != msaaSamples &&
                    !(currentQualitySettingsSampleCount == 0 && msaaSamples == 1))
                {
                    msaaSampleCountHasChanged = true;
                }

                // There's no exposed API to control how a backbuffer is created with MSAA
                // By settings antiAliasing we match what the amount of samples in camera data with backbuffer
                // We only do this for the main camera and this only takes effect in the beginning of next frame.
                // This settings should not be changed on a frame basis so that's fine.
                if (msaaSampleCountHasChanged)
                {
                    QualitySettings.antiAliasing = msaaSamples;
                    XR.XRDevice.UpdateEyeTextureMSAASetting();
                }  
    #endif
        }

        /// <summary>
        /// Checks if the pipeline needs to create a intermediate render texture.
        /// </summary>
        /// <param name="cameraData">CameraData contains all relevant render target information for the camera.</param>
        /// <seealso cref="CameraData"/>
        /// <returns>Return true if pipeline needs to render to a intermediate render texture.</returns>
        bool RequiresIntermediateColorTexture(ref CameraData cameraData)
        {
            // When rendering a camera stack we always create an intermediate render texture to composite camera results.
            // We create it upon rendering the Base camera.
            if (cameraData.renderType == CameraRenderType.Base && !cameraData.resolveFinalTarget)
                return true;

            bool isSceneViewCamera = cameraData.isSceneViewCamera;
            var cameraTargetDescriptor = cameraData.cameraTargetDescriptor;
            int msaaSamples = cameraTargetDescriptor.msaaSamples;
            bool isStereoEnabled = false;
            bool isScaledRender = !Mathf.Approximately(cameraData.renderScale, 1.0f) && !isStereoEnabled;
            bool isCompatibleBackbufferTextureDimension = cameraTargetDescriptor.dimension == TextureDimension.Tex2D;
            bool requiresExplicitMsaaResolve = msaaSamples > 1 && !SystemInfo.supportsMultisampleAutoResolve;
            bool isOffscreenRender = cameraData.targetTexture != null && !isSceneViewCamera;
            bool isCapturing = cameraData.captureActions != null;

    #if ENABLE_VR && ENABLE_VR_MODULE
                if (isStereoEnabled)
                    isCompatibleBackbufferTextureDimension = UnityEngine.XR.XRSettings.deviceEyeTextureDimension == cameraTargetDescriptor.dimension;
    #endif

            bool requiresBlitForOffscreenCamera = cameraData.postProcessEnabled || cameraData.requiresOpaqueTexture || requiresExplicitMsaaResolve || !cameraData.isDefaultViewport;
            if (isOffscreenRender)
                return requiresBlitForOffscreenCamera;

            return requiresBlitForOffscreenCamera || isSceneViewCamera || isScaledRender || cameraData.isHdrEnabled ||
                   !isCompatibleBackbufferTextureDimension || isCapturing ||
                   (Display.main.requiresBlitToBackbuffer && !isStereoEnabled);
        }

        bool CanCopyDepth(ref CameraData cameraData)
        {
            bool msaaEnabledForCamera = cameraData.cameraTargetDescriptor.msaaSamples > 1;
            bool supportsTextureCopy = SystemInfo.copyTextureSupport != CopyTextureSupport.None;
            bool supportsDepthTarget = RenderingUtils.SupportsRenderTextureFormat(RenderTextureFormat.Depth);
            bool supportsDepthCopy = !msaaEnabledForCamera && (supportsDepthTarget || supportsTextureCopy);

            // TODO:  We don't have support to highp Texture2DMS currently and this breaks depth precision.
            // currently disabling it until shader changes kick in.
            //bool msaaDepthResolve = msaaEnabledForCamera && SystemInfo.supportsMultisampledTextures != 0;
            bool msaaDepthResolve = false;
            return supportsDepthCopy || msaaDepthResolve;
        }
    }
}
