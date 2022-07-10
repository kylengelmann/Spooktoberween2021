#ifndef SPRITE_COLOR_PASS
#define SPRITE_COLOR_PASS

struct defaultColorAppdata
{
	float4 vertex   : POSITION;
	float4 color    : COLOR;
	float2 texcoord : TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct defaultColorV2f
{
	float4 vertex   : SV_POSITION;
	fixed4 color : COLOR;
	float2 texcoord : TEXCOORD0;
};

inline float4 UnityFlipSprite(in float3 pos, in fixed2 flip)
{
	return float4(pos.xy * flip, pos.z, 1.0);
};

defaultColorV2f SpriteVert(defaultColorAppdata IN)
{
	defaultColorV2f OUT;

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

fixed4 SpriteFragBase(defaultColorV2f i) : SV_Target
{
	float4 emissive = _Emissive.Sample(Point_Clamp_GBufferSampler, i.texcoord);
	emissive.rgb *= _EmissiveMultiplier.rgb;

	float4 diffuse = _MainTex.Sample(Point_Clamp_GBufferSampler, i.texcoord);
	diffuse.rgb *= _DiffuseMultiplier.rgb;
	float diffuseAlpha = diffuse.a * _DiffuseMultiplier.rgb;

	clip(diffuseAlpha - _AlphaCutoff);
	
	float4 ambient = _AmbientLightColor * diffuse;
	emissive.rgb += ambient.rgb;
	return emissive;
}

fixed4 SpriteFragBaseTransparent(defaultColorV2f i) : SV_Target
{
	float4 emissive = _Emissive.Sample(Point_Clamp_GBufferSampler, i.texcoord);
	emissive.rgb *= emissive.a;

	float4 diffuse = _MainTex.Sample(Point_Clamp_GBufferSampler, i.texcoord);
	diffuse.rgb *= _DiffuseMultiplier.rgb * diffuse.a;

	float4 ambient = _AmbientLightColor * diffuse;
	emissive.rgb += ambient.rgb;
	return emissive;
}

struct PixelOutput
{
	fixed4 emissive : SV_Target;
	fixed4 diffuse : SV_Target1;
	fixed4 specular : SV_Target2;
};

struct LightingParams
{
	fixed4 emissive;
	fixed4 diffuse;
	fixed4 specular;
};

LightingParams GetLightingParams(fixed2 texcoord)
{
	// Get alpha from diffuse and clip if alpha is below alpha cutoff
	LightingParams params;
	params.diffuse = _MainTex.Sample(Point_Clamp_GBufferSampler, texcoord);
	float alpha = params.diffuse.a;

	clip(alpha - _AlphaCutoff);

	// Get diffuse and specular textures
	params.emissive = _Emissive.Sample(Point_Clamp_GBufferSampler, texcoord);
	params.specular = _Specular.Sample(Point_Clamp_GBufferSampler, texcoord);

	return params;
}

PixelOutput SpriteFragLighting(defaultColorV2f i)
{
	// Get alpha from diffuse and clip if alpha is below alpha cutoff
	PixelOutput output;
	//output.diffuse = _MainTex.Sample(Point_Clamp_GBufferSampler, i.texcoord) * i.color;
	//float alpha = output.diffuse.a;

	//clip(alpha - _AlphaCutoff);

	//// Get diffuse and specular textures
	//output.emissive = _Emissive.Sample(Point_Clamp_GBufferSampler, i.texcoord) * i.color;
	//output.specular = _Specular.Sample(Point_Clamp_GBufferSampler, i.texcoord);

	LightingParams params = GetLightingParams(i.texcoord);
	output.emissive = params.emissive * i.color;
	output.diffuse = params.diffuse * i.color;
	output.specular = params.specular;

	// Calculate diffuse and specular values
	output.emissive *= _EmissiveMultiplier;
	output.diffuse *= _DiffuseMultiplier;
	output.specular *= _SpecularMultiplier;

	// Calculate ambient and add it to emissive
	float4 ambient = _AmbientLightColor * output.diffuse;
	output.emissive.rgb += ambient.rgb;

	return output;
}

PixelOutput SpriteFragLightingTransparent(defaultColorV2f i)
{
	PixelOutput output;
	
	output.diffuse = _MainTex.Sample(Point_Clamp_GBufferSampler, i.texcoord) * i.color;
	output.diffuse *= _DiffuseMultiplier;
	output.diffuse.rgb *= output.diffuse.a;
	
	output.emissive = _Emissive.Sample(Point_Clamp_GBufferSampler, i.texcoord) * i.color;
	output.emissive *= _EmissiveMultiplier;
	output.emissive.rgb *= output.emissive.a;
	
	output.specular = _Specular.Sample(Point_Clamp_GBufferSampler, i.texcoord);
	output.specular *= _SpecularMultiplier;

	// Calculate ambient and add it to emissive
	float4 ambient = _AmbientLightColor * output.diffuse;
	
	output.emissive.rgb += ambient.rgb;

	return output;
}

PixelOutput SpriteFragUnlit(defaultColorV2f i)
{
	PixelOutput output;

	output.emissive = _MainTex.Sample(Point_Clamp_GBufferSampler, i.texcoord) * i.color;
	float alpha = output.emissive.a;

	clip(alpha - _AlphaCutoff);

	output.diffuse = 0.f;
	output.specular = 0.f;

	return output;
}
#endif