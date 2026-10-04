# Bezpieczeństwo

Vertical slice jest local-first i nie zawiera telemetrii ani backendu.

Zapis Unity używa AES-CBC do poufności oraz HMAC-SHA256 do integralności. Odpowiedzi pojedynczych rund są przechowywane wyłącznie w pamięci i nie trafiają do sejwu. Klucz demonstracyjny jest generowany losowo i zapisany przez PlayerPrefs.

## Hardening przed produkcją

Przed produkcyjnym wydaniem klucz musi zostać przeniesiony do Android Keystore / iOS Keychain. Dual-device wymaga protokołu E2EE, rotacji kluczy sesji, ochrony przed replay i minimalizacji metadanych. Logi nie mogą zawierać promptów użytkownika, odpowiedzi prywatnych ani stanów zgody.

Nie należy traktować PlayerPrefs jako bezpiecznego magazynu kluczy w wersji produkcyjnej.
