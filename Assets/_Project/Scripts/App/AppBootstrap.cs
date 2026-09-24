using PhysSim.Core;
using PhysSim.Interaction;
using PhysSim.Simulation;
using UnityEngine;

namespace PhysSim.App
{
    /// <summary>
    /// Единственный composition root (P3). Собирает сервисы в строгом порядке
    /// (ARCHITECTURE.md §13) и регистрирует их в ServiceRegistry.
    /// UI инициализируется последним — он только читает домен и шлёт команды.
    /// </summary>
    public sealed class AppBootstrap : MonoBehaviour
    {
        [Tooltip("База материалов (ассет MaterialDatabase.asset).")]
        [SerializeField] private MaterialDatabase materialDatabase;

        private ServiceRegistry _services;

        private void Awake()
        {
            _services = new ServiceRegistry();

            // 1. Core: шина, реестр, стек команд.
            var bus = new EventBus();
            var world = new WorldRegistry();
            var history = new CommandStack();
            _services.Register(bus);
            _services.Register(world);
            _services.Register(history);

            // 2. Simulation: приложение стартует на паузе (это редактор).
            var simulation = CreateChild<SimulationManager>("Simulation");
            simulation.Initialize(bus);
            _services.Register(simulation);

            // 3. Materials: аплаер материалов.
            var materialApplier = CreateChild<MaterialDatabase.MaterialApplier>("MaterialApplier");
            _services.Register(materialApplier);

            // 4. Geometry: фабрика объектов.
            var factory = CreateChild<Geometry.ObjectFactory>("ObjectFactory");
            factory.Configure(world, bus, simulation, materialApplier);
            _services.Register(factory);

            // 5. Interaction: камера, выбор, гизмо.
            var selection = CreateChild<SelectionManager>("Selection");
            selection.Initialize(bus);
            _services.Register(selection);

            // 6. ImportExport: импортёры/экспортёр/сериализатор (подключаются в M4).
            // TODO(M4): ObjImporter, FbxImporter, ObjExporter, SceneSerializer
            //           + IFileDialogService из UI.

            // 7. UI — последним.
            // TODO(M0): UIRoot.Initialize(bus, world, simulation, materialDatabase).
            //           CreateChild<UIRoot>("UIRoot") — на GO с UIDocument в сцене Main.

            // 8. Стартовое состояние: пустая сцена или восстановление последней
            //    (PlayerPrefs: путь) — решается настройкой в M4.
        }

        private T CreateChild<T>(string name) where T : MonoBehaviour
        {
            var child = new GameObject(name);
            child.transform.SetParent(transform, false);
            return child.AddComponent<T>();
        }
    }
}
