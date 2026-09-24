using UnityEngine;

namespace PhysSim.Core
{
    /// <summary>
    /// Фасад объекта сцены: идентификатор, отображаемое имя и ссылки на
    /// стандартные компоненты. Логики здесь минимум — она живёт в модулях
    /// (Simulation, Materials, Geometry). См. ARCHITECTURE.md §5.
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

        [Tooltip("true — геометрия получена импортом (OBJ/FBX), а не примитивом.")]
        public bool IsImportedMesh;

        /// <summary>Инициализация фасада. Вызывается только фабрикой/импортёром.</summary>
        public void Initialize(ObjectId id, string displayName)
        {
            Id = id;
            DisplayName = string.IsNullOrEmpty(displayName) ? "Объект" : displayName;
            gameObject.name = DisplayName;
        }
    }
}
