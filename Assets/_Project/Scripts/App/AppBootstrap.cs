using PhysSim.Core;
using PhysSim.Geometry;
using PhysSim.ImportExport;
using PhysSim.Interaction;
using PhysSim.Materials;
using PhysSim.Simulation;
using PhysSim.UI;
using UnityEngine;

namespace PhysSim.App
{
    /// <summary>
    /// Единственный composition root (P3). Собирает сервисы в строгом порядке
    /// (ARCHITECTURE.md §13), создаёт сцену по умолчанию (камера, свет, земля)
    /// и запускает UI последним. Приложение стартует НА ПАУЗЕ — это редактор.
    /// </summary>
    public sealed class AppBootstrap : MonoBehaviour
    {
        [Tooltip("База материалов (ассет). null — рантайм-база из встроенной таблицы 121 материала.")]
        [SerializeField] private MaterialDatabase materialDatabase;

        private void Awake()
        {
            // 1. Core.
            var bus = new EventBus();
            var world = new WorldRegistry();
            var history = new CommandStack();
            var destroyer = new ObjectDestroyer(world, bus);

            // 2. Simulation (старт на паузе — внутри Awake менеджера).
            var simulation = NewChild<SimulationManager>("Simulation");
            simulation.Initialize(bus);

            // 3. Materials: ассет либо рантайм-база из встроенной таблицы.
            var materials = materialDatabase != null
                ? materialDatabase
                : MaterialDatabase.CreateRuntime(MaterialSpecs.All, MaterialSpecs.DefaultId);
            var applier = NewChild<MaterialApplier>("MaterialApplier");
            applier.Initialize(bus);

            // 4. Камера и свет вьюпорта.
            var cameraObject = new GameObject(UiStrings.CameraName);
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 60f;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 2000f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.13f, 0.15f, 0.18f);
            cameraObject.AddComponent<AudioListener>();

            var lightObject = new GameObject(UiStrings.LightName);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.color = new Color(1f, 0.97f, 0.92f);
            light.shadows = LightShadows.Soft;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // 5. Interaction.
            var selection = NewChild<SelectionManager>("Selection");
            selection.Initialize(bus);
            var cameraRig = cameraObject.AddComponent<CameraRig>();
            cameraRig.Initialize(camera, selection, world);

            // 6. Geometry.
            var factory = NewChild<ObjectFactory>("ObjectFactory");
            factory.Configure(world, bus, simulation, applier, materials);
            CreateGround();

            // 7. ImportExport.
            var exporter = NewChild<ObjExporter>("ObjExporter");
            exporter.Configure(world);
            var serializer = NewChild<SceneSerializer>("SceneSerializer");
            serializer.Configure(world, bus, simulation, factory, materials, destroyer, history);
            var objImporter = NewChild<ObjImporter>("ObjImporter");
            objImporter.Configure(factory, materials);
            var fbxImporter = NewChild<FbxImporter>("FbxImporter");
            fbxImporter.Configure(bus);

            // 8. Гизмо и горячие клавиши.
            var gizmo = NewChild<TransformGizmoService>("GizmoService");
            gizmo.Initialize(camera, bus, selection, history);
            var hotkeys = NewChild<Hotkeys>("Hotkeys");
            hotkeys.Initialize(simulation, history, selection, factory, destroyer, gizmo, cameraRig, world);

            // 9. UI — последним.
            var uiRoot = NewChild<UIRoot>("UIRoot");
            uiRoot.Initialize(new UiContext
            {
                Bus = bus,
                World = world,
                Simulation = simulation,
                History = history,
                Materials = materials,
                Applier = applier,
                Factory = factory,
                Destroyer = destroyer,
                Selection = selection,
                Gizmo = gizmo,
                Camera = cameraRig,
                Importers = new IModelImporter[] { objImporter, fbxImporter },
                Exporter = exporter,
                Serializer = serializer
            });
        }

        private T NewChild<T>(string name) where T : MonoBehaviour
        {
            var child = new GameObject(name);
            child.transform.SetParent(transform, false);
            return child.AddComponent<T>();
        }

        /// <summary>Земля-сетка 200×200 м с коллайдером (не SceneObject — окружение).</summary>
        private static void CreateGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = UiStrings.GroundName;
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(20f, 1f, 20f); // примитив Plane = 10×10 м

            var renderer = ground.GetComponent<MeshRenderer>();
            var shader = Shader.Find("Standard");
            if (shader == null)
            {
                return;
            }

            var material = new Material(shader);
            material.mainTexture = MakeGridTexture();
            material.mainTextureScale = new Vector2(100f, 100f);
            renderer.sharedMaterial = material;
        }

        private static Texture2D MakeGridTexture()
        {
            const int size = 256;
            const int cell = 32;
            var background = new Color32(44, 44, 47, 255);
            var line = new Color32(60, 60, 64, 255);

            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Point
            };

            var pixels = new Color32[size * size];
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var isLine = x % cell == 0 || y % cell == 0;
                    pixels[y * size + x] = isLine ? line : background;
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
        }
    }
}
