# Ścieżki Rozkoszy / Paths of Delight

Consent-first, privacy-by-design digital board game for adult couples (18+).

## Status

Production-oriented Unity vertical slice. The repository contains the Unity client, deterministic game rules, edition-separated content, automated tests and Android CI.

## Core safety invariants

- No penalty for **No**, **Skip**, **Pause** or withdrawal of consent.
- A private unilateral answer is never revealed to the other player.
- The lower intensity ceiling always wins.
- Play Edition and Adult Edition use separate build-time content catalogs.
- Intimate answers are not written to logs or analytics.
- Optional integrations never gate core gameplay.

See `docs/` after bootstrap for architecture, security and release details.
