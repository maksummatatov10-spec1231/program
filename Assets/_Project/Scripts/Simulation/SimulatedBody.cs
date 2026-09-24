using PhysSim.Core;
using UnityEngine;

namespace PhysSim.Simulation
{
    /// <summary>
    /// Физическое тело объекта сцены (ARCHITECTURE.md §6.2, §9).
    ///
    /// Обязанности:
    ///  • вкл/выкл физики → Rigidbody.isKinematic (компонент создаётся всегда);
    ///  • масса = ρ_материала × V_меша (объём приходит из Geometry.MeshVolumeCalculator);
    ///  • честная аэродинамика: F = ½·ρ_возд·Cd·A·|v|² против скорости (AddForce, не Rigidbody.drag),
    ///    поэтому бумага (малая m, большой Cd) падает медленно, а железо — почти без торможения.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class SimulatedBody : MonoBehaviour
    {
        [Tooltip("Базовый Cd (парусность). Перезаписывается MaterialApplier при смене материала.")]
        [SerializeField] private float dragCoefficient = 1f;

        [Tooltip("Площадь сечения, м². Кеш по AABB; обновляется RefreshGeometryCache().")]
        [SerializeField] private float crossSectionArea = 1f;

        private const float AirDensityFallback = 1.225f;
        private const float MinMass = 0.0001f;    // 0.1 г — предохранитель от нулевой массы
        private const float MinSpeed = 0.001f;    // м/с — ниже не считаем drag

        public bool PhysicsEnabled { get; private set; }

        /// <summary>Объём геометрии, м³. Заполняется через SetVolume.</summary>
        public float Volume { get; private set; }

        /// <summary>Расчётная масса, кг (ρ·V).</summary>
        public float Mass
        {
            get { return _rigidbody != null ? _rigidbody.mass : 0f; }
        }

        public float DragCoefficient
        {
            get { return dragCoefficient; }
            set { dragCoefficient = Mathf.Max(0f, value); }
        }

        /// <summary>Ссылка на менеджер симуляции. Внедряется фабрикой (не FindObjectOfType!).</summary>
        public SimulationManager World;

        private Rigidbody _rigidbody;
        private Renderer _renderer;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _renderer = GetComponent<Renderer>();
            _rigidbody.isKinematic = true; // до явного включения физики объект неподвижен
        }

        /// <summary>Включить/выключить физику у объекта. Выкл = кинематика (тела об него ударяются).</summary>
        public void SetPhysicsEnabled(bool enabled)
        {
            if (_rigidbody == null)
            {
                return;
            }

            PhysicsEnabled = enabled;
            _rigidbody.isKinematic = !enabled;
            if (enabled)
            {
                _rigidbody.WakeUp();
            }
        }

        /// <summary>Установить объём геометрии (вызывает Geometry.MeshVolumeCalculator через фабрику).</summary>
        public void SetVolume(float volume)
        {
            Volume = Mathf.Max(0f, volume);
        }

        /// <summary>Пересчитать массу из плотности материала: m = ρ·V.</summary>
        public void RecalculateMass(float density)
        {
            if (_rigidbody == null)
            {
                return;
            }

            _rigidbody.mass = Mathf.Max(MinMass, Mathf.Max(0f, density) * Volume);
        }

        /// <summary>Обновить кеш площади сечения. Вызывается фабрикой/инспектором после смены Scale или меша.</summary>
        public void RefreshGeometryCache()
        {
            if (_renderer == null)
            {
                _renderer = GetComponent<Renderer>();
            }

            if (_renderer != null)
            {
                // v1: максимальная грань мирового AABB. v2 — проекция по направлению скорости.
                var size = _renderer.bounds.size;
                crossSectionArea = Mathf.Max(
                    size.x * size.y,
                    Mathf.Max(size.y * size.z, size.x * size.z));
            }
        }

        /// <summary>Площадь сечения, м².</summary>
        public float GetCrossSectionArea()
        {
            return crossSectionArea;
        }

        /// <summary>
        /// Расчётная терминальная скорость: v_t = √(2·m·g / (ρ_возд·Cd·A)).
        /// Показывается в инспекторе — проверка, что материал применился.
        /// </summary>
        public float GetTerminalSpeed()
        {
            var airDensity = World != null ? World.AirDensity : AirDensityFallback;
            if (airDensity <= 0f || dragCoefficient <= 0f || crossSectionArea <= 0f)
            {
                return float.PositiveInfinity; // вакуум — предела нет
            }

            var g = World != null ? World.GravityMagnitude : 9.80665f;
            return Mathf.Sqrt(2f * Mass * Mathf.Abs(g) / (airDensity * dragCoefficient * crossSectionArea));
        }

        private void FixedUpdate()
        {
            ApplyAerodynamics();
        }

        private void ApplyAerodynamics()
        {
            if (!PhysicsEnabled || _rigidbody == null || _rigidbody.IsSleeping())
            {
                return;
            }

            var airDensity = World != null ? World.AirDensity : AirDensityFallback;
            if (airDensity <= 0f)
            {
                return; // вакуум
            }

            var velocity = _rigidbody.velocity;
            var speed = velocity.magnitude;
            if (speed < MinSpeed)
            {
                return;
            }

            // F = ½·ρ·Cd·A·v²  против вектора скорости.
            var dragForce = 0.5f * airDensity * dragCoefficient * crossSectionArea * speed * speed;
            _rigidbody.AddForce(-velocity.normalized * dragForce, ForceMode.Force);
        }
    }
}
