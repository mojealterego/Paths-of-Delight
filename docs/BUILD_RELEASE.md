# Build i release

## Walidacja treści

```bash
python Tools/validate_content.py
```

## Unity

Wersja projektu: Unity 6000.3.13f1 (Unity 6.3 LTS).

CI używa GameCI. Canonical Unity APK wymaga sekretów repozytorium:

- `UNITY_LICENSE`
- `UNITY_EMAIL`
- `UNITY_PASSWORD`

Build method:

```text
PathsOfDelight.Editor.BuildCommand.PerformAndroidBuild
```

Parametr `-edition play` buduje Play Edition. `-edition adult` buduje Adult Edition. Target Android ustawiany jest na API 36, ARM64 i IL2CPP.

## Preview APK

`android-preview/` jest awaryjnym, instalowalnym smoke buildem bez Unity. CI buduje go przez AGP 8.12.2 / Gradle 8.13 / JDK 17 i publikuje artefakt `Paths-of-Delight-preview-apk`.
