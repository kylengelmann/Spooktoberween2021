Shader "Spooktober/Spooks/SpookyThing"
{
	Properties
	{
		_MainTex("Main Texture", 2D) = "white" {}
		_Emissive("Emissive Texture", 2D) = "black" {}
		_Specular("Specular Texture", 2D) = "white" {}
		[PerRendererData] _Color("Tint", Color) = (1,1,1,1)
		_AlphaCutoff("Alpha Cutoff", Range(0, 1)) = .3
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		[HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
		[HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
		[PerRendererData] _AlphaTex("External Alpha", 2D) = "white" {}
		[PerRendererData] _EnableExternalAlpha("Enable External Alpha", Float) = 0
		_EmissiveMultiplier("Emissive Multiplier", Color) = (0, 0, 0, 1)
		_DiffuseMultiplier("Diffuse Multiplier", Color) = (1, 1, 1, 1)
		_SpecularMultiplier("Specular Multiplier", Color) = (1, 1, 1, 1)
		_FocusedEmissiveMultiplier("Focused Emissive Multiplier", Color) = (1, 1, 1, 1)
		_FocusedDiffuseMultiplier("Focused Diffuse Multiplier", Color) = (.5, .5, .5, 1)
		_PossessedColorSubtraction("Possessed Color Subtraction", Color) = (.5, .8, .8, 0)
		[PerRendererData] _TimeFocused("Time Focused", Float) = -100
		[PerRendererData] _TimeUnfocused("Time Unfocused", Float) = -100
		[PerRendererData] _TimePossessed("Time Possessed", Float) = -100
		_FocusTransitionTime("Focus Transition Time", Float) = .5
		_PossessTransitionTime("Possess Transition Time", Float) = .5
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
				#pragma vertex SpookyVert
				#pragma fragment SpookyFrag

				#include "UnityCG.cginc"
				#include "SpookyThingInputCG.cginc"
				#include "/Packages/com.cinnamon.spritelight/Assets/Shaders/SpriteColorPass.cginc"

				struct spookyV2f
				{
					float4 vertex : SV_POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
					float2 spookyParams : TEXCOORD1;
				};

				float _FocusTransitionTime;
				float _PossessTransitionTime;

				spookyV2f SpookyVert(defaultColorAppdata IN)
				{
				 	defaultColorV2f defaultV2f = SpriteVert(IN);

					spookyV2f v2f;
					v2f.vertex = defaultV2f.vertex;
					v2f.color = defaultV2f.color;
					v2f.texcoord = defaultV2f.texcoord;
					
					float focusProgress = clamp((_Time.y - _TimeFocused) / _FocusTransitionTime, 0, 1);
					float unfocusProgress = _TimeUnfocused > _TimeFocused ? clamp((_Time.y - _TimeUnfocused) / _FocusTransitionTime, 0, 1) : 0;
					float possessionProgress = _TimePossessed > 0 ? clamp((_Time.y - _TimePossessed) / _PossessTransitionTime, 0, 1) : 0;

					v2f.spookyParams = fixed2(focusProgress * (1 - unfocusProgress), possessionProgress);

					return v2f;
				};

				PixelOutput SpookyFrag(spookyV2f i)
				{
					LightingParams params = GetLightingParams(i.texcoord);
					PixelOutput output;
					output.emissive = params.emissive * i.color;
					output.diffuse = params.diffuse * i.color;
					output.specular = params.specular;

					output.emissive *= lerp(_EmissiveMultiplier, _FocusedEmissiveMultiplier, i.spookyParams.x);
					output.diffuse *= lerp(_DiffuseMultiplier, _FocusedDiffuseMultiplier, i.spookyParams.x);

					float4 ambient = _AmbientLightColor * output.diffuse;
					output.emissive.rgb += ambient.rgb;

					output.emissive -= _PossessedColorSubtraction * i.spookyParams.y;
					output.diffuse -= _PossessedColorSubtraction * i.spookyParams.y;

					return output;
				};

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
				#include "SpookyThingInputCG.cginc"
				#include "/Packages/com.cinnamon.spritelight/Assets/Shaders/SpriteDepthPass.cginc"
			ENDCG
		}

		Pass
		{
			Tags{"LightMode" = "NormalsPass"}

			CGPROGRAM
			#pragma vertex NormalVert
			#pragma fragment NormalFrag

			#include "UnityCG.cginc"
			#include "SpookyThingInputCG.cginc"
			#include "/Packages/com.cinnamon.spritelight/Assets/Shaders/SpriteNormalPass.cginc"
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
				#include "SpookyThingInputCG.cginc"
				#include "/Packages/com.cinnamon.spritelight/Assets/Shaders/SpriteShadowCastPass.hlsl"
			ENDHLSL
		}
	}
}
