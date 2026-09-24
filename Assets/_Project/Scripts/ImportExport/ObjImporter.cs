using System;
using PhysSim.Core;
using UnityEngine;

namespace PhysSim.ImportExport
{
    /// <summary>
    /// Импортёр Wavefront OBJ (собственный парсер, без нативных зависимостей).
    /// План реализации (M4):
    ///  • вершины: v (позиция), vn (нормали), vt (UV);
    ///  • полигоны: f — fan-триангуляция (полигон > 4 вершин), отрицательные индексы;
    ///  • группы: o/g — отдельные SceneObject с именем группы;
    ///  • материалы: mtllib/usemtl — опционально, маппинг на ближайший MaterialDefinition по имени;
    ///  • координаты: RH (OBJ) → LH (Unity) — флип X, пересборка порядка обхода треугольников;
    ///  • пост-обработка: RecalculateNormals/Tangents, Bounds, MeshVolumeCalculator.CalculateOrEstimate,
    ///    тяжёлый парс — в background Task, сборка меша — на главном потоке.
    /// </summary>
    public sealed class ObjImporter : MonoBehaviour, IModelImporter
    {
        public string[] SupportedExtensions
        {
            get { return new[] { "obj" }; }
        }

        public bool CanImport(string filePath)
        {
            return filePath != null && filePath.EndsWith(".obj", StringComparison.OrdinalIgnoreCase);
        }

        public SceneObject[] Import(string filePath, ImportOptions options)
        {
            throw new NotImplementedException("M4: парсер OBJ");
        }
    }
}
