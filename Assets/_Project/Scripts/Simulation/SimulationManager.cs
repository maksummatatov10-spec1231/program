using System;
using PhysSim.Core;
using UnityEngine;

namespace PhysSim.Simulation
{
    /// <summary>
    /// Центральное управление временем симуляции и глобальной физикой (ARCHITECTURE.md §6.2).
    ///
    /// Пауза реализуется через Physics.simulationMode = Script, а не Time.timeScale = 0:
    /// это даёт честный покадровый шаг (StepOnce) и не замораживает камеру/UI.
    /// Приложение всегда стартует на паузе — это редактор, а не игра.
    /// </summary>
    public sealed class SimulationManager : MonoBehaviour
    {
        [Header("Настройки по умолчанию (переопределяются SimSettings)")]
        [SerializeField] private float defaultGravity = 9.80665f;   // м/с²
        [SerializeField] private float defaultAirDensity = 1.225f;  // кг/м³ (ISO, уровень моря)
        [SerializeField] private float fixedDeltaTime = 0.02f;      // 50 Гц

        public bool IsRunning { get; private set; }

        /// <summary>Масштаб времени симуляции (0.05–4). Применяется только при IsRunning.</summary>
        public float TimeScale
        {
            get { return Time.timeScale; }
            set { Time.timeScale = Mathf.Clamp(value, 0.05f, 4f); }
        }

        /// <summary>Модуль гравитации, м/с². Направление v1 — строго вниз (−Y).</summary>
        public float GravityMagnitude { get; private set; }

        /// <summary>Плотность воздуха, кг/м³. 0 — вакуум (все тела падают одинаково).</summary>
        public float AirDensity { get; private set; }

        public Vector3 GravityVector
        {
            get { return new Vector3(0f, -GravityMagnitude, 0f); }
        }

        /// <summary>Оповещение UI об изменении состояния (Play/Pause/Step).</summary>
        public event Action StateChanged;

        private EventBus _bus;

        public void Initialize(EventBus bus)
        {
            _bus = bus;
        }

        private void Awake()
        {
            Time.fixedDeltaTime = fixedDeltaTime;
            Physics.defaultSolverIterations = 8;
            Physics.defaultSolverVelocityIterations = 2;

            SetGravity(defaultGravity);
            AirDensity = Mathf.Max(0f, defaultAirDensity);
            Pause(); // Редактор всегда стартует на паузе.
        }

        public void Play()
        {
            Physics.simulationMode = SimulationMode.Fixed;
            IsRunning = true;
            RaiseState();
        }

        public void Pause()
        {
            Physics.simulationMode = SimulationMode.Script; // Физика замирает, UI живёт.
            IsRunning = false;
            RaiseState();
        }

        public void Toggle()
        {
            if (IsRunning)
            {
                Pause();
            }
            else
            {
                Play();
            }
        }

        /// <summary>Один шаг физики ровно на fixedDeltaTime (доступен и на паузе).</summary>
        public void StepOnce()
        {
            if (IsRunning)
            {
                Pause();
            }

            Physics.Simulate(Time.fixedDeltaTime * Time.timeScale);
        }

        public void SetGravity(float magnitude)
        {
            GravityMagnitude = Mathf.Clamp(magnitude, -50f, 50f);
            Physics.gravity = GravityVector;
            Publish(new GravityChanged(GravityMagnitude));
        }

        public void SetAirDensity(float density)
        {
            AirDensity = Mathf.Max(0f, density);
            Publish(new AirDensityChanged(AirDensity));
        }

        /// <summary>Обнулить скорости всех тел, не сбрасывая позиции (кнопка «Стоп»).</summary>
        public void ZeroAllMotion(WorldRegistry world)
        {
            if (world == null)
            {
                return;
            }

            for (var i = 0; i < world.Objects.Count; i++)
            {
                var body = world.Objects[i].Body;
                if (body != null && !body.isKinematic)
                {
                    body.velocity = Vector3.zero;
                    body.angularVelocity = Vector3.zero;
                }
            }
        }

        private void RaiseState()
        {
            StateChanged?.Invoke();
            Publish(new SimulationStateChanged(IsRunning, Time.timeScale));
        }

        private void Publish<TEvent>(TEvent evt) where TEvent : struct
        {
            if (_bus != null)
            {
                _bus.Publish(evt);
            }
        }
    }
}
