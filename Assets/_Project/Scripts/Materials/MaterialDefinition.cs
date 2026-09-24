using UnityEngine;

namespace PhysSim.Materials
{
    /// <summary>
    /// Один материал библиотеки — единственный источник правды о свойствах.
    /// Из него MaterialApplier выводит и визуал (URP/Lit), и физику
    /// (масса ρ·V, трение, упругость, Cd). Ассеты создаются CSV-импортёром (M3).
    /// </summary>
    [CreateAssetMenu(
        fileName = "MaterialDefinition",
        menuName = "PhysSim/Material Definition",
        order = 0)]
    public sealed class MaterialDefinition : ScriptableObject
    {
        [Header("Идентификация")]
        [SerializeField] private string id = "";            // "iron" — стабильный ключ, не менять!
        [SerializeField] private string displayName = "";   // "Железо" — русское имя для UI
        [SerializeField] private MaterialCategory category = MaterialCategory.Other;

        [Header("Физика")]
        [SerializeField] private float density = 1000f;          // кг/м³
        [SerializeField] private float staticFriction = 0.6f;    // 0..1.5
        [SerializeField] private float dynamicFriction = 0.5f;   // 0..1.5
        [SerializeField] private float restitution = 0.3f;       // 0..1
        [SerializeField] private float dragCoefficient = 1f;     // парусность Cd, 0.02..2.5

        [Header("Визуал (URP/Lit)")]
        [SerializeField] private Color color = Color.white;
        [SerializeField, Range(0f, 1f)] private float metallic = 0f;
        [SerializeField, Range(0f, 1f)] private float smoothness = 0.5f; // аналог invRoughness
        [SerializeField] private Texture2D albedoMap;
        [SerializeField] private Texture2D normalMap;
        [SerializeField] private Texture2D maskMap;              // URP MaskMap (metallic/occlusion/detail/smoothness)
        [SerializeField] private Vector2 tiling = Vector2.one;

        public string Id
        {
            get { return id; }
        }

        public string DisplayName
        {
            get { return string.IsNullOrEmpty(displayName) ? name : displayName; }
        }

        public MaterialCategory Category
        {
            get { return category; }
        }

        public float Density
        {
            get { return density; }
        }

        public float StaticFriction
        {
            get { return staticFriction; }
        }

        public float DynamicFriction
        {
            get { return dynamicFriction; }
        }

        public float Restitution
        {
            get { return restitution; }
        }

        public float DragCoefficient
        {
            get { return dragCoefficient; }
        }

        public Color Color
        {
            get { return color; }
        }

        public float Metallic
        {
            get { return metallic; }
        }

        public float Smoothness
        {
            get { return smoothness; }
        }

        public Texture2D AlbedoMap
        {
            get { return albedoMap; }
        }

        public Texture2D NormalMap
        {
            get { return normalMap; }
        }

        public Texture2D MaskMap
        {
            get { return maskMap; }
        }

        public Vector2 Tiling
        {
            get { return tiling; }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Строгие предохранители: физика не должна уезжать в бессмысленные значения.
            density = Mathf.Max(1f, density);
            staticFriction = Mathf.Clamp(staticFriction, 0f, 1.5f);
            dynamicFriction = Mathf.Clamp(dynamicFriction, 0f, 1.5f);
            restitution = Mathf.Clamp01(restitution);
            dragCoefficient = Mathf.Clamp(dragCoefficient, 0.01f, 2.5f);

            if (string.IsNullOrEmpty(id))
            {
                id = name.ToLowerInvariant().Replace(' ', '_');
            }
        }
#endif
    }
}
