Shader "SpriteLight/UpscaleBlit"
{
    Properties
    {
		_SrcBlendMode("Source Blend Mode", Float) = 0
		_DestBlendMode("Destination Blend Mode", Float) = 0
    }
    SubShader
    {
		Name "Upscale Blit"
		Tags {"LightMode" = "UpscaleBlit"}
        LOD 100

		ZWrite Off
		ZTest Always
		Cull Off

		Blend [_SrcBlendMode] [_DestBlendMode]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma shader_feature __ UNITY_PIXEL_PERFECT
			#pragma shader_feature __ UNITY_SCENE_VIEW

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            //Texture2D _LightResult;
			Texture2D _BlitTex;
			SamplerState Point_Clamp_BlitSampler;

			float4 UVOffset;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = v.vertex;
				o.uv = v.uv;
				o.uv.x = UVOffset.x + o.uv.x * UVOffset.z;
				o.uv.y = UVOffset.y + o.uv.y * UVOffset.w;
#if UNITY_UV_STARTS_AT_TOP && (defined(UNITY_PIXEL_PERFECT) || defined(UNITY_SCENE_VIEW))
				o.uv.y = 1.f - o.uv.y;
#endif
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				fixed4 color = _BlitTex.Sample(Point_Clamp_BlitSampler, i.uv);
				return color;
            }
            ENDCG
        }
    }
}
