using System;
using System.Collections.Generic;
using UnityEngine;

namespace PhysSim.ImportExport
{
    /// <summary>
    /// DTO сцены (ARCHITECTURE.md §10.2). Плоские типы без ссылок на Unity-объекты —
    /// устойчивость к рефакторингу, версионирование полем Version + миграторы.
    /// Сейв = scene.json + папка meshes/*.obj с импортированной геометрией.
    /// </summary>
    [Serializable]
    public sealed class SceneDocument
    {
        public const int CurrentVersion = 1;

        public int Version = CurrentVersion;

        /// <summary>ISO-8601 момент сохранения.</summary>
        public string SavedAtIso8601 = "";

        /// <summary>Модуль гравитации, м/с² (направление −Y в v1).</summary>
        public float Gravity = 9.80665f;

        /// <summary>Плотность воздуха, кг/м³.</summary>
        public float AirDensity = 1.225f;

        public float TimeScale = 1f;

        public List<SceneObjectDto> Objects = new List<SceneObjectDto>();
    }

    [Serializable]
    public sealed class SceneObjectDto
    {
        public string Id = "";

        public string Name = "";

        /// <summary>Имя примитива ("Cube"/"Sphere"/...). null — кастомная геометрия.</summary>
        public string Primitive;

        /// <summary>Относительный путь к меш-файлу (meshes/part.obj) для импортированной геометрии.</summary>
        public string MeshFile;

        /// <summary>Позиция [x, y, z].</summary>
        public float[] Position = { 0f, 0f, 0f };

        /// <summary>Вращение в градусах Эйлера [x, y, z].</summary>
        public float[] Rotation = { 0f, 0f, 0f };

        /// <summary>Масштаб [x, y, z].</summary>
        public float[] Scale = { 1f, 1f, 1f };

        /// <summary>id материала из MaterialDatabase. Пустая строка — материал по умолчанию.</summary>
        public string MaterialId = "";

        public bool PhysicsEnabled;

        /// <summary>Явное переопределение массы, кг. null — масса = ρ·V.</summary>
        public float? MassOverride;
    }

    /// <summary>
    /// Сохранение/загрузка сцены (Newtonsoft JSON, версионирование).
    /// </summary>
    public sealed class SceneSerializer : MonoBehaviour
    {
        public void Save(SceneDocument document, string filePath)
        {
            // TODO(M4): JsonSerializerSettings (Formatting.Indented, NullValueHandling.Ignore),
            // атомарная запись (tmp → move), публикация SceneSaved.
            throw new NotImplementedException("M4: сохранение сцены");
        }

        public SceneDocument Load(string filePath)
        {
            // TODO(M4): чтение, проверка Version, миграторы при несовпадении,
            // публикация SceneLoaded(path, objects.Count).
            throw new NotImplementedException("M4: загрузка сцены");
        }
    }
}
