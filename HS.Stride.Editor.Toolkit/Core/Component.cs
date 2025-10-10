// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using HS.Stride.Editor.Toolkit.Core.AssetEditing;
using HS.Stride.Editor.Toolkit.Core.DataTypes;

namespace HS.Stride.Editor.Toolkit.Core
{
    /// <summary>
    /// Represents a component with its type, ID, and property data.
    /// </summary>
    public class Component
    {
        /// <summary>
        /// The GUID key for this component (used in YAML).
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// The full type name of the component (e.g., "Namespace.ClassName,AssemblyName").
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// The unique identifier for this component instance.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Raw YAML content for this component (used for surgical edits).
        /// </summary>
        public string RawContent { get; set; } = string.Empty;

        /// <summary>
        /// Dictionary of property names and their values.
        /// </summary>
        public Dictionary<string, object> Properties { get; set; } = new();

        /// <summary>
        /// Reference to the parent entity (used for strict mode validation).
        /// </summary>
        public Entity? ParentEntity { get; set; }
        

        /// <summary>
        /// Gets a property value by name. Supports nested paths with dot notation (e.g., "Position.X").
        /// Only properties saved in the .sdscene file (visible in Stride's Property Grid) can be accessed.
        /// </summary>
        /// <typeparam name="T">The type to cast the property value to</typeparam>
        /// <param name="propertyName">The property name (e.g., "health", "Position.X")</param>
        /// <returns>The property value cast to T, or default(T) if not found</returns>
        public T? Get<T>(string propertyName)
        {
            var parts = propertyName.Split('.');
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

            // Parse reference types from string
            if (typeof(T) == typeof(EntityRefData) && current is string entityRefStr)
            {
                return (T?)(object?)EntityRefData.Parse(entityRefStr);
            }

            if (typeof(T) == typeof(AssetRefData) && current is string assetRefStr)
            {
                return (T?)(object?)AssetRefData.Parse(assetRefStr);
            }

            // Try conversion
            try
            {
                return (T)Convert.ChangeType(current, typeof(T));
            }
            catch (InvalidCastException)
            {
                return default;
            }
            catch (FormatException)
            {
                return default;
            }
        }

        /// <summary>
        /// Sets a property value by name. Supports nested paths with dot notation (e.g., "Position.X").
        /// Only properties that Stride serializes will persist when saved.
        /// In strict mode, throws if property doesn't exist in the component's script.
        /// </summary>
        /// <param name="propertyName">The property name (e.g., "health", "Position.X")</param>
        /// <param name="propertyValue">The value to set</param>
        /// <exception cref="InvalidOperationException">Thrown in strict mode if property doesn't exist or type mismatch</exception>
        public void Set(string propertyName, object propertyValue)
        {
            var parts = propertyName.Split('.');

            // Strict mode validation - check if property exists and type matches
            if (ParentEntity?.ParentProject?.Mode == ProjectMode.Strict && parts.Length == 1)
            {
                ValidatePropertyExists(propertyName);
                ValidatePropertyType(propertyName, propertyValue);
            }

            // For simple (non-nested) properties, do surgical replacement in RawContent
            if (parts.Length == 1 && !string.IsNullOrEmpty(RawContent))
            {
                RawContent = ReplaceSinglePropertyInYaml(RawContent, propertyName, propertyValue);
                Properties[parts[0]] = propertyValue;
                return;
            }

            // For nested properties, fall back to dictionary manipulation
            // (These are typically complex structures that need full rewrite anyway)
            if (parts.Length == 1)
            {
                Properties[parts[0]] = propertyValue;
                return;
            }

            Dictionary<string, object> current = Properties;

            for (int i = 0; i < parts.Length - 1; i++)
            {
                if (!current.ContainsKey(parts[i]) || current[parts[i]] is not Dictionary<string, object>)
                {
                    current[parts[i]] = new Dictionary<string, object>();
                }
                current = (Dictionary<string, object>)current[parts[i]];
            }

            current[parts[^1]] = propertyValue;
        }

        /// <summary>
        /// Surgically replaces a single property value in YAML content without touching anything else
        /// </summary>
        private string ReplaceSinglePropertyInYaml(string yamlContent, string propertyName, object propertyValue)
        {
            var lines = yamlContent.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var trimmed = line.Trim();

                // Look for "propertyName: value" or "propertyName*: value"
                if (trimmed.StartsWith(propertyName + ":") || trimmed.StartsWith(propertyName + "*:"))
                {
                    var colonIndex = trimmed.IndexOf(':');
                    if (colonIndex >= 0)
                    {
                        // Get the indentation from the original line
                        var indent = line.Substring(0, line.Length - trimmed.Length);

                        // Format the new value
                        var formattedValue = FormatValueForYaml(propertyValue);

                        // Preserve the asterisk if it exists
                        var propertyKey = trimmed.Substring(0, colonIndex);

                        // Replace the line
                        lines[i] = $"{indent}{propertyKey}: {formattedValue}";
                        break;
                    }
                }
            }

