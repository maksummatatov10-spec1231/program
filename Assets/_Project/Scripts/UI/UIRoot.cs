using System.Collections.Generic;
using PhysSim.Core;
using PhysSim.ImportExport;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PhysSim.UI
{
    /// <summary>
    /// Корень интерфейса: canvas, панели, файловые операции, справка.
    /// Инициализируется ПОСЛЕДНИМ из AppBootstrap — только читает домен и шлёт команды.
    /// </summary>
    public sealed class UIRoot : MonoBehaviour
    {
        private UiContext _ctx;
        private readonly ToolbarPanel _toolbar = new ToolbarPanel();
        private readonly CreatePanel _create = new CreatePanel();
        private readonly HierarchyPanel _hierarchy = new HierarchyPanel();
        private readonly InspectorPanel _inspector = new InspectorPanel();
        private readonly MaterialLibraryPanel _library = new MaterialLibraryPanel();
        private readonly StatusBarPanel _status = new StatusBarPanel();

        private GameObject _helpOverlay;
        private RectTransform _canvasRoot;
        private float _tickTimer;
        private bool _slowTick;

        public void Initialize(UiContext ctx)
        {
            _ctx = ctx;

            EnsureEventSystem();
            _canvasRoot = BuildCanvas();
            ctx.FileDialog = new RuntimeFileDialogService(_canvasRoot);

            // Области экрана.
            var toolbarRect = Region(canvas, "Toolbar",
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -46f), new Vector2(0f, 0f));
            var createRect = Region(canvas, "CreatePanel",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(8f, -204f), new Vector2(288f, -54f));
            var hierarchyRect = Region(canvas, "HierarchyPanel",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(8f, -472f), new Vector2(288f, -212f));
            var libraryRect = Region(canvas, "LibraryPanel",
                new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(8f, 32f), new Vector2(288f, -480f));
            var inspectorRect = Region(canvas, "InspectorPanel",
                new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-308f, 32f), new Vector2(-8f, -54f));
            var statusRect = Region(canvas, "StatusBar",
                new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 26f));

            _toolbar.Build(toolbarRect, ctx);
            _toolbar.OnNew = FileNew;
            _toolbar.OnOpen = FileOpen;
            _toolbar.OnSave = FileSave;
            _toolbar.OnImport = FileImport;
            _toolbar.OnExport = FileExport;
            _toolbar.OnToggleHelp = ToggleHelp;

            _create.Build(createRect, ctx, SpawnPoint,
                created => ctx.Selection.Select(created, false));
            _hierarchy.Build(hierarchyRect, ctx);
            _inspector.Build(inspectorRect, ctx);
            _library.Build(libraryRect, ctx);
            _status.Build(statusRect, ctx);

            ctx.Bus.Publish(new StatusMessage("info",
                "Готово. Материалов в базе: " + ctx.Materials.All.Count));
        }

        private void Update()
        {
            _tickTimer += Time.unscaledDeltaTime;
            if (_tickTimer < 0.25f)
            {
                return;
            }

            _tickTimer = 0f;
            _slowTick = !_slowTick;
            _inspector.UpdateTick();
            if (_slowTick)
            {
                _status.Tick();
            }
        }

        private void OnDestroy()
        {
            _toolbar.Dispose();
            _create.Dispose();
            _hierarchy.Dispose();
            _inspector.Dispose();
            _library.Dispose();
            _status.Dispose();
        }

        // ───────────────────────────── Файловые операции ─────────────────────────────

        private void FileNew()
        {
            _ctx.Destroyer.DestroyAll();
            _ctx.History.Clear();
            _ctx.Simulation.Pause();
            _ctx.Simulation.SetGravity(9.80665f);
            _ctx.Simulation.SetAirDensity(1.225f);
            _ctx.Simulation.TimeScale = 1f;
            _ctx.Bus.Publish(new StatusMessage("info", UiStrings.SceneCleared));
        }

        private void FileOpen()
        {
            _ctx.FileDialog.OpenFile(UiStrings.SceneFileFilterTitle, UiStrings.SceneExtension, path =>
            {
                if (string.IsNullOrEmpty(path))
                {
                    return;
                }

                try
                {
                    _ctx.Serializer.LoadScene(path);
                }
                catch (System.Exception exception)
                {
                    _ctx.Bus.Publish(new StatusMessage("error",
                        string.Format(UiStrings.ImportFailedFormat, exception.Message)));
                }
            });
        }

        private void FileSave()
        {
            _ctx.FileDialog.SaveFile(UiStrings.SceneFileFilterTitle, UiStrings.DefaultSceneName,
                UiStrings.SceneExtension, path =>
                {
                    if (string.IsNullOrEmpty(path))
                    {
                        return;
                    }

                    try
                    {
                        _ctx.Serializer.SaveScene(path);
                    }
                    catch (System.Exception exception)
                    {
                        _ctx.Bus.Publish(new StatusMessage("error",
                            string.Format(UiStrings.ImportFailedFormat, exception.Message)));
                    }
                });
        }

        private void FileImport()
        {
            _ctx.FileDialog.OpenFile(UiStrings.ImportFileTitle, "obj", path =>
            {
                if (string.IsNullOrEmpty(path))
                {
                    return;
                }

                var extension = System.IO.Path.GetExtension(path).TrimStart('.').ToLowerInvariant();
                IModelImporter importer = null;
                for (var i = 0; i < _ctx.Importers.Length; i++)
                {
                    var extensions = _ctx.Importers[i].SupportedExtensions;
                    for (var e = 0; e < extensions.Length; e++)
                    {
                        if (extensions[e] == extension)
                        {
                            importer = _ctx.Importers[i];
                        }
                    }
                }

                if (importer == null)
                {
                    _ctx.Bus.Publish(new StatusMessage("warn",
                        $"Формат «{extension}» не поддерживается. Доступны: OBJ."));
                    return;
                }

                try
                {
                    var options = new ImportOptions { Position = SpawnPoint() };
                    var created = importer.Import(path, options);
                    if (created != null && created.Length > 0)
                    {
                        _ctx.Selection.Select(created[0], false);
                        _ctx.Camera.FrameSelectionOrAll();
                        _ctx.Bus.Publish(new StatusMessage("info", string.Format(
                            UiStrings.ImportedFormat, System.IO.Path.GetFileName(path), created.Length)));
                    }
                }
                catch (System.Exception exception)
                {
                    _ctx.Bus.Publish(new StatusMessage("error",
                        string.Format(UiStrings.ImportFailedFormat, exception.Message)));
                }
            });
        }

        private void FileExport()
        {
            var count = _ctx.World.Objects.Count;
            if (count == 0)
            {
                _ctx.Bus.Publish(new StatusMessage("warn", UiStrings.NothingToExport));
                return;
            }

            _ctx.FileDialog.SaveFile(UiStrings.ExportFileTitle, UiStrings.DefaultModelName, "obj", path =>
            {
                if (string.IsNullOrEmpty(path))
                {
                    return;
                }

                try
                {
                    if (_ctx.Selection.Selected.Count > 0)
                    {
                        var selected = new List<SceneObject>(_ctx.Selection.Selected);
                        _ctx.Exporter.ExportObjects(selected, path);
                    }
                    else
                    {
                        _ctx.Exporter.ExportWholeScene(path);
                    }

                    _ctx.Bus.Publish(new StatusMessage("info",
                        string.Format(UiStrings.SavedToFormat, path)));
                }
                catch (System.Exception exception)
                {
                    _ctx.Bus.Publish(new StatusMessage("error",
                        string.Format(UiStrings.ImportFailedFormat, exception.Message)));
                }
            });
        }

        // ───────────────────────────── Справка ─────────────────────────────

        private void ToggleHelp()
        {
            if (_helpOverlay != null)
            {
                Destroy(_helpOverlay);
                _helpOverlay = null;
                return;
            }

            var canvas = (RectTransform)transform.GetChild(0).GetChild(0); // UI Canvas → Root
            var dim = UiFactory.NewRect(canvas, "HelpOverlay");
            UiFactory.Stretch(dim, 0f, 0f, 0f, 0f);
            UiFactory.AddImage(dim.gameObject, new Color(0f, 0f, 0f, 0.55f), true);
            _helpOverlay = dim.gameObject;

            var panel = UiFactory.NewRect(dim, "HelpPanel");
            UiFactory.Place(panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-300f, -220f), new Vector2(300f, 220f));
            UiFactory.AddImage(panel.gameObject, UiFactory.PanelColor, true);

            var title = UiFactory.NewRect(panel, "Title");
            UiFactory.Place(title, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(16f, -40f), new Vector2(-16f, -10f));
            UiFactory.AddText(title.gameObject, UiStrings.HelpTitle, 16,
                TextAnchor.MiddleLeft, UiFactory.TextColor);

            var text = UiFactory.NewRect(panel, "Text");
            UiFactory.Place(text, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(16f, -320f), new Vector2(-16f, -44f));
            UiFactory.AddText(text.gameObject, UiStrings.HelpText, 13,
                TextAnchor.UpperLeft, UiFactory.TextColor);

            var close = UiFactory.DecorateButton(UiFactory.NewRect(panel, "Close"),
                UiStrings.Close, 13, UiFactory.ButtonAccentColor, ToggleHelp);
            UiFactory.Place((RectTransform)close.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(-70f, 10f), new Vector2(70f, 40f));
        }

        // ───────────────────────────── Инфраструктура ─────────────────────────────

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            var eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        private static RectTransform BuildCanvas()
        {
            var canvasObject = new GameObject("UI Canvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600f, 900f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();

            var root = UiFactory.NewRect(canvasObject.transform, "Root");
            UiFactory.Stretch(root, 0f, 0f, 0f, 0f);
            return root;
        }

        private Vector3 SpawnPoint()
        {
            var camera = _ctx.Camera != null ? _ctx.Camera.ViewCamera : Camera.main;
            if (camera == null)
            {
                return new Vector3(0f, 1f, 0f);
            }

            var forward = camera.transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.01f)
            {
                forward = Vector3.forward;
            }
            forward.Normalize();
            return camera.transform.position + forward * 5f;
        }

        private static RectTransform Region(RectTransform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var rect = UiFactory.NewRect(parent, name);
            UiFactory.Place(rect, anchorMin, anchorMax, offsetMin, offsetMax);
            return rect;
        }
    }
}
