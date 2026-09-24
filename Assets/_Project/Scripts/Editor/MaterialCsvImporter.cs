using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using PhysSim.Materials;

namespace PhysSim.EditorTools
{
    /// <summary>
    /// Designer-workflow: импорт docs/materials.csv → SO-ассеты MaterialDefinition
    /// + ассет MaterialDatabase. Программа работает и без этого (встроенная таблица),
    /// но ассеты удобны для правки в инспекторе и назначения текстур.
    ///
    /// Формат CSV (разделитель «;», кодировка UTF-8):
    /// id;имя;категория;плотность;статТрение;динТрение;упругость;Cd;r;g;b;metallic;smoothness
    /// </summary>
    public static class MaterialCsvImporter
    {
        private const string OutputFolder = "Assets/_Project/Data/Materials";

        [MenuItem("PhysSim/Материалы: импорт CSV в ассеты", priority = 20)]
        public static void ImportCsv()
        {
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            var csvPath = Path.Combine(projectRoot ?? ".", "docs", "materials.csv");
            if (!File.Exists(csvPath))
            {
                EditorUtility.DisplayDialog("PhysSim Studio", "Не найден файл docs/materials.csv", "ОК");
                return;
            }

            if (!Directory.Exists(OutputFolder))
            {
                Directory.CreateDirectory(OutputFolder);
            }

            var databasePath = $"{OutputFolder}/MaterialDatabase.asset";
            var database = AssetDatabase.LoadAssetAtPath<MaterialDatabase>(databasePath);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<MaterialDatabase>();
                AssetDatabase.CreateAsset(database, databasePath);
            }

            var lines = File.ReadAllLines(csvPath, Encoding.UTF8);
            var imported = 0;
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (line.Length == 0 || line.StartsWith("#") || line.StartsWith("id;"))
                {
                    continue;
                }

                var parts = line.Split(';');
                if (parts.Length < 13)
                {
                    Debug.LogWarning($"[Материалы] Строка {i + 1}: ожидалось 13 колонок, пропущена.");
                    continue;
                }

                var id = parts[0].Trim();
                var assetPath = $"{OutputFolder}/{id}.asset";
                var definition = AssetDatabase.LoadAssetAtPath<MaterialDefinition>(assetPath);
                if (definition == null)
                {
                    definition = ScriptableObject.CreateInstance<MaterialDefinition>();
                    AssetDatabase.CreateAsset(definition, assetPath);
                }

                var culture = CultureInfo.InvariantCulture;
                var spec = new MaterialSpec(
                    id,
                    parts[1].Trim(),
                    ParseCategory(parts[2]),
                    float.Parse(parts[3], culture),
                    float.Parse(parts[4], culture),
                    float.Parse(parts[5], culture),
                    float.Parse(parts[6], culture),
                    float.Parse(parts[7], culture),
                    new Color32(
                        byte.Parse(parts[8], culture),
                        byte.Parse(parts[9], culture),
                        byte.Parse(parts[10], culture),
                        255),
                    float.Parse(parts[11], culture),
                    float.Parse(parts[12], culture));

                definition.RuntimeSetup(spec);
                definition.name = id;
                EditorUtility.SetDirty(definition);

                if (!database.Register(definition))
                {
                    Debug.LogWarning($"[Материалы] Дубликат id «{id}» в базе.");
                }
                imported++;
            }

            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Материалы] Импортировано {imported} материалов → {OutputFolder}");
        }

        private static MaterialCategory ParseCategory(string text)
        {
            switch (text.Trim().ToLowerInvariant())
            {
                case "metal": return MaterialCategory.Metal;
                case "alloy": return MaterialCategory.Alloy;
                case "wood": return MaterialCategory.Wood;
                case "paper": return MaterialCategory.Paper;
                case "plastic": return MaterialCategory.Plastic;
                case "rubber": return MaterialCategory.Rubber;
                case "glass": return MaterialCategory.Glass;
                case "ceramic": return MaterialCategory.Ceramic;
                case "stone": return MaterialCategory.Stone;
                case "fabric": return MaterialCategory.Fabric;
                case "organic": return MaterialCategory.Organic;
                case "composite": return MaterialCategory.Composite;
                default: return MaterialCategory.Other;
            }
        }
    }
}
