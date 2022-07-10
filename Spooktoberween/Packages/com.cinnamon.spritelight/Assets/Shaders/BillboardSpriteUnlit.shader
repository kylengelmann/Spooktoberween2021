﻿Shader "SpriteLight/Unlit/BillboardSprite"
{
	Properties
	{
		[PerRendererData] _MainTex("Main Texture", 2D) = "white" {}
		[PerRendererData] _Color("Tint", Color) = (1,1,1,1)
		_AlphaCutoff("Alpha Cutoff", Range(0, 1)) = .3
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		[HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
		[HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
		[PerRendererData] _AlphaTex("External Alpha", 2D) = "white" {}
		[PerRendererData] _EnableExternalAlpha("Enable External Alpha", Float) = 0
		_StencilRef("Stencil Ref", Integer) = 0
		_StencilWriteMask("Stencil Write Mask", Integer) = 255
	}
	SubShader
	{
		Tags {"Queue" = "Geometry" "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

		Cull Off

		Pass
		{
			Name "LitSpriteColor"
			Tags { "LightMode" = "LitSpriteColor" }

			ZWrite Off

			Stencil
			{
				Ref [_StencilRef]
				WriteMask [_StencilWriteMask]
				Pass Replace
			}

			CGPROGRAM
				#pragma vertex SpriteVert
				#pragma fragment SpriteFragUnlit
				
				#include "UnityCG.cginc"
				#include "../ShaderLibrary/SpriteLightLitInputCG.cginc"
				#include "SpriteColorPass.cginc"

			ENDCG
		}
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
				#include "../ShaderLibrary/SpriteLightLitInputCG.cginc"
				#include "SpriteDepthPass.cginc"
			ENDCG
		}

		Pass
		{
			Tags{"LightMode" = "NormalsPass"}

			CGPROGRAM
			#pragma vertex NormalVert
			#pragma fragment NormalFrag

			#include "UnityCG.cginc"
			#include "../ShaderLibrary/SpriteLightLitInputCG.cginc"
			#include "SpriteNormalPass.cginc"
			ENDCG
		}

		Pass
		{
			Name "ShadowCaster"
			Tags{"LightMode" = "ShadowCaster"}

			HLSLPROGRAM
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
				#include "../ShaderLibrary/SpriteLightLitInputCG.cginc"
				#include "SpriteShadowCastPass.hlsl"
			ENDHLSL
		}
	}
}
