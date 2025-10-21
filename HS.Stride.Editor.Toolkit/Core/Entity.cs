// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using HS.Stride.Editor.Toolkit.Core.DataTypes;
using HS.Stride.Editor.Toolkit.Core.Wrappers;
using HS.Stride.Editor.Toolkit.Core.SceneEditing;
using HS.Stride.Editor.Toolkit.Core.ScriptEditing;
using HS.Stride.Editor.Toolkit.Core.StrideYamlParser;

namespace HS.Stride.Editor.Toolkit.Core
{
/// <summary>
    /// Represents an entity in a scene or prefab.
    /// </summary>
    public class Entity
    {
        /// <summary>
        /// The unique identifier of this entity.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The name of this entity.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Dictionary of components attached to this entity.
        /// </summary>
        public Dictionary<string, Component> Components { get; set; } = new();

        /// <summary>
        /// Optional folder this entity belongs to (for organization in the editor).
        /// </summary>
        public string? Folder { get; set; }

        /// <summary>
        /// If this entity is a prefab instance, contains the prefab reference data.
        /// </summary>
        public PrefabData? ParentPrefab { get; set; }

        internal SceneContent? ParentScene { get; set; }  // For lazy loading

        /// <summary>
        /// Reference to the parent project (used for script scanning when adding components).
        /// </summary>
        public StrideProject? ParentProject { get; set; }

        /// <summary>
        /// Tracks if this entity was modified (components accessed/added/removed).
        /// Used for surgical YAML editing - only modified entities are regenerated.
        /// </summary>
        internal bool IsModified { get; set; } = false;



        /// <summary>
        /// Gets a component by type name (e.g., "TransformComponent", "ModelComponent", "MyScript").
        /// Returns null if the component is not found.
        /// </summary>
        /// <param name="componentType">The component type name (class name or full type)</param>
        /// <returns>The component if found, otherwise null</returns>
        public Component? GetComponent(string componentType)
        {
            // Check already-loaded components first
            var existing = Components.Values.FirstOrDefault(c => c.Type.Contains(componentType));
            if (existing != null)
            {
                // Set ParentEntity if not already set
                if (existing.ParentEntity == null)
                    existing.ParentEntity = this;
                return existing;
            }

            //Lazy Load from raw YAML if not found
            if (ParentScene != null && !string.IsNullOrEmpty(ParentScene.RawContent))
            {
                var component = StrideYamlScene.ParseComponentFromRaw(ParentScene.RawContent, Id, componentType);
                if (component != null)
                {
                    // Set ParentEntity for strict mode validation
                    component.ParentEntity = this;

                    // Cache it for next time
                    var componentKey = component.Key;
                    Components[componentKey] = component;

                    // Mark entity as modified since we accessed a component
                    IsModified = true;

                    return component;
                }
            }

            return null;
        }

        /// <summary>
        /// Adds a component to this entity by type name (e.g., "ModelComponent", "MyScript").
        /// If the component already exists, returns the existing component.
        /// </summary>
        /// <param name="componentType">The component type name (class name)</param>
        /// <returns>The added or existing component</returns>
        /// <exception cref="ArgumentNullException">Thrown if componentType is null or empty</exception>
        /// <exception cref="InvalidOperationException">Thrown if custom script is not found in project</exception>
        public Component AddComponent(string componentType)
        {
            if (string.IsNullOrWhiteSpace(componentType))
                throw new ArgumentNullException(nameof(componentType));

            // Check if already exists
            var existing = GetComponent(componentType);
            if (existing != null)
                return existing;

            Component component;

            // Try to find the script and get full metadata
            if (ParentProject != null)
            {
                var scriptInfo = ScriptScanner.FindScript(ParentProject, componentType);
                if (scriptInfo != null)
                {
                    // Create component with full namespace and initialized properties
                    component = ScriptToComponent.Create(scriptInfo);
                    component.ParentEntity = this;  // Set for strict mode validation
                    Components[component.Key] = component;
                    IsModified = true;
                    return component;
                }

                // If we have a ParentProject but couldn't find the script
                // Check if this looks like a custom script name
                if (!IsBuiltInStrideComponent(componentType))
                {
                    throw new InvalidOperationException(
                        $"Script '{componentType}' not found in project. " +
                        $"Make sure the script file exists and the class name matches. " +
                        $"Available scripts: {string.Join(", ", ParentProject.GetScripts().Select(s => s.Name))}");
                }
            }

            // Fallback: Create basic component (for built-in Stride components when ParentProject is null)
            var componentKey = Utilities.GuidHelper.NewGuidNoDashes();
            component = new Component
            {
                Type = componentType,
                Id = Utilities.GuidHelper.NewGuid(),
                Properties = new Dictionary<string, object>(),
                ParentEntity = this  // Set for strict mode validation
            };

            Components[componentKey] = component;
            IsModified = true;
            return component;
        }

