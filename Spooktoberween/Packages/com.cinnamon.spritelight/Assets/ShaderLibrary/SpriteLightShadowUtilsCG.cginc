#ifndef SPRITELIGHT_SHADOW_UTILS_CG_INCLUDED
#define SPRITELIGHT_SHADOW_UTILS_CG_INCLUDED

#define MAX_SHADOW_CASCADES 4
float4x4 _MainLightWorldToShadow[MAX_SHADOW_CASCADES + 1];

#define MAX_POINT_LIGHT_SHADOWS 2
#define NUM_CUBE_SIDES 6
float4x4 _PointLightWorldToShadow[MAX_POINT_LIGHT_SHADOWS * NUM_CUBE_SIDES];

float4 _PointLightWorldPosition[MAX_POINT_LIGHT_SHADOWS];

int _LightIndex;

float4x4 _FlashlightWorldToShadow;

int GetPointLightWorldToShadowIndex(float3 positionWS)
{
#ifdef POINT_LIGHTING
	float3 lightToPosition = positionWS - _PointLightWorldPosition[_LightIndex].xyz;

	float absX = abs(lightToPosition.x);
	float absY = abs(lightToPosition.y);
	float absZ = abs(lightToPosition.z);

	if (absX > absY && absX > absZ)
	{
		if (lightToPosition.x > 0)
		{
			return 0;
		}
		else
		{
			return 1;
		}
	}
	else if(absY > absX && absY > absZ)
	{
		if (lightToPosition.y > 0)
		{
			return 2;
		}
		else
		{
			return 3;
		}
	}
	else
	{
		if (lightToPosition.z > 0)
		{
			return 4;
		}
		else
		{
			return 5;
		}
	}
#endif

	return 0;
}

float4 TransformWorldToShadowCoord(float3 positionWS)
{
#ifdef DIRECTIONAL_LIGHTING
//#ifdef _MAIN_LIGHT_SHADOWS_CASCADE
	//half cascadeIndex = ComputeCascadeIndex(positionWS);
//#else
	half cascadeIndex = 0;
//#endif

	return mul(_MainLightWorldToShadow[cascadeIndex], float4(positionWS, 1.0));
#else
#ifdef POINT_LIGHTING
	float4 result = mul(_PointLightWorldToShadow[_LightIndex*NUM_CUBE_SIDES + GetPointLightWorldToShadowIndex(positionWS)], float4(positionWS, 1.0));
	result = result / result.w;

	return result;
	//result.y = 1.f - result.y;
#else
#ifdef SPOT_LIGHTING
	float4 result = mul(_FlashlightWorldToShadow, float4(positionWS, 1.0));
	result = result / result.w;
	return result;
#endif
#endif
#endif
	//result = result / result.w;
	//result.y = 1.f - result.y;
	//return float4(1.f, 0.f, 0.f, 0.f);
}

Texture2D _MainLightShadowmapTexture;
Texture2D _PointLightShadowTexture;
Texture2D _VisibilityShadowTexture;
SamplerComparisonState Point_Clamp_Compare_ShadowSampler
{
	// sampler comparison state
	ComparisonFunc = LESS;
};

float SampleShadowMap(float3 positionVS)
{
	positionVS.z = -positionVS.z;

	float3 positionWS = mul(unity_CameraToWorld, float4(positionVS, 1.0)).xyz;

	float4 shadowCoords = TransformWorldToShadowCoord(positionWS.xyz);

	if (shadowCoords.z > 1.f || shadowCoords.z < 0.f)
	{
		return 1.f;
	}

#ifdef DIRECTIONAL_LIGHTING
	return _MainLightShadowmapTexture.SampleCmp(Point_Clamp_Compare_ShadowSampler, shadowCoords, shadowCoords.z);
#endif

#ifdef UNITY_REVERSED_Z
	float biasK = shadowCoords.z * .1f;
#else
	float biasK = (shadowCoords.z - 1.f) * .1f;
#endif
#ifdef POINT_LIGHTING
	return _PointLightShadowTexture.SampleCmp(Point_Clamp_Compare_ShadowSampler, shadowCoords, shadowCoords.z + biasK);
#endif
#ifdef SPOT_LIGHTING
	return _VisibilityShadowTexture.SampleCmp(Point_Clamp_Compare_ShadowSampler, shadowCoords, shadowCoords.z + biasK);
#endif
}

float4x4 _VisibilityWorldToShadow[NUM_CUBE_SIDES];

SamplerComparisonState Point_Clamp_Compare_VisibilityShadowSampler
{
	// sampler comparison state
	ComparisonFunc = LESS;
};

int GetVisibilityWorldToShadowIndex(float3 positionWS)
{
	float3 lightToPosition = positionWS - _PlayerWorldPosition.xyz;

	float absX = abs(lightToPosition.x);
	float absY = abs(lightToPosition.y);
	float absZ = abs(lightToPosition.z);

	if (absX > absY && absX > absZ)
	{
		if (lightToPosition.x > 0)
		{
			return 0;
		}
		else
		{
			return 1;
		}
	}
	else if (absY > absX && absY > absZ)
	{
		if (lightToPosition.y > 0)
		{
			return 2;
		}
		else
		{
			return 3;
		}
	}
	else
	{
		if (lightToPosition.z > 0)
		{
			return 4;
		}
		else
		{
			return 5;
		}
	}
}

float4 TransformVisibilityWorldToShadowCoord(float3 positionWS)
{
	float4 result = mul(_VisibilityWorldToShadow[GetVisibilityWorldToShadowIndex(positionWS)], float4(positionWS, 1.0));
	result = result / result.w;

	return result;
}

float SampleVisibilityShadowMap(float3 positionVS)
{
	positionVS.z = -positionVS.z;
	float3 positionWS = mul(unity_CameraToWorld, float4(positionVS, 1.0)).xyz;

	float4 shadowCoords = TransformVisibilityWorldToShadowCoord(positionWS);

	//if (shadowCoords.z > 1.f) || shadowCoords.z < 0.f)
	//{
	//	return 1.f;
	//}

	shadowCoords.z = clamp(shadowCoords.z, .001f, .999f);

#ifdef UNITY_REVERSED_Z
	float biasK = shadowCoords.z * .1f;
#else
	float biasK = (shadowCoords.z - 1.f) * .1f;
#endif

	return  _VisibilityShadowTexture.SampleCmp(Point_Clamp_Compare_VisibilityShadowSampler, shadowCoords, shadowCoords.z + biasK);
}
#endif