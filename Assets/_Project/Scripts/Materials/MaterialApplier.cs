using PhysSim.Core;
using PhysSim.Simulation;
using UnityEngine;

namespace PhysSim.Materials
{
    /// <summary>
    /// Применяет материал к объекту сцены АТОМАРНО: визуал (URP-инстанс:
    /// цвет, металличность, шероховатость, карты) + физика (PhysicMaterial,
    /// масса ρ·V, аэродинамический Cd). Публикует ObjectMaterialChanged.
    /// </summary>
    public sealed class MaterialApplier : MonoBehaviour
    {
        [Tooltip("Шейдер объектов. Оставьте пустым — возьмётся из URP/Lit.")]
        [SerializeField] private Shader standardShader;

        /// <summary>
        /// Применить материал. Требуется SimulatedBody с уже посчитанным объёмом
        /// (масса пересчитается как ρ·V). null-материал допустим — просто снимет перезапись.
        /// </summary>
        public void Apply(SceneObject target, MaterialDefinition definition, EventBus bus)
        {
            if (target == null)
            {
                return;
            }

            var simulatedBody = target.GetComponent<SimulatedBody>();

            if (definition != null)
            {
                ApplyVisual(target, definition);
                ApplyPhysics(target, simulatedBody, definition);
            }

            if (bus != null)
            {
                bus.Publish(new ObjectMaterialChanged(
                    target.Id,
                    definition != null ? definition.Id : string.Empty));
            }
        }

        private void ApplyVisual(SceneObject target, MaterialDefinition definition)
        {
            var renderer = target.MeshRenderer;
            if (renderer == null)
            {
                return;
            }

            var shader = standardShader != null ? standardShader : Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                return; // URP не найден — в редакторе предупредит валидация M0
            }

            // Индивидуальный материал на объект (позже: MaterialPropertyBlock + атласы для тысяч объектов).
            var material = new Material(shader);
            material.name = $"PSM_{definition.Id}";
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", definition.Color);
            }
            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", definition.Metallic);
            }
            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", definition.Smoothness);
            }
            if (definition.AlbedoMap != null && material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", definition.AlbedoMap);
                material.SetTextureScale("_BaseMap", definition.Tiling);
            }
            if (definition.NormalMap != null && material.HasProperty("_BumpMap"))
            {
                material.SetTexture("_BumpMap", definition.NormalMap);
                material.EnableKeyword("_NORMALMAP");
            }
            if (definition.MaskMap != null && material.HasProperty("_MaskMap"))
            {
                material.SetTexture("_MaskMap", definition.MaskMap);
                material.EnableKeyword("_MASKMAP");
            }

            renderer.sharedMaterial = material;
        }

        private void ApplyPhysics(SceneObject target, SimulatedBody simulatedBody, MaterialDefinition definition)
        {
            // Трение/упругость — через PhysicMaterial.
            var physicMaterial = new PhysicMaterial($"PSM_{definition.Id}")
            {
                staticFriction = definition.StaticFriction,
                dynamicFriction = definition.DynamicFriction,
                bounciness = definition.Restitution,
                frictionCombine = PhysicMaterialCombine.Average,
                bounceCombine = PhysicMaterialCombine.Average
            };

            if (target.Collider != null)
            {
                target.Collider.sharedMaterial = physicMaterial;
            }

            // Масса и парусность — через SimulatedBody.
            if (simulatedBody != null)
            {
                simulatedBody.RecalculateMass(definition.Density);
                simulatedBody.DragCoefficient = definition.DragCoefficient;
            }
        }
    }
}
