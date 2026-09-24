using System.Collections.Generic;
using PhysSim.Core;
using PhysSim.Materials;
using PhysSim.Simulation;
using UnityEngine;

namespace PhysSim.Geometry
{
    /// <summary>
    /// Единая точка создания объектов сцены (P5). Комплектует GameObject по схеме §5:
    /// MeshFilter/Renderer, Collider, Rigidbody (isKinematic), SimulatedBody,
    /// MaterialAssignment; считает объём, назначает материал, регистрирует в
    /// WorldRegistry и публикует SceneObjectAdded. Весь спавн идёт через PrimitiveSpawnSpec
    /// — поэтому undo/redo, дублирование и загрузка сцены используют один кодовый путь.
    /// </summary>
    public sealed class ObjectFactory : MonoBehaviour
    {
        private WorldRegistry _world;
        private EventBus _bus;
        private SimulationManager _simulation;
        private MaterialApplier _materialApplier;
        private MaterialDatabase _materialDatabase;

        private readonly Dictionary<PrimitiveType, int> _counters = new Dictionary<PrimitiveType, int>();
        private int _meshCounter;

        /// <summary>Внедрение зависимостей. Вызывается только AppBootstrap.</summary>
        public void Configure(
            WorldRegistry world,
            EventBus bus,
            SimulationManager simulation,
            MaterialApplier materialApplier,
            MaterialDatabase materialDatabase)
        {
            _world = world;
            _bus = bus;
            _simulation = simulation;
            _materialApplier = materialApplier;
            _materialDatabase = materialDatabase;
        }

        /// <summary>Спека примитива с автоименованием (для undo-дружественного создания).</summary>
        public PrimitiveSpawnSpec BuildPrimitiveSpec(
            PrimitiveType type, Vector3 position, Vector3 scale, MaterialDefinition material = null)
        {
            return new PrimitiveSpawnSpec
            {
                Id = ObjectId.NewId(),
                Name = AutoName(type),
                Primitive = type.ToString(),
                Position = position,
                RotationEuler = Vector3.zero,
                Scale = scale,
                Material = material
            };
        }

        /// <summary>Спека объекта из меша (импорт, дублирование).</summary>
        public PrimitiveSpawnSpec BuildMeshSpec(
            Mesh mesh, string displayName, Vector3 position, Vector3 scale,
            bool useConvexCollider, MaterialDefinition material = null)
        {
            _meshCounter++;
            return new PrimitiveSpawnSpec
            {
                Id = ObjectId.NewId(),
                Name = string.IsNullOrEmpty(displayName)
                    ? $"{UiStrings.NameMesh} {_meshCounter}"
                    : displayName,
                Mesh = mesh,
                ConvexCollider = useConvexCollider,
                Position = position,
                RotationEuler = Vector3.zero,
                Scale = scale,
                Material = material
            };
        }

        /// <summary>Создать примитив с ручным масштабом (панель создания, Ctrl+D и т.д.).</summary>
        public SceneObject CreatePrimitive(
            PrimitiveType type,
            string displayName,
            Vector3 position,
            Vector3 scale,
            MaterialDefinition material = null)
        {
            var spec = BuildPrimitiveSpec(type, position, scale, material);
            if (!string.IsNullOrEmpty(displayName))
            {
                spec.Name = displayName;
            }
            return Spawn(spec);
        }

        /// <summary>Создать объект из произвольного меша (результат импорта OBJ).</summary>
        public SceneObject CreateFromMesh(
            Mesh mesh,
            string displayName,
            Vector3 position,
            Vector3 scale,
            bool useConvexCollider,
            MaterialDefinition material = null)
        {
            return Spawn(BuildMeshSpec(mesh, displayName, position, scale, useConvexCollider, material));
        }

