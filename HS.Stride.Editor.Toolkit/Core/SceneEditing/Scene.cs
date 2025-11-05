// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using HS.Stride.Editor.Toolkit.Core.AssetEditing;
using HS.Stride.Editor.Toolkit.Core.DataTypes;
using HS.Stride.Editor.Toolkit.Core.StrideYamlParser;
using HS.Stride.Editor.Toolkit.Utilities;

namespace HS.Stride.Editor.Toolkit.Core.SceneEditing
{
    /// <summary>
    /// Represents an editable Stride Scene asset (.sdscene).
    /// </summary>
    public class Scene : IStrideAsset
    {
        private SceneContent _sceneContent;
        private SceneManager _manager;

        private Scene(SceneContent sceneContent)
        {
            _sceneContent = sceneContent;
            _manager = new SceneManager(sceneContent);
        }

        /// <summary>
        /// Sets the parent project for this scene (enables script scanning for AddComponent).
        /// </summary>
        internal void SetParentProject(StrideProject project)
        {
            _sceneContent.ParentProject = project;

            // Also update all existing entities
            foreach (var entity in _sceneContent.Entities)
            {
                entity.ParentProject = project;
            }
        }

        /// <summary>
        /// Loads a scene from the specified file path.
        /// </summary>
        /// <param name="filePath">Path to the .sdscene file</param>
        /// <returns>The loaded scene</returns>
        /// <exception cref="ArgumentNullException">Thrown if filePath is null or empty</exception>
        public static Scene Load(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            var sceneData = StrideYamlScene.ParseScene(filePath);
            return new Scene(sceneData);
        }

        /// <summary>
        /// The unique identifier of this scene.
        /// </summary>
        public string Id => _sceneContent.Id;

        /// <summary>
        /// The file path where this scene is stored.
        /// </summary>
        public string FilePath => _sceneContent.FilePath;

        /// <summary>
        /// All entities in this scene.
        /// </summary>
        public List<Entity> AllEntities => _manager.AllEntities;

        // Entity search methods

