using PhysSim.Materials;
using UnityEngine;

namespace PhysSim.Materials
{
    /// <summary>
    /// Компонент-ссылка на текущий материал объекта (см. компонентную модель §5).
    /// Читается UI (инспектор, библиотека) и сериализатором сцены.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MaterialAssignment : MonoBehaviour
    {
        [Tooltip("Текущий материал объекта. null — материал по умолчанию ещё не назначен.")]
        public MaterialDefinition Definition;
    }
}
