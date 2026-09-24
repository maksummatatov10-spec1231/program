using PhysSim.Core;
using PhysSim.Geometry;
using PhysSim.Interaction;
using PhysSim.Materials;
using PhysSim.Simulation;
using UnityEngine;

namespace PhysSim.UI
{
    /// <summary>
    /// Контекст зависимостей UI. Заполняется AppBootstrap, читается панелями.
    /// Панели — тонкие: только отображение и команды, без бизнес-логики.
    /// </summary>
    public sealed class UiContext
    {
        public EventBus Bus;
        public WorldRegistry World;
        public SimulationManager Simulation;
        public CommandStack History;
        public MaterialDatabase Materials;
        public MaterialApplier Applier;
        public ObjectFactory Factory;
        public ObjectDestroyer Destroyer;
        public SelectionManager Selection;
        public TransformGizmoService Gizmo;
        public CameraRig Camera;
        public ImportExport.IModelImporter[] Importers;
        public ImportExport.ObjExporter Exporter;
        public ImportExport.SceneSerializer Serializer;
        public Core.IFileDialogService FileDialog;
    }
}
