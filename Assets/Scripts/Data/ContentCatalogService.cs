using System;
using System.Collections.Generic;
using UnityEngine;

namespace PathsOfDelight
{
    public sealed class ContentCatalogService
    {
        public List<ContentCard> Load(Edition edition, IEnumerable<ContentCard> custom)
        {
            var result = new List<ContentCard>();
            var asset = Resources.Load<TextAsset>("Generated/content");
            if (asset != null)
            {
                var catalog = JsonUtility.FromJson<CardCatalog>(asset.text);
                if (catalog != null && catalog.cards != null) result.AddRange(catalog.cards);
            }
            if (result.Count == 0) result.AddRange(DefaultCards());
            if (custom != null) result.AddRange(custom);
            if (edition == Edition.Play) result.RemoveAll(c => string.Equals(c.edition, "adult", StringComparison.OrdinalIgnoreCase));
            return result;
        }

        private static IEnumerable<ContentCard> DefaultCards()
        {
            return new[]
            {
                Card("play-001", "Czy macie ochotę na minutę spokojnego kontaktu wzrokowego?", "Usiądźcie naprzeciwko siebie i przez minutę skupcie uwagę wyłącznie na sobie.", 1, "play", 1, "connection"),
                Card("play-002", "Czy oboje macie dziś ochotę na dłuższy pocałunek?", "Jeśli oboje nadal chcecie, wybierzcie spokojne miejsce i poświęćcie chwilę na pocałunek bez pośpiechu.", 1, "play", 1, "kiss"),
                Card("play-003", "Czy chcecie spróbować krótkiej gry w ciepło–zimno z neutralnym dotykiem dłoni i ramion?", "Jedna osoba zamyka oczy, druga prowadzi ją słowami ciepło–zimno do wybranego miejsca na dłoni lub ramieniu. Po chwili zamieńcie role.", 1, "play", 2, "touch"),
                Card("play-004", "Czy macie ochotę powiedzieć sobie po jednej rzeczy, która dziś szczególnie was przyciąga?", "Każde z was mówi jedno zdanie uznania. Bez oceniania i bez obowiązku rozwijania tematu.", 1, "play", 1, "communication"),
                Card("play-005", "Czy chcecie podkręcić atmosferę przez wybór muzyki i światła?", "Wspólnie wybierzcie jeden utwór i ustawcie światło tak, by obojgu było komfortowo.", 1, "play", 1, "atmosphere")
            };
        }

        private static ContentCard Card(string id, string prompt, string activity, int intensity, string edition, int reward, params string[] tags)
        {
            return new ContentCard { id = id, prompt = prompt, activity = activity, intensity = intensity, edition = edition, rewardPoints = reward, tags = tags };
        }
    }
}
