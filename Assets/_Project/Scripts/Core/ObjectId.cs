using System;

namespace PhysSim.Core
{
    /// <summary>
    /// Стабильный идентификатор объекта сцены. Переживает сохранение/загрузку
    /// и используется командами Undo вместо ссылок на MonoBehaviour.
    /// </summary>
    public readonly struct ObjectId : IEquatable<ObjectId>
    {
        public readonly Guid Value;

        public ObjectId(Guid value)
        {
            Value = value;
        }

        public static ObjectId NewId()
        {
            return new ObjectId(Guid.NewGuid());
        }

        public static bool TryParse(string text, out ObjectId id)
        {
            if (Guid.TryParseExact(text, "N", out var guid) || Guid.TryParse(text, out guid))
            {
                id = new ObjectId(guid);
                return true;
            }

            id = default;
            return false;
        }

        public bool Equals(ObjectId other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            return obj is ObjectId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString("N");
        }

        public static bool operator ==(ObjectId left, ObjectId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ObjectId left, ObjectId right)
        {
            return !left.Equals(right);
        }
    }
}
