// Per-sprite radioactive glow effect (CE 0.21.0 render pipeline).
//
// Applied to a SpriteComponent via its companion ShaderComponent's declarative EffectAsset
// ("Effects/GlowRadioactive"). This is the data-driven per-sprite shader path: MonoGame applies an
// Effect at SpriteBatch.Begin, so the entity system groups entities by effect and opens a dedicated
// Begin/End for this shader while plain sprites keep the default batch.
//
// Convention (see docs/RenderPipeline.md): the render pipeline auto-syncs a `Projection` matrix
// whenever an effect exposes one, using the same orthographic convention MonoGame's own SpriteEffect
// uses. The sprite texture is bound by SpriteBatch to sampler slot 0, so we sample it explicitly.
//
// Modeled on the playground's Glow.fx, retinted to a sickly radioactive green and driven by a
// `GlowStrength` uniform (owned by the ShaderComponent) so it can be tuned or animated later.
float4x4 Projection;

// How strong the glow is. 0 = plain sprite, ~1 = full radioactive look, higher = hotter.
float GlowStrength;

sampler GlowTex;

struct VSInput
{
    float3 Position : POSITION;
    float4 Color : COLOR;
    float2 UV : TEXCOORD0;
};

struct VSOutput
{
    float4 Position : POSITION;
    float4 Color : COLOR;
    float2 UV : TEXCOORD0;
};

VSOutput MainVS(VSInput input)
{
    VSOutput output;
    // Promote the 3-component stream position to a full clip-space vector (w = 1, no perspective).
    output.Position = mul(float4(input.Position, 1.0), Projection);
    output.Color = input.Color;
    output.UV = input.UV;
    return output;
}

float4 MainPS(VSOutput input) : COLOR
{
    float4 tex = tex2D(GlowTex, input.UV);

    // The sprite texture is premultiplied-alpha. Gate the final color by the source alpha so fully
    // transparent pixels stay exactly (0,0,0,0) — otherwise a constant glow term would bleed a faint
    // box around the sprite's bounding rectangle.
    float a = tex.a * input.Color.a;
    float presence = tex.a;                       // 0..1: how much of the sprite is at this pixel
    float3 base = tex.rgb * input.Color.rgb;      // original (premultiplied) color

    // Radioactive green glow: blend toward a saturated sickly green and add an emissive lift so the
    // target reads as clearly "irradiated" compared with its un-effected neighbors. Driven by
    // `presence` so it only affects real sprite content, never the transparent background.
    float3 tint = float3(0.45, 1.0, 0.35);
    float3 glow = base * 0.3 + tint * (0.5 + 0.5 * presence);

    // GlowStrength scales the effect: 0 -> the plain sprite, 1 -> the full radioactive look above,
    // and >1 keeps pushing past it so a tweened value reads as "hotter".
    float3 c = base + (glow - base) * GlowStrength;

    return float4(c * a, a);
}

technique GlowRadioactiveTechnique
{
    pass GlowRadioactivePass
    {
        // SM 2.0 profile: the cross-platform target MonoGame DesktopGL/ANGLE compiles to (the
        // WindowsDX-only vs_4_0_level_9_1 profile rejects the COLOR output semantic + tex2D here).
        VertexShader = compile vs_2_0 MainVS();
        PixelShader  = compile ps_2_0 MainPS();
    }
}
