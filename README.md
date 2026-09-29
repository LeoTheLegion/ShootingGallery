# ShootingGallery

A Simple Shooting Gallery game like Duck hunt to learn the Monogame Framework.

## Setup

The game engine is [CoreEssentials-MonoGame](https://github.com/LeoTheLegion/core-essentials-monogame),
consumed from **GitHub Packages** (not nuget.org). It needs a one-time auth setup:

1. **Create a token** — GitHub → Settings → Developer settings → Personal access tokens →
   Tokens (classic) → generate with the **`read:packages`** scope only.

2. **Set it as an environment variable** (once, per machine):

   ```powershell
   [Environment]::SetEnvironmentVariable("GITHUB_PACKAGES_TOKEN", "ghp_YOUR_TOKEN", "User")
   ```

   Open a *new* terminal after this so the variable is picked up.

3. **Restore & build**:

   ```powershell
   dotnet restore ShootingGallery.csproj
   dotnet build ShootingGallery.csproj
   ```

### How it works (and two gotchas that cost hours)

`nuget.config` declares the feed and resolves the token from the environment variable —
**no secret is stored in any file**:

```xml
<packageSources>
  <add key="github-packages" value="https://nuget.pkg.github.com/leothelegion/index.json" />
</packageSources>
<packageSourceCredentials>
  <!-- The element name MUST match the source KEY above, not the URL. -->
  <github-packages>
    <add key="Username" value="LeoTheLegion" />
    <add key="ClearTextPassword" value="%GITHUB_PACKAGES_TOKEN%" />
  </github-packages>
</packageSourceCredentials>
```

- **Credential element name = source key.** A mismatched key (e.g. the feed URL) silently sends
  *no* auth at all → `401 Unauthorized` on every restore, even with a valid token.
- **Env-var syntax is `%VAR%`.** NuGet expands config values with .NET's
  `Environment.ExpandEnvironmentVariables`, which only understands `%VAR%` — not `$(VAR)` or `$VAR`.

## Scope

- Start Menu
		 - **Button** to Start Gameplay
- Gameplay
		 - Left click to shoot **Target**
		 - **Target** randomly moves after being shot
		 - **Score** is based on **Target** size
		 - On time out, goes to Gameover
- Gameover
		 - **Button** to Start Gameplay

## Time Taken
Around 6 hours. 20% - 30% spend on experimentation 