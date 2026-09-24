using System;
using PhysSim.Core;
using PhysSim.Geometry;
using UnityEngine;
using UnityEngine.UI;

namespace PhysSim.UI.Panels
{
    /// <summary>
    /// Панель создания: примитивы + ручной ввод масштаба (требование ТЗ).
    /// Создание идёт командой SpawnObjectCommand — работает undo.
    /// </summary>
    public sealed class CreatePanel
    {
        private UiContext _ctx;
        private InputField _scaleX;
        private InputField _scaleY;
        private InputField _scaleZ;
        private Func<Vector3> _spawnPoint;
        private Action<SceneObject> _onCreated;

        public void Build(RectTransform parent, UiContext ctx,
            Func<Vector3> spawnPoint, Action<SceneObject> onCreated)
        {
            _ctx = ctx;
            _spawnPoint = spawnPoint;
            _onCreated = onCreated;

            UiFactory.AddImage(parent.gameObject, UiFactory.PanelColor, true);

            var title = UiFactory.NewRect(parent, "Title");
            UiFactory.Place(title, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(8f, -24f), new Vector2(-8f, -2f));
            UiFactory.AddText(title.gameObject, "Создать", 13, TextAnchor.MiddleLeft, UiFactory.DimTextColor);

            AddPrimitiveButton(parent, "BtnCube", UiStrings.CreateCube, PrimitiveType.Cube, 8f, 34f);
            AddPrimitiveButton(parent, "BtnSphere", UiStrings.CreateSphere, PrimitiveType.Sphere, 146f, 34f);
            AddPrimitiveButton(parent, "BtnCylinder", UiStrings.CreateCylinder, PrimitiveType.Cylinder, 8f, 68f);
            AddPrimitiveButton(parent, "BtnCapsule", UiStrings.CreateCapsule, PrimitiveType.Capsule, 146f, 68f);
            var scaleLabel = UiFactory.NewRect(parent, "ScaleLabel");
            UiFactory.Place(scaleLabel, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(8f, -76f), new Vector2(-8f, -52f));
            UiFactory.AddText(scaleLabel.gameObject, UiStrings.ScaleLabel, 12,
                TextAnchor.MiddleLeft, UiFactory.DimTextColor);

            _scaleX = AddScaleField(parent, "SX", "X", 8f);
            _scaleY = AddScaleField(parent, "SY", "Y", 100f);
            _scaleZ = AddScaleField(parent, "SZ", "Z", 192f);

            var hint = UiFactory.NewRect(parent, "Hint");
            UiFactory.Place(hint, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(8f, -142f), new Vector2(-8f, -120f));
            UiFactory.AddText(hint.gameObject, UiStrings.CreateHint, 11,
                TextAnchor.MiddleLeft, UiFactory.DimTextColor);
        }

        private void AddPrimitiveButton(RectTransform parent, string name, string caption,
            PrimitiveType type, float x, float yFromTop)
        {
            var rect = UiFactory.NewRect(parent, name);
            UiFactory.Place(rect, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(x, -yFromTop - 30f), new Vector2(x + 130f, -yFromTop));
            var primitiveType = type;
            UiFactory.DecorateButton(rect, caption, 13, UiFactory.ButtonColor,
                () => CreatePrimitive(primitiveType));
        }

        private static InputField AddScaleField(RectTransform parent, string name, string axis,
            float x)
        {
            var label = UiFactory.NewRect(parent, name + "_Label");
            UiFactory.Place(label, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(x, -112f), new Vector2(x + 18f, -92f));
            UiFactory.AddText(label.gameObject, axis, 12, TextAnchor.MiddleCenter, UiFactory.DimTextColor);

            var field = UiFactory.MakeInputField(parent, name, "1", 12, null);
            UiFactory.Place((RectTransform)field.transform, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(x + 20f, -112f), new Vector2(x + 78f, -92f));
            field.text = "1";
            return field;
        }

        /// <summary>Создать примитив командой (undoable).</summary>
        public void CreatePrimitive(PrimitiveType type)
        {
            var scale = new Vector3(
                ParseScale(_scaleX),
                ParseScale(_scaleY),
                ParseScale(_scaleZ));
            scale = Vector3.Max(scale, new Vector3(0.01f, 0.01f, 0.01f));

            var position = _spawnPoint != null ? _spawnPoint() : Vector3.zero;
            position.y = HalfHeight(type) * scale.y + 0.01f;

            var spec = _ctx.Factory.BuildPrimitiveSpec(type, position, scale);
            _ctx.History.Execute(new SpawnObjectCommand(_ctx.Factory, _ctx.Destroyer, spec));

            var created = _ctx.World.GetById(spec.Id);
            if (created != null)
            {
                _onCreated?.Invoke(created);
            }
        }

        private static float ParseScale(InputField field)
        {
            return field != null && UiFactory.TryParseFloat(field.text, out var value) && value > 0f
                ? value
                : 1f;
        }

        private static float HalfHeight(PrimitiveType type)
        {
            switch (type)
            {
                case PrimitiveType.Cube:
                case PrimitiveType.Sphere:
                    return 0.5f;
                default:
                    return 1f; // цилиндр и капсула высотой 2
            }
        }

        public void Dispose()
        {
        }
    }
}
