using PhysSim.Core;
using UnityEngine;

namespace PhysSim.Simulation
{
    /// <summary>Включение/выключение физики объекта (undoable).</summary>
    public sealed class TogglePhysicsCommand : ICommand
    {
        private readonly SceneObject _target;
        private readonly SimulatedBody _body;
        private readonly bool _enable;

        public string Description
        {
            get { return $"Физика {_enable} для {_target.DisplayName}"; }
        }

        public TogglePhysicsCommand(SceneObject target, bool enable)
        {
            _target = target;
            _body = target != null ? target.GetComponent<SimulatedBody>() : null;
            _enable = enable;
        }

        public void Execute()
        {
            Apply(_enable);
        }

        public void Undo()
        {
            Apply(!_enable);
        }

        private void Apply(bool enable)
        {
            if (_body != null)
            {
                _body.SetPhysicsEnabled(enable);
            }
        }
    }
}
