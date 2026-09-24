using System;
using System.Collections.Generic;
using PhysSim.Core;
using UnityEngine;

namespace PhysSim.Interaction
{
    /// <summary>
    /// Выбор объектов: ЛКМ-рейкаст — выбор, Shift — добавить к выбору, клик в пустоту — снять.
    /// Публикует SelectionChanged в шину (подсветку и инспектор обновляет UI/Interaction).
    /// </summary>
    public sealed class SelectionManager : MonoBehaviour
    {
        private readonly List<SceneObject> _selected = new List<SceneObject>(16);

        public IReadOnlyList<SceneObject> Selected
        {
            get { return _selected; }
        }

        public event Action<IReadOnlyList<SceneObject>> SelectionChanged;

        private EventBus _bus;

        public void Initialize(EventBus bus)
        {
            _bus = bus;
        }

        public bool IsSelected(SceneObject sceneObject)
        {
            return sceneObject != null && _selected.Contains(sceneObject);
        }

        /// <summary>Выбрать объект. additive = Shift.</summary>
        public void Select(SceneObject sceneObject, bool additive = false)
        {
            // TODO(M1): логика выбора/добавления + подсветка + Publish(SelectionChanged).
        }

        public void Deselect(SceneObject sceneObject)
        {
            // TODO(M1).
        }

        public void Clear()
        {
            // TODO(M1).
        }

        /// <summary>Выбор по клику: рейкаст из камеры по всем коллайдерам сцены.</summary>
        public void PickFromCamera(Camera camera, Vector2 screenPosition, bool additive)
        {
            // TODO(M1): Raycast → collider.GetComponentInParent<SceneObject>() → Select().
        }
    }
}
