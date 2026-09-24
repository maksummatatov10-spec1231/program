using PhysSim.Core;
using PhysSim.Geometry;
using PhysSim.Simulation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PhysSim.Interaction
{
    /// <summary>
    /// Глобальные горячие клавиши редактора. Игнорируются, пока фокус в поле ввода.
    /// W/E/R — гизмо, P — пауза, B — физика, Del/Ctrl+D — удалить/дублировать,
    /// Ctrl+Z/Y — undo/redo, F — кадрировать, Esc — снять выбор.
    /// </summary>
    public sealed class Hotkeys : MonoBehaviour
    {
        private SimulationManager _simulation;
        private CommandStack _history;
        private SelectionManager _selection;
        private ObjectFactory _factory;
        private ObjectDestroyer _destroyer;
        private TransformGizmoService _gizmo;
        private CameraRig _cameraRig;
        private WorldRegistry _world;

        public void Initialize(
            SimulationManager simulation,
            CommandStack history,
            SelectionManager selection,
            ObjectFactory factory,
            ObjectDestroyer destroyer,
            TransformGizmoService gizmo,
            CameraRig cameraRig,
            WorldRegistry world)
        {
            _simulation = simulation;
            _history = history;
            _selection = selection;
            _factory = factory;
            _destroyer = destroyer;
            _gizmo = gizmo;
            _cameraRig = cameraRig;
            _world = world;
        }

        private void Update()
        {
            if (IsTypingInField())
            {
                return;
            }

            var control = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            var shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            if (control && Input.GetKeyDown(KeyCode.Z))
            {
                if (shift)
                {
                    _history.Redo();
                }
                else
                {
                    _history.Undo();
                }
                return;
            }

            if (control && Input.GetKeyDown(KeyCode.Y))
            {
                _history.Redo();
                return;
            }

            if (control && Input.GetKeyDown(KeyCode.D))
            {
                DuplicateSelection();
                return;
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                _gizmo.SetMode(TransformGizmoService.GizmoMode.Move);
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                _gizmo.SetMode(TransformGizmoService.GizmoMode.Rotate);
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                _gizmo.SetMode(TransformGizmoService.GizmoMode.Scale);
            }
            else if (Input.GetKeyDown(KeyCode.P))
            {
                _simulation.Toggle();
            }
            else if (Input.GetKeyDown(KeyCode.B))
            {
                TogglePhysicsSelection();
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                _cameraRig.FrameSelectionOrAll();
            }
            else if (Input.GetKeyDown(KeyCode.Delete))
            {
                DeleteSelection();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                _selection.Clear();
            }
        }

        private static bool IsTypingInField()
        {
            if (EventSystem.current == null)
            {
                return false;
            }

            var selected = EventSystem.current.currentSelectedGameObject;
            return selected != null && selected.GetComponent<InputField>() != null;
        }

        private void DuplicateSelection()
        {
            if (_selection.Selected.Count == 0)
            {
                return;
            }

            var source = _selection.Selected[_selection.Selected.Count - 1];
            var spec = _factory.Describe(source);
            spec.Id = ObjectId.NewId();
            spec.Position += new Vector3(0.6f, 0f, 0.6f);
            spec.Name = $"{source.DisplayName} (копия)";

            _history.Execute(new SpawnObjectCommand(_factory, _destroyer, spec));
            var created = _world.GetById(spec.Id);
            if (created != null)
            {
                _selection.Select(created, false);
            }
        }

        private void DeleteSelection()
        {
            if (_selection.Selected.Count == 0)
            {
                return;
            }

            var snapshot = new SceneObject[_selection.Selected.Count];
            for (var i = 0; i < snapshot.Length; i++)
            {
                snapshot[i] = _selection.Selected[i];
            }

            for (var i = 0; i < snapshot.Length; i++)
            {
                _history.Execute(new DeleteObjectCommand(_factory, _destroyer, snapshot[i]));
            }
        }

        private void TogglePhysicsSelection()
        {
            if (_selection.Selected.Count == 0)
            {
                return;
            }

            var target = _selection.Selected[_selection.Selected.Count - 1];
            var body = target.GetComponent<SimulatedBody>();
            if (body != null)
            {
                _history.Execute(new TogglePhysicsCommand(target, !body.PhysicsEnabled));
            }
        }
    }
}
