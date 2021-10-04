Shader "Hidden/SpriteLight/EditorNormalsDiffuse"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

		ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
				o.vertex = v.vertex;
                o.vertex = UnityObjectToClipPos(o.vertex);

#if defined(UNITY_REVERSED_Z)
				o.vertex.z = 0.f;
#else
				o.vertex.z = 1.f;
#endif
                return o;
            }

            fixed4 frag (v2f i) : SV_Target1
            {
				return fixed4(.5f, .5f, .5f, 1.f);
            }
            ENDCG
        }
    }
}
