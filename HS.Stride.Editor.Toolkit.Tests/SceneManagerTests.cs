// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using NUnit.Framework;
using FluentAssertions;
using HS.Stride.Editor.Toolkit.Core.SceneEditing;
using HS.Stride.Editor.Toolkit.Core;
using HS.Stride.Editor.Toolkit.Core.StrideYamlParser;

namespace HS.Stride.Editor.Toolkit.Tests
{
    [TestFixture]
    public class SceneManagerTests
    {
        private SceneContent _sceneContent;
        private SceneManager _manager;

        [SetUp]
        public void Setup()
        {
            var testScenePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Example Scenes", "Scene.sdscene");
            _sceneContent = StrideYamlScene.ParseScene(testScenePath);
            _manager = new SceneManager(_sceneContent);
        }

        [Test]
        public void Constructor_ValidSceneContent_ShouldCreateManager()
        {
            // Assert
            _manager.Should().NotBeNull();
            _manager.AllEntities.Should().NotBeNull();
        }

        [Test]
        public void AllEntities_ShouldReturnEntitiesFromContent()
        {
            // Act
            var entities = _manager.AllEntities;

            // Assert
            entities.Should().NotBeNull();
            entities.Should().BeSameAs(_sceneContent.Entities);
        }

        [Test]
        public void FindEntityById_ExistingEntity_ShouldReturnEntity()
        {
            // Arrange
            var expectedEntity = _sceneContent.Entities.First();

            // Act
            var result = _manager.FindEntityById(expectedEntity.Id);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(expectedEntity);
        }

