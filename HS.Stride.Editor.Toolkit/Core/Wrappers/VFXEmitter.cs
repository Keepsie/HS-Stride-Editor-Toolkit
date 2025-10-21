// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using HS.Stride.Editor.Toolkit.Utilities;
using HS.Stride.Editor.Toolkit.Core.AssetEditing;

namespace HS.Stride.Editor.Toolkit.Core.Wrappers
{
    /// <summary>
    /// Represents a single particle emitter within a particle system.
    /// Each emitter can have its own shape, material, spawners, initializers, and updaters.
    /// </summary>
    public class VFXEmitter
    {
        internal string Key { get; set; } = string.Empty;
        internal Dictionary<string, object> Properties { get; set; } = new();

        public VFXEmitter()
        {
            Key = GuidHelper.NewGuid();
        }

        /// <summary>
        /// Creates a VFXEmitter from existing emitter data (for loading)
        /// </summary>
        internal static VFXEmitter FromData(string key, Dictionary<string, object> data)
        {
            return new VFXEmitter
            {
                Key = key,
                Properties = data
            };
        }

        /// <summary>
        /// Gets the internal emitter data dictionary
        /// </summary>
        internal Dictionary<string, object> GetEmitterData() => Properties;

        /// <summary>
        /// Optional name for this emitter (e.g., "fire", "smoke", "sparks")
        /// </summary>
        public string? EmitterName
        {
            get => Get<string>("EmitterName");
            set => Set("EmitterName", value);
        }

        /// <summary>
        /// Particle lifetime range (Min, Max) in seconds
        /// </summary>
        public (float min, float max) ParticleLifetime
        {
            get
            {
                var dict = Get<Dictionary<string, object>>("ParticleLifetime");
                if (dict != null)
                {
                    var x = dict.ContainsKey("X") ? Convert.ToSingle(dict["X"]) : 1.0f;
                    var y = dict.ContainsKey("Y") ? Convert.ToSingle(dict["Y"]) : 1.0f;
                    return (x, y);
                }
                return (1.0f, 1.0f);
            }
            set => Set("ParticleLifetime", new Dictionary<string, object>
            {
                ["X"] = value.min,
                ["Y"] = value.max
            });
        }

        /// <summary>
        /// Maximum number of particles override (optional)
        /// </summary>
        public int? MaxParticlesOverride
        {
            get => Get<int?>("MaxParticlesOverride");
            set => Set("MaxParticlesOverride", value);
        }

        /// <summary>
        /// Draw priority (higher values render later/on top)
        /// </summary>
        public int? DrawPriority
        {
            get => Get<int?>("DrawPriority");
            set => Set("DrawPriority", value);
        }

        /// <summary>
        /// Sorting policy for particles (e.g., "ByOrder" for ribbons/trails)
        /// </summary>
        public string? SortingPolicy
        {
            get => Get<string>("SortingPolicy");
            set => Set("SortingPolicy", value);
        }

        /// <summary>
        /// Gets a property value. Supports nested paths with dot notation (e.g., "Material.AlphaAdditive").
        /// </summary>
        public T? Get<T>(string key)
        {
            // Support dot notation for nested properties
            var parts = key.Split('.');
            object? current = Properties;

            foreach (var part in parts)
            {
                if (current is Dictionary<string, object> dict)
                {
                    if (!dict.ContainsKey(part))
                        return default;
                    current = dict[part];
                }
                else
                {
                    return default;
                }
            }

            if (current is T typed)
                return typed;

            try
            {
                return (T)Convert.ChangeType(current, typeof(T));
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// Sets a property value
        /// </summary>
        public void Set(string key, object? value)
        {
            if (value == null)
                Properties.Remove(key);
            else
                Properties[key] = value;
        }

        /// <summary>
        /// Sets the shape builder for this emitter
        /// </summary>
        /// <param name="shapeType">Type of shape</param>
        /// <param name="shapeData">Shape-specific configuration</param>
        public void SetShapeBuilder(VFXShapeType shapeType, Dictionary<string, object>? shapeData = null)
        {
            SetShapeBuilderInternal(shapeType.ToString(), shapeData);
        }

        /// <summary>
        /// Internal method for setting shape builder (used by loading logic)
        /// </summary>
        internal void SetShapeBuilderInternal(string shapeType, Dictionary<string, object>? shapeData = null)
        {
            var shape = shapeData ?? new Dictionary<string, object>();
            shape["!ShapeBuilder" + shapeType] = "";
            Properties["ShapeBuilder"] = shape;
        }

        /// <summary>
        /// Sets the material for this emitter
        /// </summary>
        /// <param name="materialData">Material configuration dictionary</param>
        public void SetMaterial(Dictionary<string, object> materialData)
        {
            Properties["Material"] = materialData;
        }

        /// <summary>
        /// Adds a spawner to this emitter
        /// </summary>
        /// <param name="spawnerType">Type of spawner (Burst, PerSecond, PerFrame)</param>
        /// <param name="spawnerData">Spawner configuration</param>
        public void AddSpawner(string spawnerType, Dictionary<string, object> spawnerData)
        {
            if (!Properties.ContainsKey("Spawners"))
                Properties["Spawners"] = new Dictionary<string, object>();

            var spawners = (Dictionary<string, object>)Properties["Spawners"];
            var key = GuidHelper.NewGuid();

            spawnerData["!Spawner" + spawnerType] = "";
            spawners[key] = spawnerData;
        }

        /// <summary>
        /// Adds an initializer to this emitter
        /// </summary>
        /// <param name="initializerType">Type of initializer (Position, Velocity, Size, Rotation, Color, etc.)</param>
        /// <param name="initializerData">Initializer configuration</param>
        public void AddInitializer(string initializerType, Dictionary<string, object> initializerData)
        {
            if (!Properties.ContainsKey("Initializers"))
                Properties["Initializers"] = new Dictionary<string, object>();

            var initializers = (Dictionary<string, object>)Properties["Initializers"];
            var key = GuidHelper.NewGuid();

            initializerData["!Initial" + initializerType] = "";
            initializers[key] = initializerData;
        }

        /// <summary>
        /// Adds an updater to this emitter
        /// </summary>
        /// <param name="updaterType">Type of updater (ColorOverTime, SizeOverTime, ForceField, etc.)</param>
        /// <param name="updaterData">Updater configuration</param>
        public void AddUpdater(string updaterType, Dictionary<string, object> updaterData)
        {
            if (!Properties.ContainsKey("Updaters"))
                Properties["Updaters"] = new Dictionary<string, object>();

            var updaters = (Dictionary<string, object>)Properties["Updaters"];
            var key = GuidHelper.NewGuid();

            updaterData["!Updater" + updaterType] = "";
            updaters[key] = updaterData;
        }
    }
}
