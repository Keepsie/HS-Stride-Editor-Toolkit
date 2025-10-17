// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using HS.Stride.Editor.Toolkit.Core.AssetEditing;
using HS.Stride.Editor.Toolkit.Core.DataTypes;
using HS.Stride.Editor.Toolkit.Core.PrefabEditing;
using HS.Stride.Editor.Toolkit.Utilities;

namespace HS.Stride.Editor.Toolkit.Core.SceneEditing
{
    /// <summary>
    /// Manages entity manipulation for SceneContent (used by both Scene and PrefabAsset).
    /// </summary>
    public class SceneManager
    {
        private readonly SceneContent _content;

        public SceneManager(SceneContent content)
        {
            _content = content;
        }

        public List<Entity> AllEntities => _content.Entities;

        // Find by ID
        public Entity? FindEntityById(string id)
        {
            return _content.Entities.FirstOrDefault(e => e.Id == id);
        }

        // Find by name (exact)
        public Entity? FindEntityByName(string name)
        {
            return _content.Entities.FirstOrDefault(e => e.Name == name);
        }

        // Find by name pattern (wildcard)
        public List<Entity> FindEntitiesByName(string pattern)
        {
            var regex = "^" + System.Text.RegularExpressions.Regex.Escape(pattern)
                .Replace("\\*", ".*")
                .Replace("\\?", ".") + "$";

            return _content.Entities
                .Where(e => System.Text.RegularExpressions.Regex.IsMatch(e.Name, regex))
                .ToList();
        }

        // Find by component type
        public List<Entity> FindEntitiesWithComponent(string componentType)
        {
            return _content.Entities
                .Where(e => e.HasComponent(componentType))
                .ToList();
        }

        // Find by predicate
        public List<Entity> FindEntities(Func<Entity, bool> predicate)
        {
            return _content.Entities.Where(predicate).ToList();
        }

        // Entity creation
        public Entity CreateEntity(string name, string? folder = null)
        {
            var entity = new Entity
            {
                Id = GuidHelper.NewGuid(),
                Name = name,
                Folder = folder,
                ParentProject = _content.ParentProject
            };

            // Add transform component by default
            var transformKey = GuidHelper.NewGuidNoDashes();
            entity.Components[transformKey] = new Component
            {
                Type = "TransformComponent",
                Id = GuidHelper.NewGuid(),
                Properties = new Dictionary<string, object>
                {
                    ["Position"] = Vector3Data.Zero.ToMultiValueProperty(),
                    ["Rotation"] = QuaternionData.Identity.ToMultiValueProperty(),
                    ["Scale"] = Vector3Data.One.ToMultiValueProperty(),
                    ["Children"] = new Dictionary<string, object>()
                }
            };

            _content.Entities.Add(entity);
            _content.RootEntityIds.Add(entity.Id);

            return entity;
        }

        /// <summary>
        /// Creates a new entity with specified parent (folder or entity).
        /// </summary>
        /// <param name="name">Entity name</param>
        /// <param name="parent">Parent name (folder name or parent entity name). Supports nested paths like "House/Floor/Room".</param>
        /// <param name="parentType">Type of parent (Folder or Entity)</param>
        public Entity CreateEntity(string name, string parent, ParentType parentType)
        {
            if (parentType == ParentType.Folder)
            {
                // Use folder-based organization (no transform hierarchy)
                return CreateEntity(name, parent);
            }
            else
            {
                // Use entity-based parenting (transform hierarchy)
                var entity = CreateEntity(name, folder: null);

                // Handle nested paths like "House/Floor/Room"
                var parentEntity = FindOrCreateParentHierarchy(parent);

                // Remove from root entities (it's now a child)
                _content.RootEntityIds.Remove(entity.Id);

                // Add to parent's Children
                var parentTransform = parentEntity.GetTransform();
                if (parentTransform == null)
                    throw new InvalidOperationException($"Parent entity '{parent}' has no TransformComponent.");

                var children = parentTransform.Component.GetMultiValueProperty("Children") ?? new Dictionary<string, object>();
                var childKey = GuidHelper.NewGuidNoDashes();
                var childTransform = entity.GetTransform();
                if (childTransform == null)
                    throw new InvalidOperationException("Created entity has no TransformComponent.");

                children[childKey] = $"ref!! {childTransform.Component.Id}";
                parentTransform.Component.Set("Children", children);

                return entity;
            }
        }

