#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_3_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float global_gamma = 0.0f;

sampler s0;
sampler paletteSprite = sampler_state
{
    Filter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};

float4 PixelShaderFunction(float4 position : SV_POSITION, float4 color1 : COLOR0, float2 texCoord : TEXCOORD0) : SV_TARGET0
{
    float4 color = tex2D(s0, texCoord);

    if (color.a < 1.0f)
        discard;
	
    if (color1.b <= 0.0f || global_gamma <= -1.0f)
        return float4(0, 0, 0, 1);
    
    float brightness = color.b + global_gamma + ((color1.b - 0.5f) * 2.0f);
    if (color1.r < 1.0f)
    {
        float2 palCoord = float2(color1.r, brightness);
        return tex2D(paletteSprite, palCoord);
    }

    float2 palCoord = float2(color.r, brightness);
    return tex2D(paletteSprite, palCoord);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}