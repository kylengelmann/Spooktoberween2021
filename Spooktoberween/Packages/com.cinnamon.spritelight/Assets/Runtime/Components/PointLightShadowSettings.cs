using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace SpriteLightRendering
{
    [ExecuteInEditMode]
    public class PointLightShadowSettings : MonoBehaviour
    {
        new Light light;
        public LightShadows lightShadowsMode = LightShadows.None;

        [Range(0f, 1f)]
        public float ShadowStrength = 1f;

        public enum ShadowBiasMode
        {
            PipelineSettings,
            Custom
        }
        public ShadowBiasMode shadowBiasMode;

        [Range(0f, 10f)]
        public float depthShadowBias = 1f;
        
        [Range(0f, 10f)]
        public float normalShadowBias = 1f;

        void Start()
        {
            UpdateShadowSettings();
        }

        public void UpdateShadowSettings()
        {
            if(!light)
            {
                light = GetComponent<Light>();
            }

            if(light && light.type == LightType.Point)
            {
                light.shadows = lightShadowsMode;
                light.shadowStrength = ShadowStrength;

                if(shadowBiasMode == ShadowBiasMode.PipelineSettings)
                {
                    UniversalAdditionalLightData lightData = GetComponent<UniversalAdditionalLightData>();
                    if(lightData)
                    {
                        lightData.usePipelineSettings = true;
                    }
                }
                else
                {
                    UniversalAdditionalLightData lightData = GetComponent<UniversalAdditionalLightData>();
                    if (lightData)
                    {
                        lightData.usePipelineSettings = false;
                    }

                    light.shadowBias = depthShadowBias;
                    light.shadowNormalBias = normalShadowBias;
                }
            }
        }

    #if UNITY_EDITOR
        private void Update()
        {
            if (!Application.isPlaying)
            {
                UpdateShadowSettings();
            }
        }

        private void OnValidate()
        {
            UpdateShadowSettings();
        }
    #endif
    }
}
