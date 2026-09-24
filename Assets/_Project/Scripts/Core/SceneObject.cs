using UnityEngine;

namespace PhysSim.Core
{
    /// <summary>
    /// Фасад объекта сцены: идентификатор, отображаемое имя и ссылки на
    /// стандартные компоненты. Логики минимум — она живёт в модулях.
    /// ApplyTrs — единственная точка изменения TRS: события TransformApplied
    /// слушает фабрика (пересчёт объёма/массы при смене масштаба).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SceneObject : MonoBehaviour
    {
        /// <summary>Назначается фабрикой/импортёром ровно один раз.</summary>
        public ObjectId Id { get; private set; }

        [Tooltip("Отображаемое имя в иерархии и UI.")]
        public string DisplayName = "Объект";

        [Tooltip("Физика. Создаётся всегда; isKinematic = true, пока физика выключена.")]
        public Rigidbody Body;

        [Tooltip("Коллайдер (Box/Sphere/Capsule/Mesh).")]
        public Collider Collider;

        [Tooltip("Рендерер (MeshRenderer либо SkinnedMeshRenderer после FBX-импорта).")]
        public Renderer MeshRenderer;

        [Tooltip("true — геометрия получена импортом (OBJ), а не примитивом.")]
        public bool IsImportedMesh;

        /// <summary>Вызывается после любого изменения TRS (гизмо, инспектор, undo).</summary>
        public event System.Action<SceneObject> TransformApplied;

        /// <summary>Инициализация фасада. Вызывается только фабрикой/импортёром.</summary>
        public void Initialize(ObjectId id, string displayName)
        {
            Id = id;
            Rename(displayName);
        }

        /// <summary>Переименовать (фасад синхронизирует GameObject.name).</summary>
        public void Rename(string displayName)
        {
            DisplayName = string.IsNullOrEmpty(displayName) ? "Объект" : displayName;
            gameObject.name = DisplayName;
        }

        /// <summary>Применить TRS. Единственная точка входа для команд трансформации.</summary>
        public void ApplyTrs(Vector3 position, Vector3 rotationEuler, Vector3 scale)
        {
            var t = transform;
            t.position = position;
            t.rotation = Quaternion.Euler(rotationEuler);
            t.localScale = Vector3.Max(scale, new Vector3(0.0001f, 0.0001f, 0.0001f));
            TransformApplied?.Invoke(this);
        }
    }
}
