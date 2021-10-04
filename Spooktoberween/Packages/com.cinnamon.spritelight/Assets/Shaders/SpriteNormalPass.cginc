#ifndef SPRITE_NORMAL_PASS
#define SPRITE_NORMAL_PASS

#include "UnityCG.cginc"
#include "../ShaderLibrary/SpriteLightCommonCG.cginc"
#include "../ShaderLibrary/SpriteLightLitInputCG.cginc"

struct appdata
{
	float4 Position : POSITION;
	float3 Normal : NORMAL;
	float4 uv : TEXCOORD0;
};

struct v2f
{
	float4 Position : SV_POSITION;
	float4 uv : TEXCOORD0;
	float4 ViewNormal : TEXCOORD1;
};

v2f NormalVert(appdata v)
{
	v2f o;
	o.Position = UnityObjectToClipPos(v.Position);
	o.ViewNormal = mul(unity_MatrixITMV, float4(v.Normal.xyz, 0));
	o.uv = v.uv;
	return o;
}

SamplerState Point_Clamp_MainTexSampler
{
	Filter = POINT;
};

fixed4 NormalFrag(v2f i) : SV_Target
{
	half alpha = _MainTex.Sample(Point_Clamp_MainTexSampler, i.uv).a;
	clip(alpha - _AlphaCutoff);
	return EncodeNormal(i.ViewNormal);
}
#endif