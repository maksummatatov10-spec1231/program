using System;
using PhysSim.Core;
using PhysSim.Materials;
using UnityEngine;

namespace PhysSim.ImportExport
{
    /// <summary>
    /// Опции импорта модели (ARCHITECTURE.md §10).
    /// </summary>
    public sealed class ImportOptions
    {
        /// <summary>Позиция размещения в сцене.</summary>
        public Vector3 Position = Vector3.zero;

        /// <summary>Масштаб после импорта (поверх нормализации см→м).</summary>
        public Vector3 Scale = Vector3.one;

        /// <summary>Материал. null — ближайший подбор по имени файла/материала FBX.</summary>
        public MaterialDefinition Material;

        /// <summary>Кадрировать камеру на импортированное.</summary>
        public bool FrameCamera = true;

        /// <summary>Принудительный множитель масштаба (для кривых по размеру файлов).</summary>
        public float UniformScale = 1f;
    }

    /// <summary>
    /// Контракт импортёра моделей (P5). OBJ и FBX — два независимых плагина
    /// за одним интерфейсом; добавление glTF не затронет ни одного модуля.
    /// </summary>
    public interface IModelImporter
    {
        /// <summary>Поддерживаемые расширения без точки, в нижнем регистре: "obj", "fbx".</summary>
        string[] SupportedExtensions { get; }

        bool CanImport(string filePath);

        /// <summary>Импортировать файл: создать SceneObject(ы) через ObjectFactory и вернуть их.</summary>
        SceneObject[] Import(string filePath, ImportOptions options);
    }
}
