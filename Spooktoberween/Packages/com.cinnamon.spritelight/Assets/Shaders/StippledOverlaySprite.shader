Shader "SpriteLight/Opaque/StippledOverlaySprite"
{
    Properties
    {
		[PerRendererData]_MainTex("Main Texture", 2D) = "white" {}
		_Emissive("Emissive Texture", 2D) = "black" {}
		_Specular("Specular Texture", 2D) = "white" {}
		[PerRendererData] _Color("Tint", Color) = (1,1,1,1)
		_AlphaCutoff("Alpha Cutoff", Range(0, 1)) = .3
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		[HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
		[HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
		[PerRendererData] _AlphaTex("External Alpha", 2D) = "white" {}
		[PerRendererData] _EnableExternalAlpha("Enable External Alpha", Float) = 0
		_EmissiveMultiplier("Emissive Multiplier", Color) = (1, 1, 1, 1)
		_DiffuseMultiplier("Diffuse Multiplier", Color) = (1, 1, 1, 1)
		_SpecularMultiplier("Specular Multiplier", Color) = (1, 1, 1, 1)
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
				Ref[_StencilRef]
				WriteMask[_StencilWriteMask]
				Pass Replace
			}

			CGPROGRAM
				#pragma vertex SpriteVert
				#pragma fragment StippledSpriteFragLighting

				#include "UnityCG.cginc"
				#include "../ShaderLibrary/SpriteLightLitInputCG.cginc"
				#include "SpriteColorPass.cginc"
				#include "../ShaderLibrary/SpriteLightCommonCG.cginc"

				PixelOutput StippledSpriteFragLighting(defaultColorV2f i)
				{
					clip(i.color.a - GetStippleClip(i.vertex.xy));

					return SpriteFragLighting(i);
				}

			ENDCG
		}
    }
}