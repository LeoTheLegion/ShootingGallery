using System;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ShootingGallery.Core;

/// <summary>
/// Drives the game's screen-space radiation effects (CE 0.21.x render pipeline):
/// <list type="bullet">
///   <item><b>Radiation vignette</b> — a full-screen SCENE ENTITY (id "radiationOverlay") at ZLayer -2,
///     behind the targets, so they occlude it pixel-wise. This component eases its ShaderComponent's
///     Intensity toward the current radiation level.</item>
///   <item><b>Bomb kill-flash</b> — a full-screen POST PASS on the static <see cref="RenderPipeline"/>
///     that spikes on detonation and decays over the beat. It is registered on attach and removed on
///     detach so unloading the scene restores the pipeline's default no-op state.</item>
/// </list>
///
/// Declared as a plain component on the "screenFx" entity in Content/Scenes/game_scene.xml;
/// GameDirectorComponent resolves it by id and pokes <see cref="SetRadiationIntensity"/> /
/// <see cref="TriggerKillFlash"/> from gameplay.
/// </summary>
public class ScreenFxComponent : EntityComponent
{
    // Asset name (content-pipeline key) for the kill-flash post-pass effect.
    public string KillFlashEffectAsset { get; set; } = "Effects/KillFlash";

    // Id of the full-screen radiation-overlay scene entity whose ShaderComponent we drive. Declared in
    // Content/Scenes/game_scene.xml (ZLayer -2, behind the targets).
    public string RadiationOverlayId { get; set; } = "radiationOverlay";

    // How fast the vignette eases toward its target (per second). Higher = snappier.
    public float VignetteEaseSpeed { get; set; } = 4f;

    // How fast the kill-flash decays after a trigger (per second). ~3 => gone in ~0.3s of the beat.
    public float KillFlashDecay { get; set; } = 3f;

    // Gate for registering the kill-flash post pass on the static RenderPipeline. Currently enabled:
    // during debugging, DesktopGL/ANGLE once rendered the whole frame solid white — the full-screen
    // effect quad's raw opaque texture blitted without the custom effect applying (CE's DrawPostPasses
    // uses the same generic SpriteBatch+Effect path on every backend, so it was an effect-not-applying
    // issue in this environment, not a missing API). That no longer reproduces in this build. Non-const
    // on purpose: keeps the registration code below live (no dead-code warning) and lets the flag flip
    // off again if that backend issue ever returns.
    //
    // Contract for post-pass effects: DrawPostPasses uses BlendState.AlphaBlend (One, InverseSourceAlpha),
    // i.e. PREMULTIPLIED blending — each effect shader must emit rgb already scaled by alpha. Emitting
    // straight color+alpha instead makes SpriteBatch add the full base color to every pixel regardless of
    // alpha (the earlier full-screen green/white wash). The KillFlash shader honors this contract.
    private static readonly bool RegisterPostPasses = true;

    private Effect _killFlashEffect;

    // The full-screen radiation-overlay entity's shader, resolved lazily on first LateUpdate (the
    // overlay entity may not have its components attached yet at this component's OnAttach).
    private ShaderComponent _radiationOverlayShader;
    private bool _overlayResolved;

    // Live uniform state. The vignette eases toward _radiationTarget; the flash decays from its peak.
    private float _radiationTarget;
    private float _vignetteIntensity;
    private float _flashIntensity;

    public override void OnAttach()
    {
        // The radiation vignette is a scene entity (driven in LateUpdate), so nothing to register here.
        // Only the kill-flash is a post pass on the static pipeline.
        if (!RegisterPostPasses)
            return;

        _killFlashEffect = LoadEffect(KillFlashEffectAsset, out var killFlashFailed);
        if (_killFlashEffect != null)
            RenderPipeline.AddPostPass(_killFlashEffect);

        if (killFlashFailed)
            Console.WriteLine("[ScreenFxComponent] Kill-flash effect failed to load; pass skipped.");
    }

    public override void OnDetach()
    {
        // Remove the kill-flash pass so unloading the scene restores the pipeline's default no-op state.
        if (_killFlashEffect != null)
            RenderPipeline.RemovePostPass(_killFlashEffect);
        _killFlashEffect = null;

        _radiationOverlayShader = null;
        _overlayResolved = false;
    }

    public override void Update(GameTime gameTime)
    {
        if (Owner == null || Owner.Destroyed)
            return;

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Decay the kill-flash exponentially toward zero after its peak. (Post pass — drawn in Draw,
        // so it only needs a per-frame value; no late-update dependency.)
        _flashIntensity = Math.Max(0f, _flashIntensity - KillFlashDecay * dt);

        if (_killFlashEffect != null && _killFlashEffect.Parameters["Intensity"] != null)
            _killFlashEffect.Parameters["Intensity"].SetValue(_flashIntensity);
    }

    // LateUpdate runs AFTER all regular updates (including the director's SetRadiationIntensity) and
    // BEFORE Draw. Easing + applying the vignette here means it always tracks this frame's FINAL
    // radiation level — doing it in the regular Update could run before the director refreshes the
    // target, leaving a one-frame lag behind the current radiation value.
    public override void LateUpdate(GameTime gameTime)
    {
        if (Owner == null || Owner.Destroyed)
            return;

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Frame-rate-independent exponential ease toward the target: move a fixed fraction of the
        // remaining gap per second, so convergence speed is identical at any refresh rate. Clamped to
        // 1 so a large dt (first frame / hiccup) can't overshoot or go unstable.
        _vignetteIntensity += (_radiationTarget - _vignetteIntensity) * Math.Min(1f, VignetteEaseSpeed * dt);

        // Drive the radiation-overlay entity's shader (scene entity behind the targets).
        if (!_overlayResolved)
        {
            _overlayResolved = true;
            _radiationOverlayShader = EntitySystem?.FindById(RadiationOverlayId)?.GetComponent<ShaderComponent>();
            if (_radiationOverlayShader == null)
                Console.WriteLine($"[ScreenFxComponent] Radiation overlay entity '{RadiationOverlayId}' not found; vignette disabled.");
        }
        _radiationOverlayShader?.SetFloat("Intensity", _vignetteIntensity);
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
