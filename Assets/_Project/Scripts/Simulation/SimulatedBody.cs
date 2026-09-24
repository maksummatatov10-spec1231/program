using PhysSim.Core;
using UnityEngine;

namespace PhysSim.Simulation
{
    /// <summary>
    /// Физическое тело объекта сцены (ARCHITECTURE.md §6.2, §9).
    ///
    ///  • вкл/выкл физики → Rigidbody.isKinematic (компонент Rigidbody существует всегда);
    ///  • масса = ρ_материала × V, где объём масштабируется transform.lossyScale;
    ///  • честная аэродинамика: F = ½·ρ_возд·Cd·A·|v|² против скорости (AddForce, не Rigidbody.drag) —
    ///    бумага (малая m, большой Cd) падает медленно, железо — почти без торможения.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class SimulatedBody : MonoBehaviour
    {
        [Tooltip("Базовый Cd (парусность). Перезаписывается MaterialApplier при смене материала.")]
        [SerializeField] private float dragCoefficient = 1f;

        private const float AirDensityFallback = 1.225f;
        private const float MinMass = 0.0001f; // 0.1 г — предохранитель от нулевой массы
        private const float MinSpeed = 0.001f; // м/с — ниже не считаем drag

        public bool PhysicsEnabled { get; private set; }

        /// <summary>Объём геометрии при единичном масштабе, м³ (из MeshVolumeCalculator).</summary>
        public float UnitVolume { get; private set; } = 1f;

        /// <summary>Текущий объём с учётом масштаба, м³.</summary>
        public float Volume
        {
            get { return UnitVolume * ScaleVolumeFactor; }
        }

        /// <summary>Расчётная масса, кг (ρ·V или override).</summary>
        public float Mass
        {
            get { return _rigidbody != null ? _rigidbody.mass : 0f; }
        }

        /// <summary>Ручное переопределение массы, кг. null — масса считается как ρ·V.</summary>
        public float? MassOverride { get; private set; }

        public float DragCoefficient
        {
            get { return dragCoefficient; }
            set { dragCoefficient = Mathf.Clamp(value, 0f, 2.5f); }
        }

        /// <summary>Менеджер симуляции. Внедряется фабрикой (не FindObjectOfType!).</summary>
        public SimulationManager World;

        private Rigidbody _rigidbody;
        private Renderer _renderer;
        private float _lastDensity = 1000f;
        private float _crossSectionArea = 1f;

        private float ScaleVolumeFactor
        {
            get
            {
                var s = transform.lossyScale;
                return Mathf.Abs(s.x * s.y * s.z);
            }
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _renderer = GetComponent<Renderer>();
            _rigidbody.isKinematic = true; // до явного включения физики объект неподвижен
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        }

        /// <summary>Включить/выключить физику. Выкл = кинематика (другие тела об него ударяются).</summary>
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

        /// <summary>Установить объём геометрии при единичном масштабе (вызывает фабрика).</summary>
        public void SetUnitVolume(float unitVolume)
        {
            UnitVolume = Mathf.Max(0f, unitVolume);
        }

        /// <summary>Ручное переопределение массы. null — вернуться к расчёту ρ·V.</summary>
        public void SetMassOverride(float? massOverride)
        {
            MassOverride = massOverride;
            RecalculateMass(_lastDensity);
        }

        /// <summary>Пересчитать массу из плотности материала: m = ρ·V (с учётом масштаба).</summary>
        public void RecalculateMass(float density)
        {
            if (_rigidbody == null)
            {
                return;
            }

            _lastDensity = Mathf.Max(0f, density);
            var calculated = _lastDensity * Volume;
            _rigidbody.mass = Mathf.Max(MinMass, MassOverride ?? calculated);
        }

        /// <summary>
        /// Вызвать после изменения Scale или меша: пересчитывает кеш площади сечения
        /// и массу. Числовые поля инспектора и гизмо дергают это через команды.
        /// </summary>
        public void NotifyScaleChanged()
        {
            RefreshGeometryCache();
            RecalculateMass(_lastDensity);
        }

        /// <summary>Площадь сечения, м² (максимальная грань мирового AABB).</summary>
        public float GetCrossSectionArea()
        {
            return _crossSectionArea;
        }

        public void RefreshGeometryCache()
        {
            if (_renderer == null)
            {
                _renderer = GetComponent<Renderer>();
            }

            if (_renderer != null)
            {
                var size = _renderer.bounds.size;
                _crossSectionArea = Mathf.Max(
                    size.x * size.y,
                    Mathf.Max(size.y * size.z, size.x * size.z));
            }
        }

        /// <summary>
        /// Расчётная терминальная скорость: v_t = √(2·m·g / (ρ_возд·Cd·A)).
        /// Показывается в инспекторе — проверка, что материал применился.
        /// </summary>
        public float GetTerminalSpeed()
        {
            var airDensity = World != null ? World.AirDensity : AirDensityFallback;
            if (airDensity <= 0f || dragCoefficient <= 0f || _crossSectionArea <= 0f)
            {
                return float.PositiveInfinity; // вакуум — предела нет
            }

            var g = World != null ? World.GravityMagnitude : 9.80665f;
            return Mathf.Sqrt(2f * Mass * Mathf.Abs(g) / (airDensity * dragCoefficient * _crossSectionArea));
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

            // F = ½·ρ·Cd·A·v² против вектора скорости.
            var dragForce = 0.5f * airDensity * dragCoefficient * _crossSectionArea * speed * speed;
            _rigidbody.AddForce(-velocity / speed * dragForce, ForceMode.Force);
        }
    }
}
