#!/usr/bin/env python3
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

def load(path: Path):
    data = json.loads(path.read_text(encoding="utf-8"))
    cards = data.get("cards")
    if not isinstance(cards, list) or not cards:
        raise SystemExit(f"{path}: cards must be a non-empty list")
    return cards

def validate(cards, source, expected_edition):
    seen = set()
    for card in cards:
        required = {"id", "prompt", "activity", "intensity", "edition", "tags", "rewardPoints"}
        missing = required - set(card)
        if missing:
            raise SystemExit(f"{source}: {card.get('id','?')} missing {sorted(missing)}")
        if card["id"] in seen:
            raise SystemExit(f"{source}: duplicate id {card['id']}")
        seen.add(card["id"])
        if card["edition"] != expected_edition:
            raise SystemExit(f"{source}: wrong edition for {card['id']}")
        if card["intensity"] not in (1, 2, 3):
            raise SystemExit(f"{source}: invalid intensity for {card['id']}")
        if card["rewardPoints"] < 0:
            raise SystemExit(f"{source}: negative reward for {card['id']}")
        if not card["prompt"].strip() or not card["activity"].strip():
            raise SystemExit(f"{source}: empty text for {card['id']}")

play_path = ROOT / "Content" / "Play" / "cards.json"
adult_path = ROOT / "Content" / "Adult" / "cards.json"
play = load(play_path)
adult = load(adult_path)
validate(play, play_path, "play")
validate(adult, adult_path, "adult")
adult_ids = {c["id"] for c in adult}
if adult_ids & {c["id"] for c in play}:
    raise SystemExit("Play and Adult catalogs share ids")
print(f"OK: {len(play)} Play cards, {len(adult)} Adult cards; catalogs are separated.")
