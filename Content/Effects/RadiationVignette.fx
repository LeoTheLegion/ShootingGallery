// Full-screen additive radiation vignette post pass (CE 0.21.0 render pipeline).
//
// Registered through RenderPipeline.AddPostPass (additive mode — no render target required). The
// pipeline draws a full-screen quad through this effect after the scene + GUI. It only needs the
// screen-space UV to shape the vignette; the 1x1 white quad texture SpriteBatch binds to sampler0
// is ignored. Output is a sickly radioactive green whose alpha rises toward the corners, scaled by
// `Intensity` (0..1) which ScreenFxComponent drives from the current radiation level. AlphaBlend
// tints the frame edges green as radiation builds, leaving the center untouched at low levels.
float4x4 Projection;

// 0 = no vignette, 1 = full. Driven by ScreenFxComponent from the director's radiation state.
float Intensity;

sampler VignetteTex;

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
    // Distance from screen center in UV space (center is 0.5, 0.5). Normalize so the corner is ~1.
    float2 centered = input.UV - 0.5;
    float dist = length(centered * 2.0);

    // Ease the falloff so the effect stays off-center and only grips the outer edges as it grows.
    float edge = saturate(dist - 0.45) / (1.0 - 0.45);

    // Scale by the radiation intensity — ScreenFxComponent eases this up/down, so no per-frame
    // temporal term is needed here (keeps the pass cheap and free of banding artifacts).
    float alpha = saturate(edge * Intensity);

    // Sample sampler0 (the white full-screen quad) so the read stays live on ANGLE. The quad is pure
    // white, so multiplying by it is a visual no-op.
    float4 src = tex2D(VignetteTex, input.UV);

    // Sickly radioactive green (matches the game's accent hue).
    float3 baseColor = float3(0.35, 1.0, 0.4) * src.rgb;

    // PREMULTIPLIED output: CE draws post passes with BlendState.AlphaBlend (One, InverseSourceAlpha),
    // which expects rgb already scaled by alpha. Emitting straight color+alpha instead makes SpriteBatch
    // add the full base color to every pixel regardless of alpha — the earlier full-screen green wash.
    return float4(baseColor * alpha, alpha);
}

technique RadiationVignetteTechnique
{
    pass RadiationVignettePass
    {
        // SM 2.0 profile: the cross-platform target MonoGame DesktopGL/ANGLE compiles to (the
        // WindowsDX-only vs_4_0_level_9_1 profile rejects the COLOR output semantic + tex2D here).
        VertexShader = compile vs_2_0 MainVS();
        PixelShader  = compile ps_2_0 MainPS();
    }
}
