Shader "SpriteLight/Normals"
{
    Properties
    {
		[PerRendererData] _MainTex("Main Texture", 2D) = "white" {}
		_AlphaCutoff("Alpha Cutoff", Range(0, 1)) = .3
    }
    SubShader
    {
        LOD 100

		Pass
		{
			Name "DepthOnly"
			Tags{"LightMode" = "DepthOnly"}

			ZWrite On
			ColorMask 0
			
			CGPROGRAM
				#pragma vertex DepthVert
				#pragma fragment DepthFrag

				#include "UnityCG.cginc"
				#include "../ShaderLibrary/SpriteLightNormalsInputCG.cginc"
				#include "SpriteDepthPass.cginc"
			ENDCG
		}

        Pass
        {
			Tags{"LightMode" = "NormalsPass"}

			ZWrite On

            CGPROGRAM
            #pragma vertex NormalVert
            #pragma fragment NormalFrag

			#include "UnityCG.cginc"
			#include "../ShaderLibrary/SpriteLightNormalsInputCG.cginc"
			#include "SpriteNormalPass.cginc"
            ENDCG
        }

		Pass
		{
			Name "ShadowCaster"
			Tags{"LightMode" = "ShadowCaster"}

			ZWrite On
			ZTest LEqual
			Cull[_Cull]

			CGPROGRAM
			// Required to compile gles 2.0 with standard srp library
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			#pragma target 2.0

			// -------------------------------------
			// Material Keywords
			#pragma shader_feature _ALPHATEST_ON

			//--------------------------------------
			// GPU Instancing
			#pragma multi_compile_instancing
			#pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

			#pragma vertex ShadowPassVertex
			#pragma fragment ShadowPassFragment
			
			#include "UnityCG.cginc"
			#include "../ShaderLibrary/SpriteLightNormalsInputCG.cginc"
			#include "SpriteShadowCastPass.hlsl"
			ENDCG
		}
    }
}
