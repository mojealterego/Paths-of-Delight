# Schemat kart

Każdy katalog jest obiektem JSON z tablicą `cards`.

Wymagane pola karty:

- `id` — unikalny identyfikator.
- `prompt` — prywatne pytanie wyświetlane obu dorosłym graczom.
- `activity` — opis aktywności widoczny dopiero po blind-match i osobnym consent gate.
- `intensity` — 1–3.
- `edition` — `play` albo `adult`.
- `tags` — kategorie filtrowane przez prywatne granice.
- `rewardPoints` — nieujemna nagroda za dobrowolne ukończenie rundy; nigdy za samą zgodę.

Źródła Adult znajdują się poza katalogiem `Assets`. Build Play nie kopiuje ich do `Resources`.