            return string.Join("\n", lines);
        }

        private string FormatValueForYaml(object value)
        {
            return value switch
            {
                float f => f.ToString("0.0#######", System.Globalization.CultureInfo.InvariantCulture),
                double d => d.ToString("0.0#######", System.Globalization.CultureInfo.InvariantCulture),
                bool b => b.ToString().ToLower(),
                _ => value.ToString() ?? "null"
            };
        }

        /// <summary>
        /// Gets a property that contains multiple fields (e.g., Vector3 with X,Y,Z or Color with R,G,B,A).
        /// Returns the property as a dictionary, or null if not found or not a multi-field type.
        /// </summary>
        public Dictionary<string, object>? GetMultiValueProperty(string propertyName)
        {
            if (Properties.ContainsKey(propertyName) && Properties[propertyName] is Dictionary<string, object> dict)
                return dict;
            return null;
        }

        /// <summary>
        /// Sets a property that contains multiple fields (e.g., Vector3 with X,Y,Z or Color with R,G,B,A).
        /// </summary>
        public void SetMultiValueProperty(string propertyName, Dictionary<string, object> value)
        {
            Properties[propertyName] = value;
        }

        /// <summary>
        /// Sets an Entity reference property. Automatically formats as "ref!! {entity-guid}".
        /// </summary>
        /// <param name="propertyName">The property name (e.g., "targetEntity")</param>
        /// <param name="entity">The entity to reference</param>
        /// <exception cref="ArgumentNullException">Thrown if entity is null</exception>
        public void SetEntityRef(string propertyName, Entity entity)
        {
            Set(propertyName, $"ref!! {entity.Id}");
        }

        /// <summary>
        /// Sets an Asset reference property (Prefab, Model, Material, Texture, RawAsset, etc.).
        /// Automatically formats as "guid:path".
        /// </summary>
        /// <param name="propertyName">The property name (e.g., "prefabRef", "RawDatabase")</param>
        /// <param name="asset">The asset reference to set</param>
        /// <exception cref="ArgumentNullException">Thrown if asset is null</exception>
        public void SetAssetRef(string propertyName, AssetReference asset)
        {
            Set(propertyName, asset.Reference);
        }

        /// <summary>
        /// Adds an item to a list/array property. Creates GUID-keyed dictionary entry.
        /// </summary>
        /// <param name="propertyName">The list property name</param>
        /// <param name="value">The value to add</param>
        /// <exception cref="InvalidOperationException">Thrown if property is not a list/array</exception>
        public void AddToList(string propertyName, object value)
        {
            // Get or create the list dictionary
            if (!Properties.ContainsKey(propertyName) || Properties[propertyName] is string str && str == "null")
            {
                Properties[propertyName] = new Dictionary<string, object>();
            }

            if (Properties[propertyName] is not Dictionary<string, object> listDict)
            {
                throw new InvalidOperationException($"Property '{propertyName}' is not a list/array");
            }

            // Add item with new GUID key
            var itemGuid = Utilities.GuidHelper.NewGuid();
            listDict[itemGuid] = value;
        }

        /// <summary>
        /// Sets a dictionary property value. Creates GUID~key entry.
        /// </summary>
        /// <param name="propertyName">The dictionary property name</param>
        /// <param name="key">The dictionary key</param>
        /// <param name="value">The value to set</param>
        /// <exception cref="InvalidOperationException">Thrown if property is not a dictionary</exception>
        public void SetDictionary(string propertyName, object key, object value)
        {
            // Get or create the dictionary
            if (!Properties.ContainsKey(propertyName) || Properties[propertyName] is string str && str == "null")
            {
                Properties[propertyName] = new Dictionary<string, object>();
            }

            if (Properties[propertyName] is not Dictionary<string, object> dict)
            {
                throw new InvalidOperationException($"Property '{propertyName}' is not a dictionary");
            }

            // Format: guid~key
            var itemGuid = Utilities.GuidHelper.NewGuid();
            var guidKey = $"{itemGuid}~{key}";
            dict[guidKey] = value;
        }

        /// <summary>
        /// Replaces entire list/array with new values. Creates GUID-keyed dictionary entries.
        /// </summary>
        /// <param name="propertyName">The list property name</param>
        /// <param name="values">The values to set</param>
        /// <exception cref="ArgumentNullException">Thrown if values is null</exception>
        public void SetList(string propertyName, IEnumerable<object> values)
        {
            var listDict = new Dictionary<string, object>();

            foreach (var value in values)
            {
                var itemGuid = Utilities.GuidHelper.NewGuid();
                listDict[itemGuid] = value;
            }

            Properties[propertyName] = listDict;
        }

