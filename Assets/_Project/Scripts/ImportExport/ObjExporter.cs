using System;
using System.Collections.Generic;
using PhysSim.Core;
using UnityEngine;

namespace PhysSim.ImportExport
{
    /// <summary>
    /// Экспорт выделенных объектов или всей сцены в Wavefront OBJ (M4).
    /// План реализации:
    ///  • вершины в мировых координатах, Y-up, LH→RH (флип X) — файлы открываются в Blender;
    ///  • один объект = одна группа 'o', UV и нормали включаются;
    ///  • переиспользуется сохранением сцены: импортированные меши пишутся в meshes/*.obj
    ///    рядом с scene.json (полностью переносимый сейв — ARCHITECTURE.md §10.2);
    ///  • запись в StringBuilder → File.WriteAllText, без аллокаций в цикле по-максимуму.
    /// </summary>
    public sealed class ObjExporter : MonoBehaviour
    {
        /// <summary>Экспортировать перечисленные объекты в один OBJ-файл.</summary>
        public void ExportObjects(IReadOnlyList<SceneObject> objects, string filePath)
        {
            throw new NotImplementedException("M4: экспорт выделенного в OBJ");
        }

        /// <summary>Экспортировать всю сцену в один OBJ-файл.</summary>
        public void ExportWholeScene(string filePath)
        {
            throw new NotImplementedException("M4: экспорт сцены в OBJ");
        }
    }
}
