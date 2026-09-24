using PhysSim.Core;
using UnityEngine;

namespace PhysSim.Interaction
{
    /// <summary>
    /// Орбитальная камера вьюпорта: ПКМ — орбита, СКМ — панорама, колесо — зум,
    /// F — кадрировать выделенное/всю сцену. Не зависит от паузы симуляции.
    /// </summary>
    public sealed class CameraRig : MonoBehaviour
    {
        [SerializeField] private float orbitSpeed = 0.25f;      // градусы/пиксель
        [SerializeField] private float panSpeed = 0.0016f;      // метры/пиксель×дистанция
        [SerializeField] private float zoomFactor = 1.12f;      // на «щелчок» колеса
        [SerializeField] private float minDistance = 0.4f;
        [SerializeField] private float maxDistance = 800f;

        public Vector3 Pivot = new Vector3(0f, 1f, 0f);
        public float Distance = 12f;
        public float Yaw = 40f;
        public float Pitch = 30f;

        private Camera _camera;
        private SelectionManager _selection;
        private WorldRegistry _world;

        public void Initialize(Camera camera, SelectionManager selection, WorldRegistry world)
        {
            _camera = camera;
            _selection = selection;
            _world = world;
            Apply();
        }

        /// <summary>Камера вьюпорта (для рейкастов выбора и точек спавна).</summary>
        public Camera ViewCamera
        {
            get { return _camera; }
        }

        /// <summary>Кадрировать выделенное; если пусто — всю сцену.</summary>
        public void FrameSelectionOrAll()
        {
            var bounds = ComputeBounds();
            if (!bounds.HasValue)
            {
                return;
            }

            var b = bounds.Value;
            Pivot = b.center;
            var fovRad = (_camera != null ? _camera.fieldOfView : 60f) * Mathf.Deg2Rad;
            Distance = Mathf.Clamp(
                b.extents.magnitude / Mathf.Tan(fovRad * 0.5f) * 1.35f + 0.5f,
                minDistance, maxDistance);
            Apply();
        }

        private Bounds? ComputeBounds()
        {
            var hasAny = false;
            var bounds = new Bounds();

            if (_selection != null && _selection.Selected.Count > 0)
            {
                for (var i = 0; i < _selection.Selected.Count; i++)
                {
                    var renderer = _selection.Selected[i].MeshRenderer;
                    if (renderer == null)
                    {
                        continue;
                    }

                    if (!hasAny)
                    {
                        bounds = renderer.bounds;
                        hasAny = true;
                    }
                    else
                    {
                        bounds.Encapsulate(renderer.bounds);
                    }
                }
            }
            else if (_world != null)
            {
                for (var i = 0; i < _world.Objects.Count; i++)
                {
                    var renderer = _world.Objects[i].MeshRenderer;
                    if (renderer == null)
                    {
                        continue;
                    }

                    if (!hasAny)
                    {
                        bounds = renderer.bounds;
                        hasAny = true;
                    }
                    else
                    {
                        bounds.Encapsulate(renderer.bounds);
                    }
                }
            }

            return hasAny ? bounds : (Bounds?)null;
        }

        private void Update()
        {
            var changed = false;

            // Орбита (ПКМ).
            if (Input.GetMouseButton(1))
            {
                Yaw += Input.GetAxis("Mouse X") * orbitSpeed;
                Pitch -= Input.GetAxis("Mouse Y") * orbitSpeed;
                Pitch = Mathf.Clamp(Pitch, -89f, 89f);
                changed = true;
            }

            // Панорама (СКМ) в плоскости камеры.
            if (Input.GetMouseButton(2))
            {
                var right = transform.right;
                var up = transform.up;
                var delta = -right * (Input.GetAxis("Mouse X") * Distance * panSpeed)
                            - up * (Input.GetAxis("Mouse Y") * Distance * panSpeed);
                Pivot += delta;
                changed = true;
            }

            // Зум (колесо).
            var scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.001f)
            {
                Distance = Mathf.Clamp(Distance * Mathf.Pow(zoomFactor, -scroll), minDistance, maxDistance);
                changed = true;
            }

            if (changed)
            {
                Apply();
            }
        }

        private void Apply()
        {
            var rotation = Quaternion.Euler(Pitch, Yaw, 0f);
            transform.rotation = rotation;
            transform.position = Pivot - rotation * Vector3.forward * Distance;
        }
    }
}
