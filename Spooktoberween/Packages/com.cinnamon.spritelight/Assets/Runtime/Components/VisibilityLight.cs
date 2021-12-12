using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class VisibilityLight : MonoBehaviour
{
    public float VisibilityBoundsAngle = 60f;
    public float VisibilityBoundsDistanceFalloff = 1f;
    public float VisibilityBoundsFalloffSlope = .2f;

    int PlayerWorldPositionID;
    int PlayerViewPositionID;
    int PlayerViewBoundsID;
    int PlayerViewBoundsParamsID;

    new Light light;

    [System.NonSerialized] public Vector2 ViewDir = Vector2.right;

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
        PlayerViewBoundsID = Shader.PropertyToID("_PlayerViewBounds");
        PlayerViewBoundsParamsID = Shader.PropertyToID("_PlayerViewBoundsParams");
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

        Vector3 PlayerDirectionView = new Vector3(ViewDir.x, 0f, ViewDir.y);

        Quaternion BoundsRotation = Quaternion.Euler(0f, VisibilityBoundsAngle, 0f);
        Vector3 LeftBounds = BoundsRotation * PlayerDirectionView;
        Vector3 RightBounds = Quaternion.Inverse(BoundsRotation) * PlayerDirectionView;
        Shader.SetGlobalVector(PlayerViewBoundsID, new Vector4(LeftBounds.x, LeftBounds.z, RightBounds.x, RightBounds.z));
        Shader.SetGlobalVector(PlayerViewBoundsParamsID, new Vector4(VisibilityBoundsDistanceFalloff, VisibilityBoundsFalloffSlope, 0f, 0f));
    }

    private void OnEnable()
    {
        if(light)
        {
            light.enabled = true;
            RenderPipelineManager.beginCameraRendering += OnFrameRender;
        }
    }

    private void OnDisable()
    {
        if (light)
        {
            light.enabled = false;
            RenderPipelineManager.beginCameraRendering -= OnFrameRender;
        }
    }
}
