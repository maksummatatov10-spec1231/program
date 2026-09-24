using System.Collections.Generic;
using PhysSim.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PhysSim.Interaction
{
    /// <summary>
    /// Гизмо трансформаций: Move/Rotate/Scale (режимы W/E/R). Хэндлы — процедурные
    /// меши с коллайдерами; драг по осям через рейкаст-математику (линия/плоскость/
    /// экранный угол). На отпускание кнопки в стек истории уходит TransformObjectCommand.
    /// </summary>
    public sealed class TransformGizmoService : MonoBehaviour
    {
        public enum GizmoMode
        {
            None = 0,
            Move = 1,
            Rotate = 2,
            Scale = 3
        }

        public enum HandleKind
        {
            MoveAxis = 0,
            MovePlane = 1,
            RotateAxis = 2,
            ScaleAxis = 3,
            ScaleUniform = 4
        }

        public GizmoMode Mode { get; private set; } = GizmoMode.None;

        /// <summary>Шаг привязки перемещения, м (удерживайте Shift).</summary>
        public float MoveSnap = 0.25f;

        /// <summary>Шаг привязки вращения, градусы (Shift).</summary>
        public float RotateSnap = 15f;

        private static readonly Color[] AxisColors =
        {
            new Color(0.9f, 0.25f, 0.25f),
            new Color(0.3f, 0.85f, 0.3f),
            new Color(0.3f, 0.5f, 0.95f)
        };

        private static readonly Vector3[] AxisVectors =
        {
            Vector3.right, Vector3.up, Vector3.forward
        };

        private Camera _camera;
        private EventBus _bus;
        private SelectionManager _selection;
        private CommandStack _history;
        private readonly List<EventBus.Subscription> _subscriptions = new List<EventBus.Subscription>(4);

        private SceneObject _target;
        private GameObject _root;
        private readonly List<GameObject> _handleObjects = new List<GameObject>(16);

        // Состояние драга.
        private bool _dragging;
        private HandleKind _dragKind;
        private int _dragAxis;
        private TransformSnapshot _dragStart;
        private Vector3 _dragAxisWorld;
        private float _startLineT;
        private Vector3 _planeStartPoint;
        private float _startScreenAngle;
        private Vector2 _startMouse;
        private Vector2 _axisScreenDir;

        public void Initialize(Camera camera, EventBus bus, SelectionManager selection, CommandStack history)
        {
            _camera = camera;
            _bus = bus;
            _selection = selection;
            _history = history;

            _root = new GameObject("Gizmo");
            _root.SetActive(false);

            _subscriptions.Add(bus.Subscribe<SelectionChanged>(OnSelectionChanged));
        }

        private void OnSelectionChanged(SelectionChanged evt)
        {
            _target = evt.Selected != null && evt.Selected.Length == 1 ? evt.Selected[0] : null;
            Rebuild();
        }

        public void SetMode(GizmoMode mode)
        {
            Mode = mode;
            Rebuild();
        }

        private void OnDestroy()
        {
            for (var i = 0; i < _subscriptions.Count; i++)
            {
                _subscriptions[i].Dispose();
            }
        }

        // ───────────────────────────── Построение ─────────────────────────────

        private void Rebuild()
        {
            ClearHandles();

            if (_target == null || Mode == GizmoMode.None)
            {
                if (_root != null)
                {
                    _root.SetActive(false);
                }
                return;
            }

            _root.SetActive(true);

            if (Mode == GizmoMode.Move)
            {
                for (var axis = 0; axis < 3; axis++)
                {
                    CreateArrow(axis);
                }
                CreateBoxHandle(HandleKind.MovePlane, 0, Vector3.zero, 0.16f, new Color(1f, 1f, 1f, 0.6f));
            }
            else if (Mode == GizmoMode.Rotate)
            {
                for (var axis = 0; axis < 3; axis++)
                {
                    CreateRing(axis);
                    var knobPosition = RingKnobPosition(axis);
                    CreateBoxHandle(HandleKind.RotateAxis, axis, knobPosition, 0.14f, AxisColors[axis]);
                }
            }
            else if (Mode == GizmoMode.Scale)
            {
                for (var axis = 0; axis < 3; axis++)
                {
                    CreateBoxHandle(HandleKind.ScaleAxis, axis, AxisVectors[axis] * 0.85f, 0.16f, AxisColors[axis]);
                }
                CreateBoxHandle(HandleKind.ScaleUniform, 0, Vector3.zero, 0.16f, Color.white);
            }
        }

        private void ClearHandles()
        {
            for (var i = 0; i < _handleObjects.Count; i++)
            {
                Destroy(_handleObjects[i]);
            }
            _handleObjects.Clear();
        }

        private void CreateArrow(int axis)
        {
            var color = AxisColors[axis];
            var direction = AxisVectors[axis];

            // Стержень.
            var shaft = CreateHandleGameObject(HandleKind.MoveAxis, axis, color);
            shaft.transform.localPosition = direction * 0.5f;
            shaft.transform.localRotation = Quaternion.FromToRotation(Vector3.up, direction);
            shaft.transform.localScale = new Vector3(0.05f, 1f, 0.05f);
            var shaftCollider = shaft.AddComponent<BoxCollider>();
            shaftCollider.size = Vector3.one;

            // Наконечник.
            var tip = CreateHandleGameObject(HandleKind.MoveAxis, axis, color);
            tip.transform.localPosition = direction * 1.05f;
            tip.transform.localScale = Vector3.one * 0.16f;
            var tipCollider = tip.AddComponent<BoxCollider>();
            tipCollider.size = Vector3.one;
        }

        private void CreateBoxHandle(HandleKind kind, int axis, Vector3 localPosition, float size, Color color)
        {
            var handle = CreateHandleGameObject(kind, axis, color);
            handle.transform.localPosition = localPosition;
            handle.transform.localScale = Vector3.one * size;
            var collider = handle.AddComponent<BoxCollider>();
            collider.size = Vector3.one;
        }

        private GameObject CreateHandleGameObject(HandleKind kind, int axis, Color color)
        {
            var handle = new GameObject($"H_{kind}_{axis}");
            handle.transform.SetParent(_root.transform, false);

            var renderer = handle.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = GetHandleMaterial(color);

            var filter = handle.AddComponent<MeshFilter>();
            filter.sharedMesh = GetUnitCube();

            handle.AddComponent<GizmoHandle>().Kind = kind;
            var gizmoHandle = handle.GetComponent<GizmoHandle>();
            gizmoHandle.Axis = axis;

            _handleObjects.Add(handle);
            return handle;
        }

        private void CreateRing(int axis)
        {
            var ringObject = new GameObject($"Ring_{axis}");
            ringObject.transform.SetParent(_root.transform, false);
            _handleObjects.Add(ringObject);

            var line = ringObject.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.loop = true;
            line.positionCount = 48;
            line.widthMultiplier = 0.025f;
            line.sharedMaterial = GetHandleMaterial(AxisColors[axis]);

            for (var i = 0; i < 48; i++)
            {
                var angle = i / 48f * Mathf.PI * 2f;
                var cos = Mathf.Cos(angle);
                var sin = Mathf.Sin(angle);
                switch (axis)
                {
                    case 0: line.SetPosition(i, new Vector3(0f, cos, sin)); break;   // вокруг X
                    case 1: line.SetPosition(i, new Vector3(cos, 0f, sin)); break;   // вокруг Y
                    default: line.SetPosition(i, new Vector3(cos, sin, 0f)); break;  // вокруг Z
                }
            }
        }

        private static Vector3 RingKnobPosition(int axis)
        {
            switch (axis)
            {
                case 0: return new Vector3(0f, 1f, 0f);
                case 1: return new Vector3(1f, 0f, 0f);
                default: return new Vector3(1f, 0f, 0f);
            }
        }

        private static Mesh _unitCube;
        private static Mesh GetUnitCube()
        {
            if (_unitCube != null)
            {
                return _unitCube;
            }

            _unitCube = new Mesh
            {
                vertices = new[]
                {
                    new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, -0.5f, -0.5f),
                    new Vector3(0.5f, 0.5f, -0.5f), new Vector3(-0.5f, 0.5f, -0.5f),
                    new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(0.5f, -0.5f, 0.5f),
                    new Vector3(0.5f, 0.5f, 0.5f), new Vector3(-0.5f, 0.5f, 0.5f)
                },
                triangles = new[]
                {
                    0, 2, 1, 0, 3, 2, // зад
                    4, 5, 6, 4, 6, 7, // перед
                    0, 1, 5, 0, 5, 4, // низ
                    3, 6, 2, 3, 7, 6, // верх
                    0, 4, 7, 0, 7, 3, // лево
                    1, 2, 6, 1, 6, 5  // право
                }
            };
            _unitCube.RecalculateNormals();
            _unitCube.RecalculateBounds();
            return _unitCube;
        }

        private static Material GetHandleMaterial(Color color)
        {
            if (_handleMaterials == null)
            {
                _handleMaterials = new Dictionary<Color, Material>();
            }

            if (!_handleMaterials.TryGetValue(color, out var material))
            {
                var shader = Shader.Find("Standard");
                material = new Material(shader)
                {
                    color = color,
                    hideFlags = HideFlags.HideAndDontSave
                };
                if (material.HasProperty("_Metallic"))
                {
                    material.SetFloat("_Metallic", 0f);
                }
                if (material.HasProperty("_Glossiness"))
                {
                    material.SetFloat("_Glossiness", 0.3f);
                }
                _handleMaterials.Add(color, material);
            }

            return material;
        }

        private static Dictionary<Color, Material> _handleMaterials;

        // ───────────────────────────── Апдейт и драг ─────────────────────────────

        private void Update()
        {
            if (_target == null || _root == null || !_root.activeSelf)
            {
                return;
            }

            // Объект мог быть удалён.
            if (_target == null)
            {
                _target = null;
                Rebuild();
                return;
            }

            PositionRoot();

            if (!_dragging)
            {
                if (Input.GetMouseButtonDown(0) && !IsPointerOverUi())
                {
                    TryBeginDrag();
                }
            }
            else
            {
                DragUpdate();
                if (Input.GetMouseButtonUp(0))
                {
                    EndDrag();
                }
            }
        }

        private void PositionRoot()
        {
            _root.transform.position = _target.transform.position;
            var distance = _camera != null
                ? Vector3.Distance(_camera.transform.position, _target.transform.position)
                : 10f;
            var scale = Mathf.Clamp(distance * 0.07f, 0.15f, 50f);
            _root.transform.localScale = new Vector3(scale, scale, scale);
        }

        private static bool IsPointerOverUi()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }

        private void TryBeginDrag()
        {
            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            var hits = Physics.RaycastAll(ray, 5000f);
            GameObject best = null;
            var bestDistance = float.MaxValue;
            for (var i = 0; i < hits.Length; i++)
            {
                var handle = hits[i].collider.GetComponentInParent<GizmoHandle>();
                if (handle == null || hits[i].distance >= bestDistance)
                {
                    continue;
                }

                bestDistance = hits[i].distance;
                best = handle.gameObject;
                _dragKind = handle.Kind;
                _dragAxis = handle.Axis;
            }

            if (best == null)
            {
                // ЛКМ мимо гизмо → выбор объекта под курсором (Shift — добавить).
                _selection.PickFromCamera(_camera, Input.mousePosition,
                    Input.GetKey(KeyCode.LeftShift));
                return;
            }

            _dragging = true;
            _dragStart = TransformSnapshot.From(_target);
            _dragAxisWorld = _target.transform.TransformDirection(AxisVectors[_dragAxis]).normalized;
            _startMouse = Input.mousePosition;

            switch (_dragKind)
            {
                case HandleKind.MoveAxis:
                    _startLineT = RayLineParameter(ray.origin, ray.direction,
                        _target.transform.position, _dragAxisWorld);
                    break;

                case HandleKind.MovePlane:
                    var plane = new Plane(-_camera.transform.forward, _target.transform.position);
                    if (plane.Raycast(ray, out var enter))
                    {
                        _planeStartPoint = ray.GetPoint(enter);
                    }
                    break;

                case HandleKind.RotateAxis:
                    _startScreenAngle = ScreenAngleAroundAxis();
                    break;

                case HandleKind.ScaleAxis:
                    _axisScreenDir = AxisScreenDirection();
                    break;
            }
        }

        private void DragUpdate()
        {
            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            var snapping = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            switch (_dragKind)
            {
                case HandleKind.MoveAxis:
                {
                    var lineT = RayLineParameter(ray.origin, ray.direction,
                        _dragStart.Position, _dragAxisWorld);
                    var delta = lineT - _startLineT;
                    if (snapping && MoveSnap > 0f)
                    {
                        delta = Mathf.Round(delta / MoveSnap) * MoveSnap;
                    }
                    var position = _dragStart.Position + _dragAxisWorld * delta;
                    _target.ApplyTrs(position, _dragStart.RotationEuler, _dragStart.Scale);
                    break;
                }

                case HandleKind.MovePlane:
                {
                    var plane = new Plane(-_camera.transform.forward, _dragStart.Position);
                    if (plane.Raycast(ray, out var enter))
                    {
                        var point = ray.GetPoint(enter);
                        var position = _dragStart.Position + (point - _planeStartPoint);
                        _target.ApplyTrs(position, _dragStart.RotationEuler, _dragStart.Scale);
                    }
                    break;
                }

                case HandleKind.RotateAxis:
                {
                    var angle = ScreenAngleAroundAxis();
                    var delta = Mathf.DeltaAngle(_startScreenAngle, angle);
                    if (snapping && RotateSnap > 0f)
                    {
                        delta = Mathf.Round(delta / RotateSnap) * RotateSnap;
                    }
                    var rotation = Quaternion.AngleAxis(delta, _dragAxisWorld)
                                   * Quaternion.Euler(_dragStart.RotationEuler);
                    _target.ApplyTrs(_dragStart.Position, rotation.eulerAngles, _dragStart.Scale);
                    break;
                }

                case HandleKind.ScaleAxis:
                {
                    var mouse = (Vector2)Input.mousePosition;
                    var t = Vector2.Dot(mouse - _startMouse, _axisScreenDir);
                    var factor = Mathf.Clamp(1f + t * 0.01f, 0.001f, 10000f);
                    var scale = _dragStart.Scale;
                    scale[_dragAxis] = _dragStart.Scale[_dragAxis] * factor;
                    _target.ApplyTrs(_dragStart.Position, _dragStart.RotationEuler, scale);
                    break;
                }

                case HandleKind.ScaleUniform:
                {
                    var mouse = (Vector2)Input.mousePosition;
                    var factor = Mathf.Clamp(1f + (mouse.x - _startMouse.x) * 0.005f, 0.001f, 10000f);
                    _target.ApplyTrs(_dragStart.Position, _dragStart.RotationEuler,
                        _dragStart.Scale * factor);
                    break;
                }
            }
        }

        private void EndDrag()
        {
            _dragging = false;
            if (_target == null)
            {
                return;
            }

            var current = TransformSnapshot.From(_target);
            if (current.Position != _dragStart.Position
                || current.RotationEuler != _dragStart.RotationEuler
                || current.Scale != _dragStart.Scale)
            {
                _history.Execute(new TransformObjectCommand(_target, _dragStart, current));
            }
        }

        // ───────────────────────────── Математика ─────────────────────────────

        /// <summary>Параметр s ближайшей точки линии (P + s·L) к лучу (O + t·D).</summary>
        private static float RayLineParameter(Vector3 rayOrigin, Vector3 rayDirection,
            Vector3 linePoint, Vector3 lineDirection)
        {
            var r = rayOrigin - linePoint;
            var a = Vector3.Dot(rayDirection, rayDirection);
            var b = Vector3.Dot(rayDirection, lineDirection);
            var c = Vector3.Dot(lineDirection, lineDirection);
            var d = Vector3.Dot(rayDirection, r);
            var e = Vector3.Dot(lineDirection, r);
            var denominator = a * c - b * b;
            return Mathf.Abs(denominator) < 1e-8f ? 0f : (a * e - b * d) / denominator;
        }

        /// <summary>Угол мыши вокруг проекции центра (экранная плоскость), со знаком по оси.</summary>
        private float ScreenAngleAroundAxis()
        {
            var center = _camera.WorldToScreenPoint(_target.transform.position);
            var mouse = Input.mousePosition;
            var angle = Mathf.Atan2(mouse.y - center.y, mouse.x - center.x) * Mathf.Rad2Deg;
            var axisTowardsCamera = Vector3.Dot(_dragAxisWorld, _camera.transform.forward) > 0f;
            return axisTowardsCamera ? -angle : angle;
        }

        private Vector2 AxisScreenDirection()
        {
            var origin = _camera.WorldToScreenPoint(_target.transform.position);
            var tip = _camera.WorldToScreenPoint(_target.transform.position + _dragAxisWorld);
            var direction = new Vector2(tip.x - origin.x, tip.y - origin.y);
            return direction.sqrMagnitude > 1e-6f ? direction.normalized : Vector2.right;
        }
    }
}
