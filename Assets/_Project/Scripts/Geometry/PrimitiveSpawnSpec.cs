using PhysSim.Core;
using PhysSim.Materials;
using UnityEngine;

namespace PhysSim.Geometry
{
    /// <summary>
    /// Декларативное описание создаваемого объекта: единый формат для создания,
    /// дублирования, undo/redo, загрузки сцены и сериализации.
    /// Primitive == null → используется Mesh (импортированная геометрия).
    /// </summary>
    public sealed class PrimitiveSpawnSpec
    {
        public ObjectId Id;

        public string Name = "";

        /// <summary>Имя примитива ("Cube"/"Sphere"/"Cylinder"/"Capsule"). null — кастомный меш.</summary>
        public string Primitive;

        /// <summary>Меш для не-примитивной геометрии (держится ссылкой; OBJ-экспорт при сейве).</summary>
        public Mesh Mesh;

        public bool ConvexCollider = true;

        public Vector3 Position = Vector3.zero;
        public Vector3 RotationEuler = Vector3.zero;
        public Vector3 Scale = Vector3.one;

        /// <summary>id материала (строка для сериализации). null — материал по умолчанию.</summary>
        public string MaterialId;

        /// <summary>Прямая ссылка на материал (приоритетнее MaterialId, для сессии).</summary>
        public MaterialDefinition Material;

        public bool PhysicsEnabled;

        /// <summary>Переопределение массы, кг. null — масса = ρ·V.</summary>
        public float? MassOverride;

        public PrimitiveSpawnSpec Clone()
        {
            return new PrimitiveSpawnSpec
            {
                Id = Id,
                Name = Name,
                Primitive = Primitive,
                Mesh = Mesh,
                ConvexCollider = ConvexCollider,
                Position = Position,
                RotationEuler = RotationEuler,
                Scale = Scale,
                MaterialId = MaterialId,
                Material = Material,
                PhysicsEnabled = PhysicsEnabled,
                MassOverride = MassOverride
            };
        }
    }
}
