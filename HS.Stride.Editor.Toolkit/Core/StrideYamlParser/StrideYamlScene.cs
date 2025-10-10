using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HS.Stride.Editor.Toolkit.Core.SceneEditing;
using HS.Stride.Editor.Toolkit.Core;
using HS.Stride.Editor.Toolkit.Core.DataTypes;

namespace HS.Stride.Editor.Toolkit.Core.StrideYamlParser
{
    /// <summary>
    /// Handles both parsing AND writing of Stride scene (.sdscene) YAML files.
    /// Consolidates all scene YAML operations in one place.
    /// </summary>
    public class StrideYamlScene : StrideYaml
    {
        #region YAML Writing

        /// <summary>
        /// Generates complete scene YAML from SceneContent.
        /// </summary>
        public static string GenerateSceneYaml(SceneContent sceneContent)
        {
            var sb = new StringBuilder();

            // Write scene header
            WriteSceneHeader(sb, sceneContent);

            // Write hierarchy (RootParts and Parts)
            WriteHierarchy(sb, sceneContent);

            return sb.ToString();
        }

        /// <summary>
        /// Writes the scene header: type tag, ID, version, tags, offset.
        /// </summary>
        private static void WriteSceneHeader(StringBuilder sb, SceneContent sceneContent)
        {
            sb.AppendLine("!SceneAsset");
            sb.AppendLine($"Id: {sceneContent.Id}");
            sb.AppendLine("SerializedVersion: {Stride: 3.1.0.1}");
            sb.AppendLine("Tags: []");
            sb.AppendLine("ChildrenIds: []");
            sb.AppendLine("Offset: {X: 0.0, Y: 0.0, Z: 0.0}");
        }

        /// <summary>
        /// Writes the Hierarchy section containing RootParts and Parts.
        /// </summary>
        private static void WriteHierarchy(StringBuilder sb, SceneContent sceneContent)
        {
            sb.AppendLine("Hierarchy:");

            // Write RootParts (list of root entity references)
            WriteRootParts(sb, sceneContent);

            // Write Parts (list of entity definitions)
            WriteParts(sb, sceneContent);
        }

        /// <summary>
        /// Writes RootParts section (entity references at root level).
        /// Format:
        ///     RootParts:
        ///         - ref!! guid1
        ///         - ref!! guid2
        /// </summary>
        private static void WriteRootParts(StringBuilder sb, SceneContent sceneContent)
        {
            sb.AppendLine("    RootParts:");

            foreach (var rootId in sceneContent.RootEntityIds)
            {
                sb.AppendLine($"        - ref!! {rootId}");
            }
        }

        /// <summary>
        /// Writes Parts section (all entity definitions).
        /// Each entity is prefixed with "-   Entity:" and delegates to StrideYamlEntity for content.
        /// Format:
        ///     Parts:
        ///         -   Entity:
        ///                 Id: guid
        ///                 Name: EntityName
        ///                 Components:
        ///                     ...
        /// </summary>
        private static void WriteParts(StringBuilder sb, SceneContent sceneContent)
        {
            sb.AppendLine("    Parts:");

            foreach (var entity in sceneContent.Entities)
            {
                // Write Folder BEFORE Entity if specified (at same indentation level as "-   Entity:")
                if (!string.IsNullOrEmpty(entity.Folder))
                {
                    sb.AppendLine($"        -   Folder: {entity.Folder}");
                    sb.AppendLine("            Entity:");
                }
                else
                {
                    // Entity marker with proper indentation
                    sb.AppendLine("        -   Entity:");
                }

                // Delegate entity content generation to StrideYamlEntity
                // Base indent is 16 spaces (4 levels: Hierarchy=4, Parts=8, Entity=12, Content=16)
                string entityYaml = StrideYamlEntity.GenerateEntityYaml(entity, 4);
                sb.Append(entityYaml);

                // Write Base section if entity is a prefab instance
                if (entity.ParentPrefab != null)
                {
                    WriteBasePrefabInfo(sb, entity);
                }
            }
        }

        /// <summary>
        /// Writes the Base section for prefab instances.
        /// Format:
        ///     Base:
        ///         BasePartAsset: guid:path
        ///         BasePartId: guid
        ///         InstanceId: guid
        /// </summary>
        private static void WriteBasePrefabInfo(StringBuilder sb, Entity entity)
        {
            var info = entity.ParentPrefab!;

            sb.AppendLine("            Base:");
            sb.AppendLine($"                BasePartAsset: {info.PrefabSourcePath}");
            sb.AppendLine($"                BasePartId: {info.PrefabEntityId}");
            sb.AppendLine($"                InstanceId: {info.InstanceId}");
        }

