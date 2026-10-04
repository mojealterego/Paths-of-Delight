# Ścieżki Rozkoszy / Paths of Delight

Consent-first, privacy-by-design digital board game for adult couples (18+).

## Implemented vertical slice

- Unity 6.3 LTS client for Android.
- Private Player A / Player B setup.
- Per-player intensity ceiling and private category blocks.
- Deterministic board and Game Director.
- Private answers: TAK / MOŻE / NIE.
- Blind matching: only mutual TAK is disclosed.
- Separate consent gate before an activity.
- Skip / Pause / End without penalties.
- Shared points only for voluntarily completed rounds, never for consent itself.
- Encrypted local save; per-round answers are memory-only.
- Local private custom-card editor.
- Build-time Play / Adult content separation.
- EditMode regression tests.
- Android API 36, ARM64, IL2CPP build configuration.
- GitHub CI content validation and installable preview APK.
- Unity Build Automation hooks for cloud-only canonical Unity builds.

## Repository map

```text
Assets/
  Editor/                 deterministic Unity + cloud build hooks
  Scripts/
    Data/                 content + local vault
    Gameplay/             director + consent rules
    Runtime/              models + bootstrap
    UI/                   complete vertical-slice UI
  Tests/EditMode/
Content/
  Play/                   Play Edition source catalog
  Adult/                  Adult Edition source catalog, outside Assets
Tools/                    build-time validators
docs/                     architecture, security, consent and build notes
android-preview/          installable smoke APK built by GitHub Actions
.github/workflows/ci.yml  validation + preview APK pipeline
```

## Cloud-only development model

No local Unity installation is required for the intended workflow.

GitHub remains the source repository. The canonical Unity APK is designed to be built by **Unity Build Automation** after the repository is connected once in the Unity Dashboard.

Cloud pre-export method:

```text
PathsOfDelight.Editor.CloudBuildHooks.PreExport
```

Environment variable:

```text
POD_EDITION=play
```

For the direct Adult build use `POD_EDITION=adult`.

## Installable preview APK

GitHub Actions always builds `android-preview/` with Android API 36, so a smoke APK can be produced without Unity credentials or local software.

## Safety invariants

- No penalty for **No**, **Maybe**, **Skip**, **Pause** or withdrawal of consent.
- A private unilateral answer is never revealed to the other player.
- The lower intensity ceiling always wins.
- Play Edition cannot package Adult Edition source content.
- Intimate round answers are not written to logs, analytics or the local save.
- Optional future integrations never gate core gameplay.

See `docs/` for architecture, security and cloud build details.
