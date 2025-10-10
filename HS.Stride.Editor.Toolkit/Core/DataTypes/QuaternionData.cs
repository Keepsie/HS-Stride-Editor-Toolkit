// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

namespace HS.Stride.Editor.Toolkit.Core.DataTypes
{
    /// <summary>
    /// Represents a rotation quaternion with X, Y, Z, W components
    /// </summary>
    public class QuaternionData
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; } = 1.0f;

        public QuaternionData() { W = 1.0f; }

        public QuaternionData(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public static QuaternionData Identity => new QuaternionData(0, 0, 0, 1);

        public Dictionary<string, object> ToMultiValueProperty()
        {
            return new Dictionary<string, object>
            {
                ["X"] = X,
                ["Y"] = Y,
                ["Z"] = Z,
                ["W"] = W
            };
        }

        public static QuaternionData FromMultiValueProperty(Dictionary<string, object>? dict)
        {
            if (dict == null) return Identity;
            return new QuaternionData(
                Convert.ToSingle(dict.GetValueOrDefault("X", 0.0f)),
                Convert.ToSingle(dict.GetValueOrDefault("Y", 0.0f)),
                Convert.ToSingle(dict.GetValueOrDefault("Z", 0.0f)),
                Convert.ToSingle(dict.GetValueOrDefault("W", 1.0f))
            );
        }
    }
}
