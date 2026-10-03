// Full-screen radiation vignette — a SCENE ENTITY, not a post pass (CE 0.21.x render pipeline).
//
// It is drawn by a full-screen SpriteComponent (radiationOverlay, 1280x720) at ZLayer -2 — above the
// backdrop (-3), below the radioactive halos (-1) and every target (0). That placement is the whole
// point: because it sits behind the targets in normal scene order, the targets OCCLUDE it pixel-wise —
// a post pass would render on top of everything and tint the targets too. The shader only needs screen-space UV to shape
// the vignette; the white carrier texture bound to sampler0 is ignored. Output is a sickly radioactive
// green whose alpha rises toward the edges, scaled by `Intensity` (0..1) which ScreenFxComponent drives
// from the current radiation level via the overlay's ShaderComponent.
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
    // Distance from screen center in UV space (center is 0.5, 0.5). Normalized so the straight-edge
    // midpoint is ~1.0 and the corners reach ~1.41.
    float2 centered = input.UV - 0.5;
    float dist = length(centered * 2.0);

    // Confine the effect to a narrow outer frame: zero everywhere inside dist < 0.85 (the whole
    // central target grid stays clean) and ramping to full at the straight-edge midpoint (dist ~1.0),
    // clamped at the corners. As radiation rises the green only ever lives in this border band — it
    // grows stronger there but never washes over the targets.
    float edge = saturate((dist - 0.85) / (1.0 - 0.85));

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
