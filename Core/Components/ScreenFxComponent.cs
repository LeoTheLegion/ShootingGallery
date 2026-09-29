using System;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ShootingGallery.Core;

/// <summary>
/// Owns the game's screen-space post passes (CE 0.21.0 render pipeline) and drives their uniforms:
/// a radiation vignette whose intensity tracks the current radiation level, and a bomb kill-flash
/// that spikes on detonation and decays over the beat. Both are additive full-screen overlays, so no
/// render target is required.
///
/// The passes live on the static <see cref="RenderPipeline"/> (global, game-thread only), so this
/// component registers them on attach and removes them on detach — unloading the scene restores the
/// pipeline to its default no-op state. Declared as a plain component on the round director entity in
/// Content/Scenes/game_scene.xml; GameDirectorComponent resolves it by id and pokes
/// <see cref="SetRadiationIntensity"/> / <see cref="TriggerKillFlash"/> from gameplay.
/// </summary>
public class ScreenFxComponent : EntityComponent
{
    // Asset names (content-pipeline keys) for the two post-pass effects.
    public string VignetteEffectAsset { get; set; } = "Effects/RadiationVignette";
    public string KillFlashEffectAsset { get; set; } = "Effects/KillFlash";

    // How fast the vignette eases toward its target (per second). Higher = snappier.
    public float VignetteEaseSpeed { get; set; } = 4f;

    // How fast the kill-flash decays after a trigger (per second). ~3 => gone in ~0.3s of the beat.
    public float KillFlashDecay { get; set; } = 3f;

    // GATED OFF: with these passes registered, DesktopGL/ANGLE renders the whole frame solid white
    // (the full-screen effect quad's raw opaque-white texture is blitted without the effect applying).
    // CE's DrawPostPasses uses the same generic SpriteBatch+Effect path on every backend, so this is a
    // custom-effect-not-applying failure specific to this environment, not a missing API. Keep disabled
    // until we reproduce it in isolation and fix it. Non-const on purpose so the code below stays live
    // (and this flag flips back with no dead-code warning) once the white-screen cause is resolved.
    //
    // Post passes are additive full-screen overlays drawn by CE's RenderPipeline.DrawPostPasses using
    // BlendState.AlphaBlend (One, InverseSourceAlpha) — the pipeline expects PREMULTIPLIED output from
    // each effect shader. The shaders emit rgb already scaled by alpha; emitting straight color+alpha
    // instead makes SpriteBatch add the full base color to every pixel regardless of alpha (the earlier
    // full-screen green/white wash on this DesktopGL/ANGLE build).
    private static readonly bool RegisterPostPasses = true;

    private Effect _vignetteEffect;
    private Effect _killFlashEffect;

    // Live uniform state. The vignette eases toward _radiationTarget; the flash decays from its peak.
    private float _radiationTarget;
    private float _vignetteIntensity;
    private float _flashIntensity;

    public override void OnAttach()
    {
        if (!RegisterPostPasses)
            return;

        _vignetteEffect = LoadEffect(VignetteEffectAsset, out var vignetteFailed);
        _killFlashEffect = LoadEffect(KillFlashEffectAsset, out var killFlashFailed);

        if (_vignetteEffect != null)
            RenderPipeline.AddPostPass(_vignetteEffect);
        if (_killFlashEffect != null)
            RenderPipeline.AddPostPass(_killFlashEffect);

        if (vignetteFailed)
            Console.WriteLine("[ScreenFxComponent] Radiation vignette effect failed to load; pass skipped.");
        if (killFlashFailed)
            Console.WriteLine("[ScreenFxComponent] Kill-flash effect failed to load; pass skipped.");
    }

    public override void OnDetach()
    {
        // Remove both passes so unloading the scene restores the pipeline's default no-op state.
        if (_vignetteEffect != null)
            RenderPipeline.RemovePostPass(_vignetteEffect);
        if (_killFlashEffect != null)
            RenderPipeline.RemovePostPass(_killFlashEffect);

        _vignetteEffect = null;
        _killFlashEffect = null;
    }

    public override void Update(GameTime gameTime)
    {
        if (Owner == null || Owner.Destroyed)
            return;

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Ease the vignette toward its radiation-driven target so it swells/recedes smoothly instead
        // of stepping. Frame-rate independent: move a fixed fraction of the remaining gap per second.
        _vignetteIntensity += (_radiationTarget - _vignetteIntensity) * Math.Min(1f, VignetteEaseSpeed * dt);

        // Decay the kill-flash exponentially toward zero after its peak.
        _flashIntensity = Math.Max(0f, _flashIntensity - KillFlashDecay * dt);

        if (_vignetteEffect != null && _vignetteEffect.Parameters["Intensity"] != null)
            _vignetteEffect.Parameters["Intensity"].SetValue(_vignetteIntensity);
        if (_killFlashEffect != null && _killFlashEffect.Parameters["Intensity"] != null)
            _killFlashEffect.Parameters["Intensity"].SetValue(_flashIntensity);
    }

    /// <summary>
    /// Sets the radiation vignette's target intensity (0 = off, 1 = full). Call with the current
    /// radiation fraction each time it changes; the pass eases toward it.
    /// </summary>
    public void SetRadiationIntensity(float fraction)
    {
        _radiationTarget = MathHelper.Clamp(fraction, 0f, 1f);
    }

    /// <summary>Spikes the kill-flash to full strength; it then decays over the detonation beat.</summary>
    public void TriggerKillFlash()
    {
        _flashIntensity = 1f;
    }

    private static Effect LoadEffect(string assetName, out bool failed)
    {
        try
        {
            failed = false;
            return AssetManager.LoadAsset<EffectAsset>(assetName).Effect;
        }
        catch (Exception ex)
        {
            failed = true;
            Console.WriteLine($"[ScreenFxComponent] Could not load effect asset '{assetName}': {ex.Message}");
            return null;
        }
    }
}
