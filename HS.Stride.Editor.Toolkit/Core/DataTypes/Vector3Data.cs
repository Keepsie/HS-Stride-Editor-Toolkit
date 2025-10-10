// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

namespace HS.Stride.Editor.Toolkit.Core.DataTypes
{
    /// <summary>
    /// Represents a 3D vector with X, Y, Z components
    /// </summary>
    public class Vector3Data
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Vector3Data(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vector3Data Zero => new Vector3Data(0, 0, 0);
        public static Vector3Data One => new Vector3Data(1, 1, 1);

        public Dictionary<string, object> ToMultiValueProperty()
        {
            return new Dictionary<string, object>
            {
                ["X"] = X,
                ["Y"] = Y,
                ["Z"] = Z
            };
        }

        public static Vector3Data FromMultiValueProperty(Dictionary<string, object>? dict)
        {
            if (dict == null) return Zero;
            return new Vector3Data(
                Convert.ToSingle(dict.GetValueOrDefault("X", 0.0f)),
                Convert.ToSingle(dict.GetValueOrDefault("Y", 0.0f)),
                Convert.ToSingle(dict.GetValueOrDefault("Z", 0.0f))
            );
        }
    }
}
