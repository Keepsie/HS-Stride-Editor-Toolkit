// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using HS.Stride.Editor.Toolkit.Core.SceneEditing;

namespace HS.Stride.Editor.Toolkit.Core.DataTypes
{
    /// <summary>
    /// Represents an Entity reference in Stride YAML format (ref!! guid)
    /// </summary>
    public class EntityRefData
    {
        public string Guid { get; set; } = string.Empty;

        /// <summary>
        /// Gets the YAML reference format: "ref!! {guid}"
        /// </summary>
        public string Reference => $"ref!! {Guid}";

        /// <summary>
        /// Parses a YAML entity reference string into an EntityRef object
        /// </summary>
        public static EntityRefData? Parse(string? value)
        {
            if (string.IsNullOrEmpty(value) || !value.StartsWith("ref!! "))
                return null;

            return new EntityRefData { Guid = value.Substring(6) };
        }

        /// <summary>
        /// Resolves this reference to the actual Entity in the scene
        /// </summary>
        public Entity? Resolve(Scene scene) => scene.FindEntityById(Guid);
    }
}
