// Radiating halo effect for radioactive targets (CE 0.21.x render pipeline).
//
// Applied to a SpriteComponent via its companion ShaderComponent's declarative EffectAsset
// ("Effects/GlowHalo"). Unlike GlowRadioactive.fx (which tints the sprite it sits on), this one
// IGNORES the carrier texture's color and paints a purely procedural radial falloff from the quad
// center out. It is drawn on a LARGER carrier sprite (halo_carrier, 200x200) centered BEHIND the
// 90x200 target at ZLayer -1, so light appears to radiate past the target's edge and fade to
// nothing at the carrier quad's border — no rectangular box artifact.
//
// Convention (see docs/RenderPipeline.md): the render pipeline auto-syncs a `Projection` matrix
// whenever an effect exposes one. The carrier texture is bound by SpriteBatch to sampler slot 0;
// we sample it only to stay well-behaved on ANGLE — its color is not used.
float4x4 Projection;

// Overall halo brightness. 0 = invisible, ~1 = full radioactive glow, higher = hotter.
float GlowStrength;

sampler HaloTex;

struct VSInput
{
    float3 Position : POSITION;
    float4 Color : COLOR;
    float2 UV : TEXCOORD0;
};

struct VSOutput
{
    float4 Position : POSITION;
    float2 UV : TEXCOORD0;
};

VSOutput MainVS(VSInput input)
{
    VSOutput output;
    // Promote the 3-component stream position to a full clip-space vector (w = 1, no perspective).
    output.Position = mul(float4(input.Position, 1.0), Projection);
    output.UV = input.UV;
    return output;
}

float4 MainPS(VSOutput input) : COLOR
{
    // Sample the carrier so the effect stays valid on ANGLE; color intentionally unused.
    float4 tex = tex2D(HaloTex, input.UV);

    // Radial distance from quad center: 0 at center, 1 at the edge of the quad.
    float d = length(input.UV - 0.5) * 2.0;

    // Falloff that reaches exactly zero at the quad border (d == 1) — this is what prevents a
    // hard rectangular edge around the glow. Quadratic falloff reads as soft light emission.
    float t = saturate(1.0 - d);
    float intensity = t * t * GlowStrength;

    // Sickly radioactive green, matching the target tint in GlowRadioactive.fx.
    float3 tint = float3(0.45, 1.0, 0.35);

    // Premultiplied output: rgb already carries the falloff, alpha gates how much of this quad
    // replaces what's behind it. At the edge both go to zero -> fully transparent border.
    float3 c = tint * intensity;
    return float4(c, intensity);
}

technique GlowHaloTechnique
{
    pass GlowHaloPass
    {
        // SM 2.0 profile: the cross-platform target MonoGame DesktopGL/ANGLE compiles to (the
        // WindowsDX-only vs_4_0_level_9_1 profile rejects the COLOR output semantic + tex2D here).
        VertexShader = compile vs_2_0 MainVS();
        PixelShader  = compile ps_2_0 MainPS();
    }
}
