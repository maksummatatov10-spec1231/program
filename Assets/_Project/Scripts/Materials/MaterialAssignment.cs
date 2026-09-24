using UnityEngine;

namespace PhysSim.Materials
{
    /// <summary>
    /// Компонент-ссылка на текущий материал объекта (компонентная модель §5).
    /// Читается инспектором и сериализатором сцены.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MaterialAssignment : MonoBehaviour
    {
        [Tooltip("Текущий материал объекта. null — ещё не назначен.")]
        public MaterialDefinition Definition;
    }
}
