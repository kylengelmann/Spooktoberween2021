using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ClearTargetsPass : ScriptableRenderPass
{
    string m_PassName;
    ClearFlag m_clearFlag = ClearFlag.None;
    Color m_clearColor = Color.black;

    public ClearTargetsPass(string PassName, RenderPassEvent passEvent) : base()
    {
        renderPassEvent = passEvent;

        m_PassName = PassName;
    }

    public void Setup(ClearFlag clearFlag, Color clearColor)
    {
        m_clearFlag = clearFlag;
        m_clearColor = clearColor;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer commandBuffer = CommandBufferPool.Get(m_PassName);
        Rect pixRect = renderingData.cameraData.camera.pixelRect;
        //commandBuffer.SetViewport(new Rect(Vector2.zero, new Vector2(Screen.width, Screen.height)));
        commandBuffer.ClearRenderTarget((m_clearFlag & ClearFlag.Depth) != 0, (m_clearFlag & ClearFlag.Color) != 0, m_clearColor);
        //commandBuffer.SetViewport(pixRect);
        context.ExecuteCommandBuffer(commandBuffer);
        CommandBufferPool.Release(commandBuffer);
    }
}
