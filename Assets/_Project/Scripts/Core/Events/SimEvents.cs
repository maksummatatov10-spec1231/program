namespace PhysSim.Core
{
    /// <summary>
    /// Каталог доменных событий (см. ARCHITECTURE.md §7).
    /// Публикуются модулями домена, потребляются UI и Interaction.
    /// Все события — readonly-структуры, чтобы не аллоцировать.
    /// </summary>
    public readonly struct SceneObjectAdded
    {
        public readonly ObjectId Id;
        public readonly SceneObject Object;

        public SceneObjectAdded(ObjectId id, SceneObject sceneObject)
        {
            Id = id;
            Object = sceneObject;
        }
    }

    public readonly struct SceneObjectRemoved
    {
        public readonly ObjectId Id;

        public SceneObjectRemoved(ObjectId id)
        {
            Id = id;
        }
    }

    public readonly struct SelectionChanged
    {
        public readonly SceneObject[] Selected;

        public SelectionChanged(SceneObject[] selected)
        {
            Selected = selected;
        }
    }

    public readonly struct SimulationStateChanged
    {
        public readonly bool IsRunning;
        public readonly float TimeScale;

        public SimulationStateChanged(bool isRunning, float timeScale)
        {
            IsRunning = isRunning;
            TimeScale = timeScale;
        }
    }

    public readonly struct GravityChanged
    {
        public readonly float Magnitude;

        public GravityChanged(float magnitude)
        {
            Magnitude = magnitude;
        }
    }

    public readonly struct AirDensityChanged
    {
        public readonly float Density;

        public AirDensityChanged(float density)
        {
            Density = density;
        }
    }

    public readonly struct ObjectPhysicsToggled
    {
        public readonly ObjectId Id;
        public readonly bool Enabled;

        public ObjectPhysicsToggled(ObjectId id, bool enabled)
        {
            Id = id;
            Enabled = enabled;
        }
    }

    public readonly struct ObjectMaterialChanged
    {
        public readonly ObjectId Id;
        public readonly string MaterialId;

        public ObjectMaterialChanged(ObjectId id, string materialId)
        {
            Id = id;
            MaterialId = materialId;
        }
    }

    public readonly struct ObjectTransformEdited
    {
        public readonly ObjectId Id;

        public ObjectTransformEdited(ObjectId id)
        {
            Id = id;
        }
    }

    public readonly struct SceneSaved
    {
        public readonly string Path;

        public SceneSaved(string path)
        {
            Path = path;
        }
    }

    public readonly struct SceneLoaded
    {
        public readonly string Path;
        public readonly int ObjectCount;

        public SceneLoaded(string path, int objectCount)
        {
            Path = path;
            ObjectCount = objectCount;
        }
    }

    public readonly struct StatusMessage
    {
        public readonly string Severity; // "info" | "warn" | "error"
        public readonly string Text;

        public StatusMessage(string severity, string text)
        {
            Severity = severity;
            Text = text;
        }
    }
}
