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

float4 _PlayerWorldPosition;
float4 _PlayerViewPosition;

float GetStippleClip(float2 screenPos)
{
#ifdef UNITY_PIXEL_PERFECT
	screenPos.x = UVOffset.x + screenPos.x * UVOffset.z;
	screenPos.y = UVOffset.y + screenPos.y * UVOffset.w;
#endif	
	const float4x4 _StippleAlphas = { 31.f / 32.f,  15.f / 32.f,  27.f / 32.f,  11.f / 32.f,
				  7.f / 32.f,   23.f / 32.f,  3.f / 32.f,   19.f / 32.f,
				  25.f / 32.f,  9.f / 32.f,   29.f / 32.f,  13.f / 32.f,
				  1.f / 32.f,   17.f / 32.f,  5.f / 32.f,   21.f / 32.f };

	uint pixelX = screenPos.x % 4;
	uint pixelY = screenPos.y % 4;

	return _StippleAlphas[pixelX][pixelY];
}
#endif