        #endregion

        #region YAML Parsing

        /// <summary>
        /// Parses a scene file and returns SceneContent with entities.
        /// </summary>
        public static SceneContent ParseScene(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Scene file not found: {filePath}");

            var content = File.ReadAllText(filePath);
            return ParseSceneContent(content, filePath);
        }

        /// <summary>
        /// Parses scene YAML content and returns SceneContent.
        /// </summary>
        public static SceneContent ParseSceneContent(string content, string filePath)
        {
            var scene = new SceneContent
            {
                FilePath = filePath,
                RawContent = content
            };

            var lines = content.Split('\n');

            // Parse scene ID
            var idLine = lines.FirstOrDefault(l => l.StartsWith("Id: "));
            if (idLine != null)
            {
                scene.Id = idLine.Substring(4).Trim();
            }

            // Parse hierarchy root parts
            scene.RootEntityIds = ParseRootParts(content);

            // Parse all entities
            scene.Entities = ParseEntities(content);

            // Link entities to parent scene for lazy loading
            foreach (var entity in scene.Entities)
            {
                entity.ParentScene = scene;
            }

            return scene;
        }

        private static List<string> ParseRootParts(string content)
        {
            var rootParts = new List<string>();
            var lines = content.Split('\n');
            bool inRootParts = false;

            foreach (var line in lines)
            {
                if (line.Contains("RootParts:"))
                {
                    inRootParts = true;
                    continue;
                }

                if (inRootParts)
                {
                    if (line.Contains("Parts:"))
                        break;

                    if (line.Contains("ref!!"))
                    {
                        var parts = line.Split("ref!!");
                        if (parts.Length > 1)
                        {
                            rootParts.Add(parts[1].Trim());
                        }
                    }
                }
            }

            return rootParts;
        }

        private static List<Entity> ParseEntities(string content)
        {
            var entities = new List<Entity>();
            var lines = content.Split('\n');
            Entity? currentEntity = null;
            Component? currentComponent = null;
            int componentStartIndent = 0;
            bool inComponentsSection = false;

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var indent = GetIndentLevel(line);

                // Entity start
                if (line.Contains("Entity:") && !line.Contains("BasePartAsset"))
                {
                    if (currentEntity != null)
                        entities.Add(currentEntity);

                    currentEntity = new Entity();
                    currentComponent = null;
                    inComponentsSection = false;
                    continue;
                }

                if (currentEntity == null) continue;

                // Entity Folder
                if (line.Trim().StartsWith("Folder: ") && !inComponentsSection)
                {
                    currentEntity.Folder = line.Substring(line.IndexOf("Folder: ") + 8).Trim();
                }

                // Components section marker
                if (line.Contains("Components:"))
                {
                    inComponentsSection = true;
                    continue;
                }

                // Entity ID - only capture if we're NOT in the components section yet
                if (line.Trim().StartsWith("Id: ") && currentComponent == null && !inComponentsSection)
                {
                    currentEntity.Id = line.Substring(line.IndexOf("Id: ") + 4).Trim();
                }

                // Entity Name
                if (line.Trim().StartsWith("Name: ") && !inComponentsSection)
                {
                    currentEntity.Name = line.Substring(line.IndexOf("Name: ") + 6).Trim();
                }

                // ALL components are lazy-loaded on demand - no upfront parsing
                // This ensures ParseComponentFromRaw is used which properly loads all properties

                // Base (prefab instances in scenes)
                if (line.Contains("Base:"))
                {
                    currentEntity.ParentPrefab = new PrefabData();
                    currentComponent = null;
                }

                if (currentEntity.ParentPrefab != null)
                {
                    if (line.Contains("BasePartAsset:"))
                        currentEntity.ParentPrefab.PrefabSourcePath = line.Split("BasePartAsset:")[1].Trim();
                    if (line.Contains("BasePartId:"))
                        currentEntity.ParentPrefab.PrefabEntityId = line.Split("BasePartId:")[1].Trim();
                    if (line.Contains("InstanceId:"))
                        currentEntity.ParentPrefab.InstanceId = line.Split("InstanceId:")[1].Trim();
                }
            }

