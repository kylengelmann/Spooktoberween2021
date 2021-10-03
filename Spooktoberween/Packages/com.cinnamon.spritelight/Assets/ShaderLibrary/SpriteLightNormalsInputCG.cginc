#ifndef SPRITELIGHT_NORMALS_INPUT_CG_INCLUDED
#define SPRITELIGHT_NORMALS_INPUT_CG_INCLUDED

CBUFFER_START(UnityPerMaterial)
Texture2D _MainTex;
float _AlphaCutoff;
CBUFFER_END

float4 _ShadowBias;

#endif