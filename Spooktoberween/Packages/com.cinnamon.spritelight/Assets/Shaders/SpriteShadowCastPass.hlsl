#ifndef SPRITE_SHADOW_CASTER_PASS_INCLUDED
#define SPRITE_SHADOW_CASTER_PASS_INCLUDED

struct Attributes
{
	float4 positionOS : POSITION;
	float3 normalOS : NORMAL;
	float4 uv : TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
	float4 positionCS : SV_POSITION;
	float4 uv : TEXCOORD0;
};

float3 ApplyShadowBias(float3 positionVS, float3 normalVS)
{
	float invNdotL = 1.0 - saturate(-normalVS.z);
	float scale = invNdotL * _ShadowBias.y;

	// normal bias is negative since we want to apply an inset normal offset
	positionVS = float3(0.f, 0.f, -1.f)  * _ShadowBias.xxx + positionVS;
	positionVS = normalVS * scale.xxx + positionVS;
	return positionVS;
}

float4 GetShadowPositionHClip(Attributes input)
{
	float3 positionVS = mul(UNITY_MATRIX_MV, float4(input.positionOS.xyz, 1.f)).xyz;
	float3 normalVS = mul(UNITY_MATRIX_IT_MV, float4(input.normalOS, 0.f)).xyz;

	float4 positionCS = mul(UNITY_MATRIX_P, float4(ApplyShadowBias(positionVS, normalVS), 1.f));

#if UNITY_REVERSED_Z
	positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
#else
	positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
#endif

	return positionCS;
}

Varyings ShadowPassVertex(Attributes input)
{
	Varyings output;
	UNITY_SETUP_INSTANCE_ID(input);

	output.positionCS = GetShadowPositionHClip(input);
	output.uv = input.uv;
	return output;
}

SamplerState Point_Clamp_MainTexSampler
{
	Filter = POINT;
};

half4 ShadowPassFragment(Varyings input) : SV_TARGET
{
	half alpha = _MainTex.Sample(Point_Clamp_MainTexSampler, input.uv.xy).a;
    clip(alpha - _AlphaCutoff);
	return 0;
}

#endif