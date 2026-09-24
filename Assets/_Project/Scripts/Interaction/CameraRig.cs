using PhysSim.Core;
using UnityEngine;

namespace PhysSim.Interaction
{
    /// <summary>
    /// Орбитальная камера вьюпорта (M0):
    ///  • ПКМ-драг — орбита вокруг целевой точки, MMB — панорама, колесо — зум;
    ///  • F — кадрировать выделенное (FrameBounds);
    ///  • не тормозит при паузе симуляции (SimulationManager не влияет на Update).
    /// </summary>
    public sealed class CameraRig : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float initialDistance = 10f;
        [SerializeField] private float minDistance = 0.5f;
        [SerializeField] private float maxDistance = 500f;
        [SerializeField] private float orbitSpeed = 0.25f;     // градусы/пиксель
        [SerializeField] private float zoomSpeed = 1.1f;       // множитель на «щелчок» колеса

        /// <summary>Точка вращения (перемещается панорамой и FrameBounds).</summary>
        public Transform Target
        {
            get { return target; }
            set { target = value; }
        }

        /// <summary>Навести камеру так, чтобы границы попали в кадр.</summary>
        public void FrameBounds(Bounds bounds)
        {
            // TODO(M0): расстояние от размера сферы, охватывающей bounds (FOV-математика).
        }

        private void Update()
        {
            // TODO(M0): орбита/панорама/зум через Input System (InputActionAsset из Settings/).
        }
    }
}
