using System;
using System.Collections.Generic;

namespace PhysSim.Core
{
    /// <summary>
    /// Типизированная шина событий (P2): домен публикует readonly-структуры,
    /// UI и Interaction подписываются. Домен не знает своих подписчиков.
    /// Обработчик может отписаться во время рассылки — берём снапшот списка.
    /// </summary>
    public sealed class EventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _subscribers =
            new Dictionary<Type, List<Delegate>>(32);

        /// <summary>Токен подписки: храните и вызовите Dispose() при закрытии панели.</summary>
        public readonly struct Subscription : IDisposable
        {
            private readonly EventBus _bus;
            private readonly Type _eventType;
            private readonly Delegate _handler;

            public Subscription(EventBus bus, Type eventType, Delegate handler)
            {
                _bus = bus;
                _eventType = eventType;
                _handler = handler;
            }

            public void Dispose()
            {
                if (_bus != null && _bus._subscribers.TryGetValue(_eventType, out var list))
                {
                    list.Remove(_handler);
                }
            }
        }

        public Subscription Subscribe<TEvent>(Action<TEvent> handler) where TEvent : struct
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (!_subscribers.TryGetValue(typeof(TEvent), out var list))
            {
                list = new List<Delegate>(8);
                _subscribers.Add(typeof(TEvent), list);
            }

            if (!list.Contains(handler))
            {
                list.Add(handler);
            }

            return new Subscription(this, typeof(TEvent), handler);
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : struct
        {
            if (_subscribers.TryGetValue(typeof(TEvent), out var list))
            {
                list.Remove(handler);
            }
        }

        public void Publish<TEvent>(TEvent evt) where TEvent : struct
        {
            if (!_subscribers.TryGetValue(typeof(TEvent), out var list) || list.Count == 0)
            {
                return;
            }

            // Снапшот: обработчик вправе подписаться/отписаться внутри рассылки.
            var snapshot = list.ToArray();
            for (var i = 0; i < snapshot.Length; i++)
            {
                ((Action<TEvent>)snapshot[i])(evt);
            }
        }

        public void Clear()
        {
            _subscribers.Clear();
        }
    }
}
