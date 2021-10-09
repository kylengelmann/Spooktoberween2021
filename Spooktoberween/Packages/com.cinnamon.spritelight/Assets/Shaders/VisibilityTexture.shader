Shader "Hidden/SpriteLight/Visibility"
{
    Properties
    {
    }
    SubShader
    {
        LOD 100

        Pass
        {
            Name "VisibilityTexturePass"
            Tags{"LightMode" = "VisibilityTexture"}

            ZWrite Off
            ZTest Always
            Cull Front

            CGPROGRAM
            #pragma shader_feature __ UNITY_PIXEL_PERFECT

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "../ShaderLibrary/SpriteLightCommonCG.cginc"
            #include "../ShaderLibrary/SpriteLightShadowUtilsCG.cginc"
            #include "../ShaderLibrary/SpriteLightLightingUtilsCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 viewPos : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.viewPos = UnityObjectToViewPos(v.vertex);
                return o;
            }

            Texture2D _NormalsDepth;
            SamplerState Point_Clamp_GBufferSampler;

            float _PixelRatio;

            float4 frag(v2f i) : SV_Target
            {
                float4 screenPosTexture = GetScreenSpaceUV(i.vertex, true);
#ifdef UNITY_PIXEL_PERFECT
                screenPosTexture *= _PixelRatio;
#endif
                float4 screenPosDepth = screenPosTexture;

                float3 viewPos = i.viewPos;

                viewPos.z = GetViewDepth(_NormalsDepth.Sample(Point_Clamp_GBufferSampler, screenPosDepth));

                float PlayerDistance = length(_PlayerViewPosition.xy - viewPos.xy);

                float DistanceAttenuation = (_PlayerViewPosition.w - PlayerDistance) / (_PlayerViewPosition.w + .001f);
                //float DiatanceAttenuation = 1.f;

                float ShadowAttenuation = SampleVisibilityShadowMap(viewPos);

                //return SampleVisibilityShadowMap(viewPos);

                return ShadowAttenuation * DistanceAttenuation;
            }

            ENDCG
        }
    }
}
