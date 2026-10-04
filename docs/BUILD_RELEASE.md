# Build i release

## Założenie operacyjne

Projekt ma być możliwy do rozwijania i budowania bez lokalnej instalacji Unity. Repozytorium pozostaje źródłem prawdy, a docelowy build Unity wykonuje **Unity Build Automation** w chmurze.

Aktualny GameCI v4 dla bezpłatnej licencji Unity Personal wymaga aktywacji licencji przez Unity Hub, dlatego nie jest używany jako główna ścieżka dla tego repo.

## Walidacja treści

GitHub Actions uruchamia automatycznie:

```bash
python Tools/validate_content.py
```

## Unity Build Automation

Wersja projektu: Unity 6000.3.13f1 (Unity 6.3 LTS).

Repo jest przygotowane pod cloud build:

- pre-export method: `PathsOfDelight.Editor.CloudBuildHooks.PreExport`
- środowisko Play Edition: `POD_EDITION=play`
- środowisko Adult Edition: `POD_EDITION=adult`
- Android target API: 36
- architektura: ARM64
- scripting backend: IL2CPP
- Play package id: `com.mojealterego.pathsofdelight`
- Adult package id: `com.mojealterego.pathsofdelight.adult`

Pre-export generuje scenę runtime, katalog treści odpowiedniej edycji i ustawia scenę w Build Settings przed właściwym buildem.

## Konfiguracja bez instalowania czegokolwiek lokalnie

Jednorazowe czynności wykonuje się wyłącznie w przeglądarce:

1. Utworzyć Unity ID.
2. W Unity Dashboard wejść do Development / DevOps / Build Automation.
3. Utworzyć projekt Unity Cloud.
4. Połączyć GitHub repo `mojealterego/Paths-of-Delight`.
5. Utworzyć target Android.
6. Ustawić Unity 6000.3.13f1.
7. Ustawić branch `main`.
8. Ustawić Pre-Export Method:
   `PathsOfDelight.Editor.CloudBuildHooks.PreExport`
9. Dodać environment variable:
   `POD_EDITION=play`
10. Włączyć auto-build po zmianie `main` albo uruchamiać build ręcznie z dashboardu.

Nie wymaga to Unity Hub ani Unity Editor na urządzeniu użytkownika.

## Preview APK

`android-preview/` pozostaje repozytoryjnym smoke buildem budowanym wyłącznie przez GitHub Actions. Jest użyteczny do szybkiej instalacji i testu przepływu bez oczekiwania na build Unity.

## Wydania

Play Edition i Adult Edition są rozdzielane w czasie buildu. Adult content nie jest dołączany do Play Edition jako ukryty katalog ani zdalny feature flag.
