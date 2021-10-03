#ifndef SPRITE_COLOR_PASS
#define SPRITE_COLOR_PASS

struct appdata_t
{
	float4 vertex   : POSITION;
	float4 color    : COLOR;
	float2 texcoord : TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
	float4 vertex   : SV_POSITION;
	fixed4 color : COLOR;
	float2 texcoord : TEXCOORD0;
};

inline float4 UnityFlipSprite(in float3 pos, in fixed2 flip)
{
	return float4(pos.xy * flip, pos.z, 1.0);
}

v2f SpriteVert(appdata_t IN)
{
	v2f OUT;

	UNITY_SETUP_INSTANCE_ID(IN);

	OUT.vertex = UnityFlipSprite(IN.vertex, _Flip);
	OUT.vertex = UnityObjectToClipPos(OUT.vertex);
	OUT.texcoord = IN.texcoord;
	OUT.color = IN.color * _Color * _RendererColor;

#ifdef PIXELSNAP_ON
	OUT.vertex = UnityPixelSnap(OUT.vertex);
#endif
	return OUT;
}

SamplerState Point_Clamp_GBufferSampler;

float4 _AmbientLightColor;

fixed4 SpriteFragBase(v2f i) : SV_Target
{
	float4 diffuse = _MainTex.Sample(Point_Clamp_GBufferSampler, i.texcoord);
	float alpha = diffuse.a;

	clip(alpha - _AlphaCutoff);

	diffuse.rgb *= _DiffuseMultiplier.rgb;
	float4 emissive = _Emissive.Sample(Point_Clamp_GBufferSampler, i.texcoord);
	float4 ambient = _AmbientLightColor * diffuse;
	return emissive + ambient;
}

struct PixelOutput
{
	fixed4 emissive : SV_Target;
	fixed4 diffuse : SV_Target1;
	fixed4 specular : SV_Target2;
};

PixelOutput SpriteFragLighting(v2f i)
{
	// Get alpha from emissive and clip if alpha is below alpha cutoff
	PixelOutput output;
	output.diffuse = _MainTex.Sample(Point_Clamp_GBufferSampler, i.texcoord) * i.color;
	float alpha = output.diffuse.a;

	clip(alpha - _AlphaCutoff);

	// Get diffuse and specular textures
	output.emissive = _Emissive.Sample(Point_Clamp_GBufferSampler, i.texcoord) * i.color;
	output.specular = _Specular.Sample(Point_Clamp_GBufferSampler, i.texcoord);

	// Calculate ambient and add it to emissive
	float4 ambient = _AmbientLightColor * output.diffuse;
	output.emissive += ambient;

	// Calculate diffuse and specular values
	output.emissive.rgb *= _EmissiveMultiplier.rgb;
	output.diffuse.rgb *= _DiffuseMultiplier.rgb;
	output.specular.rgb *= _SpecularMultiplier.rgb;

	return output;
}
#endif