        /// <summary>
        /// Finds an entity by its unique ID.
        /// </summary>
        /// <param name="id">The entity ID</param>
        /// <returns>The entity if found, otherwise null</returns>
        /// <exception cref="ArgumentNullException">Thrown if id is null or empty</exception>
        public Entity? FindEntityById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));
            return _manager.FindEntityById(id);
        }

        /// <summary>
        /// Finds an entity by its exact name.
        /// </summary>
        /// <param name="name">The entity name</param>
        /// <returns>The entity if found, otherwise null</returns>
        /// <exception cref="ArgumentNullException">Thrown if name is null or empty</exception>
        public Entity? FindEntityByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            return _manager.FindEntityByName(name);
        }

        /// <summary>
        /// Finds entities by name pattern (supports * and ? wildcards).
        /// </summary>
        /// <param name="pattern">The name pattern (e.g., "Enemy*", "Player?")</param>
        /// <returns>List of matching entities</returns>
        /// <exception cref="ArgumentNullException">Thrown if pattern is null or empty</exception>
        public List<Entity> FindEntitiesByName(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
                throw new ArgumentNullException(nameof(pattern));
            return _manager.FindEntitiesByName(pattern);
        }

        /// <summary>
        /// Finds all entities that have a specific component type.
        /// </summary>
        /// <param name="componentType">The component type name</param>
        /// <returns>List of entities with the component</returns>
        /// <exception cref="ArgumentNullException">Thrown if componentType is null or empty</exception>
        public List<Entity> FindEntitiesWithComponent(string componentType)
        {
            if (string.IsNullOrWhiteSpace(componentType))
                throw new ArgumentNullException(nameof(componentType));
            return _manager.FindEntitiesWithComponent(componentType);
        }

        /// <summary>
        /// Finds entities using a custom predicate function.
        /// </summary>
        /// <param name="predicate">The filter function</param>
        /// <returns>List of entities matching the predicate</returns>
        /// <exception cref="ArgumentNullException">Thrown if predicate is null</exception>
        public List<Entity> FindEntities(Func<Entity, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            return _manager.FindEntities(predicate);
        }

        // Entity creation and manipulation

        /// <summary>
        /// Creates a new entity in this scene.
        /// </summary>
        /// <param name="name">The entity name</param>
        /// <param name="folder">Optional folder for organization</param>
        /// <returns>The created entity</returns>
        /// <exception cref="ArgumentNullException">Thrown if name is null or empty</exception>
        public Entity CreateEntity(string name, string? folder = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            return _manager.CreateEntity(name, folder);
        }

        /// <summary>
        /// Creates a new entity as a child of another entity or folder.
        /// </summary>
        /// <param name="name">The entity name</param>
        /// <param name="parent">The parent entity name or folder path</param>
        /// <param name="parentType">Whether parent is an entity or folder</param>
        /// <returns>The created entity</returns>
        /// <exception cref="ArgumentNullException">Thrown if name or parent is null or empty</exception>
        public Entity CreateEntity(string name, string parent, ParentType parentType)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(parent))
                throw new ArgumentNullException(nameof(parent));
            return _manager.CreateEntity(name, parent, parentType);
        }

        /// <summary>
        /// Instantiates a prefab into this scene.
        /// </summary>
        /// <param name="prefab">The prefab asset reference</param>
        /// <param name="position">Optional spawn position</param>
        /// <param name="folder">Optional folder for organization</param>
        /// <returns>The instantiated prefab root entity</returns>
        /// <exception cref="ArgumentNullException">Thrown if prefab is null</exception>
        public Entity InstantiatePrefab(AssetReference prefab, Vector3Data? position = null, string? folder = null)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));
            return _manager.InstantiatePrefab(prefab, position, folder);
        }

        /// <summary>
        /// Instantiates a prefab into this scene.
        /// </summary>
        /// <param name="prefabId">The prefab GUID</param>
        /// <param name="prefabPath">The prefab path</param>
        /// <param name="position">Optional spawn position</param>
        /// <param name="folder">Optional folder for organization</param>
        /// <returns>The instantiated prefab root entity</returns>
        /// <exception cref="ArgumentNullException">Thrown if prefabId or prefabPath is null or empty</exception>
        public Entity InstantiatePrefab(string prefabId, string prefabPath, Vector3Data? position = null, string? folder = null)
        {
            if (string.IsNullOrWhiteSpace(prefabId))
                throw new ArgumentNullException(nameof(prefabId));
            if (string.IsNullOrWhiteSpace(prefabPath))
                throw new ArgumentNullException(nameof(prefabPath));
            return _manager.InstantiatePrefab(prefabId, prefabPath, position, folder);
        }

        /// <summary>
        /// Removes an entity from this scene.
        /// </summary>
        /// <param name="entity">The entity to remove</param>
        /// <exception cref="ArgumentNullException">Thrown if entity is null</exception>
        public void RemoveEntity(Entity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            _manager.RemoveEntity(entity);
        }

        /// <summary>
        /// Removes an entity from this scene by its ID.
        /// </summary>
        /// <param name="entityId">The entity ID</param>
        /// <exception cref="ArgumentNullException">Thrown if entityId is null or empty</exception>
        public void RemoveEntity(string entityId)
        {
            if (string.IsNullOrWhiteSpace(entityId))
                throw new ArgumentNullException(nameof(entityId));
            _manager.RemoveEntity(entityId);
        }

        /// <summary>
        /// Reloads the scene from disk, discarding any unsaved changes.
        /// </summary>
        public void Reload()
        {
            var parentProject = _sceneContent.ParentProject;
            _sceneContent = StrideYamlScene.ParseScene(_sceneContent.FilePath);
            _sceneContent.ParentProject = parentProject;

            // Update all entities with parent project
            foreach (var entity in _sceneContent.Entities)
            {
                entity.ParentProject = parentProject;
            }

            _manager = new SceneManager(_sceneContent);
        }

        /// <summary>
        /// Saves the scene's current state back to its original file and reloads.
        /// </summary>
        public void Save()
        {
            StrideYamlScene.SaveScene(_sceneContent);
            Reload();
        }

        /// <summary>
        /// Saves the scene's current state to a new file.
        /// If ParentProject exists, takes a relative path from Assets folder.
        /// If no ParentProject, takes a full file path.
        /// If path ends with / or \, uses first root entity name as filename.
        /// </summary>
        /// <param name="path">Relative path from Assets folder or full file path</param>
        /// <exception cref="ArgumentNullException">Thrown if path is null or empty</exception>
        public void SaveAs(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException(nameof(path));

            string fullPath;

            if (_sceneContent.ParentProject != null)
            {
                // If path ends with separator, append first root entity name or scene ID
                if (path.EndsWith("/") || path.EndsWith("\\"))
                {
                    var fileName = "Scene";
                    if (_sceneContent.RootEntityIds.Count > 0)
                    {
                        var rootEntity = _sceneContent.Entities.FirstOrDefault(e => e.Id == _sceneContent.RootEntityIds[0]);
                        fileName = rootEntity?.Name ?? "Scene";
                    }
                    path = path + fileName;
                }

                fullPath = StrideYamlParser.StrideYamlScene.BuildScenePath(path, _sceneContent.ParentProject);
            }
            else
            {
                // No project context, treat as full path
                fullPath = path;
            }

            _sceneContent.FilePath = fullPath;
            StrideYamlScene.SaveScene(_sceneContent);
            Reload();
        }
    }
}
