// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using HS.Stride.Editor.Toolkit.Utilities;

namespace HS.Stride.Editor.Toolkit.Core.ScriptEditing
{
    /// <summary>
    /// Converts ScriptInfo metadata into a Component object ready to be added to an entity
    /// </summary>
    public static class ScriptToComponent
    {
        /// <summary>
        /// Creates a Component from ScriptInfo with proper namespace and initialized properties
        /// </summary>
        /// <param name="scriptInfo">The script metadata</param>
        /// <returns>A Component ready to add to an entity</returns>
        public static Component Create(ScriptInfo scriptInfo)
        {
            if (scriptInfo == null)
                throw new ArgumentNullException(nameof(scriptInfo));

            var component = new Component
            {
                Key = GuidHelper.NewGuidNoDashes(),
                Type = scriptInfo.GetFullTypeName(), // e.g., "TopDownRPG.Tester,TopDownRPG.Game"
                Id = GuidHelper.NewGuid(),
                Properties = new Dictionary<string, object>()
            };

            // Initialize properties with default values based on type
            foreach (var member in scriptInfo.PublicMembers)
            {
                var propertyName = member.Key;
                var typeName = member.Value;

                component.Properties[propertyName] = GetDefaultValue(typeName);
            }

            return component;
        }

        private static object GetDefaultValue(string typeName)
        {
            // Handle common C# types and return appropriate defaults based on actual Stride YAML format
            var lowerType = typeName.ToLower();

            // Primitives (serialize as simple values)
            if (lowerType == "int" || lowerType == "int32")
                return 0;
            if (lowerType == "float" || lowerType == "single")
                return 0.0f;
            if (lowerType == "double")
                return 0.0;
            if (lowerType == "bool" || lowerType == "boolean")
                return false;
            if (lowerType == "string")
                return "null";  // Stride uses literal "null" for empty strings

            // Stride math types (serialize as inline dictionaries)
            if (lowerType == "vector3")
                return new Dictionary<string, object> { ["X"] = 0.0f, ["Y"] = 0.0f, ["Z"] = 0.0f };
            if (lowerType == "vector2")
                return new Dictionary<string, object> { ["X"] = 0.0f, ["Y"] = 0.0f };
            if (lowerType == "color" || lowerType == "color4")
                return new Dictionary<string, object> { ["R"] = 1.0f, ["G"] = 1.0f, ["B"] = 1.0f, ["A"] = 1.0f };
            if (lowerType == "quaternion")
                return new Dictionary<string, object> { ["X"] = 0.0f, ["Y"] = 0.0f, ["Z"] = 0.0f, ["W"] = 1.0f };

            // Stride reference types (all serialize as null initially)
            if (typeName == "Entity" || typeName.Contains("Entity[]") || typeName.Contains("List<Entity>"))
                return "null";
            if (typeName == "Prefab" || typeName.Contains("Prefab[]") || typeName.Contains("List<Prefab>"))
                return "null";
            if (typeName == "Model" || typeName.Contains("Model[]") || typeName.Contains("List<Model>"))
                return "null";
            if (typeName == "Material" || typeName.Contains("Material[]") || typeName.Contains("List<Material>"))
                return "null";
            if (typeName == "RawAsset" || typeName.Contains("RawAsset[]") || typeName.Contains("List<RawAsset>"))
                return "null";
            if (typeName.Contains("Component") || typeName.Contains("AnimationClip"))
                return "null";

            // Collections (serialize as null if empty, user must populate later)
            if (typeName.Contains("List<") || typeName.Contains("[]") || typeName.Contains("Dictionary<"))
                return "null";

            // Unknown types default to null
            return "null";
        }
    }
}
