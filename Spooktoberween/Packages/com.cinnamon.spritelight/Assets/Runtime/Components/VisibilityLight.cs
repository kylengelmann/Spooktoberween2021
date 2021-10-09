using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class VisibilityLight : MonoBehaviour
{
    int PlayerWorldPositionID;
    int PlayerViewPositionID;

    new Light light;

    void Start()
    {
        light = GetComponent<Light>();
        if (light)
        {
            light.renderingLayerMask = 1<<31;

            RenderPipelineManager.beginCameraRendering += OnFrameRender;
        }

        PlayerWorldPositionID = Shader.PropertyToID("_PlayerWorldPosition");
        PlayerViewPositionID = Shader.PropertyToID("_PlayerViewPosition");
    }

    private void OnDestroy()
    {
        RenderPipelineManager.beginCameraRendering -= OnFrameRender;
    }

    void OnFrameRender(ScriptableRenderContext context, Camera camera)
    {
        Vector4 PlayerWorldPosition = transform.position;
        Vector4 PlayerViewPosition = camera.worldToCameraMatrix.MultiplyPoint3x4(transform.position);
        PlayerWorldPosition.w = light.range;
        PlayerViewPosition.w = light.range;
        Shader.SetGlobalVector(PlayerWorldPositionID, PlayerWorldPosition);
        Shader.SetGlobalVector(PlayerViewPositionID, PlayerViewPosition);

    }
}
