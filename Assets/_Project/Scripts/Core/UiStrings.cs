namespace PhysSim.Core
{
    /// <summary>
    /// Все русские строки UI и команд в одном месте (легко вынести в таблицу локализации).
    /// </summary>
    public static class UiStrings
    {
        // Приложение
        public const string AppTitle = "PhysSim Studio";

        // Файл
        public const string FileNew = "Новый";
        public const string FileOpen = "Открыть";
        public const string FileSave = "Сохранить";
        public const string Import = "Импорт OBJ";
        public const string Export = "Экспорт OBJ";
        public const string SceneExtension = "pscene";
        public const string SceneFileFilterTitle = "Сцена PhysSim";
        public const string ImportFileTitle = "Импорт модели";
        public const string ExportFileTitle = "Экспорт модели в OBJ";
        public const string DefaultSceneName = "сцена.pscene";
        public const string DefaultModelName = "модель.obj";

        // Симуляция
        public const string Play = "Старт";
        public const string Pause = "Пауза";
        public const string Step = "Шаг";
        public const string StopMotion = "Стоп";
        public const string Running = "ИДЁТ";
        public const string Paused = "ПАУЗА";

        // Творчество
        public const string CreateCube = "Куб";
        public const string CreateSphere = "Сфера";
        public const string CreateCylinder = "Цилиндр";
        public const string CreateCapsule = "Капсула";
        public const string ScaleLabel = "Масштаб (X Y Z):";
        public const string CreateHint = "Создаётся перед камерой; масштаб можно ввести вручную";

        // Панели
        public const string HierarchyTitle = "Иерархия";
        public const string LibraryTitle = "Библиотека материалов";
        public const string InspectorTitle = "Инспектор";
        public const string SearchHint = "Поиск…";
        public const string NothingSelected = "Ничего не выделено";
        public const string MaterialLabel = "Материал";
        public const string ChangeMaterial = "Сменить…";
        public const string PhysicsLabel = "Физика";
        public const string MassLabel = "Масса";
        public const string VolumeLabel = "Объём";
        public const string TerminalSpeedLabel = "Предел. скорость";
        public const string PositionLabel = "Позиция";
        public const string RotationLabel = "Вращение";
        public const string NameLabel = "Имя";

        // Действия
        public const string Undo = "⟲";
        public const string Redo = "⟳";
        public const string Help = "?";
        public const string Delete = "Удалить";
        public const string Duplicate = "Дублировать";

        // Имена объектов
        public const string NameCube = "Куб";
        public const string NameSphere = "Сфера";
        public const string NameCylinder = "Цилиндр";
        public const string NameCapsule = "Капсула";
        public const string NameMesh = "Модель";
        public const string GroundName = "Земля";
        public const string CameraName = "Камера";
        public const string LightName = "Свет";

        // Статусы
        public const string StatusObjectsFormat = "Объектов: {0}";
        public const string StatusFpsFormat = "FPS: {0}";
        public const string CreatedFormat = "Создан: {0}";
        public const string DeletedFormat = "Удалён: {0}";
        public const string SelectedMaterialFormat = "Материал «{0}» применён";
        public const string SelectObjectFirst = "Сначала выделите объект на сцене";
        public const string NothingToExport = "Нечего экспортировать — сцена пуста";
        public const string ImportRequiresFile = "Укажите путь к .obj файлу";
        public const string SceneSavedFormat = "Сцена сохранена: {0}";
        public const string SceneLoadedFormat = "Сцена загружена: {0} (объектов: {1})";
        public const string SceneCleared = "Создана новая сцена";
        public const string FbxNeedsPlugin = "FBX-импорт требует плагина AssimpNet (см. docs/BUILD_RU.md). Используйте OBJ.";
        public const string ImportFailedFormat = "Ошибка импорта: {0}";
        public const string ImportedFormat = "Импортировано: {0} (объектов: {1})";
        public const string SavedToFormat = "Сохранено: {0}";

        // Справка
        public const string HelpTitle = "Управление";
        public const string HelpText =
            "ЛКМ — выбрать (Shift — добавить)\n" +
            "ПКМ — орбита, СКМ — панорама, колесо — зум\n" +
            "W/E/R — гизмо: переместить / вращать / масштаб\n" +
            "P — пауза/пуск симуляции\n" +
            "B — включить/выключить физику у выделенного\n" +
            "Del — удалить, Ctrl+D — дублировать\n" +
            "Ctrl+Z / Ctrl+Y — отмена / повтор\n" +
            "F — кадрировать выделенное, Esc — снять выбор\n" +
            "\nМатериал задаёт массу (ρ·V), трение и сопротивление\n" +
            "воздуха: бумага падает медленно, железо — быстро.";
        public const string Close = "Закрыть";
    }
}
