using System;

namespace PhysSim.Core
{
    /// <summary>
    /// Контракт сервиса приложения. Инициализация — строго из AppBootstrap
    /// (см. ARCHITECTURE.md §13), порядок композиции единственный.
    /// </summary>
    public interface IService
    {
        void Initialize();
        void Shutdown();
    }
}