        /// <summary>
        /// Главный метод спавна: строит объект по спеке. Используется всеми путями
        /// (создание, импорт, undo, загрузка сцены).
        /// </summary>
        public SceneObject Spawn(PrimitiveSpawnSpec spec)
        {
            GameObject go;
            Mesh mesh;

            if (!string.IsNullOrEmpty(spec.Primitive))
            {
                var type = (PrimitiveType)System.Enum.Parse(typeof(PrimitiveType), spec.Primitive);
                go = GameObject.CreatePrimitive(type);
                mesh = go.GetComponent<MeshFilter>().sharedMesh;
            }
            else
            {
                go = new GameObject();
                var meshFilter = go.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = spec.Mesh;
                go.AddComponent<MeshRenderer>();
                mesh = spec.Mesh;
            }

            var transform = go.transform;
            transform.position = spec.Position;
            transform.rotation = Quaternion.Euler(spec.RotationEuler);
            transform.localScale = Vector3.Max(spec.Scale, new Vector3(0.0001f, 0.0001f, 0.0001f));

            // Компонентный комплект (§5).
            var sceneObject = go.AddComponent<SceneObject>();
            sceneObject.Initialize(spec.Id, spec.Name);
            sceneObject.Body = go.AddComponent<Rigidbody>();
            sceneObject.Collider = go.GetComponent<Collider>();
            sceneObject.MeshRenderer = go.GetComponent<Renderer>();
            sceneObject.IsImportedMesh = string.IsNullOrEmpty(spec.Primitive);

            var body = go.AddComponent<SimulatedBody>();
            body.World = _simulation;

            if (sceneObject.Collider == null)
            {
                sceneObject.Collider = go.AddComponent<MeshCollider>();
            }

            var meshCollider = sceneObject.Collider as MeshCollider;
            if (meshCollider != null)
            {
                meshCollider.convex = true; // физика (динамическое тело) требует convex
            }

            // Объём → масса. Материал → визуал + физика (атомарно).
            body.SetUnitVolume(MeshVolumeCalculator.CalculateOrEstimate(mesh));
            body.RefreshGeometryCache();

            var material = spec.Material
                ?? _materialDatabase.GetById(spec.MaterialId)
                ?? _materialDatabase.Default;
            _materialApplier.Apply(sceneObject, material);

            // TRS мог измениться (гизмо/инспектор/undo) → пересчёт массы и события.
            sceneObject.TransformApplied += OnObjectTransformApplied;

            body.SetPhysicsEnabled(spec.PhysicsEnabled);
            if (spec.MassOverride.HasValue)
            {
                body.SetMassOverride(spec.MassOverride);
            }

            _world.Track(sceneObject);
            _bus.Publish(new SceneObjectAdded(sceneObject.Id, sceneObject));
            return sceneObject;
        }

        /// <summary>Снимок объекта в спеку (дублирование, команды undo, сериализация).</summary>
        public PrimitiveSpawnSpec Describe(SceneObject sceneObject)
        {
            var meshFilter = sceneObject.GetComponent<MeshFilter>();
            var primitive = sceneObject.IsImportedMesh ? null : GuessPrimitiveName(sceneObject);
            var body = sceneObject.GetComponent<SimulatedBody>();
            var assignment = sceneObject.GetComponent<MaterialAssignment>();

            return new PrimitiveSpawnSpec
            {
                Id = sceneObject.Id,
                Name = sceneObject.DisplayName,
                Primitive = primitive,
                Mesh = primitive == null && meshFilter != null ? meshFilter.sharedMesh : null,
                ConvexCollider = true,
                Position = sceneObject.transform.position,
                RotationEuler = sceneObject.transform.rotation.eulerAngles,
                Scale = sceneObject.transform.localScale,
                MaterialId = assignment != null && assignment.Definition != null
                    ? assignment.Definition.Id
                    : null,
                PhysicsEnabled = body != null && body.PhysicsEnabled,
                MassOverride = body != null ? body.MassOverride : null
            };
        }

        private static string GuessPrimitiveName(SceneObject sceneObject)
        {
            var collider = sceneObject.Collider;
            if (collider is BoxCollider)
            {
                return PrimitiveType.Cube.ToString();
            }
            if (collider is SphereCollider)
            {
                return PrimitiveType.Sphere.ToString();
            }
            if (collider is CapsuleCollider)
            {
                return PrimitiveType.Capsule.ToString();
            }
            // Cylinder в Unity — MeshCollider + Capsule-подобная геометрия; определяем по имени меша.
            var meshFilter = sceneObject.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null &&
                meshFilter.sharedMesh.name.Contains("Cylinder"))
            {
                return PrimitiveType.Cylinder.ToString();
            }

            return null;
        }

        private string AutoName(PrimitiveType type)
        {
            _counters.TryGetValue(type, out var count);
            count++;
            _counters[type] = count;

            string baseName;
            switch (type)
            {
                case PrimitiveType.Sphere: baseName = UiStrings.NameSphere; break;
                case PrimitiveType.Cylinder: baseName = UiStrings.NameCylinder; break;
                case PrimitiveType.Capsule: baseName = UiStrings.NameCapsule; break;
                default: baseName = UiStrings.NameCube; break;
            }

            return $"{baseName} {count}";
        }
    }
}
