using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using PhysSim.Core;
using PhysSim.Geometry;
using PhysSim.Materials;
using PhysSim.Simulation;
using UnityEngine;

namespace PhysSim.ImportExport
{
    /// <summary>
    /// DTO сцены (§10.2): плоские типы без ссылок на Unity-объекты, версионирование.
    /// Сейв = scene.pscene (JSON) + папка meshes/*.obj с импортированной геометрией.
    /// </summary>
    [Serializable]
    public sealed class SceneDocument
    {
        public const int CurrentVersion = 1;

        public int Version = CurrentVersion;
        public string SavedAtIso8601 = "";
        public float Gravity = 9.80665f;
        public float AirDensity = 1.225f;
        public float TimeScale = 1f;
        public List<SceneObjectDto> Objects = new List<SceneObjectDto>();
    }

    [Serializable]
    public sealed class SceneObjectDto
    {
        public string Id = "";
        public string Name = "";
        public string Primitive;
        public string MeshFile;
        public float[] Position = { 0f, 0f, 0f };
        public float[] Rotation = { 0f, 0f, 0f };
        public float[] Scale = { 1f, 1f, 1f };
        public string MaterialId = "";
        public bool PhysicsEnabled;
        public float? MassOverride;
    }

    /// <summary>
    /// Сохранение/загрузка сцены: Newtonsoft JSON, атомарная запись (tmp → move),
    /// события SceneSaved/SceneLoaded для статус-бара и UI.
    /// </summary>
    public sealed class SceneSerializer : MonoBehaviour
    {
        private WorldRegistry _world;
        private EventBus _bus;
        private SimulationManager _simulation;
        private ObjectFactory _factory;
        private MaterialDatabase _materials;
        private ObjectDestroyer _destroyer;
        private CommandStack _history;

        public void Configure(
            WorldRegistry world,
            EventBus bus,
            SimulationManager simulation,
            ObjectFactory factory,
            MaterialDatabase materials,
            ObjectDestroyer destroyer,
            CommandStack history)
        {
            _world = world;
            _bus = bus;
            _simulation = simulation;
            _factory = factory;
            _materials = materials;
            _destroyer = destroyer;
            _history = history;
        }

        /// <summary>Сохранить текущую сцену в файл .pscene.</summary>
        public void SaveScene(string filePath)
        {
            var document = new SceneDocument
            {
                SavedAtIso8601 = DateTime.UtcNow.ToString("o"),
                Gravity = _simulation.GravityMagnitude,
                AirDensity = _simulation.AirDensity,
                TimeScale = _simulation.TimeScale
            };

            var meshesDir = Path.Combine(Path.GetDirectoryName(filePath) ?? ".", "meshes");
            Directory.CreateDirectory(meshesDir);

            for (var i = 0; i < _world.Objects.Count; i++)
            {
                var sceneObject = _world.Objects[i];
                var spec = _factory.Describe(sceneObject);

                var dto = new SceneObjectDto
                {
                    Id = spec.Id.ToString(),
                    Name = spec.Name,
                    Primitive = spec.Primitive,
                    Position = ToArray(spec.Position),
                    Rotation = ToArray(spec.RotationEuler),
                    Scale = ToArray(spec.Scale),
                    MaterialId = spec.MaterialId ?? "",
                    PhysicsEnabled = spec.PhysicsEnabled,
                    MassOverride = spec.MassOverride
                };

                if (spec.Primitive == null && spec.Mesh != null)
                {
                    var meshFileName = spec.Id + ".obj";
                    ObjExporter.ExportMeshTo(spec.Mesh, Path.Combine(meshesDir, meshFileName));
                    dto.MeshFile = "meshes/" + meshFileName;
                }

                document.Objects.Add(dto);
            }

            // Атомарная запись: сначала tmp, затем move.
            var tmpPath = filePath + ".tmp";
            File.WriteAllText(tmpPath, JsonConvert.SerializeObject(document, Formatting.Indented));
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            File.Move(tmpPath, filePath);

            _bus.Publish(new SceneSaved(filePath));
        }

        /// <summary>Загрузить сцену из файла .pscene (текущая сцена заменяется).</summary>
        public void LoadScene(string filePath)
        {
            var document = JsonConvert.DeserializeObject<SceneDocument>(File.ReadAllText(filePath));
            if (document == null)
            {
                throw new InvalidDataException("Файл сцены пуст или повреждён: " + filePath);
            }

            if (document.Version > SceneDocument.CurrentVersion)
            {
                Debug.LogWarning($"[SceneSerializer] Версия сцены {document.Version} новее поддерживаемой " +
                                 $"{SceneDocument.CurrentVersion} — пытаемся загрузить как есть.");
            }

            _destroyer.DestroyAll();
            _history.Clear();

            _simulation.SetGravity(document.Gravity);
            _simulation.SetAirDensity(document.AirDensity);
            _simulation.TimeScale = document.TimeScale;

            var baseDirectory = Path.GetDirectoryName(filePath) ?? ".";

            for (var i = 0; i < document.Objects.Count; i++)
            {
                var dto = document.Objects[i];
                if (!ObjectId.TryParse(dto.Id, out var id))
                {
                    id = ObjectId.NewId();
                }

                var spec = new PrimitiveSpawnSpec
                {
                    Id = id,
                    Name = dto.Name,
                    Primitive = dto.Primitive,
                    Position = ToVector3(dto.Position),
                    RotationEuler = ToVector3(dto.Rotation),
                    Scale = ToVector3(dto.Scale),
                    MaterialId = string.IsNullOrEmpty(dto.MaterialId) ? null : dto.MaterialId,
                    PhysicsEnabled = dto.PhysicsEnabled,
                    MassOverride = dto.MassOverride
                };

                if (spec.Primitive == null && !string.IsNullOrEmpty(dto.MeshFile))
                {
                    var meshPath = Path.Combine(baseDirectory, dto.MeshFile);
                    spec.Mesh = File.Exists(meshPath) ? ObjImporter.LoadSingleMesh(meshPath) : null;
                }

                _factory.Spawn(spec);
            }

            _bus.Publish(new SceneLoaded(filePath, document.Objects.Count));
        }

        private static float[] ToArray(Vector3 vector)
        {
            return new[] { vector.x, vector.y, vector.z };
        }

        private static Vector3 ToVector3(float[] values)
        {
            return values != null && values.Length >= 3
                ? new Vector3(values[0], values[1], values[2])
                : Vector3.zero;
        }
    }
}
