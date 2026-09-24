using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace PhysSim.EditorTools
{
    /// <summary>
    /// Сборка standalone-билда одной кнопкой: PhysSim → Собрать EXE (Windows x64).
    /// Результат: Build/Windows/PhysSimStudio.exe — готовая переносимая программа.
    /// </summary>
    public static class BuildScript
    {
        private const string OutputPath = "Build/Windows/PhysSimStudio.exe";
        private const string ScenePath = "Assets/_Project/Scenes/Main.unity";

        [MenuItem("PhysSim/Собрать EXE (Windows x64)", priority = 0)]
        public static void BuildWindows64()
        {
            if (!File.Exists(ScenePath))
            {
                EditorUtility.DisplayDialog("PhysSim Studio",
                    "Не найдена сцена " + ScenePath, "ОК");
                return;
            }

            var outputDirectory = Path.GetDirectoryName(OutputPath);
            if (!string.IsNullOrEmpty(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = OutputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            });

            if (report.summary.result == BuildResult.Succeeded)
            {
                var fullPath = Path.GetFullPath(OutputPath);
                Debug.Log($"[PhysSim] Сборка готова: {fullPath} " +
                          $"({report.summary.totalSize / (1024 * 1024)} МБ)");
                EditorUtility.RevealInFinder(fullPath);
            }
            else
            {
                Debug.LogError("[PhysSim] Сборка не удалась — подробности в Console.");
            }
        }

        [MenuItem("PhysSim/Открыть сцену Main", priority = 10)]
        public static void OpenMainScene()
        {
            if (File.Exists(ScenePath))
            {
                EditorSceneManager.OpenScene(ScenePath);
            }
        }
    }
}
