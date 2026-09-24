using PhysSim.Core;
using PhysSim.Simulation;
using UnityEngine;

namespace PhysSim.Materials
{
    /// <summary>
    /// Применяет материал к объекту сцены АТОМАРНО: визуал (индивидуальный материал:
    /// цвет, металличность, шероховатость, карты) + физика (PhysicMaterial,
    /// масса ρ·V, аэродинамический Cd). Поддерживает Standard (built-in) и URP/Lit.
    /// Публикует ObjectMaterialChanged и обновляет MaterialAssignment.
    /// </summary>
    public sealed class MaterialApplier : MonoBehaviour
    {
        private EventBus _bus;

        public void Initialize(EventBus bus)
        {
            _bus = bus;
        }

        /// <summary>
        /// Применить материал. Требуется SimulatedBody с посчитанным объёмом
        /// (масса пересчитается как ρ·V). null-материал — только публикация события.
        /// </summary>
        public void Apply(SceneObject target, MaterialDefinition definition)
        {
            if (target == null)
            {
                return;
            }

            if (definition != null)
            {
                ApplyVisual(target, definition);
                ApplyPhysics(target, definition);

                var assignment = target.GetComponent<MaterialAssignment>();
                if (assignment == null)
                {
                    assignment = target.AddComponent<MaterialAssignment>();
                }
                assignment.Definition = definition;
            }

            if (_bus != null)
            {
                _bus.Publish(new ObjectMaterialChanged(
                    target.Id,
                    definition != null ? definition.Id : string.Empty));
            }
        }

        private static void ApplyVisual(SceneObject target, MaterialDefinition definition)
        {
            var meshRenderer = target.MeshRenderer;
            if (meshRenderer == null)
            {
                return;
            }

            // Поддержка обоих пайплайнов: URP/Lit, если проект на URP, иначе Standard.
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            var isUrp = shader != null;
            if (!isUrp)
            {
                shader = Shader.Find("Standard");
            }
            if (shader == null)
            {
                return;
            }

            // Индивидуальный материал на объект — выделение через _EmissionColor безопасно.
            var material = new Material(shader);
            material.name = $"PSM_{definition.Id}";

            if (isUrp)
            {
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
            }
            else
            {
                if (material.HasProperty("_Color"))
                {
                    material.SetColor("_Color", definition.Color);
                }
                if (material.HasProperty("_Metallic"))
                {
                    material.SetFloat("_Metallic", definition.Metallic);
                }
                if (material.HasProperty("_Glossiness"))
                {
                    material.SetFloat("_Glossiness", definition.Smoothness);
                }
                if (definition.AlbedoMap != null && material.HasProperty("_MainTex"))
                {
                    material.SetTexture("_MainTex", definition.AlbedoMap);
                    material.SetTextureScale("_MainTex", definition.Tiling);
                }
            }

            meshRenderer.sharedMaterial = material;
        }

        private static void ApplyPhysics(SceneObject target, MaterialDefinition definition)
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

            // Масса (ρ·V) и парусность — через SimulatedBody.
            var simulatedBody = target.GetComponent<SimulatedBody>();
            if (simulatedBody != null)
            {
                simulatedBody.RecalculateMass(definition.Density);
                simulatedBody.DragCoefficient = definition.DragCoefficient;
            }
        }
    }
}
