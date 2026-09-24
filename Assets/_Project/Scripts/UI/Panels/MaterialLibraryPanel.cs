using System.Collections.Generic;
using PhysSim.Core;
using PhysSim.Materials;
using UnityEngine;
using UnityEngine.UI;

namespace PhysSim.UI.Panels
{
    /// <summary>
    /// Библиотека материалов (121 позиция): поиск по имени/id, свотчи, плотность.
    /// Клик по строке назначает материал всем выделенным объектам (undoable).
    /// </summary>
    public sealed class MaterialLibraryPanel
    {
        private const float RowHeight = 26f;

        private UiContext _ctx;
        private RectTransform _content;
        private Text _countLabel;
        private InputField _search;

        public void Build(RectTransform parent, UiContext ctx)
        {
            _ctx = ctx;
            UiFactory.AddImage(parent.gameObject, UiFactory.PanelColor, true);

            var title = UiFactory.NewRect(parent, "Title");
            UiFactory.Place(title, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(8f, -24f), new Vector2(-8f, -2f));
            UiFactory.AddText(title.gameObject, UiStrings.LibraryTitle, 13,
                TextAnchor.MiddleLeft, UiFactory.DimTextColor);

            _search = UiFactory.MakeInputField(parent, "Search", UiStrings.SearchHint, 12,
                value => Rebuild());
            UiFactory.Place((RectTransform)_search.transform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(8f, -54f), new Vector2(-110f, -28f));

            var countRect = UiFactory.NewRect(parent, "Count");
            UiFactory.Place(countRect, new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-100f, -52f), new Vector2(-8f, -28f));
            _countLabel = UiFactory.AddText(countRect.gameObject, "", 11,
                TextAnchor.MiddleRight, UiFactory.DimTextColor);

            var listRect = UiFactory.NewRect(parent, "List");
            UiFactory.Place(listRect, new Vector2(0f, 0f), new Vector2(1f, 1f),
                new Vector2(4f, 4f), new Vector2(-4f, -58f));
            UiFactory.MakeScrollList(listRect, "Scroll", out _content);

            Rebuild();
        }

        private void Rebuild()
        {
            for (var i = _content.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(_content.GetChild(i).gameObject);
            }

            var filtered = MaterialLibraryFilter.Filter(_ctx.Materials.All, _search != null ? _search.text : "");
            for (var i = 0; i < filtered.Count; i++)
            {
                var material = filtered[i];
                var index = i;
                UiFactory.MakeRow(_content, i, RowHeight,
                    $"{material.DisplayName}  ·  {UiFactory.Format(material.Density, "0")} кг/м³",
                    12,
                    material.Color,
                    () => ApplyMaterial(material));
            }

            _content.sizeDelta = new Vector2(0f, filtered.Count * RowHeight);
            _countLabel.text = $"{filtered.Count}/{_ctx.Materials.All.Count}";
        }

        private void ApplyMaterial(MaterialDefinition definition)
        {
            if (_ctx.Selection.Selected.Count == 0)
            {
                _ctx.Bus.Publish(new StatusMessage("warn", UiStrings.SelectObjectFirst));
                return;
            }

            var snapshot = new SceneObject[_ctx.Selection.Selected.Count];
            for (var i = 0; i < snapshot.Length; i++)
            {
                snapshot[i] = _ctx.Selection.Selected[i];
            }

            for (var i = 0; i < snapshot.Length; i++)
            {
                var assignment = snapshot[i].GetComponent<MaterialAssignment>();
                var previous = assignment != null ? assignment.Definition : null;
                _ctx.History.Execute(new SetMaterialCommand(_ctx.Applier, snapshot[i], definition, previous));
            }

            _ctx.Bus.Publish(new StatusMessage("info",
                string.Format(UiStrings.SelectedMaterialFormat, definition.DisplayName)));
        }

        public void Dispose()
        {
        }
    }
}
