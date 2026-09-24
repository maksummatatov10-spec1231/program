namespace PhysSim.Core
{
    /// <summary>
    /// Диалоги выбора файлов. Контракт живёт в Core, реализация — в UI
    /// (StandaloneFileBrowser / EditorUtility). Импорт/экспорт не знает,
    /// откуда пришёл путь (P5).
    /// </summary>
    public interface IFileDialogService
    {
        /// <summary>Открыть существующий файл. null — пользователь отменил.</summary>
        string OpenFile(string title, params string[] extensions);

        /// <summary>Сохранить файл под именем. null — пользователь отменил.</summary>
        string SaveFile(string title, string defaultName, string extension);
    }
}
