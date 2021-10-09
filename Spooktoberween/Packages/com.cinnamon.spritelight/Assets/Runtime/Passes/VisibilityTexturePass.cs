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

        public VisibilityTexturePass(string PassName, RenderPassEvent passEvent) : base()
        {
            renderPassEvent = passEvent;

            m_PassName = PassName;
            m_ProfilingSampler = new ProfilingSampler(m_PassName);

            m_ShaderTagIdList.Add(new ShaderTagId(VisibilityShaderTag));

            m_VisiblityMaterial = new Material(Shader.Find(VisibilityShaderName));
        }

        public void Setup(RenderTargetIdentifier colorTarget, int LightIndex)
        {
            ConfigureTarget(colorTarget);
            ConfigureClear(ClearFlag.Color, Color.black);
            VisibilityLightIndex = LightIndex;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer commandBuffer = CommandBufferPool.Get(m_PassName);

            using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
            {
                context.ExecuteCommandBuffer(commandBuffer);
                commandBuffer.Clear();

                VisibleLight light = renderingData.lightData.visibleLights[VisibilityLightIndex];
                Matrix4x4 lightMatrix = light.localToWorldMatrix;
                lightMatrix = lightMatrix * Matrix4x4.Scale(light.range * Vector3.one);
                commandBuffer.DrawMesh(DeferredUtils.SphereMesh, lightMatrix, m_VisiblityMaterial, 0, -1);

                context.ExecuteCommandBuffer(commandBuffer);
                commandBuffer.Clear();
            }

            context.ExecuteCommandBuffer(commandBuffer);
            CommandBufferPool.Release(commandBuffer);
        }
    }
}