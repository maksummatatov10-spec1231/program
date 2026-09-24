using System;
using System.Collections.Generic;

namespace PhysSim.Core
{
    /// <summary>
    /// Реестр всех объектов сцены — источник истины для UI-иерархии и поиска по id.
    /// Наполняется только через ObjectFactory/импортёры; чтение — из любого модуля.
    /// </summary>
    public sealed class WorldRegistry
    {
        private readonly Dictionary<ObjectId, SceneObject> _index =
            new Dictionary<ObjectId, SceneObject>(256);
        private readonly List<SceneObject> _objects = new List<SceneObject>(256);

        public IReadOnlyList<SceneObject> Objects
        {
            get { return _objects; }
        }

        /// <summary>Объект добавлен на сцену.</summary>
        public event Action<SceneObject> Added;

        /// <summary>Объект удалён со сцены.</summary>
        public event Action<SceneObject> Removed;

        public void Track(SceneObject sceneObject)
        {
            if (sceneObject == null)
            {
                throw new ArgumentNullException(nameof(sceneObject));
            }

            if (_index.ContainsKey(sceneObject.Id))
            {
                throw new InvalidOperationException(
                    $"Объект с id {sceneObject.Id} уже зарегистрирован.");
            }

            _index.Add(sceneObject.Id, sceneObject);
            _objects.Add(sceneObject);
            Added?.Invoke(sceneObject);
        }

        public bool Untrack(SceneObject sceneObject)
        {
            if (sceneObject == null || !_index.Remove(sceneObject.Id))
            {
                return false;
            }

            _objects.Remove(sceneObject);
            Removed?.Invoke(sceneObject);
            return true;
        }

        public SceneObject GetById(ObjectId id)
        {
            return _index.TryGetValue(id, out var sceneObject) ? sceneObject : null;
        }

        public bool TryGetById(ObjectId id, out SceneObject sceneObject)
        {
            return _index.TryGetValue(id, out sceneObject);
        }

        /// <summary>Очистка сцены (Новый документ). Уничтожение GameObject — обязанность вызывающего.</summary>
        public void Clear()
        {
            _index.Clear();
            _objects.Clear();
        }
    }
}
