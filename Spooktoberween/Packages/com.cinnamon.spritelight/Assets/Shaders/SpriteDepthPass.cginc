#ifndef SPRITE_DEPTH_PASS
#define SPRITE_DEPTH_PASS

struct appdata
{
	float4 Position : POSITION;
	float4 uv : TEXCOORD0;
};

struct v2f
{
	float4 Position : SV_POSITION;
	float4 uv : TEXCOORD0;
};

v2f DepthVert(appdata v)
{
	v2f o;
	o.Position = UnityObjectToClipPos(v.Position);
	o.uv = v.uv;
	return o;
}

SamplerState Point_Clamp_MainTexSampler
{
	Filter = POINT;
};

half4 DepthFrag(v2f i) : SV_TARGET
{
	half alpha = _MainTex.Sample(Point_Clamp_MainTexSampler, i.uv).a;
	clip(alpha - _AlphaCutoff);
	return 0;
}
#endif