        /// <summary>
        /// Validates that a property exists and the value type matches (strict mode only)
        /// </summary>
        private void ValidatePropertyExists(string propertyName)
        {
            // For custom script components, check if property exists in script and validate type
            if (ParentEntity?.ParentProject != null)
            {
                // Extract class name from Type (e.g., "TestNamespace.SimpleScript,Assembly" -> "SimpleScript")
                var typePart = Type.Split(',')[0].TrimStart('.');  // Remove leading dot if present
                var className = typePart.Contains('.') ? typePart.Split('.').Last() : typePart;

                var scriptInfo = ScriptEditing.ScriptScanner.FindScript(ParentEntity.ParentProject, className);
                if (scriptInfo != null)
                {
                    if (!scriptInfo.PublicMembers.ContainsKey(propertyName))
                    {
                        throw new InvalidOperationException(
                            $"Property '{propertyName}' does not exist in script '{scriptInfo.ClassName}'. " +
                            $"Available properties: {string.Join(", ", scriptInfo.PublicMembers.Keys)}");
                    }
                }
            }
        }

        /// <summary>
        /// Validates that the value type matches the property type (strict mode only)
        /// </summary>
        private void ValidatePropertyType(string propertyName, object propertyValue)
        {
            if (ParentEntity?.ParentProject == null)
                return;

            // Extract class name from Type
            var typePart = Type.Split(',')[0].TrimStart('.');
            var className = typePart.Contains('.') ? typePart.Split('.').Last() : typePart;

            var scriptInfo = ScriptEditing.ScriptScanner.FindScript(ParentEntity.ParentProject, className);
            if (scriptInfo == null || !scriptInfo.PublicMembers.ContainsKey(propertyName))
                return; // Skip validation if script not found or property not in script

            var expectedType = scriptInfo.PublicMembers[propertyName];
            var actualType = propertyValue?.GetType();

            // Validate type match
            if (!IsTypeMatch(expectedType, actualType))
            {
                throw new InvalidOperationException(
                    $"Type mismatch for property '{propertyName}' in script '{scriptInfo.ClassName}'. " +
                    $"Expected: {expectedType}, Got: {actualType?.Name ?? "null"}. " +
                    $"Example: use 'component.Set(\"{propertyName}\", {GetExampleValue(expectedType)})' instead.");
            }
        }

        /// <summary>
        /// Checks if the actual .NET type matches the expected C# type string
        /// </summary>
        private bool IsTypeMatch(string expectedType, Type? actualType)
        {
            if (actualType == null)
                return expectedType.ToLower() == "string" || expectedType.Contains("?"); // null is valid for strings and nullables

            var lowerExpected = expectedType.ToLower();

            // Integer types
            if (lowerExpected == "int" || lowerExpected == "int32")
                return actualType == typeof(int);

            // Float types
            if (lowerExpected == "float" || lowerExpected == "single")
                return actualType == typeof(float);

            // Double types
            if (lowerExpected == "double")
                return actualType == typeof(double);

            // Bool types
            if (lowerExpected == "bool" || lowerExpected == "boolean")
                return actualType == typeof(bool);

            // String types (also accept null)
            if (lowerExpected == "string")
                return actualType == typeof(string) || actualType == typeof(Nullable<>);

            // Reference types and complex types - accept string representations
            if (expectedType == "Entity" || expectedType.Contains("Prefab") || expectedType.Contains("Model") ||
                expectedType.Contains("Material") || expectedType.Contains("Texture") || expectedType.Contains("RawAsset"))
                return actualType == typeof(string); // References stored as strings in YAML

            // Vector and math types - accept dictionaries
            if (lowerExpected == "vector2" || lowerExpected == "vector3" || lowerExpected == "quaternion" ||
                lowerExpected == "color" || lowerExpected == "color4")
                return actualType == typeof(Dictionary<string, object>);

            // Collections - accept dictionaries or null string
            if (expectedType.Contains("List<") || expectedType.Contains("[]") || expectedType.Contains("Dictionary<"))
                return actualType == typeof(Dictionary<string, object>) || actualType == typeof(string);

            // Unknown types - allow anything
            return true;
        }

        /// <summary>
        /// Provides an example value for error messages
        /// </summary>
        private string GetExampleValue(string typeName)
        {
            var lower = typeName.ToLower();

            if (lower == "int" || lower == "int32")
                return "100";
            if (lower == "float" || lower == "single")
                return "100f";
            if (lower == "double")
                return "100.0";
            if (lower == "bool" || lower == "boolean")
                return "true";
            if (lower == "string")
                return "\"example\"";

            return "value";
        }
    }
}