        /// <summary>
        /// Finds or creates a hierarchy of parent entities from a path like "House/Floor/Room".
        /// Creates empty entities as needed. Returns the final entity in the path.
        /// </summary>
        private Entity FindOrCreateParentHierarchy(string path)
        {
            var parts = path.Split('/');
            Entity? currentParent = null;

            foreach (var partName in parts)
            {
                var trimmedName = partName.Trim();
                if (string.IsNullOrEmpty(trimmedName))
                    continue;

                // Try to find existing entity
                Entity? found = null;
                if (currentParent == null)
                {
                    // Look in root entities
                    found = FindEntityByName(trimmedName);
                }
                else
                {
                    // Look in children of current parent
                    found = FindChildEntityByName(currentParent, trimmedName);
                }

                if (found != null)
                {
                    currentParent = found;
                }
                else
                {
                    // Create new empty entity
                    if (currentParent == null)
                    {
                        // Create as root entity
                        currentParent = CreateEntity(trimmedName);
                    }
                    else
                    {
                        // Create as child of current parent
                        var newEntity = CreateEntity(trimmedName, folder: null);

                        // Remove from root
                        _content.RootEntityIds.Remove(newEntity.Id);

                        // Add to parent's children
                        var parentTransform = currentParent.GetTransform();
                        if (parentTransform == null)
                            throw new InvalidOperationException($"Parent entity '{currentParent.Name}' has no TransformComponent.");

                        var children = parentTransform.Component.GetMultiValueProperty("Children") ?? new Dictionary<string, object>();
                        var childKey = GuidHelper.NewGuidNoDashes();
                        var childTransform = newEntity.GetTransform();
                        if (childTransform == null)
                            throw new InvalidOperationException("Created entity has no TransformComponent.");

                        children[childKey] = $"ref!! {childTransform.Component.Id}";
                        parentTransform.Component.Set("Children", children);

                        currentParent = newEntity;
                    }
                }
            }

            if (currentParent == null)
                throw new InvalidOperationException($"Failed to create parent hierarchy for path: {path}");

            return currentParent;
        }

        /// <summary>
        /// Finds a child entity by name within a parent's transform children.
        /// </summary>
        private Entity? FindChildEntityByName(Entity parent, string childName)
        {
            var transform = parent.GetTransform();
            if (transform == null)
                return null;

            var children = transform.Component.GetMultiValueProperty("Children") as Dictionary<string, object>;
            if (children == null)
                return null;

            // Children format: { "guidkey": "ref!! transformComponentId", ... }
            foreach (var childRef in children.Values)
            {
                if (childRef is string refStr && refStr.StartsWith("ref!! "))
                {
                    var transformId = refStr.Substring(6).Trim();

                    // Find entity with this transform component ID
                    var childEntity = _content.Entities.FirstOrDefault(e =>
                    {
                        var childTransform = e.GetTransform();
                        return childTransform?.Component.Id == transformId;
                    });

                    if (childEntity != null && childEntity.Name == childName)
                        return childEntity;
                }
            }

            return null;
        }

