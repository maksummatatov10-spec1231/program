using System.Collections.Generic;
using PhysSim.Core;
using PhysSim.Geometry;
using PhysSim.Materials;
using PhysSim.Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace PhysSim.UI.Panels
{
    /// <summary>
    /// Инспектор выделенного объекта: имя, TRS с ручным вводом (включая Scale —
    /// требование ТЗ), физика вкл/выкл, расчётные масса/объём/предел. скорость,
    /// материал, удаление/дублирование. Все изменения — через команды (undoable).
    /// </summary>
    public sealed class InspectorPanel
    {
        private UiContext _ctx;
        private SceneObject _target;

        private InputField _nameField;
        private readonly InputField[] _positionFields = new InputField[3];
        private readonly InputField[] _rotationFields = new InputField[3];
        private readonly InputField[] _scaleFields = new InputField[3];
        private Toggle _physicsToggle;

        private Text _massValue;
        private Text _volumeValue;
        private Text _terminalValue;
        private Text _materialValue;
        private Text _header;

        private readonly List<InputField> _allFields = new List<InputField>(10);
        private readonly List<EventBus.Subscription> _subscriptions = new List<EventBus.Subscription>(4);

        public void Build(RectTransform parent, UiContext ctx)
        {
            _ctx = ctx;
            UiFactory.AddImage(parent.gameObject, UiFactory.PanelColor, true);

            _header = AddTitle(parent, "Header", UiStrings.NothingSelected, -24f);

            // Имя.
            var nameLabel = UiFactory.NewRect(parent, "NameLabel");
            UiFactory.Place(nameLabel, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(8f, -52f), new Vector2(66f, -30f));
            UiFactory.AddText(nameLabel.gameObject, UiStrings.NameLabel, 12,
                TextAnchor.MiddleLeft, UiFactory.DimTextColor);

            _nameField = UiFactory.MakeInputField(parent, "Name", "имя объекта", 12,
                value => CommitName(value));
            UiFactory.Place((RectTransform)_nameField.transform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(70f, -52f), new Vector2(-8f, -30f));
            _allFields.Add(_nameField);

            // TRS.
            AddAxisHeader(parent);
            AddTrsRow(parent, UiStrings.PositionLabel, _positionFields, -118f, CommitPosition);
            AddTrsRow(parent, UiStrings.RotationLabel, _rotationFields, -160f, CommitRotation);
            AddTrsRow(parent, UiStrings.ScaleLabel, _scaleFields, -202f, CommitScale);
            for (var axis = 0; axis < 3; axis++)
            {
                _allFields.Add(_positionFields[axis]);
                _allFields.Add(_rotationFields[axis]);
                _allFields.Add(_scaleFields[axis]);
            }

            // Физика.
            _physicsToggle = UiFactory.MakeToggle(parent, "Physics", UiStrings.PhysicsLabel, false,
                isOn => CommitPhysics(isOn));

            // Расчётные величины.
            _massValue = AddInfoRow(parent, UiStrings.MassLabel, -322f);
            _volumeValue = AddInfoRow(parent, UiStrings.VolumeLabel, -348f);
            _terminalValue = AddInfoRow(parent, UiStrings.TerminalSpeedLabel, -374f);
            _materialValue = AddInfoRow(parent, UiStrings.MaterialLabel, -400f);

            var materialHint = UiFactory.NewRect(parent, "MaterialHint");
            UiFactory.Place(materialHint, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(8f, -428f), new Vector2(-8f, -404f));
            UiFactory.AddText(materialHint.gameObject,
                "Новый материал — кликом в библиотеке слева ↓", 11,
                TextAnchor.MiddleLeft, UiFactory.DimTextColor);

            // Действия.
            var deleteRect = UiFactory.NewRect(parent, "Delete");
            UiFactory.Place(deleteRect, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(8f, -466f), new Vector2(148f, -438f));
            UiFactory.DecorateButton(deleteRect, UiStrings.Delete, 12, UiFactory.ButtonColor, DeleteSelected);

            var duplicateRect = UiFactory.NewRect(parent, "Duplicate");
            UiFactory.Place(duplicateRect, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(156f, -466f), new Vector2(296f, -438f));
            UiFactory.DecorateButton(duplicateRect, UiStrings.Duplicate, 12, UiFactory.ButtonColor, DuplicateSelected);

            _subscriptions.Add(ctx.Bus.Subscribe<SelectionChanged>(evt =>
            {
                _target = evt.Selected != null && evt.Selected.Length == 1 ? evt.Selected[0] : null;
                RefreshAll();
            }));
            _subscriptions.Add(ctx.Bus.Subscribe<ObjectMaterialChanged>(evt => RefreshInfo()));
            _subscriptions.Add(ctx.Bus.Subscribe<ObjectRenamed>(evt => RefreshAll()));

            RefreshAll();
        }

        public void UpdateTick()
        {
            if (_target == null)
            {
                return;
            }

            // Обновляем только нефокусированные поля — чтобы не мешать вводу.
            if (!_nameField.isFocused)
            {
                _nameField.SetTextWithoutNotify(_target.DisplayName);
            }

            var body = _target.GetComponent<SimulatedBody>();
            _physicsToggle.SetIsOnWithoutNotify(body != null && body.PhysicsEnabled);

            RefreshTrs();
            RefreshInfo();
        }

        public void Dispose()
        {
            for (var i = 0; i < _subscriptions.Count; i++)
            {
                _subscriptions[i].Dispose();
            }
            _subscriptions.Clear();
        }

        // ───────────────────── Коммиты полей → команды ─────────────────────

        private void CommitName(string value)
        {
            if (_target == null || string.IsNullOrEmpty(value) || value == _target.DisplayName)
            {
                RefreshAll();
                return;
            }

            _ctx.History.Execute(new RenameObjectCommand(_target, _target.DisplayName, value, _ctx.Bus));
        }

        private void CommitPosition(int axis, string text)
        {
            CommitTrs(axis, text, TrsKind.Position);
        }

        private void CommitRotation(int axis, string text)
        {
            CommitTrs(axis, text, TrsKind.Rotation);
        }

        private void CommitScale(int axis, string text)
        {
            CommitTrs(axis, text, TrsKind.Scale);
        }

        private enum TrsKind
        {
            Position = 0,
            Rotation = 1,
            Scale = 2
        }

        private void CommitTrs(int axis, string text, TrsKind kind)
        {
            if (_target == null || !UiFactory.TryParseFloat(text, out var value))
            {
                RefreshTrs();
                return;
            }

            var before = TransformSnapshot.From(_target);
            var after = before;

            switch (kind)
            {
                case TrsKind.Position:
                    value = Mathf.Clamp(value, -100000f, 100000f);
                    after = WithAxis(before.Position, axis, value, before, 0);
                    break;
                case TrsKind.Rotation:
                    after = WithAxis(before.RotationEuler, axis, value, before, 1);
                    break;
                case TrsKind.Scale:
                    value = Mathf.Clamp(Mathf.Abs(value), 0.001f, 10000f);
                    after = WithAxis(before.Scale, axis, value, before, 2);
                    break;
            }

            _ctx.History.Execute(new TransformObjectCommand(_target, before, after));
            RefreshTrs();
        }

        private static TransformSnapshot WithAxis(Vector3 source, int axis, float value,
            TransformSnapshot baseSnapshot, int kindIndex)
        {
            var vector = source;
            vector[axis] = value;
            switch (kindIndex)
            {
                case 0: return new TransformSnapshot(vector, baseSnapshot.RotationEuler, baseSnapshot.Scale);
                case 1: return new TransformSnapshot(baseSnapshot.Position, vector, baseSnapshot.Scale);
                default: return new TransformSnapshot(baseSnapshot.Position, baseSnapshot.RotationEuler, vector);
            }
        }

        private void CommitPhysics(bool enabled)
        {
            if (_target == null)
            {
                return;
            }

            var body = _target.GetComponent<SimulatedBody>();
            if (body != null && body.PhysicsEnabled != enabled)
            {
                _ctx.History.Execute(new TogglePhysicsCommand(_target, enabled));
            }
        }

        private void DeleteSelected()
        {
            if (_ctx.Selection.Selected.Count == 0)
            {
                return;
            }

            var snapshot = new SceneObject[_ctx.Selection.Selected.Count];
            for (var i = 0; i < snapshot.Length; i++)
            {
                snapshot[i] = _ctx.Selection.Selected[i];
            }

            for (var i = 0; i < snapshot.Length; i++)
            {
                _ctx.History.Execute(new DeleteObjectCommand(_ctx.Factory, _ctx.Destroyer, snapshot[i]));
            }
        }

        private void DuplicateSelected()
        {
            if (_ctx.Selection.Selected.Count == 0)
            {
                return;
            }

            var source = _ctx.Selection.Selected[_ctx.Selection.Selected.Count - 1];
            var spec = _ctx.Factory.Describe(source);
            spec.Id = ObjectId.NewId();
            spec.Position += new Vector3(0.6f, 0f, 0.6f);
            spec.Name = $"{source.DisplayName} (копия)";

            _ctx.History.Execute(new SpawnObjectCommand(_ctx.Factory, _ctx.Destroyer, spec));
            var created = _ctx.World.GetById(spec.Id);
            if (created != null)
            {
                _ctx.Selection.Select(created, false);
            }
        }

        // ───────────────────────────── Отрисовка ─────────────────────────────

        private void RefreshAll()
        {
            var empty = _target == null;
            _header.text = empty
                ? UiStrings.NothingSelected
                : _target.DisplayName;

            _nameField.gameObject.SetActive(!empty);
            _physicsToggle.gameObject.SetActive(!empty);

            RefreshTrs();
            RefreshInfo();
        }

        private void RefreshTrs()
        {
            if (_target == null)
            {
                return;
            }

            var t = _target.transform;
            if (!_positionFields[0].isFocused && !_positionFields[1].isFocused && !_positionFields[2].isFocused)
            {
                SetVector(_positionFields, t.position);
            }
            if (!_rotationFields[0].isFocused && !_rotationFields[1].isFocused && !_rotationFields[2].isFocused)
            {
                SetVector(_rotationFields, t.rotation.eulerAngles);
            }
            if (!_scaleFields[0].isFocused && !_scaleFields[1].isFocused && !_scaleFields[2].isFocused)
            {
                SetVector(_scaleFields, t.localScale);
            }
        }

        private void RefreshInfo()
        {
            if (_target == null)
            {
                _massValue.text = "—";
                _volumeValue.text = "—";
                _terminalValue.text = "—";
                _materialValue.text = "—";
                return;
            }

            var body = _target.GetComponent<SimulatedBody>();
            if (body != null)
            {
                _massValue.text = $"{UiFactory.Format(body.Mass, "0.####")} кг";
                _volumeValue.text = $"{UiFactory.Format(body.Volume, "0.######")} м³";
                var terminal = body.GetTerminalSpeed();
                _terminalValue.text = float.IsInfinity(terminal)
                    ? "∞ (вакуум)"
                    : $"{UiFactory.Format(terminal, "0.#")} м/с";
            }

            var assignment = _target.GetComponent<MaterialAssignment>();
            _materialValue.text = assignment != null && assignment.Definition != null
                ? $"{assignment.Definition.DisplayName} ({UiFactory.Format(assignment.Definition.Density, "0")} кг/м³)"
                : "—";
        }

        private void SetVector(InputField[] fields, Vector3 vector)
        {
            fields[0].SetTextWithoutNotify(UiFactory.Format(vector.x));
            fields[1].SetTextWithoutNotify(UiFactory.Format(vector.y));
            fields[2].SetTextWithoutNotify(UiFactory.Format(vector.z));
        }

        // ───────────────────────────── Виджеты ─────────────────────────────

        private static Text AddTitle(RectTransform parent, string name, string caption, float y)
        {
            var rect = UiFactory.NewRect(parent, name);
            UiFactory.Place(rect, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(8f, y), new Vector2(-8f, y + 22f));
            return UiFactory.AddText(rect.gameObject, caption, 14, TextAnchor.MiddleLeft, UiFactory.TextColor);
        }

        private static void AddAxisHeader(RectTransform parent)
        {
            var label = UiFactory.NewRect(parent, "AxisSpacer");
            UiFactory.Place(label, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(8f, -88f), new Vector2(66f, -60f));
            UiFactory.AddText(label.gameObject, "TRS", 12, TextAnchor.MiddleLeft, UiFactory.DimTextColor);

            AddAxisLetter(parent, "X", 74f);
            AddAxisLetter(parent, "Y", 148f);
            AddAxisLetter(parent, "Z", 222f);
        }

        private static void AddAxisLetter(RectTransform parent, string letter, float x)
        {
            var rect = UiFactory.NewRect(parent, "Axis" + letter);
            UiFactory.Place(rect, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(x, -88f), new Vector2(x + 68f, -60f));
            UiFactory.AddText(rect.gameObject, letter, 12, TextAnchor.MiddleCenter, UiFactory.DimTextColor);
        }

        private void AddTrsRow(RectTransform parent, string caption, InputField[] fields,
            float yFromTop, System.Action<int, string> onCommit)
        {
            var label = UiFactory.NewRect(parent, caption + "Label");
            UiFactory.Place(label, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(8f, yFromTop), new Vector2(68f, yFromTop + 22f));
            UiFactory.AddText(label.gameObject, caption, 12, TextAnchor.MiddleLeft, UiFactory.DimTextColor);

            for (var axis = 0; axis < 3; axis++)
            {
                var axisIndex = axis;
                var field = UiFactory.MakeInputField(parent, caption + axis, "0", 12,
                    value => onCommit(axisIndex, value));
                UiFactory.Place((RectTransform)field.transform, new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(74f + axis * 74f, yFromTop), new Vector2(142f + axis * 74f, yFromTop + 22f));
                fields[axis] = field;
            }
        }

        private static Text AddInfoRow(RectTransform parent, string caption, float yFromTop)
        {
            var label = UiFactory.NewRect(parent, caption + "L");
            UiFactory.Place(label, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(8f, yFromTop), new Vector2(148f, yFromTop + 24f));
            UiFactory.AddText(label.gameObject, caption, 12, TextAnchor.MiddleLeft, UiFactory.DimTextColor);

            var value = UiFactory.NewRect(parent, caption + "V");
            UiFactory.Place(value, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(150f, yFromTop), new Vector2(-8f, yFromTop + 24f));
            return UiFactory.AddText(value.gameObject, "—", 12, TextAnchor.MiddleLeft, UiFactory.TextColor);
        }
    }
}
