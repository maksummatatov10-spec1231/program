using System.Collections.Generic;
using PhysSim.Core;
using UnityEngine;
using UnityEngine.UI;

namespace PhysSim.UI.Panels
{
    /// <summary>
    /// Иерархия сцены: список объектов (клик — выбор, Shift — добавить).
    /// Перестраивается по событиям добавления/удаления/переименования.
    /// </summary>
    public sealed class HierarchyPanel
    {
        private const float RowHeight = 24f;

        private UiContext _ctx;
        private RectTransform _content;
        private Text _countLabel;
        private readonly List<Button> _rowButtons = new List<Button>(64);
        private readonly List<SceneObject> _rowObjects = new List<SceneObject>(64);
        private readonly List<EventBus.Subscription> _subscriptions = new List<EventBus.Subscription>(4);

        public void Build(RectTransform parent, UiContext ctx)
        {
            _ctx = ctx;
            UiFactory.AddImage(parent.gameObject, UiFactory.PanelColor, true);

            var title = UiFactory.NewRect(parent, "Title");
            UiFactory.Place(title, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(8f, -24f), new Vector2(-8f, -2f));
            UiFactory.AddText(title.gameObject, UiStrings.HierarchyTitle, 13,
                TextAnchor.MiddleLeft, UiFactory.DimTextColor);

            var countRect = UiFactory.NewRect(parent, "Count");
            UiFactory.Place(countRect, new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-90f, -24f), new Vector2(-8f, -2f));
            _countLabel = UiFactory.AddText(countRect.gameObject, "", 12,
                TextAnchor.MiddleRight, UiFactory.DimTextColor);

            var listRect = UiFactory.NewRect(parent, "List");
            UiFactory.Place(listRect, new Vector2(0f, 0f), new Vector2(1f, 1f),
                new Vector2(4f, 4f), new Vector2(-4f, -28f));
            UiFactory.MakeScrollList(listRect, "Scroll", out _content);

            _subscriptions.Add(ctx.Bus.Subscribe<SceneObjectAdded>(evt => Rebuild()));
            _subscriptions.Add(ctx.Bus.Subscribe<SceneObjectRemoved>(evt => Rebuild()));
            _subscriptions.Add(ctx.Bus.Subscribe<ObjectRenamed>(evt => Rebuild()));
            _subscriptions.Add(ctx.Bus.Subscribe<SelectionChanged>(evt => RefreshHighlight()));

            Rebuild();
        }

        private void Rebuild()
        {
            for (var i = _content.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(_content.GetChild(i).gameObject);
            }
            _rowButtons.Clear();
            _rowObjects.Clear();

            var objects = _ctx.World.Objects;
            for (var i = 0; i < objects.Count; i++)
            {
                var sceneObject = objects[i];
                var index = i;
                var row = UiFactory.MakeRow(_content, i, RowHeight, sceneObject.DisplayName, 12, null,
                    () => _ctx.Selection.Select(sceneObject, Input.GetKey(KeyCode.LeftShift)));
                _rowButtons.Add(row);
                _rowObjects.Add(sceneObject);
            }

            _content.sizeDelta = new Vector2(0f, objects.Count * RowHeight);
            _countLabel.text = string.Format(UiStrings.StatusObjectsFormat, objects.Count);
            RefreshHighlight();
        }

        private void RefreshHighlight()
        {
            for (var i = 0; i < _rowButtons.Count; i++)
            {
                if (_rowButtons[i] == null || _rowObjects[i] == null)
                {
                    continue;
                }

                var isSelected = _ctx.Selection.IsSelected(_rowObjects[i]);
                _rowButtons[i].image.color = isSelected
                    ? UiFactory.RowHighlightColor
                    : new Color(0f, 0f, 0f, 0f);
            }
        }

        public void Dispose()
        {
            for (var i = 0; i < _subscriptions.Count; i++)
            {
                _subscriptions[i].Dispose();
            }
            _subscriptions.Clear();
        }
    }
}
