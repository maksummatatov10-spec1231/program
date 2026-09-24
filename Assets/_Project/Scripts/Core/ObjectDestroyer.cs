using UnityEngine;

namespace PhysSim.Core
{
    /// <summary>
    /// Единая точка удаления объектов: снимает с учёта и публикует SceneObjectRemoved.
    /// Все удаления (команды, «Новый документ», загрузка сцены) идут только через него.
    /// </summary>
    public sealed class ObjectDestroyer
    {
        private readonly WorldRegistry _world;
        private readonly EventBus _bus;

        public ObjectDestroyer(WorldRegistry world, EventBus bus)
        {
            _world = world;
            _bus = bus;
        }

        /// <summary>Полное удаление объекта со сцены.</summary>
        public void HardDestroy(SceneObject target)
        {
            if (target == null)
            {
                return;
            }

            _world.Untrack(target);
            Object.Destroy(target.gameObject);
            _bus.Publish(new SceneObjectRemoved(target.Id));
        }

        /// <summary>Очистить всю сцену («Новый документ»).</summary>
        public void DestroyAll()
        {
            var objects = _world.Objects;
            // Копия: Untrack меняет список.
            var snapshot = new SceneObject[objects.Count];
            for (var i = 0; i < objects.Count; i++)
            {
                snapshot[i] = objects[i];
            }

            for (var i = 0; i < snapshot.Length; i++)
            {
                HardDestroy(snapshot[i]);
            }
        }
    }
}
