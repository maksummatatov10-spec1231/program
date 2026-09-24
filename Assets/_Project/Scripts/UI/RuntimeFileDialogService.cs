namespace PhysSim.UI
{
    /// <summary>
    /// Реализация Core.IFileDialogService для standalone-платформ
    /// (обёртка над StandaloneFileBrowser; в редакторе — EditorUtility).
    /// </summary>
    public sealed class RuntimeFileDialogService : Core.IFileDialogService
    {
        public string OpenFile(string title, params string[] extensions)
        {
            // TODO(M4): StandaloneFileBrowser.OpenFilePanel(title, "", фильтры, false).
            throw new System.NotImplementedException("M4: диалоги файлов");
        }

        public string SaveFile(string title, string defaultName, string extension)
        {
            // TODO(M4): StandaloneFileBrowser.SaveFilePanel(title, "", defaultName, extension).
            throw new System.NotImplementedException("M4: диалоги файлов");
        }
    }
}
