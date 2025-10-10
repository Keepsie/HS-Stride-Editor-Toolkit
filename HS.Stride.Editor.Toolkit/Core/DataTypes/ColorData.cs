// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

namespace HS.Stride.Editor.Toolkit.Core.DataTypes
{
    /// <summary>
    /// Represents an RGBA color with values from 0-1
    /// </summary>
    public class ColorData
    {
        public float R { get; set; } = 1.0f;
        public float G { get; set; } = 1.0f;
        public float B { get; set; } = 1.0f;
        public float A { get; set; } = 1.0f;

        public static ColorData White => new ColorData { R = 1, G = 1, B = 1, A = 1 };
        public static ColorData Black => new ColorData { R = 0, G = 0, B = 0, A = 1 };

        public Dictionary<string, object> ToMultiValueProperty()
        {
            return new Dictionary<string, object>
            {
                ["R"] = R,
                ["G"] = G,
                ["B"] = B,
                ["A"] = A
            };
        }

        public static ColorData FromMultiValueProperty(Dictionary<string, object>? dict)
        {
            if (dict == null) return White;
            return new ColorData
            {
                R = Convert.ToSingle(dict.GetValueOrDefault("R", 1.0f)),
                G = Convert.ToSingle(dict.GetValueOrDefault("G", 1.0f)),
                B = Convert.ToSingle(dict.GetValueOrDefault("B", 1.0f)),
                A = Convert.ToSingle(dict.GetValueOrDefault("A", 1.0f))
            };
        }
    }
}
