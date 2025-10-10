// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using HS.Stride.Editor.Toolkit.Core.SceneEditing;
using HS.Stride.Editor.Toolkit.Core.StrideYamlParser;
using HS.Stride.Editor.Toolkit.Utilities;

namespace HS.Stride.Editor.Toolkit.Core.PrefabEditing
{
    /// <summary>
    /// Represents an editable Stride Prefab asset (.sdprefab).
    /// </summary>
    public class Prefab : IStrideAsset
    {
        private readonly PrefabContent _prefabContent;
        private readonly SceneManager _sceneManager;

        private Prefab(PrefabContent prefabContent)
        {
            _prefabContent = prefabContent;
            _sceneManager = new SceneManager(prefabContent);
        }

        /// <summary>
        /// Loads a prefab asset from the specified file path.
        /// </summary>
        public static Prefab Load(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            var prefabData = StrideYamlPrefab.ParsePrefab(filePath);
            return new Prefab(prefabData);
        }

        /// <summary>
        /// Creates a new prefab with the specified name and root entity.
        /// The prefab is not saved to disk until Save() or SaveAs() is called.
        /// </summary>
        /// <param name="name">Name of the root entity</param>
        /// <param name="filePath">Optional file path for the prefab (used when saving with Save())</param>
        /// <returns>A new PrefabAsset with an empty root entity</returns>
        public static Prefab Create(string name, string? filePath = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            var prefabContent = new PrefabContent
            {
                Id = GuidHelper.NewGuid(),
                FilePath = filePath ?? string.Empty,
                Entities = new List<Entity>(),
                RootEntityIds = new List<string>(),
                RawContent = string.Empty
            };

            var prefab = new Prefab(prefabContent);

            // Create the root entity with transform component
            var rootEntity = prefab.CreateEntity(name);

            return prefab;
        }

        /// <summary>
        /// The unique identifier of this prefab.
        /// </summary>
        public string Id => _prefabContent.Id;

        /// <summary>
        /// The file path where this prefab is stored.
        /// </summary>
        public string FilePath => _prefabContent.FilePath;

        /// <summary>
        /// All entities in this prefab (including root and children).
        /// </summary>
        public List<Entity> AllEntities => _prefabContent.Entities;

        /// <summary>
        /// Gets the root entity of this prefab. Returns null if no root entity exists.
        /// </summary>
        /// <returns>The root entity or null</returns>
        public Entity? GetRootEntity()
        {
            if (_prefabContent.RootEntityIds.Count == 0)
                return null;

            var rootId = _prefabContent.RootEntityIds[0];
            return _prefabContent.Entities.FirstOrDefault(e => e.Id == rootId);
        }

        /// <summary>
        /// Saves the prefab's current state back to its original file.
        /// </summary>
        public void Save()
        {
            var yaml = StrideYamlPrefab.GeneratePrefabYaml(_prefabContent);
            FileHelper.SaveFile(yaml, _prefabContent.FilePath);
        }

        /// <summary>
        /// Saves the prefab's current state to a new file.
        /// </summary>
        public void SaveAs(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            var yaml = StrideYamlPrefab.GeneratePrefabYaml(_prefabContent);
            FileHelper.SaveFile(yaml, filePath);
        }

        /// <summary>
        /// Sets the parent project for this prefab and all its entities.
        /// This enables script scanning for custom components.
        /// </summary>
        internal void SetParentProject(StrideProject project)
        {
            _prefabContent.ParentProject = project;

            // Also update all existing entities
            foreach (var entity in _prefabContent.Entities)
            {
                entity.ParentProject = project;
            }
        }

        // Entity search methods

        /// <summary>
        /// Finds an entity by its unique ID.
        /// </summary>
        /// <param name="id">The entity ID</param>
        /// <returns>The entity if found, otherwise null</returns>
        public Entity? FindEntityById(string id) => _sceneManager.FindEntityById(id);

        /// <summary>
        /// Finds an entity by its exact name.
        /// </summary>
        /// <param name="name">The entity name</param>
        /// <returns>The entity if found, otherwise null</returns>
        public Entity? FindEntityByName(string name) => _sceneManager.FindEntityByName(name);

        /// <summary>
        /// Finds entities by name pattern (supports * and ? wildcards).
        /// </summary>
        /// <param name="pattern">The name pattern (e.g., "Enemy*", "Player?")</param>
        /// <returns>List of matching entities</returns>
        public List<Entity> FindEntitiesByName(string pattern) => _sceneManager.FindEntitiesByName(pattern);

        /// <summary>
        /// Finds all entities that have a specific component type.
        /// </summary>
        /// <param name="componentType">The component type name</param>
        /// <returns>List of entities with the component</returns>
        public List<Entity> FindEntitiesWithComponent(string componentType) => _sceneManager.FindEntitiesWithComponent(componentType);

        /// <summary>
        /// Finds entities using a custom predicate function.
        /// </summary>
        /// <param name="predicate">The filter function</param>
        /// <returns>List of entities matching the predicate</returns>
        public List<Entity> FindEntities(Func<Entity, bool> predicate) => _sceneManager.FindEntities(predicate);

        // Entity creation and manipulation

        /// <summary>
        /// Creates a new entity in this prefab.
        /// </summary>
        /// <param name="name">The entity name</param>
        /// <param name="folder">Optional folder for organization</param>
        /// <returns>The created entity</returns>
        public Entity CreateEntity(string name, string? folder = null) => _sceneManager.CreateEntity(name, folder);

        /// <summary>
        /// Creates a new entity as a child of another entity or folder.
        /// </summary>
        /// <param name="name">The entity name</param>
        /// <param name="parent">The parent entity name or folder path</param>
        /// <param name="parentType">Whether parent is an entity or folder</param>
        /// <returns>The created entity</returns>
        public Entity CreateEntity(string name, string parent, ParentType parentType) => _sceneManager.CreateEntity(name, parent, parentType);

        /// <summary>
        /// Removes an entity from this prefab.
        /// </summary>
        /// <param name="entity">The entity to remove</param>
        public void RemoveEntity(Entity entity) => _sceneManager.RemoveEntity(entity);
    }
}
