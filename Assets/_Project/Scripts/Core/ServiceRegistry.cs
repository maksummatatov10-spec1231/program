using System;
using System.Collections.Generic;

namespace PhysSim.Core
{
    /// <summary>
    /// Лёгкий DI-контейнер. Регистрация — только в AppBootstrap, остальной код
    /// получает сервисы только на чтение. Никаких статических синглтонов (P3).
    /// </summary>
    public sealed class ServiceRegistry
    {
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>(32);

        public void Register<TService>(TService service) where TService : class
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (!_services.TryAdd(typeof(TService), service))
            {
                throw new InvalidOperationException(
                    $"Сервис {typeof(TService).Name} уже зарегистрирован.");
            }
        }

        public TService Get<TService>() where TService : class
        {
            if (_services.TryGetValue(typeof(TService), out var service))
            {
                return (TService)service;
            }

            throw new InvalidOperationException(
                $"Сервис {typeof(TService).Name} не зарегистрирован. " +
                "Проверьте порядок инициализации в AppBootstrap.");
        }

        public bool TryGet<TService>(out TService service) where TService : class
        {
            if (_services.TryGetValue(typeof(TService), out var found))
            {
                service = (TService)found;
                return true;
            }

            service = null;
            return false;
        }
    }
}