            if (currentEntity != null)
                entities.Add(currentEntity);

            return entities;
        }

        private static int GetIndentLevel(string line)
        {
            int spaces = 0;
            foreach (char c in line)
            {
                if (c == ' ') spaces++;
                else if (c == '\t') spaces += 4;
                else break;
            }
            return spaces;
        }

        /// <summary>
        /// Parse a specific component from raw YAML by entity ID and component type.
        /// Only matches top-level component headers directly under the Components: section.
        /// </summary>
        public static Component? ParseComponentFromRaw(string rawContent, string entityId, string componentType)
        {
            var lines = rawContent.Split('\n');
            bool inTargetEntity = false;
            bool inComponents = false;
            int componentsIndent = -1;
            int headerIndent = -1;

            Component? targetComponent = null;
            int componentStartIndent = 0;

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var indent = GetIndentLevel(line);
                var trimmed = line.Trim();

                // Find the target entity - must be an Id line within an Entity block
                if (!inTargetEntity && trimmed.StartsWith("Id: ") && line.Contains(entityId))
                {
                    inTargetEntity = true;
                    continue;
                }

                if (!inTargetEntity) continue;

                // Exit entity if we hit another entity
                if (line.Contains("Entity:") && !line.Contains("BasePartAsset"))
                {
                    break;
                }

                // Enter/exit Components: section
                if (!inComponents && line.Contains("Components:"))
                {
                    inComponents = true;
                    componentsIndent = indent;
                    headerIndent = componentsIndent + 4;
                    continue;
                }

                if (inComponents)
                {
                    // Leaving Components section when indent reduces to or before the section's indent
                    if (!string.IsNullOrWhiteSpace(line) && indent <= componentsIndent)
                        break;

                    // Only match top-level component headers under Components:
                    if (indent == headerIndent && line.Contains(": !") && line.Contains(componentType))
                    {
                        var colonBangIndex = line.IndexOf(": !");
                        if (colonBangIndex >= 0)
                        {
                            var componentKey = line.Substring(0, colonBangIndex).Trim();

                            // Extract the FULL type tag (e.g., "TopDownRPG.Tester,TopDownRPG.Game")
                            var fullTypeTag = line.Substring(colonBangIndex + 3).Trim(); // Skip ": !"

                            targetComponent = new Component
                            {
                                Key = componentKey,
                                Type = fullTypeTag
                            };
                            componentStartIndent = indent;

                            // Build raw YAML block for this component
                            var rawComponentLines = new List<string> { line };

                            // Parse component properties  
                            for (int j = i + 1; j < lines.Length; j++)
                            {
                                var propLine = lines[j];
                                var propIndent = GetIndentLevel(propLine);

                                // Exit if indent decreased (left component scope)
                                if (propIndent <= componentStartIndent && !string.IsNullOrWhiteSpace(propLine))
                                    break;

                                // Add to raw YAML block
                                rawComponentLines.Add(propLine);

                                if (propIndent > componentStartIndent)
                                {
                                    // Component ID
                                    if (propLine.Trim().StartsWith("Id: "))
                                    {
                                        targetComponent.Id = propLine.Substring(propLine.IndexOf("Id: ") + 4).Trim();
                                        continue;
                                    }

                                    // Parse property
                                    var t2 = propLine.Trim();
                                    if (t2.Contains(":"))
                                    {
                                        // Support both "Key: value" and "Key:" (no space / multiline)
                                        var colonIndex = t2.IndexOf(':');
                                        var propName = t2.Substring(0, colonIndex);
                                        var propValue = t2.Substring(colonIndex + 1).TrimStart();

                                        if (propName.EndsWith("*"))
                                            propName = propName.TrimEnd('*');

                                        // Special handling for multi-line nested dictionaries (like ColliderShapes)
                                        if (string.IsNullOrWhiteSpace(propValue))
                                        {
                                            // This is a multi-line property, parse the nested structure
                                            var nestedDict = ParseNestedDictionary(lines, j, propIndent);
                                            if (nestedDict != null && nestedDict.Count > 0)
                                            {
                                                targetComponent.Properties[propName] = nestedDict;
                                            }

                                            // Skip ahead past the nested block we just parsed so we don't re-parse its lines as top-level properties
                                            int k = j + 1;
                                            while (k < lines.Length)
                                            {
                                                var nextLine = lines[k];
                                                if (string.IsNullOrWhiteSpace(nextLine)) { k++; continue; }

                                                var nextIndent = GetIndentLevel(nextLine);
                                                if (nextIndent <= propIndent) break;

                                                k++;
                                            }
                                            j = k - 1;
                                        }
                                        else if (propValue.StartsWith("!"))
                                        {
                                            // Inline YAML tag form for property: "PropName: !TypeTag"
                                            var nestedDict = ParseNestedDictionary(lines, j, propIndent);
                                            var withTag = new Dictionary<string, object>();
                                            withTag[propValue] = "";
                                            if (nestedDict != null && nestedDict.Count > 0)
                                            {
                                                foreach (var kvp2 in nestedDict)
                                                    withTag[kvp2.Key] = kvp2.Value;
                                            }
                                            targetComponent.Properties[propName] = withTag;
                                        }
                                        else if (propName.StartsWith("!") || (propValue.StartsWith("!") && propValue.Length < 50))
                                        {
                                            targetComponent.Properties[propName] = propValue;
                                        }
                                        else
                                        {
                                            targetComponent.Properties[propName] = ParsePropertyValue(propValue);
                                        }
                                    }
                                }
                            }

                            // Store the raw YAML block
                            targetComponent.RawContent = string.Join("\n", rawComponentLines);
                            break;
                        }
                    }
                }
            }

