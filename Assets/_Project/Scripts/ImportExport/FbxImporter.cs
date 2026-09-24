using System;
using PhysSim.Core;
using UnityEngine;

namespace PhysSim.ImportExport
{
    /// <summary>
    /// Импортёр FBX через AssimpNet (единственный сторонний «жирный» компонент,
    /// спрятан за IModelImporter — ARCHITECTURE.md §14, §17).
    /// План реализации (M4):
    ///  • AiImportFile → aiScene → обход нод (иерархия FBX → плоский список SceneObject v1);
    ///  • мешы: aiMesh → Unity Mesh (позиции/нормали/UV/кости — кости в бэклоге);
    ///  • координаты: RH+см (FBX-конвенция) → LH+м: флип X, scale 0.01 (параметр в опциях);
    ///  • материалы: FbxSurfacePhong/Lambert → подбор ближайшего MaterialDefinition по имени;
    ///  • нативные плагины (win/mac/linux) — в Assets/Plugins/, рядом с managed-сборкой.
    /// Если плагин недоступен — импортёр честно сообщает об этом в StatusMessage,
    /// приложение продолжает работать (OBJ-путь никуда не делся).
    /// </summary>
    public sealed class FbxImporter : MonoBehaviour, IModelImporter
    {
        public string[] SupportedExtensions
        {
            get { return new[] { "fbx" }; }
        }

        public bool CanImport(string filePath)
        {
            return filePath != null && filePath.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase);
        }

        public SceneObject[] Import(string filePath, ImportOptions options)
        {
            throw new NotImplementedException("M4: FBX через AssimpNet");
        }
    }
}
