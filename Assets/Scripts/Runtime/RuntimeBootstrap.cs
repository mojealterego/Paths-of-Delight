using UnityEngine;

namespace PathsOfDelight
{
    public static class RuntimeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (Object.FindFirstObjectByType<GameApp>() != null) return;
            var root = new GameObject("PathsOfDelight");
            Object.DontDestroyOnLoad(root);
            root.AddComponent<GameApp>();
        }
    }
}