        /// <summary>
        /// Instantiate a prefab in the scene/prefab
        /// </summary>
        public Entity InstantiatePrefab(AssetReference prefab, Vector3Data? position = null, string? folder = null)
        {
            // Load the prefab file
            if (string.IsNullOrEmpty(prefab.FilePath) || !File.Exists(prefab.FilePath))
            {
                throw new FileNotFoundException($"Prefab file not found: {prefab.FilePath}");
            }

            var prefabAsset = Prefab.Load(prefab.FilePath);
            var rootEntity = prefabAsset.GetRootEntity();
            if (rootEntity == null)
            {
                throw new InvalidOperationException($"Prefab has no root entity: {prefab.Name}");
            }

            // IMPORTANT: Force-load all components for all entities before cloning
            // Components are lazy-loaded, so we need to trigger loading by calling GetComponent
            foreach (var prefabEntity in prefabAsset.AllEntities)
            {
                ForceLoadAllComponents(prefabEntity);
            }

            // Generate a shared instance ID for all entities in this prefab instance
            string sharedInstanceId = GuidHelper.NewGuid();

            // Map old entity and component IDs to new ones
            var entityIdMap = new Dictionary<string, string>();
            var componentIdMap = new Dictionary<string, string>();

            // First pass: Create new IDs for all entities and their components
            foreach (var prefabEntity in prefabAsset.AllEntities)
            {
                entityIdMap[prefabEntity.Id] = GuidHelper.NewGuid();
                foreach (var component in prefabEntity.Components.Values)
                {
                    componentIdMap[component.Id] = GuidHelper.NewGuid();
                }
            }
            
            // Second pass: Clone all entities with new IDs and update references
            Entity? newRootEntity = null;
            foreach (var prefabEntity in prefabAsset.AllEntities)
            {
                var newEntityId = entityIdMap[prefabEntity.Id];
                var isRoot = prefabEntity.Id == rootEntity.Id;

                var newEntity = new Entity
                {
                    Id = newEntityId,
                    Name = prefabEntity.Name,
                    Folder = null, // Prefab entities should never have folders - they're in a transform hierarchy under wrapper
                    ParentProject = _content.ParentProject,
                    ParentPrefab = new PrefabData
                    {
                        PrefabSourcePath = prefab.Reference,
                        PrefabEntityId = prefabEntity.Id,
                        InstanceId = sharedInstanceId
                    }
                };

                // Clone all components
                foreach (var componentKvp in prefabEntity.Components)
                {
                    var originalComponentKey = componentKvp.Key;  // Get the key from the dictionary
                    var originalComponent = componentKvp.Value;
                    var newComponentId = componentIdMap[originalComponent.Id];
                    
                    var newComponent = new Component
                    {
                        Type = originalComponent.Type,
                        Id = newComponentId,
                        Key = originalComponentKey, // Use the dictionary key
                        Properties = new Dictionary<string, object>(),
                        ParentEntity = newEntity
                    };

                    // Clone all properties, updating entity and component references
                    foreach (var propKvp in originalComponent.Properties)
                    {
                        newComponent.Properties[propKvp.Key] = ClonePropertyWithUpdatedReferences(propKvp.Value, entityIdMap, componentIdMap);
                    }

                    // Do not offset the prefab root transform here; position is applied to a wrapper root entity.

                    newEntity.Components[originalComponentKey] = newComponent;
                }
                
                _content.Entities.Add(newEntity);
                
                if (isRoot)
                {
                    newRootEntity = newEntity;
                    // Do not add prefab root as scene root; we'll create a wrapper entity as root instead.
                }
            }

            if (newRootEntity == null)
                throw new InvalidOperationException("Failed to instantiate prefab root entity.");

            // Create a wrapper/root entity to host the prefab instance
            var wrapper = CreateEntity(rootEntity.Name, folder);

            // Apply requested position to wrapper's transform (if any)
            var wrapperTransform = wrapper.GetTransform();
            if (wrapperTransform != null && position != null)
            {
                wrapperTransform.Component.Set("Position", position.ToMultiValueProperty());
            }

            // Attach prefab root transform as child of wrapper transform
            var prefabRootTransform = newRootEntity.GetTransform();
            if (wrapperTransform != null && prefabRootTransform != null)
            {
                var children = wrapperTransform.Component.GetMultiValueProperty("Children") ?? new Dictionary<string, object>();
                var childKey = GuidHelper.NewGuidNoDashes();
                children[childKey] = $"ref!! {prefabRootTransform.Component.Id}";
                wrapperTransform.Component.Set("Children", children);
            }

            // Ensure only the wrapper is a scene root (CreateEntity already added it).
            return wrapper;
        }

