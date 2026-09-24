using System;

namespace PhysSim.Core
{
    /// <summary>
    /// Диалоги выбора файлов. Контракт — в Core, реализация — в UI:
    /// в редакторе нативные диалоги, в билде — встроенный модальный диалог.
    /// Выбор пользователя приходит в колбэк (null — отмена).
    /// </summary>
    public interface IFileDialogService
    {
        void OpenFile(string title, string extension, Action<string> onPicked);

        void SaveFile(string title, string defaultName, string extension, Action<string> onPicked);
    }
}
