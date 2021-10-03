#ifndef SPRITELIGHT_NORMALS_INPUT_CG_INCLUDED
#define SPRITELIGHT_NORMALS_INPUT_CG_INCLUDED

#include "UnityCG.cginc"

CBUFFER_START(UnityPerMaterial)
fixed4 _RendererColor;
fixed2 _Flip;
fixed _AlphaCutoff;

Texture2D _MainTex;
Texture2D _Emissive;
Texture2D _Specular;

fixed4 _DiffuseMultiplier;
fixed4 _SpecularMultiplier;
fixed4 _EmissiveMultiplier;

fixed4 _Color;
CBUFFER_END

float4 _ShadowBias;
#endif