        /// <summary>
        /// Force-load all components for an entity by parsing component keys from raw YAML
        /// Only loads top-level components directly under the Components: section (ignores nested tags).
        /// </summary>
        private void ForceLoadAllComponents(Entity entity)
        {
            if (entity.ParentScene == null || string.IsNullOrEmpty(entity.ParentScene.RawContent))
                return;

            var lines = entity.ParentScene.RawContent.Split('\n');
            bool inTargetEntity = false;
            bool inComponents = false;
            int componentsIndent = -1;
            int componentHeaderIndent = -1;

            foreach (var line in lines)
            {
                // Find the target entity
                if (!inTargetEntity && line.Trim().StartsWith("Id: ") && line.Contains(entity.Id))
                {
                    inTargetEntity = true;
                    continue;
                }

                if (!inTargetEntity) continue;

                // Exit if we hit another entity
                if (line.Contains("Entity:") && !line.Contains("BasePartAsset"))
                {
                    break;
                }

                var indent = GetIndentLevel(line);

                // Enter Components section and record its indent
                if (!inComponents && line.Contains("Components:"))
                {
                    inComponents = true;
                    componentsIndent = indent;
                    componentHeaderIndent = componentsIndent + 4;
                    continue;
                }

                if (inComponents)
                {
                    // Leaving Components section when indent reduces to or before the section's indent
                    if (!string.IsNullOrWhiteSpace(line) && indent <= componentsIndent)
                        break;

                    // Only consider top-level component header lines directly under Components:
                    if (indent == componentHeaderIndent && line.Contains(": !"))
                    {
                        // Found a component line like "857f8451601718419f2a3730e6385b35: !TransformComponent"
                        var colonBangIndex = line.IndexOf(": !");
                        if (colonBangIndex >= 0)
                        {
                            var componentType = line.Substring(colonBangIndex + 3).Trim();
                            // Remove any trailing text after the component type
                            var spaceIndex = componentType.IndexOf(' ');
                            if (spaceIndex > 0)
                                componentType = componentType.Substring(0, spaceIndex);

                            // Trigger lazy loading by calling GetComponent
                            entity.GetComponent(componentType);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Utility to compute leading whitespace indent in spaces (tabs count as 4)
        /// </summary>
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

        private object ClonePropertyWithUpdatedReferences(object value, Dictionary<string, string> entityIdMap, Dictionary<string, string> componentIdMap)
        {
            if (value is Dictionary<string, object> dict)
            {
                var newDict = new Dictionary<string, object>();
                foreach (var kvp in dict)
                {
                    // For child references in Transform a "ref!!" could be on the value or the key
                    if (kvp.Value is string childValueStr && childValueStr.StartsWith("ref!! "))
                    {
                        var oldId = childValueStr.Substring(6);
                        
                        // It could be a component (transform) or entity reference
                        if (componentIdMap.TryGetValue(oldId, out var newCompId))
                            newDict[kvp.Key] = $"ref!! {newCompId}";
                        else if (entityIdMap.TryGetValue(oldId, out var newEntId))
                            newDict[kvp.Key] = $"ref!! {newEntId}";
                        else
                            newDict[kvp.Key] = childValueStr; // keep original
                    }
                    else
                    {
                        newDict[kvp.Key] = ClonePropertyWithUpdatedReferences(kvp.Value, entityIdMap, componentIdMap);
                    }
                }
                return newDict;
            }
            else if (value is string str && str.StartsWith("ref!! "))
            {
                // This is an entity or component reference
                var oldId = str.Substring(6);
                if (componentIdMap.TryGetValue(oldId, out var newCompId))
                    return $"ref!! {newCompId}";
                if (entityIdMap.TryGetValue(oldId, out var newEntId))
                    return $"ref!! {newEntId}";
                
                return str; // Reference to something outside this prefab, keep as is
            }
            else if (value is List<object> list)
            {
                var newList = new List<object>();
                foreach (var item in list)
                {
                    newList.Add(ClonePropertyWithUpdatedReferences(item, entityIdMap, componentIdMap));
                }
                return newList;
            }
            else
            {
                // Primitive value, return as is
                return value;
            }
        }

        /// <summary>
        /// Instantiate a prefab using ID and path directly
        /// </summary>
        public Entity InstantiatePrefab(string prefabId, string prefabPath, Vector3Data? position = null, string? folder = null)
        {
            var prefabRef = new AssetReference
            {
                Id = prefabId,
                Path = prefabPath,
                Type = AssetType.Prefab
            };
            return InstantiatePrefab(prefabRef, position, folder);
        }

        // Entity removal
        public void RemoveEntity(Entity entity)
        {
            _content.Entities.Remove(entity);
            _content.RootEntityIds.Remove(entity.Id);
            RemoveEntityFromParentChildren(entity.Id);

            // Track removed entity for surgical YAML editing
            if (!string.IsNullOrEmpty(_content.RawContent))
            {
                _content.RemovedEntityIds.Add(entity.Id);
            }
        }

        public void RemoveEntity(string entityId)
        {
            var entity = FindEntityById(entityId);
            if (entity != null)
                RemoveEntity(entity);
        }

        private void RemoveEntityFromParentChildren(string entityId)
        {
            // Find the parent entity that has this entity in its Children
            // Children format: { "guidkey": "ref!! entityId", ... }
            foreach (var parentEntity in _content.Entities)
            {
                var transform = parentEntity.GetTransform();
                if (transform != null)
                {
                    var children = transform.Component.GetMultiValueProperty("Children") ?? new Dictionary<string, object>();
                    var keyToRemove = children.FirstOrDefault(kvp =>
                        kvp.Value is string strValue && strValue.Contains(entityId)
                    ).Key;

                    if (keyToRemove != null)
                    {
                        children.Remove(keyToRemove);
                        transform.Component.Set("Children", children);
                        break;
                    }
                }
            }
        }

    }
}
