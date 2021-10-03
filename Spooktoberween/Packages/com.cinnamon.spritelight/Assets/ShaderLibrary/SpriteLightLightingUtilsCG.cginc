#ifndef SPRITELIGHT_LIGHTING_UTILS_CG_INCLUDED
#define SPRITELIGHT_LIGHTING_UTILS_CG_INCLUDED

float4 LightPosition;

float4 GetLightDirectionAndDistance(float3 viewPos)
{
	float4 lightDirAndDistance = 0.f;
#ifdef DIRECTIONAL_LIGHTING
	lightDirAndDistance.xyz = normalize(LightPosition.xyz);
#else
	float3 lightToPoint = viewPos - LightPosition.xyz;
	float lightDistance = length(lightToPoint);
	lightDirAndDistance.xyz = lightToPoint / lightDistance;
	lightDirAndDistance.w = lightDistance;
#endif
	return lightDirAndDistance;
}

float4 GetLightAttenuation(float lightDistance)
{
	float attenuation = 1.0;
#ifndef DIRECTIONAL_LIGHTING
	attenuation = 1.f - lightDistance / max(.0001f, LightPosition.a);
#ifdef ATTENUATION_INVERSE_SQUARED
	attenuation *= attenuation;
#endif // ATTENUATION_INVERSE_SQUARED
#endif // ndef DIRECTIONAL_LIGHTING
	return attenuation;
}

#endif