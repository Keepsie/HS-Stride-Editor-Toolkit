// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using HS.Stride.Editor.Toolkit.Utilities;

namespace HS.Stride.Editor.Toolkit.Core.Wrappers
{
    /// <summary>
    /// Wrapper for ParticleSystemComponent providing easy access to VFX particle system properties.
    /// Use this to add and configure particle effects (explosions, smoke, fire, sparks, etc.) on entities.
    /// </summary>
    public class ParticleSystemWrapper
    {
        public Component Component { get; private set; }

        public ParticleSystemWrapper(Component component)
        {
            Component = component;

            // Initialize ParticleSystem structure if it doesn't exist
            if (!Component.Properties.ContainsKey("ParticleSystem") || Component.Properties["ParticleSystem"] == null)
            {
                var particleSystemDict = new Dictionary<string, object>
                {
                    ["Settings"] = new Dictionary<string, object>(),
                    ["BoundingShape"] = "null",
                    ["Emitters"] = new Dictionary<string, object>()
                };
                Component.Properties["ParticleSystem"] = particleSystemDict;
            }

            // Initialize Control if it doesn't exist
            if (!Component.Properties.ContainsKey("Control"))
            {
                Component.Properties["Control"] = new Dictionary<string, object>
                {
                    ["ResetSeconds"] = 5.0f,
                    ["Control"] = "Play"
                };
            }

            // Initialize Color if it doesn't exist
            if (!Component.Properties.ContainsKey("Color"))
            {
                Component.Properties["Color"] = new Dictionary<string, object>
                {
                    ["R"] = 1.0f,
                    ["G"] = 1.0f,
                    ["B"] = 1.0f,
                    ["A"] = 1.0f
                };
            }

            // Initialize Speed if it doesn't exist
            if (!Component.Properties.ContainsKey("Speed"))
            {
                Component.Properties["Speed"] = 1.0f;
            }
        }

        /// <summary>
        /// Creates a new ParticleSystemComponent with default values
        /// </summary>
        public static Component CreateComponent()
        {
            var particleSystemDict = new Dictionary<string, object>
            {
                ["Settings"] = new Dictionary<string, object>(),
                ["BoundingShape"] = "null",
                ["Emitters"] = new Dictionary<string, object>()
            };

            return new Component
            {
                Type = "ParticleSystemComponent",
                Id = GuidHelper.NewGuid(),
                Properties = new Dictionary<string, object>
                {
                    ["Control"] = new Dictionary<string, object>
                    {
                        ["ResetSeconds"] = 5.0f,
                        ["Control"] = "Play"
                    },
                    ["Color"] = new Dictionary<string, object>
                    {
                        ["R"] = 1.0f,
                        ["G"] = 1.0f,
                        ["B"] = 1.0f,
                        ["A"] = 1.0f
                    },
                    ["Speed"] = 1.0f,
                    ["ParticleSystem"] = particleSystemDict
                }
            };
        }

        /// <summary>
        /// Playback control (Play, Pause, or Stop)
        /// </summary>
        public string Control
        {
            get => Component.Get<string>("Control.Control") ?? "Play";
            set => Component.Set("Control.Control", value);
        }

        /// <summary>
        /// Reset time in seconds before looping
        /// </summary>
        public float ResetSeconds
        {
            get => Component.Get<float?>("Control.ResetSeconds") ?? 5.0f;
            set => Component.Set("Control.ResetSeconds", value);
        }

        /// <summary>
        /// Overall tint color for the particle system
        /// </summary>
        public Dictionary<string, object> Color
        {
            get => Component.GetMultiValueProperty("Color") ?? new Dictionary<string, object>
            {
                ["R"] = 1.0f,
                ["G"] = 1.0f,
                ["B"] = 1.0f,
                ["A"] = 1.0f
            };
            set => Component.Set("Color", value);
        }

        /// <summary>
        /// Speed multiplier for particle simulation (time scale)
        /// </summary>
        public float Speed
        {
            get => Component.Get<float?>("Speed") ?? 1.0f;
            set => Component.Set("Speed", value);
        }

        /// <summary>
        /// Internal access to the emitters dictionary
        /// </summary>
        private Dictionary<string, object> Emitters
        {
            get
            {
                var particleSystem = Component.GetMultiValueProperty("ParticleSystem");
                if (particleSystem != null && particleSystem.TryGetValue("Emitters", out var emittersObj))
                {
                    if (emittersObj is Dictionary<string, object> dict)
                        return dict;
                }
                return new Dictionary<string, object>();
            }
            set
            {
                var particleSystem = Component.GetMultiValueProperty("ParticleSystem");
                if (particleSystem != null)
                {
                    particleSystem["Emitters"] = value;
                    Component.Set("ParticleSystem", particleSystem);
                }
            }
        }

        /// <summary>
        /// Adds an emitter to this particle system
        /// </summary>
        /// <param name="emitter">The emitter data to add</param>
        public void AddEmitter(VFXEmitter emitter)
        {
            var emitters = Emitters;
            emitters[emitter.Key] = emitter.GetEmitterData();
            Emitters = emitters;
        }

        /// <summary>
        /// Gets all emitters in this particle system
        /// </summary>
        public List<VFXEmitter> GetEmitters()
        {
            var emitters = Emitters;
            var result = new List<VFXEmitter>();

            foreach (var kvp in emitters)
            {
                if (kvp.Value is Dictionary<string, object> emitterData)
                {
                    // Only count as an emitter if it has emitter-specific properties
                    // Check for ParticleLifetime which every emitter must have
                    if (emitterData.ContainsKey("ParticleLifetime") ||
                        emitterData.ContainsKey("EmitterName") ||
                        emitterData.ContainsKey("ShapeBuilder"))
                    {
                        result.Add(VFXEmitter.FromData(kvp.Key, emitterData));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Removes an emitter by its key
        /// </summary>
        public bool RemoveEmitter(string emitterKey)
        {
            var emitters = Emitters;
            var removed = emitters.Remove(emitterKey);
            if (removed)
            {
                Emitters = emitters;
            }
            return removed;
        }

        /// <summary>
        /// Sets the overall color tint
        /// </summary>
        public void SetColor(float r, float g, float b, float a = 1.0f)
        {
            Color = new Dictionary<string, object>
            {
                ["R"] = r,
                ["G"] = g,
                ["B"] = b,
                ["A"] = a
            };
        }
    }
}
