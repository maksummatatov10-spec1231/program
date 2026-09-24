using PhysSim.Core;
using PhysSim.Materials;
using PhysSim.Simulation;
using UnityEngine;
using UnityEngine.UIElements;

namespace PhysSim.UI
{
    /// <summary>
    /// Корень интерфейса (UI Toolkit, ARCHITECTURE.md §11). Инициализируется ПОСЛЕДНИМ:
    /// подписывается на события домена и отдаёт команды сервисам. Панели — тонкие,
    /// бизнес-логики не содержат (P2).
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class UIRoot : MonoBehaviour
    {
        [SerializeField] private VisualTreeAsset layoutAsset;

        // Ссылки на панели-контроллеры (создаются в Initialize):
        // ToolbarPanel, CreatePanel, HierarchyPanel, InspectorPanel,
        // MaterialLibraryPanel, FilePanel, StatusBarPanel.

        private EventBus _bus;
        private WorldRegistry _world;
        private SimulationManager _simulation;
        private MaterialDatabase _materials;

        /// <summary>Внедрение зависимостей из AppBootstrap.</summary>
        public void Initialize(
            EventBus bus,
            WorldRegistry world,
            SimulationManager simulation,
            MaterialDatabase materials)
        {
            _bus = bus;
            _world = world;
            _simulation = simulation;
            _materials = materials;

            // TODO(M0): собрать VisualTree из layoutAsset, создать StatusBar + Toolbar.
            // TODO(M1): CreatePanel, HierarchyPanel, InspectorPanel.
            // TODO(M3): MaterialLibraryPanel.
            // TODO(M4): FilePanel + регистрация IFileDialogService.
        }

        private void OnDestroy()
        {
            // TODO: отписаться от всех событий шины (панели делают это в своих Shutdown).
        }
    }
}
