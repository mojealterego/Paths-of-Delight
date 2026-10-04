using UnityEngine;

namespace PathsOfDelight
{
    public static class EditionConfig
    {
        public static Edition Current
        {
            get
            {
                var asset = Resources.Load<TextAsset>("Generated/edition");
                if (asset != null && asset.text.ToLowerInvariant().Contains("adult")) return Edition.Adult;
                return Edition.Play;
            }
        }
    }
}
