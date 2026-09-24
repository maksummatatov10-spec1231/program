using System;
using UnityEngine;

namespace PhysSim.Core
{
    /// <summary>Снимок TRS объекта для команд трансформации.</summary>
    public readonly struct TransformSnapshot
    {
        public readonly Vector3 Position;
        public readonly Vector3 RotationEuler;
        public readonly Vector3 Scale;

        public TransformSnapshot(Vector3 position, Vector3 rotationEuler, Vector3 scale)
        {
            Position = position;
            RotationEuler = rotationEuler;
            Scale = scale;
        }

        public static TransformSnapshot From(SceneObject sceneObject)
        {
            var t = sceneObject.transform;
            return new TransformSnapshot(t.position, t.rotation.eulerAngles, t.localScale);
        }
    }

    /// <summary>Трансформация объекта (гизмо, числовой ввод). Execute = «к», Undo = «от».</summary>
    public sealed class TransformObjectCommand : ICommand
    {
        private readonly SceneObject _target;
        private readonly TransformSnapshot _from;
        private readonly TransformSnapshot _to;

        public string Description { get { return $"Трансформация: {_target.DisplayName}"; } }

        public TransformObjectCommand(SceneObject target, TransformSnapshot from, TransformSnapshot to)
        {
            _target = target;
            _from = from;
            _to = to;
        }

        public void Execute()
        {
            Apply(_to);
        }

        public void Undo()
        {
            Apply(_from);
        }

        private void Apply(TransformSnapshot snapshot)
        {
            if (_target == null)
            {
                return; // объект мог быть удалён — команда нейтрализуется
            }

            _target.ApplyTrs(snapshot.Position, snapshot.RotationEuler, snapshot.Scale);
        }
    }

    /// <summary>Переименование объекта (публикует ObjectRenamed для UI).</summary>
    public sealed class RenameObjectCommand : ICommand
    {
        private readonly SceneObject _target;
        private readonly string _from;
        private readonly string _to;
        private readonly EventBus _bus;

        public string Description { get { return $"Переименовать в «{_to}»"; } }

        public RenameObjectCommand(SceneObject target, string from, string to, EventBus bus = null)
        {
            _target = target;
            _from = from;
            _to = to;
            _bus = bus;
        }

        public void Execute()
        {
            if (_target != null)
            {
                _target.Rename(_to);
                Publish(_to);
            }
        }

        public void Undo()
        {
            if (_target != null)
            {
                _target.Rename(_from);
                Publish(_from);
            }
        }

        private void Publish(string name)
        {
            if (_bus != null)
            {
                _bus.Publish(new ObjectRenamed(_target.Id, name));
            }
        }
    }
}