            return targetComponent;
        }

        private static Dictionary<string, object>? ParseNestedDictionary(string[] lines, int startIndex, int parentIndent)
        {
            var result = new Dictionary<string, object>();
            var childIndent = parentIndent + 4; // Children should be 4 spaces more indented
            
            int i = startIndex + 1;
            while (i < lines.Length)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line))
                {
                    i++;
                    continue;
                }
                
                var indent = GetIndentLevel(line);
                
                // If we're back at or before parent indent, we're done
                if (indent <= parentIndent)
                    break;
                
                // Only process direct children
                if (indent == childIndent)
                {
                    var trimmed = line.Trim();
                    if (!trimmed.Contains(":"))
                    {
                        i++;
                        continue;
                    }
                    
                    var colonIndex = trimmed.IndexOf(':');
                    var key = trimmed.Substring(0, colonIndex).Trim();
                    var value = trimmed.Substring(colonIndex + 1).Trim();
                    
                    // If value is empty, this is a multi-line nested structure
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        // Build the nested dictionary by reading ahead
                        var nestedDict = new Dictionary<string, object>();
                        int j = i + 1;
                        
                        while (j < lines.Length)
                        {
                            var nestedLine = lines[j];
                            if (string.IsNullOrWhiteSpace(nestedLine))
                            {
                                j++;
                                continue;
                            }
                            
                            var nestedIndent = GetIndentLevel(nestedLine);
                            
                            // Stop if we're back at the key's level or less
                            if (nestedIndent <= indent)
                                break;
                            
                            var nestedTrimmed = nestedLine.Trim();
                            
                            // Parse nested content
                            if (nestedTrimmed.StartsWith("!"))
                            {
                                // YAML type marker (e.g., !BoxColliderShapeDesc:)
                                var typeKey = nestedTrimmed.TrimEnd(':');
                                // Store marker as empty value, properties remain at the same level (as in YAML)
                                nestedDict[typeKey] = "";
                                
                                // Collect properties under this type marker at deeper indent
                                int k = j + 1;
                                while (k < lines.Length)
                                {
                                    var propLine = lines[k];
                                    if (string.IsNullOrWhiteSpace(propLine)) { k++; continue; }
                                    
                                    var propIndent = GetIndentLevel(propLine);
                                    if (propIndent <= nestedIndent) break;
                                    
                                    var propTrimmed = propLine.Trim();
                                    if (propTrimmed.Contains(":"))
                                    {
                                        var propColonIndex = propTrimmed.IndexOf(':');
                                        var propName = propTrimmed.Substring(0, propColonIndex).Trim();
                                        var propValue = propTrimmed.Substring(propColonIndex + 1).Trim();
                                        
                                        if (string.IsNullOrWhiteSpace(propValue))
                                        {
                                            // Another level of nesting under this property
                                            var deeperDict = ParseNestedDictionary(lines, k, propIndent);
                                            if (deeperDict != null && deeperDict.Count > 0)
                                                nestedDict[propName] = deeperDict;
                                        }
                                        else
                                        {
                                            nestedDict[propName] = ParsePropertyValue(propValue);
                                        }
                                    }
                                    
                                    k++;
                                }
                                
                                // Skip processed lines
                                j = k - 1;
                            }
                            else if (nestedTrimmed.Contains(":"))
                            {
                                var nestedColonIndex = nestedTrimmed.IndexOf(':');
                                var nestedKey = nestedTrimmed.Substring(0, nestedColonIndex).Trim();
                                var nestedValue = nestedTrimmed.Substring(nestedColonIndex + 1).Trim();
                                
                                if (string.IsNullOrWhiteSpace(nestedValue))
                                {
                                    // Another level of nesting - recursively parse
                                    var deeperDict = ParseNestedDictionary(lines, j, nestedIndent);
                                    if (deeperDict != null && deeperDict.Count > 0)
                                        nestedDict[nestedKey] = deeperDict;
                                }
                                else if (nestedValue.StartsWith("!"))
                                {
                                    // Inline YAML tag form: "key: !TypeTag"
                                    var tagDict = new Dictionary<string, object>();
                                    tagDict[nestedValue] = "";
                                    
                                    // Parse deeper properties under this key
                                    var deeperDict = ParseNestedDictionary(lines, j, nestedIndent);
                                    if (deeperDict != null && deeperDict.Count > 0)
                                    {
                                        foreach (var kvp2 in deeperDict)
                                            tagDict[kvp2.Key] = kvp2.Value;
                                    }

                                    nestedDict[nestedKey] = tagDict;

                                    // Skip ahead past nested block
                                    int k = j + 1;
                                    while (k < lines.Length)
                                    {
                                        var propLine = lines[k];
                                        if (string.IsNullOrWhiteSpace(propLine)) { k++; continue; }
                                        var propIndent = GetIndentLevel(propLine);
                                        if (propIndent <= nestedIndent) break;
                                        k++;
                                    }
                                    j = k - 1;
                                }
                                else
                                {
                                    // Simple value
                                    nestedDict[nestedKey] = ParsePropertyValue(nestedValue);
                                }
                            }
                            
                            j++;
                        }
                        
                        if (nestedDict.Count > 0)
                            result[key] = nestedDict;
                        
                        // Skip ahead past what we just parsed
                        i = j - 1;
                    }
                    else
                    {
                        // Simple key-value pair
                        result[key] = ParsePropertyValue(value);
                    }
                }
                
                i++;
            }
            
            return result.Count > 0 ? result : null;
        }

        private static object ParsePropertyValue(string value)
        {
            value = value.Trim();

            // Empty or null
            if (string.IsNullOrEmpty(value) || value == "null")
                return value;

            // Dictionary/Object (Vector3, Quaternion, etc)
            if (value.StartsWith("{") && value.EndsWith("}"))
            {
                var props = new Dictionary<string, object>();
                var content = value.Trim('{', '}').Trim();

                if (string.IsNullOrEmpty(content))
                    return props;

                var parts = content.Split(',');

                foreach (var part in parts)
                {
                    var colonIndex = part.IndexOf(':');
                    if (colonIndex > 0)
                    {
                        var key = part.Substring(0, colonIndex).Trim();
                        var val = part.Substring(colonIndex + 1).Trim();

                        // Try parse as number
                        if (float.TryParse(val, System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out float floatVal))
                        {
                            props[key] = floatVal;
                        }
                        else if (int.TryParse(val, out int intVal))
                        {
                            props[key] = intVal;
                        }
                        else if (bool.TryParse(val, out bool boolVal))
                        {
                            props[key] = boolVal;
                        }
                        else
                        {
                            props[key] = val;
                        }
                    }
                }
                return props;
            }

            // Asset reference (guid:path)
            if (value.Contains(":"))
            {
                var colonIndex = value.IndexOf(':');
                var beforeColon = value.Substring(0, colonIndex);
                if (Guid.TryParse(beforeColon, out _))
                {
                    return value; // Keep as string
                }
            }

            // Entity reference (ref!!)
            if (value.StartsWith("ref!!"))
            {
                return value;
            }

            // Boolean
            if (bool.TryParse(value, out bool bVal))
                return bVal;

            // Float/Double (including scientific notation like -4.371139E-08)
            if (float.TryParse(value, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out float fVal))
                return fVal;

            // Int
            if (int.TryParse(value, out int iVal))
                return iVal;

            // String/default
            return value;
        }

        #endregion
    }
}
