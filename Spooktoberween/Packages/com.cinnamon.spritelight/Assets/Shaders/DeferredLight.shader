Shader "Hidden/SpriteLight/DeferredLight"
{
	Properties
	{
		[PerRendererData] LightColor("Color", Color) = (1,1,1,1)
		[PerRendererData] LightPosition("Position", Color) = (0, 0, 0, 0)
		[PerRendererData] ShadowStrength("Shadow Strengh", Range(0, 1)) = 1
	}
		SubShader
	{

		LOD 100

		Pass
		{
			Name "DeferredLightingPass"
			Tags{"LightMode" = "DeferredLighting"}

			ZWrite Off
			ZTest Always
			Cull Front
			Blend One One

			CGPROGRAM
			#pragma shader_feature_local DIRECTIONAL_LIGHTING POINT_LIGHTING
			#pragma shader_feature_local ATTENUATION_LINEAR ATTENUATION_INVERSE_SQUARED
			#pragma shader_feature_local SHADOWS_OFF SHADOWS_ON
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

            v2f vert (appdata v)
            {
                v2f o;
#ifdef DIRECTIONAL_LIGHTING
				o.vertex = v.vertex;

				o.viewPos = mul(unity_CameraInvProjection, v.vertex);
				o.viewPos.y = -o.viewPos.y;
#else
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.viewPos = UnityObjectToViewPos(v.vertex);
#endif
                return o;
            }

			float4 LightColor;
#ifdef SHADOWS_ON
			float ShadowStrength;
#endif // SHADOWS_ON

			Texture2D _NormalsDepth;
			Texture2D _NormalsTexture;
			Texture2D _DiffuseTexture;
			Texture2D _SpecularTexture;
			SamplerState Point_Clamp_GBufferSampler;

			float _PixelRatio;

			float4 frag(v2f i) : SV_Target
			{
				float4 screenPosTarget = GetScreenSpaceUV(i.vertex, false);
				float4 screenPosTexture = GetScreenSpaceUV(i.vertex, true);
#ifdef UNITY_PIXEL_PERFECT
				screenPosTarget *= _PixelRatio;
				screenPosTexture *= _PixelRatio;
#endif
				float4 screenPosDepth = screenPosTexture;
				
				float4 NormalsTexVal = _NormalsTexture.Sample(Point_Clamp_GBufferSampler, screenPosDepth);
				clip(NormalsTexVal.a - .2f);

				float3 normal = normalize(DecodeNormal(NormalsTexVal));

				float3 viewPos = i.viewPos;

				viewPos.z = GetViewDepth(_NormalsDepth.Sample(Point_Clamp_GBufferSampler, screenPosDepth));

				float4 lightDirAndDistance = GetLightDirectionAndDistance(viewPos);

				// Diffuse
				float nDotL = max(0.f, dot(normal, -lightDirAndDistance.xyz));
				float4 diffuse = _DiffuseTexture.Sample(Point_Clamp_GBufferSampler, screenPosTexture) * nDotL;

				// Specular
				float3 reflectedLightDir = 2.f*nDotL*normal + lightDirAndDistance.xyz;
				float3 viewDir = float3(0.f, 0.f, -1.f);
				float vDotL = max(0.f, dot(viewDir, -reflectedLightDir)) * max(min((nDotL)*100.f, 1.f), 0.f);

				float4 specColor = _SpecularTexture.Sample(Point_Clamp_GBufferSampler, screenPosTexture);

				// add a small amount to the power as spec was occasionally somehow NaN, this fixed the issue
				float spec = pow(vDotL, specColor.a*128.f + .001f);
				specColor *= spec;

				// Attenuation
				float attenuation = GetLightAttenuation(lightDirAndDistance.w);

				// Shadows
#ifdef SHADOWS_ON
				float3 positionWS = mul(unity_CameraToWorld, float4(viewPos, 1.0)).xyz;
				//return viewPos.xyzz;

				//return positionWS.xyzz;

				float4 shadowCoords = TransformWorldToShadowCoord(positionWS);

				//return shadowCoords;

				float ShadowAttenuation = SampleShadowMap(viewPos) * max(min((nDotL - .1f)*100.f, 1.f), 0.f);

				//return ShadowAttenuation;

				attenuation *= lerp(1.f - ShadowStrength, 1.f, ShadowAttenuation);
#endif // SHADOWS_ON
				LightColor *= max(attenuation, 0.f);

				float4 result = (diffuse + specColor) * LightColor;
				result.a = 0.f;
				return result;
			}
			ENDCG
        }
    }
}
