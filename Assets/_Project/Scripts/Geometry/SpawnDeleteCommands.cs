using PhysSim.Core;
using PhysSim.Materials;
using UnityEngine;

namespace PhysSim.Geometry
{
    /// <summary>Создание объекта (undoable: undo удаляет, redo создаёт заново с тем же id).</summary>
    public sealed class SpawnObjectCommand : ICommand
    {
        private readonly ObjectFactory _factory;
        private readonly ObjectDestroyer _destroyer;
        private readonly PrimitiveSpawnSpec _spec;
        private SceneObject _created;

        public string Description { get { return $"Создать: {_spec.Name}"; } }

        public SpawnObjectCommand(ObjectFactory factory, ObjectDestroyer destroyer, PrimitiveSpawnSpec spec)
        {
            _factory = factory;
            _destroyer = destroyer;
            _spec = spec;
        }

        public void Execute()
        {
            if (_created == null)
            {
                _created = _factory.Spawn(_spec);
            }
        }

        public void Undo()
        {
            if (_created != null)
            {
                _destroyer.HardDestroy(_created);
            }
        }
    }

    /// <summary>Удаление объекта (undoable: undo возвращает объект с тем же id, TRS и материалом).</summary>
    public sealed class DeleteObjectCommand : ICommand
    {
        private readonly ObjectFactory _factory;
        private readonly ObjectDestroyer _destroyer;
        private readonly SceneObject _target;
        private readonly PrimitiveSpawnSpec _snapshot;

        public string Description { get { return $"Удалить: {_target.DisplayName}"; } }

        public DeleteObjectCommand(ObjectFactory factory, ObjectDestroyer destroyer, SceneObject target)
        {
            _factory = factory;
            _destroyer = destroyer;
            _target = target;
            _snapshot = factory.Describe(target);
        }

        public void Execute()
        {
            _destroyer.HardDestroy(_target);
        }

        public void Undo()
        {
            _factory.Spawn(_snapshot.Clone());
        }
    }
}
