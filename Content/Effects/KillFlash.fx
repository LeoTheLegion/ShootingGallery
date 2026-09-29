// Full-screen bomb kill-flash post pass (CE 0.21.0 render pipeline).
//
// Registered through RenderPipeline.AddPostPass (additive mode — no render target required). On a
// bomb detonation ScreenFxComponent spikes `Intensity` to ~1 and eases it back to 0 over the
// detonation beat, so this pass paints a brief warm-white/red flash over the whole frame. Unlike the
// vignette there is no edge falloff — the flash covers everything for maximum impact. The 1x1 white
// quad texture SpriteBatch binds to sampler0 is ignored; only `Intensity` matters.
float4x4 Projection;

// 0 = invisible, 1 = full blast. Driven by ScreenFxComponent during the detonation beat.
float Intensity;

sampler FlashTex;

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
    // Slight radial hot-spot: brightest at center, easing toward the edges so it reads as a blast
    // rather than a flat whiteout. dist 0 (center) -> 1, corner -> ~0.
    float2 centered = input.UV - 0.5;
    float dist = length(centered * 2.0);
    float falloff = saturate(1.0 - dist * 0.4);

    // Warm detonation white with a red core, scaled by intensity and the radial falloff. Cap the peak
    // alpha below 1.0 so even at full intensity the scene reads through — a bright warm pulse rather
    // than an opaque whiteout.
    float alpha = min(Intensity * (0.5 + 0.5 * falloff), 0.6);

    // Sample sampler0 (the white full-screen quad) so the read stays live on ANGLE; it is pure white,
    // so multiplying by it is a visual no-op.
    float4 src = tex2D(FlashTex, input.UV);

    // PREMULTIPLIED output: CE draws post passes with BlendState.AlphaBlend (One, InverseSourceAlpha),
    // which expects rgb already scaled by alpha. Scaling the warm-white by alpha is what prevents the
    // full-screen whiteout — at low intensity rgb -> 0 and the scene shows through untouched.
    float3 baseColor = float3(1.0, 0.85, 0.6) * src.rgb;
    return float4(baseColor * alpha, alpha);
}

technique KillFlashTechnique
{
    pass KillFlashPass
    {
        // SM 2.0 profile: the cross-platform target MonoGame DesktopGL/ANGLE compiles to (the
        // WindowsDX-only vs_4_0_level_9_1 profile rejects the COLOR output semantic + tex2D here).
        VertexShader = compile vs_2_0 MainVS();
        PixelShader  = compile ps_2_0 MainPS();
    }
}
