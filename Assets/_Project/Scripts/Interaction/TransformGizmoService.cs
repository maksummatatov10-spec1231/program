using PhysSim.Core;
using UnityEngine;

namespace PhysSim.Interaction
{
    /// <summary>
    /// Сервис гизмо трансформаций (M5): Move/Rotate/Scale (W/E/R), осевые хэндлы,
    /// удержание Shift — snap к шагу сетки/15°. Синхронизирован с числовым вводом
    /// в инспекторе: оба источника публикуют ObjectTransformEdited → пересчёт объёма/массы.
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

        public GizmoMode Mode { get; set; } = GizmoMode.None;

        /// <summary>Шаг привязки при перемещении, м (Shift). 0 — без привязки.</summary>
        public float MoveSnap { get; set; } = 0.25f;

        /// <summary>Шаг привязки вращения, градусы (Shift).</summary>
        public float RotateSnap { get; set; } = 15f;

        /// <summary>Присоединить гизмо к выделению (вызывается по SelectionChanged).</summary>
        public void Attach(SceneObject[] selection)
        {
            // TODO(M5): отрисовка хэндлов (модели поверх сцены), драг осей.
        }

        public void Detach()
        {
            // TODO(M5).
        }
    }
}
