using System;
using System.Collections.Generic;

namespace PathsOfDelight
{
    public sealed class GameDirector
    {
        public ContentCard SelectNext(IReadOnlyList<ContentCard> cards, PlayerProfile a, PlayerProfile b, SessionState session, Edition edition)
        {
            var eligible = new List<ContentCard>();
            var ceiling = Math.Min(a.intensityCeiling, b.intensityCeiling);
            foreach (var card in cards)
            {
                if (card == null || string.IsNullOrEmpty(card.id)) continue;
                if (card.intensity > ceiling) continue;
                if (edition == Edition.Play && string.Equals(card.edition, "adult", StringComparison.OrdinalIgnoreCase)) continue;
                if (session.completedCardIds.Contains(card.id)) continue;
                if (Blocked(card, a) || Blocked(card, b)) continue;
                eligible.Add(card);
            }
            if (eligible.Count == 0)
            {
                foreach (var card in cards)
                {
                    if (card == null || card.intensity > ceiling) continue;
                    if (edition == Edition.Play && string.Equals(card.edition, "adult", StringComparison.OrdinalIgnoreCase)) continue;
                    if (Blocked(card, a) || Blocked(card, b)) continue;
                    eligible.Add(card);
                }
            }
            if (eligible.Count == 0) return null;
            eligible.Sort((x, y) => string.CompareOrdinal(x.id, y.id));
            var index = PositiveHash(session.seed, session.turn, session.boardPosition) % eligible.Count;
            return eligible[index];
        }

        private static bool Blocked(ContentCard card, PlayerProfile profile)
        {
            if (card.tags == null || profile.blockedTags == null) return false;
            foreach (var tag in card.tags)
                if (profile.blockedTags.Contains(tag)) return true;
            return false;
        }

        private static int PositiveHash(int seed, int turn, int position)
        {
            unchecked
            {
                uint h = 2166136261;
                h = (h ^ (uint)seed) * 16777619;
                h = (h ^ (uint)turn) * 16777619;
                h = (h ^ (uint)position) * 16777619;
                return (int)(h & 0x7fffffff);
            }
        }
    }
}
