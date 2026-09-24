using System;
using System.Collections.Generic;
using PhysSim.Core;
using UnityEngine;

namespace PhysSim.Interaction
{
    /// <summary>
    /// Выбор объектов: ЛКМ-рейкаст — выбор, Shift — добавить, клик в пустоту — снять.
    /// Подсветка — мягкая эмиссия материала объекта. Публикует SelectionChanged в шину.
    /// Рейкаст игнорирует хэндлы гизмо (они обрабатываются TransformGizmoService).
    /// </summary>
    public sealed class SelectionManager : MonoBehaviour
    {
        private const string HighlightColorHex = "#2f6fbf";

        private readonly List<SceneObject> _selected = new List<SceneObject>(16);
        private static readonly Color HighlightColor =
            new Color(0.18f, 0.42f, 0.75f, 1f) * 0.35f;

        public IReadOnlyList<SceneObject> Selected
        {
            get { return _selected; }
        }

        private EventBus _bus;
        private EventBus.Subscription _removedSubscription;

        public void Initialize(EventBus bus)
        {
            _bus = bus;
            _removedSubscription = bus.Subscribe<SceneObjectRemoved>(OnObjectRemoved);
        }

        private void OnObjectRemoved(SceneObjectRemoved evt)
        {
            for (var i = _selected.Count - 1; i >= 0; i--)
            {
                if (_selected[i] == null || _selected[i].Id == evt.Id)
                {
                    _selected.RemoveAt(i);
                }
            }

            Publish();
        }

        public bool IsSelected(SceneObject sceneObject)
        {
            return sceneObject != null && _selected.Contains(sceneObject);
        }

        /// <summary>Выбрать объект. additive = Shift.</summary>
        public void Select(SceneObject sceneObject, bool additive = false)
        {
            if (sceneObject == null)
            {
                return;
            }

            if (!additive)
            {
                ClearInternal();
            }

            if (!_selected.Contains(sceneObject))
            {
                _selected.Add(sceneObject);
            }

            RefreshHighlights();
            Publish();
        }

        public void Deselect(SceneObject sceneObject)
        {
            if (sceneObject == null)
            {
                return;
            }

            _selected.Remove(sceneObject);
            SetHighlight(sceneObject, false);
            RefreshHighlights();
            Publish();
        }

        public void Clear()
        {
            if (_selected.Count == 0)
            {
                return;
            }

            ClearInternal();
            Publish();
        }

        /// <summary>Выбор по клику: рейкаст из камеры по всем коллайдерам сцены.</summary>
        public void PickFromCamera(Camera camera, Vector2 screenPosition, bool additive)
        {
            var ray = camera.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out var hit, 5000f))
            {
                // Хэндлы гизмо не выбирают и не сбрасывают выбор.
                if (hit.collider.GetComponentInParent<GizmoHandle>() != null)
                {
                    return;
                }

                var sceneObject = hit.collider.GetComponentInParent<SceneObject>();
                if (sceneObject != null)
                {
                    Select(sceneObject, additive);
                    return;
                }
            }

            if (!additive)
            {
                Clear();
            }
        }

        private void ClearInternal()
        {
            for (var i = 0; i < _selected.Count; i++)
            {
                if (_selected[i] != null)
                {
                    SetHighlight(_selected[i], false);
                }
            }

            _selected.Clear();
        }

        private void RefreshHighlights()
        {
            for (var i = 0; i < _selected.Count; i++)
            {
                if (_selected[i] != null)
                {
                    SetHighlight(_selected[i], true);
                }
            }
        }

        private static void SetHighlight(SceneObject sceneObject, bool on)
        {
            var renderer = sceneObject.MeshRenderer;
            if (renderer == null || renderer.sharedMaterial == null)
            {
                return;
            }

            // Материал объекта индивидуален (создан MaterialApplier) — меняем смело.
            var material = renderer.sharedMaterial;
            if (on)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", HighlightColor);
            }
            else
            {
                material.SetColor("_EmissionColor", Color.black);
            }
        }

        private void Publish()
        {
            if (_bus != null)
            {
                _bus.Publish(new SelectionChanged(_selected.ToArray()));
            }
        }

        private void OnDestroy()
        {
            _removedSubscription.Dispose();
        }
    }
}
