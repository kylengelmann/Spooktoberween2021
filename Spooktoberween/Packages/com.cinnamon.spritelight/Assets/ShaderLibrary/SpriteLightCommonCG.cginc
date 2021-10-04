#ifndef SPRITELIGHT_COMMON_CG_INCLUDED
#define SPRITELIGHT_COMMON_CG_INCLUDED

float4 EncodeNormal(float3 normal)
{
	normal = normalize(normal);
	return float4((normal.x + 1.f)/2.f, (normal.y + 1.f)/2.f, (normal.z + 1.f)/2.f, 1.f);
}

float3 DecodeNormal(float4 encodedNormal)
{
	return float3(encodedNormal.x*2.f - 1.f, encodedNormal.y*2.f - 1.f, encodedNormal.z*2.f - 1.f);
}

float4 UVOffset;
float4 GetScreenSpaceUV(float4 ClippedPos, bool bIsRenderTexture, bool bIsLowRes = false)
{
	float4 screenPos = ClippedPos;
	screenPos.x = screenPos.x / _ScreenParams.x;
	screenPos.y = screenPos.y / _ScreenParams.y;

#ifdef UNITY_PIXEL_PERFECT
        UNITY_BRANCH
        if(bIsLowRes) 
	{
          screenPos.x = UVOffset.x + screenPos.x * UVOffset.z;
          screenPos.y = UVOffset.y + screenPos.y * UVOffset.w;
        }
#endif

#if UNITY_UV_STARTS_AT_TOP && !defined(RENDERING_TO_TEMP_TARGET)
	if (!bIsRenderTexture)
	{
		screenPos.y = 1.f - screenPos.y;
	}
#endif
	return screenPos;
}

float GetViewDepth(float4 depth)
{
	float rawDepth = DecodeFloatRGBA(depth);
	
#ifdef UNITY_REVERSED_Z
	rawDepth = 1.f - rawDepth;
#endif

	float viewDepth = (_ProjectionParams.y -_ProjectionParams.z)*rawDepth - _ProjectionParams.y;

	return viewDepth;
}

#endif