using System;
using System.IO;
using PhysSim.Core;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PhysSim.UI
{
    /// <summary>
    /// Реализация Core.IFileDialogService:
    ///  • в редакторе — нативные диалоги EditorUtility;
    ///  • в билде — встроенное модальное окно ввода пути (без внешних плагинов).
    /// Апгрейд: обёртка над StandaloneFileBrowser по тому же интерфейсу.
    /// </summary>
    public sealed class RuntimeFileDialogService : IFileDialogService
    {
        private readonly RectTransform _uiRoot;
        private GameObject _modal;

        public RuntimeFileDialogService(RectTransform uiRoot)
        {
            _uiRoot = uiRoot;
        }

        public void OpenFile(string title, string extension, Action<string> onPicked)
        {
#if UNITY_EDITOR
            var path = EditorUtility.OpenFilePanel(title, LastDirectory(), extension);
            if (!string.IsNullOrEmpty(path))
            {
                RememberDirectory(path);
            }
            onPicked?.Invoke(string.IsNullOrEmpty(path) ? null : path);
#else
            ShowModal(title, "модель." + extension, extension, onPicked);
#endif
        }

        public void SaveFile(string title, string defaultName, string extension, Action<string> onPicked)
        {
#if UNITY_EDITOR
            var path = EditorUtility.SaveFilePanel(title, LastDirectory(), defaultName, extension);
            if (!string.IsNullOrEmpty(path))
            {
                RememberDirectory(path);
            }
            onPicked?.Invoke(string.IsNullOrEmpty(path) ? null : path);
#else
            ShowModal(title, defaultName, extension, onPicked);
#endif
        }

#if !UNITY_EDITOR
        private void ShowModal(string title, string defaultName, string extension, Action<string> onPicked)
        {
            CloseModal();

            var dim = UiFactory.NewRect(_uiRoot, "FileDialogDim");
            UiFactory.Stretch(dim, 0f, 0f, 0f, 0f);
            UiFactory.AddImage(dim.gameObject, new Color(0f, 0f, 0f, 0.55f), true);
            _modal = dim.gameObject;

            var panel = UiFactory.NewRect(dim, "Panel");
            UiFactory.Place(panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-280f, -80f), new Vector2(280f, 80f));
            UiFactory.AddImage(panel.gameObject, UiFactory.PanelColor, true);

            var titleRect = UiFactory.NewRect(panel, "Title");
            UiFactory.Place(titleRect, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(12f, -30f), new Vector2(-12f, -2f));
            UiFactory.AddText(titleRect.gameObject, title, 14, TextAnchor.MiddleLeft, UiFactory.TextColor);

            var input = UiFactory.MakeInputField(panel, "Path", "полный путь к файлу", 13, null);
            UiFactory.Place((RectTransform)input.transform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(12f, -74f), new Vector2(-12f, -34f));
            var defaultDirectory = LastDirectory();
            input.text = Path.Combine(defaultDirectory, string.IsNullOrEmpty(defaultName) ? "файл" : defaultName);

            void Commit()
            {
                var path = input.text.Trim();
                CloseModal();
                if (string.IsNullOrEmpty(path))
                {
                    onPicked?.Invoke(null);
                    return;
                }

                if (!Path.HasExtension(path))
                {
                    path += "." + extension;
                }
                RememberDirectory(path);
                onPicked?.Invoke(path);
            }

            var ok = UiFactory.DecorateButton(UiFactory.NewRect(panel, "OK"), "ОК", 13,
                UiFactory.ButtonAccentColor, Commit);
            UiFactory.Place((RectTransform)ok.transform, new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-130f, 8f), new Vector2(-12f, 34f));

            var cancel = UiFactory.DecorateButton(UiFactory.NewRect(panel, "Cancel"), "Отмена", 13,
                UiFactory.ButtonColor, () => { CloseModal(); onPicked?.Invoke(null); });
            UiFactory.Place((RectTransform)cancel.transform, new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-244f, 8f), new Vector2(-134f, 34f));
        }
#endif

        private void CloseModal()
        {
            if (_modal != null)
            {
                Destroy(_modal);
                _modal = null;
            }
        }

        private static void Destroy(GameObject go)
        {
            UnityEngine.Object.Destroy(go);
        }

        private static string LastDirectory()
        {
            var directory = Path.Combine(Application.persistentDataPath, "Saves");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            return directory;
        }

        private static void RememberDirectory(string path)
        {
            // v1: фиксированная папка Saves; запоминание последней директории — бэклог.
        }
    }
}
