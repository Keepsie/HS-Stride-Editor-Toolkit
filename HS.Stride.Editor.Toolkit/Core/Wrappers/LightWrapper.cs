// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using HS.Stride.Editor.Toolkit.Core.DataTypes;

namespace HS.Stride.Editor.Toolkit.Core.Wrappers
{
    /// <summary>
    /// Wrapper for LightComponent providing easy access to light properties
    /// </summary>
    public class LightWrapper
    {
        public Component Component { get; private set; }

        public LightWrapper(Component component)
        {
            Component = component;
        }

        /// <summary>
        /// Creates a new LightComponent with default values
        /// </summary>
        public static Component CreateComponent()
        {
            return new Component
            {
                Type = "LightComponent",
                Id = Utilities.GuidHelper.NewGuid(),
                Properties = new Dictionary<string, object>
                {
                    ["Intensity"] = 1.0f,
                    ["Color"] = new Dictionary<string, object>
                    {
                        ["R"] = 1.0f,
                        ["G"] = 1.0f,
                        ["B"] = 1.0f,
                        ["A"] = 1.0f
                    },
                    ["Type"] = new Dictionary<string, object>()
                }
            };
        }

        public float Intensity
        {
            get => Component.Get<float?>("Intensity") ?? 1.0f;
            set => Component.Set("Intensity", value);
        }

        /// <summary>
        /// Gets the light color
        /// </summary>
        public ColorData GetColor()
        {
            return ColorData.FromMultiValueProperty(Component.GetMultiValueProperty("Color"));
        }

        /// <summary>
        /// Sets the light color
        /// </summary>
        public void SetColor(float r, float g, float b, float a = 1.0f)
        {
            Component.Set("Color", new ColorData { R = r, G = g, B = b, A = a }.ToMultiValueProperty());
        }

        public Dictionary<string, object> Type
        {
            get => Component.GetMultiValueProperty("Type") ?? new Dictionary<string, object>();
            set => Component.Set("Type", value);
        }
    }
}
