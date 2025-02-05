#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0

float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;
 
float4 AmbientColor = float4(1, 1, 1, 1);
float AmbientIntensity = 1.0;
 
float3 DiffuseLightDirection = float3(1, 0, 0);
float4 DiffuseColor = float4(1, 1, 1, 1);
float DiffuseIntensity = 1.0;
 
float Shininess = 200;
float4 SpecularColor = float4(1, 1, 1, 1);
float SpecularIntensity = 1;
float3 ViewVector = float3(1, 0, 0);

float4 Brightness;
 
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
    VertexShaderOutput output;
 
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
 
    float4 normal = normalize(mul(input.Normal, WorldInverseTranspose));
    float lightIntensity = dot(normal, DiffuseLightDirection);
    output.Color = saturate(DiffuseColor * DiffuseIntensity * lightIntensity);
 
    output.Normal = normal;
 
    output.TextureCoordinate = input.TextureCoordinate;
    return output;
}
 
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    //float3 light = normalize(DiffuseLightDirection);
    float3 normal = normalize(input.Normal);
    //float3 r = normalize(2 * dot(light, normal) * normal - light);
    //float3 v = normalize(mul(normalize(ViewVector), World));
    //float dotProduct = dot(r, v);
 
    //float4 specular = SpecularIntensity * SpecularColor * max(pow(dotProduct, Shininess), 0) * length(input.Color);

    int lightTexU = round(input.TextureCoordinate.x * 4);
    int lightTexV = round(input.TextureCoordinate.y * 4);

    float xBright1 = lerp(Brightness.r, Brightness.g, 1 - (lightTexU / 4.0));
    float xBright2 = lerp(Brightness.b, Brightness.a, 1 - (lightTexU / 4.0));
    float realBright = lerp(xBright1, xBright2, 1 - (lightTexV / 4.0));
 
    float4 rawTextureColor = tex2D(textureSampler, input.TextureCoordinate);    
    float4 litTextureColor = float4(rawTextureColor.x * realBright, rawTextureColor.y * realBright, rawTextureColor.z * realBright, rawTextureColor.w);
    return litTextureColor;

    //return saturate(textureColor * (input.Color) + AmbientColor * AmbientIntensity + specular);
}
 
technique Textured
{
    pass Pass1
    {
        VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}