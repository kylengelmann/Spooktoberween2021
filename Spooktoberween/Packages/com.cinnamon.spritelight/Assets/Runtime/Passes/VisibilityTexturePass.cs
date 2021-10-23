using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SpriteLightRendering
{
    public class VisibilityTexturePass : ScriptableRenderPass
    {
        readonly string m_PassName;
        ProfilingSampler m_ProfilingSampler;
        List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();
        Material m_VisiblityMaterial;

        static readonly string VisibilityShaderName = "Hidden/SpriteLight/Visibility";
        static readonly string VisibilityShaderTag = "VisibilityTexture";

        int VisibilityLightIndex;

        //RenderTargetIdentifier VisTex;
        //int VisTexID = Shader.PropertyToID("_VisibilityTexture");

        public VisibilityTexturePass(string PassName, RenderPassEvent passEvent) : base()
        {
            renderPassEvent = passEvent;

            m_PassName = PassName;
            m_ProfilingSampler = new ProfilingSampler(m_PassName);

            m_ShaderTagIdList.Add(new ShaderTagId(VisibilityShaderTag));

            m_VisiblityMaterial = new Material(Shader.Find(VisibilityShaderName));
        }

        public void Setup(RenderTargetIdentifier colorTarget, RenderTargetIdentifier depthTarget, int LightIndex)
        {
            ConfigureTarget(colorTarget, depthTarget);
            ConfigureClear(ClearFlag.Color, Color.white);
            VisibilityLightIndex = LightIndex;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if(VisibilityLightIndex < 0) return;
#if UNITY_EDITOR
            if (renderingData.cameraData.isSceneViewCamera) return;
#endif

            CommandBuffer commandBuffer = CommandBufferPool.Get(m_PassName);

            using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
            {
                context.ExecuteCommandBuffer(commandBuffer);
                commandBuffer.Clear();

                VisibleLight light = renderingData.lightData.visibleLights[VisibilityLightIndex];
                Matrix4x4 lightMatrix = light.localToWorldMatrix;
                lightMatrix = lightMatrix * Matrix4x4.Scale(light.range * Vector3.one);
                commandBuffer.DrawMesh(DeferredUtils.FullscreenQuadDoubleSided, Matrix4x4.identity, m_VisiblityMaterial, 0, -1);

                //commandBuffer.SetGlobalTexture(VisTexID, VisTex);

                context.ExecuteCommandBuffer(commandBuffer);
                commandBuffer.Clear();
            }

            context.ExecuteCommandBuffer(commandBuffer);
            CommandBufferPool.Release(commandBuffer);
        }
    }
}