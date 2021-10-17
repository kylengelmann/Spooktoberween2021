Shader "Hidden/SpriteLight/Visibility"
{
    Properties
    {
        _StencilRef("Stencil Ref", Int) = 128
        _StencilReadMask("Stencil Read Mask", Int) = 128
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

            Stencil
            {
                Ref [_StencilRef]
                WriteMask [_StencilReadMask]
                Comp NotEqual
            }

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

			float4 _PlayerViewBounds;
			float4 _PlayerViewBoundsParams;

            float4 frag(v2f i) : SV_Target
            {
                float4 screenPosTexture = GetScreenSpaceUV(i.vertex, true);
#ifdef UNITY_PIXEL_PERFECT
                screenPosTexture *= _PixelRatio;
#endif
                float4 screenPosDepth = screenPosTexture;

                float3 viewPos = i.viewPos;

                viewPos.z = GetViewDepth(_NormalsDepth.Sample(Point_Clamp_GBufferSampler, screenPosDepth));

				float2 ToPlayer = _PlayerViewPosition.xy - viewPos.xy;

                float PlayerDistance = length(ToPlayer);

                float DistanceAttenuation = max(0.f, (_PlayerViewPosition.w - PlayerDistance) / (_PlayerViewPosition.w + .001f));
                //float DiatanceAttenuation = 1.f;

				float BoundsLeftPerpDist = dot(ToPlayer, float2(-_PlayerViewBounds.y, _PlayerViewBounds.x));
				float BoundsLeftParDist = -(dot(ToPlayer, float2(_PlayerViewBounds.x, _PlayerViewBounds.y)));
				float BoundsLeftEffectiveDist = max(BoundsLeftPerpDist - (BoundsLeftParDist * _PlayerViewBoundsParams.y), 0.f);

				//return BoundsLeftEffectiveDist;

				float BoundsRightPerpDist = dot(ToPlayer, float2(_PlayerViewBounds.w, -_PlayerViewBounds.z));
				float BoundsRightParDist = -(dot(ToPlayer, float2(_PlayerViewBounds.z, _PlayerViewBounds.w)));
				float BoundsRightEffectiveDist = max(BoundsRightPerpDist - (BoundsRightParDist * _PlayerViewBoundsParams.y), 0.f);

				//return BoundsRightEffectiveDist + BoundsLeftEffectiveDist;

				//return (BoundsLeftParDist < 0.f && BoundsRightParDist < 0.f) ? PlayerDistance : max(BoundsLeftEffectiveDist, BoundsRightEffectiveDist);

				float BoundsAttenuation = 1.f - ( (BoundsLeftParDist < 0.f && BoundsRightParDist < 0.f) ? PlayerDistance : max(BoundsLeftEffectiveDist, BoundsRightEffectiveDist)) / _PlayerViewBoundsParams.x;


                float ShadowAttenuation = SampleVisibilityShadowMap(viewPos);

                //return SampleVisibilityShadowMap(viewPos);

                return ShadowAttenuation * DistanceAttenuation * BoundsAttenuation;
            }

            ENDCG
        }
    }
}
