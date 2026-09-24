using PhysSim.Core;
using UnityEngine;

namespace PhysSim.Interaction
{
    /// <summary>
    /// Маркер хэндла гизмо. Селекшн-рейкаст игнорирует коллайдеры, у которых
    /// в родителях есть этот компонент.
    /// </summary>
    public sealed class GizmoHandle : MonoBehaviour
    {
        public TransformGizmoService.HandleKind Kind;
        public int Axis; // 0 = X, 1 = Y, 2 = Z
    }
}