        private static bool IsBuiltInStrideComponent(string componentType)
        {
            // List of common built-in Stride components that don't need script scanning
            var builtInComponents = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "TransformComponent",
                "ModelComponent",
                "LightComponent",
                "CameraComponent",
                "RigidbodyComponent",
                "StaticColliderComponent",
                "CharacterComponent",
                "AudioEmitterComponent",
                "AudioListenerComponent",
                "UIComponent",
                "SpriteComponent",
                "BackgroundComponent",
                "SkyboxComponent",
                "ScriptComponent",
                "AnimationComponent",
                "ParticleSystemComponent",
                "VideoComponent"
            };

            return builtInComponents.Contains(componentType);
        }

        /// <summary>
        /// Removes a component from this entity by type name.
        /// </summary>
        /// <param name="componentType">The component type name to remove</param>
        /// <exception cref="ArgumentNullException">Thrown if componentType is null or empty</exception>
        public void RemoveComponent(string componentType)
        {
            if (string.IsNullOrWhiteSpace(componentType))
                throw new ArgumentNullException(nameof(componentType));

            var key = Components.FirstOrDefault(kvp => kvp.Value.Type.Contains(componentType)).Key;
            if (key != null)
            {
                Components.Remove(key);
                IsModified = true;
            }
        }

        /// <summary>
        /// Checks if this entity has a component of the specified type.
        /// </summary>
        /// <param name="componentType">The component type name to check</param>
        /// <returns>True if the component exists, otherwise false</returns>
        public bool HasComponent(string componentType)
        {
            // Check already-loaded components first
            if (Components.Values.Any(c => c.Type.Contains(componentType)))
                return true;

            // Check raw YAML if not loaded yet
            if (ParentScene != null && !string.IsNullOrEmpty(ParentScene.RawContent))
            {
                var component = StrideYamlScene.ParseComponentFromRaw(ParentScene.RawContent, Id, componentType);
                return component != null;
            }

            return false;
        }

        // Hierarchy navigation

        /// <summary>
        /// Gets all child entities of this entity (from Transform.Children).
        /// </summary>
        /// <returns>List of direct child entities</returns>
        public List<Entity> GetChildren()
        {
            var children = new List<Entity>();

            var transform = GetTransform();
            if (transform == null)
                return children;

            var childrenDict = transform.Component.GetMultiValueProperty("Children") as Dictionary<string, object>;
            if (childrenDict == null || childrenDict.Count == 0)
                return children;

            // Children format: { "guidkey": "ref!! transformComponentId", ... }
            foreach (var childRef in childrenDict.Values)
            {
                if (childRef is string refStr && refStr.StartsWith("ref!! "))
                {
                    var transformId = refStr.Substring(6).Trim();

                    // Find entity with this transform component ID
                    var childEntity = ParentScene?.Entities.FirstOrDefault(e =>
                    {
                        var childTransform = e.GetTransform();
                        return childTransform?.Component.Id == transformId;
                    });

                    if (childEntity != null)
                        children.Add(childEntity);
                }
            }

            return children;
        }

