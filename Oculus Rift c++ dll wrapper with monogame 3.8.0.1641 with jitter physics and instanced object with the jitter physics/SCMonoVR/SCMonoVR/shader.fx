//float4x4 WVP;
float4x4 World;
float4x4 View;
float4x4 Projection;

texture cubeTexture;

sampler TextureSampler = sampler_state
{
    texture = <cubeTexture>;
    mipfilter = LINEAR;
    minfilter = LINEAR;
    magfilter = LINEAR;
};

struct InstancingVSinput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};






struct InstancingVSoutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};
//POSITION1 //NORMAL //BLENDWEIGHT//float4 instanceDirForward : POSITION2, float4 instanceDirRight : POSITION3, float4 instanceDirUp  : POSITION4 // float4x4 instanceDird : POSITION2,
InstancingVSoutput InstancingVS(InstancingVSinput input, float4x4 instanceTransform : POSITION1, float2 atlasCoord : TEXCOORD1)
{
    InstancingVSoutput output;

    float4 mod_input_vertex_pos = input.Position;

    instanceTransform._41 *= 2;
    instanceTransform._42 *= 2;
    instanceTransform._43 *= 2;

    input.Position.x += instanceTransform._41;
    input.Position.y += instanceTransform._42;
    input.Position.z += instanceTransform._43;


    float4 worldPos = mul(input.Position, transpose(instanceTransform));
    float4 worldPosition = mul(worldPos, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);


    /*instanceTransform.x *= 2;
    instanceTransform.y *= 2;
    instanceTransform.z *= 2;
    instanceTransform.w = 1.0;

    mod_input_vertex_pos += instanceTransform;
    mod_input_vertex_pos.w = 1.0;*/

    /*float4 forwardDir = float4(instanceDirForward.x, instanceDirForward.y, instanceDirForward.z, 1);
    float4 rightDir = float4(instanceDirRight.x, instanceDirRight.y, instanceDirRight.z, 1);
    float4 upDir = float4(instanceDirUp.x, instanceDirUp.y, instanceDirUp.z, 1);

    //mod_input_vertex_pos += forwardDir;
    //mod_input_vertex_pos += rightDir;
    //mod_input_vertex_pos += upDir;


    float4 MOVINGPOINT = float4(instanceTransform.x, instanceTransform.y, instanceTransform.z, 1);
    float4 vertPos = float4(input.Position.x, input.Position.y, input.Position.z, 1);

    float diffX = vertPos.x - MOVINGPOINT.x;
    float diffY = vertPos.y - MOVINGPOINT.y;
    float diffZ = vertPos.z - MOVINGPOINT.z;

    MOVINGPOINT = input.Position + -(rightDir * diffX);
    MOVINGPOINT = MOVINGPOINT + -(upDir * diffY);
    mod_input_vertex_pos = MOVINGPOINT + -(forwardDir * diffZ);


    float4 worldPosition = mul(mod_input_vertex_pos, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);*/



    /*instanceTransform.x *= 2;
    instanceTransform.y *= 2;
    instanceTransform.z *= 2;

    input.Position += (instanceTransform);
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);*/







    /*input.Position += instanceTransform;
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);*/

    //float4 worldPos = mul(input.Position + instanceTransform, transpose(World));
    //float4 worldPosition = mul(World, worldPos);
    //float4 viewPosition = mul(worldPosition, View);
    //output.Position = mul(viewPosition, Projection);


    //float4x4 worldPos = mul(World, transpose(instanceTransform));
    //float4 instancePos = input.Position + instanceTransform;// mul(World, World.transpose);
    //float4 worldPosition = mul(World, worldPos);
    //float4 viewPosition = mul(worldPosition, View);
    //output.Position = mul(viewPosition, Projection);

    //float4 pos = input.Position + instanceTransform;
    /*
    float4 pos = input.Position + instanceTransform;// +instanceTransform;
    pos = mul(pos, World);
    pos = mul(pos, View);
    pos = mul(pos, Projection);*/

    //input.Position.w = 1;
    //instanceTransform.w = 1;

    //float4 pos = input.Position + instanceTransform;
    //pos = mul(pos, WVP);

    //output.Position = pos;
    output.TexCoord = float2((input.TexCoord.x / 2.0f) + (1.0f / 2.0f * atlasCoord.x), (input.TexCoord.y / 2.0f) + (1.0f / 2.0f * atlasCoord.y));

    return output;
}

float4 InstancingPS(InstancingVSoutput input) : COLOR0
{
    return tex2D(TextureSampler, input.TexCoord);
}

technique Instancing
{
    pass Pass0
    {
        VertexShader = compile vs_4_0_level_9_1 InstancingVS();
        PixelShader = compile ps_4_0_level_9_1 InstancingPS();
    }
}