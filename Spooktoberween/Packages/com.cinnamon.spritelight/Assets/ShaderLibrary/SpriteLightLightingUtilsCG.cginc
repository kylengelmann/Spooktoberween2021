#ifndef SPRITELIGHT_LIGHTING_UTILS_CG_INCLUDED
#define SPRITELIGHT_LIGHTING_UTILS_CG_INCLUDED

float4 LightPosition;
float4 SpotlightDirection;
float4 SpotlightDirDotLRange;

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

float4 GetLightAttenuation(float4 lightDirAndDistance)
{
	float attenuation = 1.0;
#ifndef DIRECTIONAL_LIGHTING
	attenuation = 1.f - lightDirAndDistance.w / max(.0001f, LightPosition.a);
#ifdef ATTENUATION_INVERSE_SQUARED
	attenuation *= attenuation;
#endif // ATTENUATION_INVERSE_SQUARED
#endif // ndef DIRECTIONAL_LIGHTING

#ifdef SPOT_LIGHTING
	float dirDotL = acos(dot(lightDirAndDistance.xyz, SpotlightDirection.xyz));
	float dirAttenuation = 1.f - clamp((dirDotL - SpotlightDirDotLRange.x) / max(.0001f, (SpotlightDirDotLRange.y - SpotlightDirDotLRange.x)), 0.f, 1.f);

	attenuation *= dirAttenuation;
#endif // SPOT_LIGHTING
	return attenuation;
}

#endif