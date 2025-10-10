using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HS.Stride.Editor.Toolkit.Core.PrefabEditing;
using HS.Stride.Editor.Toolkit.Core;

namespace HS.Stride.Editor.Toolkit.Core.StrideYamlParser
{
    /// <summary>
    /// Handles both parsing AND writing of Stride prefab (.sdprefab) YAML files.
    /// Prefabs have the same structure as scenes (Hierarchy with RootParts and Parts).
    /// </summary>
    public class StrideYamlPrefab : StrideYaml
    {
        #region YAML Writing

        /// <summary>
        /// Generates complete prefab YAML from PrefabContent.
        /// </summary>
        public static string GeneratePrefabYaml(PrefabContent prefabContent)
        {
            var sb = new StringBuilder();

            // Write prefab header
            WritePrefabHeader(sb, prefabContent);

            // Write hierarchy (RootParts and Parts) - same as scenes
            WriteHierarchy(sb, prefabContent);

            return sb.ToString();
        }

        /// <summary>
        /// Writes the prefab header: type tag, ID, version, tags.
        /// Similar to scene but uses !PrefabAsset and no Offset or ChildrenIds.
        /// </summary>
        private static void WritePrefabHeader(StringBuilder sb, PrefabContent prefabContent)
        {
            sb.AppendLine("!PrefabAsset");
            sb.AppendLine($"Id: {prefabContent.Id}");
            sb.AppendLine("SerializedVersion: {Stride: 3.1.0.1}");
            sb.AppendLine("Tags: []");
        }

        /// <summary>
        /// Writes the Hierarchy section containing RootParts and Parts.
        /// Identical structure to scenes.
        /// </summary>
        private static void WriteHierarchy(StringBuilder sb, PrefabContent prefabContent)
        {
            sb.AppendLine("Hierarchy:");

            // Write RootParts
            WriteRootParts(sb, prefabContent);

            // Write Parts
            WriteParts(sb, prefabContent);
        }

        /// <summary>
        /// Writes RootParts section (entity references at root level).
        /// </summary>
        private static void WriteRootParts(StringBuilder sb, PrefabContent prefabContent)
        {
            sb.AppendLine("    RootParts:");

            foreach (var rootId in prefabContent.RootEntityIds)
            {
                sb.AppendLine($"        - ref!! {rootId}");
            }
        }

        /// <summary>
        /// Writes Parts section (all entity definitions).
        /// Delegates to StrideYamlEntity for entity content.
        /// </summary>
        private static void WriteParts(StringBuilder sb, PrefabContent prefabContent)
        {
            sb.AppendLine("    Parts:");

            foreach (var entity in prefabContent.Entities)
            {
                // Write Folder BEFORE Entity if specified (at same indentation level as "-   Entity:")
                if (!string.IsNullOrEmpty(entity.Folder))
                {
                    sb.AppendLine($"        -   Folder: {entity.Folder}");
                    sb.AppendLine("            Entity:");
                }
                else
                {
                    // Entity marker
                    sb.AppendLine("        -   Entity:");
                }

                // Delegate entity content generation to StrideYamlEntity
                string entityYaml = StrideYamlEntity.GenerateEntityYaml(entity, 4);
                sb.Append(entityYaml);

                // Note: Prefabs typically don't have Base sections since they ARE the base
                // But if they do (nested prefabs), write them
                if (entity.ParentPrefab != null)
                {
                    WriteBasePrefabInfo(sb, entity);
                }
            }
        }

        /// <summary>
        /// Writes the Base section for nested prefab instances.
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
        /// Parses a prefab file and returns PrefabContent with entities.
        /// </summary>
        public static PrefabContent ParsePrefab(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Prefab file not found: {filePath}");

            var content = File.ReadAllText(filePath);
            return ParsePrefabContent(content, filePath);
        }

        /// <summary>
        /// Parses prefab YAML content and returns PrefabContent.
        /// Prefabs have the same structure as scenes, so we can reuse the scene parsing logic.
        /// </summary>
        public static PrefabContent ParsePrefabContent(string content, string filePath)
        {
            var prefab = new PrefabContent
            {
                FilePath = filePath,
                RawContent = content
            };

            var lines = content.Split('\n');

            // Parse prefab ID
            var idLine = lines.FirstOrDefault(l => l.StartsWith("Id: "));
            if (idLine != null)
            {
                prefab.Id = idLine.Substring(4).Trim();
            }

            // Parse hierarchy root parts (same as scenes)
            prefab.RootEntityIds = ParseRootParts(content);

            // Parse all entities (same as scenes)
            prefab.Entities = ParseEntities(content);

            // Link entities to parent prefab for lazy loading
            foreach (var entity in prefab.Entities)
            {
                entity.ParentScene = prefab; // PrefabContent inherits from SceneContent
            }

            return prefab;
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
            // Prefabs use the same entity structure as scenes, so we can delegate to StrideYamlScene
            // Or duplicate the logic here. For now, let's use the scene parser's entity parsing
            var lines = content.Split('\n');
            var entities = new List<Entity>();
            Entity? currentEntity = null;
            bool inComponentsSection = false;

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                // Entity start
                if (line.Contains("Entity:") && !line.Contains("BasePartAsset"))
                {
                    if (currentEntity != null)
                        entities.Add(currentEntity);

                    currentEntity = new Entity();
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

                // Entity ID
                if (line.Trim().StartsWith("Id: ") && !inComponentsSection)
                {
                    currentEntity.Id = line.Substring(line.IndexOf("Id: ") + 4).Trim();
                }

                // Entity Name
                if (line.Trim().StartsWith("Name: ") && !inComponentsSection)
                {
                    currentEntity.Name = line.Substring(line.IndexOf("Name: ") + 6).Trim();
                }

                // Base (nested prefab instances)
                if (line.Contains("Base:"))
                {
                    currentEntity.ParentPrefab = new DataTypes.PrefabData();
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

        #endregion
    }
}