        [Test]
        public void FindEntityById_NonExistentEntity_ShouldReturnNull()
        {
            // Act
            var result = _manager.FindEntityById("non-existent-id");

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void FindEntityByName_ExistingEntity_ShouldReturnEntity()
        {
            // Arrange
            var expectedEntity = _sceneContent.Entities.First();

            // Act
            var result = _manager.FindEntityByName(expectedEntity.Name);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(expectedEntity);
        }

        [Test]
        public void FindEntityByName_NonExistentEntity_ShouldReturnNull()
        {
            // Act
            var result = _manager.FindEntityByName("NonExistentEntity");

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void FindEntitiesByName_WildcardPattern_ShouldReturnMatchingEntities()
        {
            // Act
            var results = _manager.FindEntitiesByName("*");

            // Assert
            results.Should().NotBeEmpty();
            results.Count.Should().Be(_sceneContent.Entities.Count);
        }

        [Test]
        public void FindEntitiesWithComponent_ExistingComponentType_ShouldReturnEntities()
        {
            // Act
            var results = _manager.FindEntitiesWithComponent("TransformComponent");

            // Assert
            results.Should().NotBeEmpty();
            results.Should().AllSatisfy(e => e.HasComponent("TransformComponent").Should().BeTrue());
        }

        [Test]
        public void FindEntitiesWithComponent_NonExistentComponentType_ShouldReturnEmpty()
        {
            // Act
            var results = _manager.FindEntitiesWithComponent("NonExistentComponent");

            // Assert
            results.Should().BeEmpty();
        }

        [Test]
        public void FindEntities_WithPredicate_ShouldReturnMatchingEntities()
        {
            // Arrange
            var firstEntity = _sceneContent.Entities.First();

            // Act
            var results = _manager.FindEntities(e => e.Id == firstEntity.Id);

            // Assert
            results.Should().ContainSingle();
            results.First().Should().Be(firstEntity);
        }

        [Test]
        public void CreateEntity_ValidParameters_ShouldCreateEntityWithTransform()
        {
            // Act
            var entity = _manager.CreateEntity("TestEntity");

            // Assert
            entity.Should().NotBeNull();
            entity.Name.Should().Be("TestEntity");
            entity.Id.Should().NotBeNullOrEmpty();
            entity.Components.Should().ContainSingle();
            entity.Components.Values.First().Type.Should().Be("TransformComponent");
        }

        [Test]
        public void CreateEntity_WithFolder_ShouldSetFolder()
        {
            // Act
            var entity = _manager.CreateEntity("TestEntity", "TestFolder");

            // Assert
            entity.Folder.Should().Be("TestFolder");
        }

        [Test]
        public void CreateEntity_ShouldAddToEntitiesAndRootIds()
        {
            // Arrange
            var initialCount = _sceneContent.Entities.Count;
            var initialRootCount = _sceneContent.RootEntityIds.Count;

            // Act
            var entity = _manager.CreateEntity("TestEntity");

            // Assert
            _sceneContent.Entities.Should().Contain(entity);
            _sceneContent.Entities.Count.Should().Be(initialCount + 1);
            _sceneContent.RootEntityIds.Should().Contain(entity.Id);
            _sceneContent.RootEntityIds.Count.Should().Be(initialRootCount + 1);
        }

        [Test]
        public void RemoveEntity_ExistingEntity_ShouldRemoveFromCollections()
        {
            // Arrange
            var entity = _manager.CreateEntity("ToRemove");
            var initialCount = _sceneContent.Entities.Count;

            // Act
            _manager.RemoveEntity(entity);

            // Assert
            _sceneContent.Entities.Should().NotContain(entity);
            _sceneContent.Entities.Count.Should().Be(initialCount - 1);
            _sceneContent.RootEntityIds.Should().NotContain(entity.Id);
        }

        [Test]
        public void RemoveEntity_ById_ShouldRemoveEntity()
        {
            // Arrange
            var entity = _manager.CreateEntity("ToRemove");

            // Act
            _manager.RemoveEntity(entity.Id);

            // Assert
            _sceneContent.Entities.Should().NotContain(entity);
            _sceneContent.RootEntityIds.Should().NotContain(entity.Id);
        }

        [Test]
        public void AddComponent_ValidParameters_ShouldAddComponentToEntity()
        {
            // Arrange
            var entity = _manager.CreateEntity("TestEntity");

            // Act
            var component = entity.AddComponent("TestComponent");
            component.Set("TestProp", "TestValue");

            // Assert
            component.Should().NotBeNull();
            component.Type.Should().Be("TestComponent");
            component.Properties.Should().ContainKey("TestProp");
            entity.Components.Should().Contain(kvp => kvp.Value == component);
        }

        [Test]
        public void RemoveComponent_ExistingComponent_ShouldRemoveFromEntity()
        {
            // Arrange
            var entity = _manager.CreateEntity("TestEntity");
            entity.AddComponent("TestComponent");

            // Act
            entity.RemoveComponent("TestComponent");

            // Assert
            entity.Components.Should().NotContain(kvp => kvp.Value.Type == "TestComponent");
        }

       

        [Test]
        public void EndToEnd_RemoveEntityAndSave_ShouldPersistDeletion()
        {
            // Arrange
            var tempPath = Path.Combine(Path.GetTempPath(), $"e2e_remove_scene_{Guid.NewGuid()}.sdscene");

            try
            {
                File.Copy(_sceneContent.FilePath, tempPath);
                var sceneContent = StrideYamlScene.ParseScene(tempPath);
                var manager = new SceneManager(sceneContent);

                // Create and then remove an entity
                var entity = manager.CreateEntity("ToBeRemoved");
                var entityId = entity.Id;

                // Act - Remove entity and save
                manager.RemoveEntity(entity);
                var yaml = StrideYamlScene.GenerateSceneYaml(sceneContent);
                File.WriteAllText(tempPath, yaml);

                // Assert - Verify entity is gone after reload
                var reloadedContent = StrideYamlScene.ParseScene(tempPath);
                var foundEntity = reloadedContent.Entities.FirstOrDefault(e => e.Id == entityId);
                foundEntity.Should().BeNull();
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        [Test]
        public void EndToEnd_ModifyExistingEntityAndSave_ShouldPersistChanges()
        {
            // Arrange
            var tempPath = Path.Combine(Path.GetTempPath(), $"e2e_modify_scene_{Guid.NewGuid()}.sdscene");

            try
            {
                File.Copy(_sceneContent.FilePath, tempPath);
                var sceneContent = StrideYamlScene.ParseScene(tempPath);
                var manager = new SceneManager(sceneContent);

                var entity = sceneContent.Entities.First();
                var originalName = entity.Name;
                var newName = $"Modified_{originalName}";

                // Act - Modify entity name
                entity.Name = newName;

                // Add a new component
                var newComp = entity.AddComponent("NewTestComponent");
                newComp.Set("TestProperty", "TestValue");

                var yaml = StrideYamlScene.GenerateSceneYaml(sceneContent);
                File.WriteAllText(tempPath, yaml);

                // Assert - Verify changes persisted
                var reloadedContent = StrideYamlScene.ParseScene(tempPath);
                var reloadedEntity = reloadedContent.Entities.FirstOrDefault(e => e.Id == entity.Id);
                
                reloadedEntity.Should().NotBeNull();
                reloadedEntity.Name.Should().Be(newName);
                
                var component = reloadedEntity.GetComponent("NewTestComponent");
                component.Should().NotBeNull();
                component.Properties["TestProperty"].Should().Be("TestValue");
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        [Test]
        public void EndToEnd_ComplexSceneManipulation_ShouldMaintainIntegrity()
        {
            // Arrange
            var tempPath = Path.Combine(Path.GetTempPath(), $"e2e_complex_scene_{Guid.NewGuid()}.sdscene");

            try
            {
                File.Copy(_sceneContent.FilePath, tempPath);
                var sceneContent = StrideYamlScene.ParseScene(tempPath);
                var manager = new SceneManager(sceneContent);

                // Act - Perform multiple operations
                // 1. Create new entities
                var newEntity1 = manager.CreateEntity("ComplexEntity1");
                var newEntity2 = manager.CreateEntity("ComplexEntity2");

                // 2. Add components
                newEntity1.AddComponent("ComponentA");
                newEntity1.AddComponent("ComponentB");
                newEntity2.AddComponent("ComponentC");

                // 3. Modify existing entity
                var existingEntity = sceneContent.Entities.First();
                existingEntity.Name = "ModifiedExisting";

                // 4. Remove a component from existing entity
                var transformComponent = existingEntity.GetComponent("TransformComponent");
                if (transformComponent != null)
                {
                    existingEntity.RemoveComponent("TransformComponent");
                }

                // 5. Create and immediately delete an entity
                var tempEntity = manager.CreateEntity("TempEntity");
                manager.RemoveEntity(tempEntity);

                // Save
                var yaml = StrideYamlScene.GenerateSceneYaml(sceneContent);
                File.WriteAllText(tempPath, yaml);

                // Assert - Verify all changes persisted correctly
                var reloadedContent = StrideYamlScene.ParseScene(tempPath);

                // Verify new entities exist
                reloadedContent.Entities.Should().Contain(e => e.Name == "ComplexEntity1");
                reloadedContent.Entities.Should().Contain(e => e.Name == "ComplexEntity2");

                // Verify temp entity doesn't exist
                reloadedContent.Entities.Should().NotContain(e => e.Name == "TempEntity");

                // Verify existing entity was modified
                var modifiedEntity = reloadedContent.Entities.FirstOrDefault(e => e.Id == existingEntity.Id);
                modifiedEntity.Should().NotBeNull();
                modifiedEntity.Name.Should().Be("ModifiedExisting");

                // Verify components on new entities
                var reloadedEntity1 = reloadedContent.Entities.First(e => e.Name == "ComplexEntity1");
                reloadedEntity1.GetComponent("ComponentA").Should().NotBeNull();
                reloadedEntity1.GetComponent("ComponentB").Should().NotBeNull();
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        [Test]
        public void AddEntityToBlankScene_WithPartsEmptyArray_ShouldProduceValidYaml()
        {
            // Arrange - Get test project and create a blank scene
            var testProjectPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Example Scenes", "TestProject");
            var project = new StrideProject(testProjectPath);

            var sceneName = $"BlankTestScene_{Guid.NewGuid()}";
            var scene = project.CreateScene(sceneName);

            try
            {
                // Act - Add entity to blank scene
                var newEntity = scene.CreateEntity("NewEntity");
                scene.Save();

                // Assert - Verify the scene can be reloaded without errors
                scene.Reload();
                scene.AllEntities.Should().ContainSingle();
                scene.AllEntities.First().Name.Should().Be("NewEntity");

                // Verify the YAML doesn't contain "Parts: []" anymore
                var savedYaml = File.ReadAllText(scene.FilePath);
                savedYaml.Should().NotContain("Parts: []");
                savedYaml.Should().Contain("Parts:");
            }
            finally
            {
                if (File.Exists(scene.FilePath))
                    File.Delete(scene.FilePath);
            }
        }
    }
}
