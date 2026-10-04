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
- CI content validation and APK artifacts.

## Repository map

```text
Assets/
  Editor/                 deterministic Unity build
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
android-preview/          installable smoke APK independent of Unity licensing
.github/workflows/ci.yml  validation + APK pipelines
```

## Local validation

```bash
python Tools/validate_content.py
```

## Canonical Unity APK

The Unity build uses `PathsOfDelight.Editor.BuildCommand.PerformAndroidBuild` and Unity `6000.3.13f1`.

GameCI requires repository secrets `UNITY_LICENSE`, `UNITY_EMAIL` and `UNITY_PASSWORD`. When they are present, CI runs Unity EditMode tests and publishes the canonical Play APK.

## Installable preview APK

CI always builds `android-preview/` with Android API 36. This is a smoke-build implementation of the same consent-first flow and exists so an installable APK can be produced without exposing Unity credentials.

## Safety invariants

- No penalty for **No**, **Maybe**, **Skip**, **Pause** or withdrawal of consent.
- A private unilateral answer is never revealed to the other player.
- The lower intensity ceiling always wins.
- Play Edition cannot package Adult Edition source content.
- Intimate round answers are not written to logs, analytics or the local save.
- Optional future integrations never gate core gameplay.

See `docs/` for architecture and security details.
