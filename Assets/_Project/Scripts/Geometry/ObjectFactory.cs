using System;
using PhysSim.Core;
using PhysSim.Materials;
using PhysSim.Simulation;
using UnityEngine;

namespace PhysSim.Geometry
{
    /// <summary>
    /// Единая точка создания и удаления объектов сцены (P5).
    /// Комплектует GameObject по схеме §5: MeshFilter/Renderer, Collider,
    /// Rigidbody (isKinematic), SimulatedBody, MaterialAssignment; считает
    /// объём, назначает материал и публикует SceneObjectAdded.
    /// </summary>
    public sealed class ObjectFactory : MonoBehaviour
    {
        [Tooltip("База материалов — материал по умолчанию для новых объектов.")]
        [SerializeField] private MaterialDatabase materialDatabase;

        [Tooltip("Префаб обёртки (пустой GO с SceneObject). Пусто — создаётся кодом.")]
        [SerializeField] private GameObject sceneObjectPrefab;

        private WorldRegistry _world;
        private EventBus _bus;
        private SimulationManager _simulation;
        private MaterialApplier _materialApplier;

        /// <summary>Внедрение зависимостей. Вызывается только AppBootstrap.</summary>
        public void Configure(
            WorldRegistry world,
            EventBus bus,
            SimulationManager simulation,
            MaterialApplier materialApplier)
        {
            _world = world;
            _bus = bus;
            _simulation = simulation;
            _materialApplier = materialApplier;
        }

        /// <summary>Создать примитив с ручным масштабом (ввод в CreatePanel).</summary>
        public SceneObject CreatePrimitive(
            PrimitiveType type,
            string displayName,
            Vector3 position,
            Vector3 scale,
            MaterialDefinition material = null)
        {
            // TODO(M1): собрать GO (Unity-примитив → снять меш в ассеты для единообразия),
            // вызвать AssembleObject, вернуть SceneObject. Здесь и ниже — контракты модуля.
            throw new NotImplementedException("M1: создание примитивов");
        }

        /// <summary>Создать объект из произвольного меша (результат импорта OBJ/FBX).</summary>
        public SceneObject CreateFromMesh(
            Mesh mesh,
            string displayName,
            Vector3 position,
            Vector3 scale,
            bool useConvexCollider,
            MaterialDefinition material = null)
        {
            // TODO(M4): MeshCollider (convex для физики), далее — общий AssembleObject.
            throw new NotImplementedException("M4: создание объектов из мешей импорта");
        }

        /// <summary>Удалить объект (через команду DeleteObjectCommand, прямые вызовы запрещены).</summary>
        public void DestroyObject(SceneObject target)
        {
            // TODO(M1): Untrack → снять выделение → Destroy + SceneObjectRemoved.
            // Удаление всегда идёт командой, чтобы работал Undo.
            throw new NotImplementedException("M1: удаление объектов");
        }

        /// <summary>Дублировать существующий объект (Ctrl+D).</summary>
        public SceneObject Duplicate(SceneObject source)
        {
            // TODO(M5): копия TRS + меша + материала, новый id, команда SpawnObjectCommand.
            throw new NotImplementedException("M5: дублирование");
        }

        /// <summary>Общая сборка: комплект компонентов + объём + материал + регистрация.</summary>
        private SceneObject AssembleObject(
            GameObject go,
            string displayName,
            MaterialDefinition material)
        {
            // План (реализуется в M1):
            //  1. SceneObject.Initialize(ObjectId.NewId(), displayName)
            //  2. Rigidbody: isKinematic = true, interpolation = Interpolate, collisionDetection = ContinuousSpeculative
            //  3. SimulatedBody + World = _simulation
            //  4. MeshVolumeCalculator.CalculateOrEstimate(mesh) → body.SetVolume(v)
            //  5. _materialApplier.Apply(go, material ?? _materialDatabase.Default, _bus)
            //  6. _world.Track(sceneObject); _bus.Publish(new SceneObjectAdded(id, sceneObject))
            throw new NotImplementedException("M1: сборка объекта");
        }
    }
}
