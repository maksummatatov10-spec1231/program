using System;
using PhysSim.Core;
using UnityEngine;

namespace PhysSim.ImportExport
{
    /// <summary>
    /// FBX — закрытый бинарный формат: в рантайме читается только через нативный
    /// плагин (AssimpNet). v1 — честная заглушка: сообщает в статус-бар и не ломает
    /// поток импорта (OBJ работает полностью). Путь подключения — docs/BUILD_RU.md §5:
    ///  1) AssimpNet managed-DLL в Assets/Plugins/, нативные — в Plugins/x86_64;
    ///  2) AiImportFile → aiScene → обход нод (иерархия FBX → плоский список объектов v1);
    ///  3) aiMesh → Unity Mesh (позиции/нормали/UV), флип X + см→м;
    ///  4) материал — по имени FbxSurfacePhong → ближайший MaterialDefinition;
    ///  5) фабрика.CreateFromMesh — дальше общий конвейер (объём, масса, материал).
    /// </summary>
    public sealed class FbxImporter : MonoBehaviour, IModelImporter
    {
        private EventBus _bus;

        public void Configure(EventBus bus)
        {
            _bus = bus;
        }

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
            if (_bus != null)
            {
                _bus.Publish(new StatusMessage("warn", UiStrings.FbxNeedsPlugin));
            }
            else
            {
                Debug.LogWarning(UiStrings.FbxNeedsPlugin);
            }

            return Array.Empty<SceneObject>();
        }
    }
}
