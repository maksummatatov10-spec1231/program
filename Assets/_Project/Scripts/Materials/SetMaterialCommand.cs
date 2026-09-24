using PhysSim.Core;
using UnityEngine;

namespace PhysSim.Materials
{
    /// <summary>Назначение материала объекту (undoable). Визуал + физика применяются атомарно.</summary>
    public sealed class SetMaterialCommand : ICommand
    {
        private readonly MaterialApplier _applier;
        private readonly SceneObject _target;
        private readonly MaterialDefinition _previous;
        private readonly MaterialDefinition _next;

        public string Description
        {
            get { return $"Материал «{(_next != null ? _next.DisplayName : "—")}»"; }
        }

        public SetMaterialCommand(MaterialApplier applier, SceneObject target,
            MaterialDefinition next, MaterialDefinition previous)
        {
            _applier = applier;
            _target = target;
            _next = next;
            _previous = previous;
        }

        public void Execute()
        {
            Apply(_next);
        }

        public void Undo()
        {
            Apply(_previous);
        }

        private void Apply(MaterialDefinition definition)
        {
            if (_target != null)
            {
                _applier.Apply(_target, definition);
            }
        }
    }
}