        /// <summary>
        /// Finds a direct child entity by exact name.
        /// </summary>
        /// <param name="childName">The exact name of the child entity</param>
        /// <returns>The child entity if found, null otherwise</returns>
        public Entity? FindChildByName(string childName)
        {
            if (string.IsNullOrWhiteSpace(childName))
                throw new ArgumentNullException(nameof(childName));

            return GetChildren().FirstOrDefault(c => c.Name == childName);
        }

        /// <summary>
        /// Finds child entities by name pattern (supports * and ?).
        /// </summary>
        /// <param name="pattern">Name pattern with wildcards (* for any characters, ? for single character)</param>
        /// <returns>List of matching child entities</returns>
        public List<Entity> FindChildrenByName(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
                throw new ArgumentNullException(nameof(pattern));

            var regex = "^" + System.Text.RegularExpressions.Regex.Escape(pattern)
                .Replace("\\*", ".*")
                .Replace("\\?", ".") + "$";

            return GetChildren()
                .Where(c => System.Text.RegularExpressions.Regex.IsMatch(c.Name, regex))
                .ToList();
        }

        /// <summary>
        /// Gets the parent entity of this entity (if this entity is a child of another).
        /// Returns null if this is a root entity.
        /// </summary>
        /// <returns>The parent entity or null if this is a root entity</returns>
        public Entity? GetParent()
        {
            if (ParentScene == null)
                return null;

            var myTransform = GetTransform();
            if (myTransform == null)
                return null;

            // Search all entities to find one that has us as a child
            foreach (var entity in ParentScene.Entities)
            {
                var transform = entity.GetTransform();
                if (transform == null)
                    continue;

                var childrenDict = transform.Component.GetMultiValueProperty("Children") as Dictionary<string, object>;
                if (childrenDict == null)
                    continue;

                // Check if any child reference points to our transform
                foreach (var childRef in childrenDict.Values)
                {
                    if (childRef is string refStr && refStr.StartsWith("ref!! "))
                    {
                        var transformId = refStr.Substring(6).Trim();
                        if (transformId == myTransform.Component.Id)
                            return entity;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets all descendant entities recursively (children, grandchildren, etc.).
        /// </summary>
        /// <returns>List of all descendant entities</returns>
        public List<Entity> GetDescendants()
        {
            var descendants = new List<Entity>();
            var children = GetChildren();

            foreach (var child in children)
            {
                descendants.Add(child);
                descendants.AddRange(child.GetDescendants()); // Recursive
            }

            return descendants;
        }

        /// <summary>
        /// Checks if this entity has any children.
        /// </summary>
        /// <returns>True if the entity has children, false otherwise</returns>
        public bool HasChildren()
        {
            var transform = GetTransform();
            if (transform == null)
                return false;

            var childrenDict = transform.Component.GetMultiValueProperty("Children") as Dictionary<string, object>;
            return childrenDict != null && childrenDict.Count > 0;
        }


        // Component wrapper helpers

        /// <summary>
        /// Gets the TransformComponent as a wrapper. Returns null if not found.
        /// </summary>
        /// <returns>TransformWrapper or null</returns>
        public TransformWrapper? GetTransform()
        {
            var component = GetComponent("TransformComponent");
            return component != null ? new TransformWrapper(component) : null;
        }

        /// <summary>
        /// Gets the ModelComponent as a wrapper. Returns null if not found.
        /// </summary>
        /// <returns>ModelWrapper or null</returns>
        public ModelWrapper? GetModel()
        {
            var component = GetComponent("ModelComponent");
            return component != null ? new ModelWrapper(component) : null;
        }

        /// <summary>
        /// Gets the StaticColliderComponent as a wrapper. Returns null if not found.
        /// </summary>
        /// <returns>StaticColliderWrapper or null</returns>
        public StaticColliderWrapper? GetStaticCollider()
        {
            var component = GetComponent("StaticColliderComponent");
            return component != null ? new StaticColliderWrapper(component) : null;
        }

        /// <summary>
        /// Gets the RigidbodyComponent as a wrapper. Returns null if not found.
        /// </summary>
        /// <returns>RigidbodyWrapper or null</returns>
        public RigidbodyWrapper? GetRigidbody()
        {
            var component = GetComponent("RigidbodyComponent");
            return component != null ? new RigidbodyWrapper(component) : null;
        }

        /// <summary>
        /// Gets the LightComponent as a wrapper. Returns null if not found.
        /// </summary>
        /// <returns>LightWrapper or null</returns>
        public LightWrapper? GetLight()
        {
            var component = GetComponent("LightComponent");
            return component != null ? new LightWrapper(component) : null;
        }

        /// <summary>
        /// Gets the ParticleSystemComponent as a wrapper. Returns null if not found.
        /// </summary>
        /// <returns>ParticleSystemWrapper or null</returns>
        public ParticleSystemWrapper? GetParticleSystem()
        {
            var component = GetComponent("ParticleSystemComponent");
            return component != null ? new ParticleSystemWrapper(component) : null;
        }

        /// <summary>
        /// Adds a StaticColliderComponent to this entity. Returns existing if already present.
        /// </summary>
        /// <returns>StaticColliderWrapper for the component</returns>
        public StaticColliderWrapper AddStaticCollider()
        {
            var existing = GetStaticCollider();
            if (existing != null) return existing;

            var componentKey = Utilities.GuidHelper.NewGuidNoDashes();
            var component = StaticColliderWrapper.CreateComponent();
            Components[componentKey] = component;
            return new StaticColliderWrapper(component);
        }

        /// <summary>
        /// Adds a RigidbodyComponent to this entity. Returns existing if already present.
        /// </summary>
        /// <param name="mass">Mass of the rigidbody (default 1.0)</param>
        /// <param name="isKinematic">Whether the rigidbody is kinematic (default false)</param>
        /// <returns>RigidbodyWrapper for the component</returns>
        public RigidbodyWrapper AddRigidbody(float mass = 1.0f, bool isKinematic = false)
        {
            var existing = GetRigidbody();
            if (existing != null) return existing;

            var componentKey = Utilities.GuidHelper.NewGuidNoDashes();
            var component = RigidbodyWrapper.CreateComponent(mass, isKinematic);
            Components[componentKey] = component;
            return new RigidbodyWrapper(component);
        }

        /// <summary>
        /// Adds a ModelComponent to this entity. Returns existing if already present.
        /// </summary>
        /// <returns>ModelWrapper for the component</returns>
        public ModelWrapper AddModel()
        {
            var existing = GetModel();
            if (existing != null) return existing;

            var componentKey = Utilities.GuidHelper.NewGuidNoDashes();
            var component = ModelWrapper.CreateComponent();
            Components[componentKey] = component;
            return new ModelWrapper(component);
        }

        /// <summary>
        /// Adds a LightComponent to this entity. Returns existing if already present.
        /// </summary>
        /// <returns>LightWrapper for the component</returns>
        public LightWrapper AddLight()
        {
            var existing = GetLight();
            if (existing != null) return existing;

            var componentKey = Utilities.GuidHelper.NewGuidNoDashes();
            var component = LightWrapper.CreateComponent();
            Components[componentKey] = component;
            return new LightWrapper(component);
        }

        /// <summary>
        /// Adds a ParticleSystemComponent to this entity. Returns existing if already present.
        /// </summary>
        /// <returns>ParticleSystemWrapper for the component</returns>
        public ParticleSystemWrapper AddParticleSystem()
        {
            var existing = GetParticleSystem();
            if (existing != null) return existing;

            var componentKey = Utilities.GuidHelper.NewGuidNoDashes();
            var component = ParticleSystemWrapper.CreateComponent();
            Components[componentKey] = component;
            return new ParticleSystemWrapper(component);
        }

        /// <summary>
        /// Removes the ParticleSystemComponent from this entity if present.
        /// </summary>
        public void RemoveParticleSystem()
        {
            RemoveComponent("ParticleSystemComponent");
        }
    }
}
