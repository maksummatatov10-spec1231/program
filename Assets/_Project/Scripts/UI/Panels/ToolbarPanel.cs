using System;
using PhysSim.Core;
using UnityEngine;
using UnityEngine.UI;

namespace PhysSim.UI.Panels
{
    /// <summary>
    /// Верхняя панель: файл (новый/открыть/сохранить/импорт/экспорт), транспорт
    /// (старт/пауза/шаг/стоп), масштаб времени, гравитация, плотность воздуха,
    /// undo/redo и справка.
    /// </summary>
    public sealed class ToolbarPanel
    {
        private static readonly float[] TimeScaleValues = { 0.25f, 0.5f, 1f, 2f };

        private UiContext _ctx;
        private Button _playButton;
        private Text _playLabel;
        private Button[] _timeButtons = new Button[TimeScaleValues.Length];
        private Slider _gravitySlider;
        private Slider _airSlider;
        private Text _gravityValue;
        private Text _airValue;
        private readonly System.Collections.Generic.List<EventBus.Subscription> _subscriptions =
            new System.Collections.Generic.List<EventBus.Subscription>(4);

        public Action OnNew;
        public Action OnOpen;
        public Action OnSave;
        public Action OnImport;
        public Action OnExport;
        public Action OnToggleHelp;

        public void Build(RectTransform parent, UiContext ctx)
        {
            _ctx = ctx;
            UiFactory.AddImage(parent.gameObject, UiFactory.PanelColor, true);

            // Файл.
            MakeAt(parent, "New", UiStrings.FileNew, 8f, 78f, () => OnNew?.Invoke());
            MakeAt(parent, "Open", UiStrings.FileOpen, 90f, 78f, () => OnOpen?.Invoke());
            MakeAt(parent, "Save", UiStrings.FileSave, 172f, 78f, () => OnSave?.Invoke());
            MakeAt(parent, "Import", UiStrings.Import, 254f, 90f, () => OnImport?.Invoke());
            MakeAt(parent, "Export", UiStrings.Export, 348f, 90f, () => OnExport?.Invoke());

            // Undo / Redo.
            MakeAt(parent, "Undo", "⟲", 446f, 34f, () => ctx.History.Undo());
            MakeAt(parent, "Redo", "⟳", 484f, 34f, () => ctx.History.Redo());

            // Транспорт.
            _playButton = MakeAt(parent, "Play", UiStrings.Play, 536f, 78f, () => ctx.Simulation.Toggle());
            _playLabel = _playButton.transform.Find("Caption").GetComponent<Text>();
            MakeAt(parent, "Step", UiStrings.Step, 618f, 58f, () => ctx.Simulation.StepOnce());
            MakeAt(parent, "Stop", UiStrings.StopMotion, 680f, 58f, () =>
            {
                ctx.Simulation.Pause();
                ctx.Simulation.ZeroAllMotion(ctx.World);
            });

            // Масштаб времени.
            for (var i = 0; i < TimeScaleValues.Length; i++)
            {
                var index = i;
                _timeButtons[i] = MakeAt(parent, "Time" + i, UiFactory.Format(TimeScaleValues[i], "0.##"),
                    756f + i * 50f, 46f, () => { ctx.Simulation.TimeScale = TimeScaleValues[index]; RefreshState(); });
            }

            // Гравитация.
            AddRightLabel(parent, "GLabel", "g:", 578f);
            _gravitySlider = UiFactory.MakeSlider(parent, "GSlider", 0f, 30f, ctx.Simulation.GravityMagnitude,
                value => { ctx.Simulation.SetGravity(value); RefreshState(); });
            PlaceRight(_gravitySlider.GetComponent<RectTransform>(), 440f, 150f);
            _gravityValue = AddRightLabel(parent, "GValue", "", 366f, width: 68f);

            // Плотность воздуха.
            AddRightLabel(parent, "AirLabel", "ρ:", 742f);
            _airSlider = UiFactory.MakeSlider(parent, "AirSlider", 0f, 3f, ctx.Simulation.AirDensity,
                value => { ctx.Simulation.SetAirDensity(value); RefreshState(); });
            PlaceRight(_airSlider.GetComponent<RectTransform>(), 830f, 150f);
            _airValue = AddRightLabel(parent, "AirValue", "", 604f, width: 68f);

            // Справка.
            MakeRight(parent, "Help", UiStrings.Help, 34f, () => OnToggleHelp?.Invoke());

            _subscriptions.Add(ctx.Bus.Subscribe<SimulationStateChanged>(evt => RefreshState()));
            _subscriptions.Add(ctx.Bus.Subscribe<GravityChanged>(evt => RefreshState()));
            _subscriptions.Add(ctx.Bus.Subscribe<AirDensityChanged>(evt => RefreshState()));

            RefreshState();
        }

        public void RefreshState()
        {
            var running = _ctx.Simulation.IsRunning;
            _playLabel.text = running ? UiStrings.Pause : UiStrings.Play;
            _playButton.image.color = running ? UiFactory.ButtonAccentColor : UiFactory.ButtonColor;

            for (var i = 0; i < _timeButtons.Length; i++)
            {
                var active = Mathf.Abs(_ctx.Simulation.TimeScale - TimeScaleValues[i]) < 0.001f;
                _timeButtons[i].image.color = active
                    ? UiFactory.ButtonAccentColor
                    : UiFactory.ButtonColor;
            }

            _gravitySlider.SetValueWithoutNotify(_ctx.Simulation.GravityMagnitude);
            _gravityValue.text = UiFactory.Format(_ctx.Simulation.GravityMagnitude) + " м/с²";
            _airSlider.SetValueWithoutNotify(_ctx.Simulation.AirDensity);
            _airValue.text = UiFactory.Format(_ctx.Simulation.AirDensity) + " кг/м³";
        }

        public void Dispose()
        {
            for (var i = 0; i < _subscriptions.Count; i++)
            {
                _subscriptions[i].Dispose();
            }
            _subscriptions.Clear();
        }

        private static Button MakeAt(RectTransform parent, string name, string caption,
            float x, float width, Action onClick)
        {
            var rect = UiFactory.NewRect(parent, name);
            UiFactory.Place(rect, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(x, -15f), new Vector2(x + width, 15f));
            return UiFactory.DecorateButton(rect, caption, 13, UiFactory.ButtonColor, onClick);
        }

        private static Text AddRightLabel(RectTransform parent, string name, string caption,
            float fromRight, float width = 20f)
        {
            var rect = UiFactory.NewRect(parent, name);
            UiFactory.Place(rect, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-fromRight - width, -14f), new Vector2(-fromRight, 14f));
            return UiFactory.AddText(rect.gameObject, caption, 13, TextAnchor.MiddleRight, UiFactory.TextColor);
        }

        private static void PlaceRight(RectTransform rect, float fromRight, float width)
        {
            UiFactory.Place(rect, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-fromRight - width, -8f), new Vector2(-fromRight, 8f));
        }

        private static Button MakeRight(RectTransform parent, string name, string caption,
            float width, Action onClick)
        {
            var rect = UiFactory.NewRect(parent, name);
            UiFactory.Place(rect, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-width - 8f, -15f), new Vector2(-8f, 15f));
            return UiFactory.DecorateButton(rect, caption, 14, UiFactory.ButtonColor, onClick);
        }
    }
}
