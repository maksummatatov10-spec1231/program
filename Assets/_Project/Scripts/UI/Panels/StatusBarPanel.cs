using System.Collections.Generic;
using PhysSim.Core;
using UnityEngine;
using UnityEngine.UI;

namespace PhysSim.UI.Panels
{
    /// <summary>
    /// Статус-бар: слева — последнее сообщение (StatusMessage), справа — число
    /// объектов, FPS и состояние симуляции.
    /// </summary>
    public sealed class StatusBarPanel
    {
        private UiContext _ctx;
        private Text _statusText;
        private Text _statsText;
        private readonly List<EventBus.Subscription> _subscriptions = new List<EventBus.Subscription>(4);

        public void Build(RectTransform parent, UiContext ctx)
        {
            _ctx = ctx;
            UiFactory.AddImage(parent.gameObject, UiFactory.PanelDarkColor, true);

            var left = UiFactory.NewRect(parent, "Status");
            UiFactory.Place(left, new Vector2(0f, 0f), new Vector2(1f, 1f),
                new Vector2(8f, 2f), new Vector2(-560f, -2f));
            _statusText = UiFactory.AddText(left.gameObject,
                "Готово. Создайте объект или откройте сцену.", 12,
                TextAnchor.MiddleLeft, UiFactory.DimTextColor);

            var right = UiFactory.NewRect(parent, "Stats");
            UiFactory.Place(right, new Vector2(1f, 0f), new Vector2(1f, 1f),
                new Vector2(-552f, 2f), new Vector2(-10f, -2f));
            _statsText = UiFactory.AddText(right.gameObject, "", 12,
                TextAnchor.MiddleRight, UiFactory.DimTextColor);

            _subscriptions.Add(ctx.Bus.Subscribe<StatusMessage>(evt =>
            {
                _statusText.text = evt.Text ?? "";
                if (evt.Severity == "error")
                {
                    _statusText.color = new Color(0.95f, 0.45f, 0.42f);
                }
                else if (evt.Severity == "warn")
                {
                    _statusText.color = new Color(0.95f, 0.8f, 0.4f);
                }
                else
                {
                    _statusText.color = UiFactory.TextColor;
                }
            }));

            _subscriptions.Add(ctx.Bus.Subscribe<SceneSaved>(evt =>
                SetStatus(string.Format(UiStrings.SceneSavedFormat, evt.Path))));
            _subscriptions.Add(ctx.Bus.Subscribe<SceneLoaded>(evt =>
                SetStatus(string.Format(UiStrings.SceneLoadedFormat, evt.Path, evt.ObjectCount))));
            _subscriptions.Add(ctx.Bus.Subscribe<SceneObjectAdded>(evt =>
                SetStatus(string.Format(UiStrings.CreatedFormat, ctx.World.GetById(evt.Id) != null
                    ? ctx.World.GetById(evt.Id).DisplayName
                    : "объект"))));
        }

        private void SetStatus(string text)
        {
            _statusText.text = text;
            _statusText.color = UiFactory.TextColor;
        }

        /// <summary>Периодическое обновление правой части (объекты/FPS/состояние).</summary>
        public void Tick()
        {
            var fps = Time.smoothDeltaTime > 0f
                ? Mathf.RoundToInt(1f / Time.smoothDeltaTime)
                : 0;

            _statsText.text = string.Format(UiStrings.StatusObjectsFormat, _ctx.World.Objects.Count)
                              + $"   FPS: {fps}   "
                              + (_ctx.Simulation.IsRunning ? UiStrings.Running : UiStrings.Paused);
            _statsText.color = _ctx.Simulation.IsRunning
                ? new Color(0.45f, 0.85f, 0.5f)
                : UiFactory.DimTextColor;
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
