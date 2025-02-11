#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_2_0
#define PS_SHADERMODEL ps_2_0

float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;
 
texture ModelTexture;
sampler2D textureSampler = sampler_state {
    Texture = (WallTexture);
    MinFilter = Point;
    MagFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};
 
struct VertexShaderInput
{
    float4 Position : POSITION0;
	float4 Color : COLOR0;
    float4 Normal : NORMAL0;
    float2 TextureCoordinate : TEXCOORD0;
};
 
struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float3 Normal : TEXCOORD0;
    float2 TextureCoordinate : TEXCOORD1;
};
 
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
	
	VertexShaderOutput output;
    output.Position = mul(viewPosition, Projection);
	output.Color = input.Color;
    output.Normal = normalize(input.Normal);
    output.TextureCoordinate = input.TextureCoordinate;
	
    return output;
}
 
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    int lightTexU = round(input.TextureCoordinate.x * 4);
    int lightTexV = round(input.TextureCoordinate.y * 4);

    float xBright1 = lerp(input.Color.r, input.Color.g, 1 - (lightTexU / 4.0));
    float xBright2 = lerp(input.Color.b, input.Color.a, 1 - (lightTexU / 4.0));
    float realBright = lerp(xBright1, xBright2, 1 - (lightTexV / 4.0));
 
    float4 rawTextureColor = tex2D(textureSampler, input.TextureCoordinate);    
    float4 litTextureColor = float4(rawTextureColor.x * realBright, rawTextureColor.y * realBright, rawTextureColor.z * realBright, rawTextureColor.w);
    return litTextureColor;
}
 
technique Textured
{
    pass Pass1
    {
        VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}