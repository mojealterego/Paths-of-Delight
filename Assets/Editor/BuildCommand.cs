using System;
using System.Collections.Generic;
using System.IO;
using PathsOfDelight;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PathsOfDelight.Editor
{
    public static class BuildCommand
    {
        public static void PerformAndroidBuild()
        {
            var edition = GetArg("-edition", "play").ToLowerInvariant();
            PrepareCloudBuild(edition);

            var output = GetArg("-customBuildPath", Path.Combine("build", "Android", "PathsOfDelight.apk"));
            var outputDir = Path.GetDirectoryName(output);
            if (!string.IsNullOrEmpty(outputDir)) Directory.CreateDirectory(outputDir);

            var scenes = EnabledScenes();
            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = output,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new Exception("Android build failed: " + report.summary.result);

            Debug.Log("APK built: " + output + " size=" + report.summary.totalSize);
        }

        public static void ValidateContentOnly()
        {
            GenerateRuntimeContent(GetArg("-edition", "play").ToLowerInvariant());
            Debug.Log("Content validation succeeded.");
        }

        public static void PrepareCloudBuild(string edition)
        {
            edition = (edition ?? "play").Trim().ToLowerInvariant();
            if (edition != "play" && edition != "adult")
                throw new ArgumentException("edition must be play or adult");

            GenerateRuntimeContent(edition);
            ConfigureAndroid(edition);

            var scenePath = GenerateScene();
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(scenePath, true)
            };

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Cloud build prepared. Edition=" + edition + ", Scene=" + scenePath);
        }

        private static string[] EnabledScenes()
        {
            var result = new List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene != null && scene.enabled && !string.IsNullOrWhiteSpace(scene.path))
                    result.Add(scene.path);
            }

            if (result.Count == 0)
                result.Add(GenerateScene());

            return result.ToArray();
        }

        private static void ConfigureAndroid(string edition)
        {
            PlayerSettings.companyName = "Moje Alterego";
            PlayerSettings.productName = "Ścieżki Rozkoszy";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.SetApplicationIdentifier(
                NamedBuildTarget.Android,
                edition == "adult"
                    ? "com.mojealterego.pathsofdelight.adult"
                    : "com.mojealterego.pathsofdelight"
            );

            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel36;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.useCustomKeystore = false;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            EditorUserBuildSettings.buildAppBundle = false;
        }

        private static string GenerateScene()
        {
            const string dir = "Assets/Generated";
            const string path = dir + "/Main.unity";

            Directory.CreateDirectory(dir);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cameraObject = new GameObject("Camera");
            cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";

            EditorSceneManager.SaveScene(scene, path);
            AssetDatabase.Refresh();
            return path;
        }

        private static void GenerateRuntimeContent(string edition)
        {
            var cards = new List<ContentCard>();

            AddCatalog(cards, "Content/Play/cards.json");
            if (edition == "adult")
                AddCatalog(cards, "Content/Adult/cards.json");

            if (edition == "play")
            {
                foreach (var card in cards)
                {
                    if (string.Equals(card.edition, "adult", StringComparison.OrdinalIgnoreCase))
                        throw new Exception("Play build contains Adult card: " + card.id);
                }
            }

            if (cards.Count == 0)
                throw new Exception("No content cards found.");

            Directory.CreateDirectory("Assets/Resources/Generated");
            File.WriteAllText(
                "Assets/Resources/Generated/content.json",
                JsonUtility.ToJson(new CardCatalog { cards = cards.ToArray() }, true)
            );
            File.WriteAllText(
                "Assets/Resources/Generated/edition.json",
                "{\"edition\":\"" + edition + "\"}"
            );

            AssetDatabase.Refresh();
        }

        private static void AddCatalog(List<ContentCard> destination, string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Missing content catalog", path);

            var catalog = JsonUtility.FromJson<CardCatalog>(File.ReadAllText(path));
            if (catalog == null || catalog.cards == null)
                throw new Exception("Invalid catalog: " + path);

            foreach (var card in catalog.cards)
            {
                ValidateCard(card, path);
                destination.Add(card);
            }
        }

        private static void ValidateCard(ContentCard card, string source)
        {
            if (card == null || string.IsNullOrWhiteSpace(card.id))
                throw new Exception("Card without id in " + source);
            if (string.IsNullOrWhiteSpace(card.prompt) || string.IsNullOrWhiteSpace(card.activity))
                throw new Exception("Card missing text: " + card.id);
            if (card.intensity < 1 || card.intensity > 3)
                throw new Exception("Invalid intensity: " + card.id);
            if (card.rewardPoints < 0)
                throw new Exception("Negative reward: " + card.id);
            if (card.edition != "play" && card.edition != "adult")
                throw new Exception("Invalid edition: " + card.id);
        }

        private static string GetArg(string key, string fallback)
        {
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], key, StringComparison.OrdinalIgnoreCase))
                    return args[i + 1];
            }
            return fallback;
        }
    }
}
