using UnityEngine;

namespace PhysSim.Materials
{
    /// <summary>
    /// Плоская спецификация материала для встроенной таблицы (MaterialSpecs)
    /// и CSV-импортёра. Из неё создаётся MaterialDefinition без ассетов.
    /// </summary>
    public struct MaterialSpec
    {
        public string Id;
        public string DisplayName;
        public MaterialCategory Category;
        public float Density;          // кг/м³
        public float StaticFriction;
        public float DynamicFriction;
        public float Restitution;
        public float DragCoefficient;
        public Color Color;
        public float Metallic;
        public float Smoothness;

        public MaterialSpec(string id, string displayName, MaterialCategory category, float density,
            float staticFriction, float dynamicFriction, float restitution, float dragCoefficient,
            Color color, float metallic, float smoothness)
        {
            Id = id;
            DisplayName = displayName;
            Category = category;
            Density = density;
            StaticFriction = staticFriction;
            DynamicFriction = dynamicFriction;
            Restitution = restitution;
            DragCoefficient = dragCoefficient;
            Color = color;
            Metallic = metallic;
            Smoothness = smoothness;
        }
    }
}
