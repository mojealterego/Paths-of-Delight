# Architektura

## Rdzeń

Unity 6.3 LTS. Runtime jest podzielony na:

- `Domain.cs` — modele i stany.
- `GameDirector.cs` — deterministyczny dobór kart.
- `ConsentEngine.cs` — blind match i consent gate.
- `ContentCatalogService.cs` — katalog runtime.
- `LocalVault.cs` — lokalny szyfrowany zapis.
- `GameApp.cs` — działający vertical slice UI bez zależności od prefabów.
- `BuildCommand.cs` — deterministyczne generowanie sceny, katalogu runtime i APK.

## Separacja edycji

Źródła Play i Adult są poza `Assets`. Build kopiuje do `Assets/Resources/Generated` wyłącznie katalog wybranej edycji. Adult nie jest ukrytym downloadem ani zdalnym przełącznikiem w Play Edition.

## Android preview

`android-preview/` jest niezależnym, lokalnym WebView smoke-buildem służącym do uzyskania instalowalnego APK nawet wtedy, gdy CI nie ma licencji Unity. Nie zastępuje docelowego klienta Unity.
