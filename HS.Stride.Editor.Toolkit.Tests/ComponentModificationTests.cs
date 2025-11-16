// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using NUnit.Framework;
using FluentAssertions;
using HS.Stride.Editor.Toolkit.Core.SceneEditing;
using HS.Stride.Editor.Toolkit.Core.StrideYamlParser;

namespace HS.Stride.Editor.Toolkit.Tests
{
    /// <summary>
    /// Tests to verify that component additions properly set the IsModified flag.
    /// This is critical for surgical YAML editing - entities that are not marked as modified
    /// will not be included in the save operation.
    /// </summary>
    [TestFixture]
    public class ComponentModificationTests
    {
        private string _testScenePath;
        private SceneContent _sceneContent;

        [SetUp]
        public void Setup()
        {
            _testScenePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Example Scenes", "Scene.sdscene");
            _sceneContent = StrideYamlScene.ParseScene(_testScenePath);
        }

        [Test]
        public void AddStaticCollider_ShouldSetIsModifiedFlag()
        {
            // Arrange
            var manager = new SceneManager(_sceneContent);
            var entity = manager.CreateEntity("TestEntity");

            // Verify initial state
            entity.IsModified.Should().BeTrue("newly created entity should be modified");

            // Reset flag to test collider addition
            entity.IsModified = false;

            // Act
            var collider = entity.AddStaticCollider();

            // Assert
            entity.IsModified.Should().BeTrue("adding StaticCollider should set IsModified flag");
            collider.Should().NotBeNull();
        }

        [Test]
        public void AddRigidbody_ShouldSetIsModifiedFlag()
        {
            // Arrange
            var manager = new SceneManager(_sceneContent);
            var entity = manager.CreateEntity("TestEntity");
            entity.IsModified = false;

            // Act
            var rigidbody = entity.AddRigidbody(mass: 5.0f, isKinematic: true);

            // Assert
            entity.IsModified.Should().BeTrue("adding Rigidbody should set IsModified flag");
            rigidbody.Should().NotBeNull();
        }

        [Test]
        public void AddModel_ShouldSetIsModifiedFlag()
        {
            // Arrange
            var manager = new SceneManager(_sceneContent);
            var entity = manager.CreateEntity("TestEntity");
            entity.IsModified = false;

            // Act
            var model = entity.AddModel();

            // Assert
            entity.IsModified.Should().BeTrue("adding ModelComponent should set IsModified flag");
            model.Should().NotBeNull();
        }

        [Test]
        public void AddLight_ShouldSetIsModifiedFlag()
        {
            // Arrange
            var manager = new SceneManager(_sceneContent);
            var entity = manager.CreateEntity("TestEntity");
            entity.IsModified = false;

            // Act
            var light = entity.AddLight();

            // Assert
            entity.IsModified.Should().BeTrue("adding LightComponent should set IsModified flag");
            light.Should().NotBeNull();
        }

        [Test]
        public void AddParticleSystem_ShouldSetIsModifiedFlag()
        {
            // Arrange
            var manager = new SceneManager(_sceneContent);
            var entity = manager.CreateEntity("TestEntity");
            entity.IsModified = false;

            // Act
            var particleSystem = entity.AddParticleSystem();

            // Assert
            entity.IsModified.Should().BeTrue("adding ParticleSystemComponent should set IsModified flag");
            particleSystem.Should().NotBeNull();
        }

        [Test]
        public void AddComponent_GenericMethod_ShouldSetIsModifiedFlag()
        {
            // Arrange
            var manager = new SceneManager(_sceneContent);
            var entity = manager.CreateEntity("TestEntity");
            entity.IsModified = false;

            // Act
            var component = entity.AddComponent("ModelComponent");

            // Assert
            entity.IsModified.Should().BeTrue("adding component via generic AddComponent should set IsModified flag");
            component.Should().NotBeNull();
        }

        [Test]
        public void RemoveComponent_ShouldSetIsModifiedFlag()
        {
            // Arrange
            var manager = new SceneManager(_sceneContent);
            var entity = manager.CreateEntity("TestEntity");
            entity.AddModel();
            entity.IsModified = false;

            // Act
            entity.RemoveComponent("ModelComponent");

            // Assert
            entity.IsModified.Should().BeTrue("removing component should set IsModified flag");
        }

        [Test]
        public void CloneComponent_ShouldSetIsModifiedFlag()
        {
            // Arrange
            var manager = new SceneManager(_sceneContent);
            var sourceEntity = manager.CreateEntity("SourceEntity");
            var sourceModel = sourceEntity.AddModel();

            var targetEntity = manager.CreateEntity("TargetEntity");
            targetEntity.IsModified = false;

            // Act
            var clonedComponent = targetEntity.CloneComponent(sourceModel.Component);

            // Assert
            targetEntity.IsModified.Should().BeTrue("cloning component should set IsModified flag");
            clonedComponent.Should().NotBeNull();
        }

        [Test]
        public void AddColliderWithShape_ShouldPersistAfterSurgicalSave()
        {
            // This test verifies the real-world scenario that was failing:
            // Adding a collider with shape to an existing entity and saving with raw content

            // Arrange - Load scene with raw YAML (simulating real-world usage)
            var sceneContent = StrideYamlScene.ParseScene(_testScenePath);
            sceneContent.RawContent.Should().NotBeNullOrEmpty("test scene should have raw YAML content");

            var manager = new SceneManager(sceneContent);

            // Find an existing entity from the scene
            var existingEntity = sceneContent.Entities.FirstOrDefault();
            existingEntity.Should().NotBeNull("test scene should have at least one entity");

            // Verify entity is not initially marked as modified
            existingEntity!.IsModified = false;

            // Act - Add collider with box shape
            var collider = existingEntity.AddStaticCollider();
            collider.AddBoxShape(1.0f, 2.0f, 3.0f);

            // Assert - Entity should be marked as modified
            existingEntity.IsModified.Should().BeTrue(
                "entity with added collider should be marked as modified for surgical save");

            // Verify the collider was actually added
            existingEntity.HasComponent("StaticColliderComponent").Should().BeTrue();
            collider.ColliderShapes.Should().NotBeEmpty();

            // Generate YAML to verify entity would be included in surgical save
            var modifiedEntities = sceneContent.Entities.Where(e => e.IsModified).ToList();
            modifiedEntities.Should().Contain(existingEntity,
                "modified entity should be included in surgical save operation");
        }

        [Test]
        public void AddRigidbodyWithShape_ShouldPersistAfterSurgicalSave()
        {
            // Arrange
            var sceneContent = StrideYamlScene.ParseScene(_testScenePath);
            var existingEntity = sceneContent.Entities.FirstOrDefault();
            existingEntity.Should().NotBeNull();
            existingEntity!.IsModified = false;

            // Act
            var rigidbody = existingEntity.AddRigidbody();
            rigidbody.AddBoxShape(1.0f, 1.0f, 1.0f);

            // Assert
            existingEntity.IsModified.Should().BeTrue(
                "entity with added rigidbody should be marked as modified");

            var modifiedEntities = sceneContent.Entities.Where(e => e.IsModified).ToList();
            modifiedEntities.Should().Contain(existingEntity);
        }

        [Test]
        public void MultipleComponentAdditions_ShouldMaintainIsModifiedFlag()
        {
            // Arrange
            var manager = new SceneManager(_sceneContent);
            var entity = manager.CreateEntity("TestEntity");
            entity.IsModified = false;

            // Act - Add multiple components
            var model = entity.AddModel();
            entity.IsModified.Should().BeTrue("first component addition should set flag");

            var light = entity.AddLight();
            entity.IsModified.Should().BeTrue("flag should remain true after second component");

            var collider = entity.AddStaticCollider();
            entity.IsModified.Should().BeTrue("flag should remain true after third component");

            // Assert
            entity.Components.Should().HaveCount(4); // Transform + Model + Light + StaticCollider
        }

        [Test]
        public void AddExistingComponent_ShouldNotChangeIsModifiedFlag()
        {
            // Arrange
            var manager = new SceneManager(_sceneContent);
            var entity = manager.CreateEntity("TestEntity");
            entity.AddModel();
            entity.IsModified = false; // Reset after first addition

            // Act - Try to add the same component again
            var model = entity.AddModel(); // Should return existing

            // Assert - IsModified should remain false since no new component was added
            entity.IsModified.Should().BeFalse(
                "adding existing component should not modify IsModified flag");
            model.Should().NotBeNull();
        }

        [Test]
        public void ComponentSet_ShouldSetIsModifiedFlag()
        {
            // Arrange
            var manager = new SceneManager(_sceneContent);
            var entity = manager.CreateEntity("TestEntity");
            var transform = entity.GetTransform();
            entity.IsModified = false;

            // Act - Modify component property using Set()
            transform.Component.Set("Position", new Dictionary<string, object>
            {
                { "X", 10.0f },
                { "Y", 20.0f },
                { "Z", 30.0f }
            });

            // Assert
            entity.IsModified.Should().BeTrue(
                "using Component.Set() should mark entity as modified");
        }

        [Test]
        public void WrapperSetPosition_ShouldSetIsModifiedFlag()
        {
            // Arrange
            var manager = new SceneManager(_sceneContent);
            var entity = manager.CreateEntity("TestEntity");
            var transform = entity.GetTransform();
            entity.IsModified = false;

            // Act - Use wrapper method that calls Component.Set()
            transform.SetPosition(100, 200, 300);

            // Assert
            entity.IsModified.Should().BeTrue(
                "wrapper methods that modify component properties should mark entity as modified");
        }

        [Test]
        public void ColliderAddBoxShape_ShouldSetIsModifiedFlag()
        {
            // This tests the scenario where you modify an existing collider
            // (not adding a new one, just modifying its shapes)

            // Arrange
            var manager = new SceneManager(_sceneContent);
            var entity = manager.CreateEntity("TestEntity");
            var collider = entity.AddStaticCollider();

            // Reset flag after creating the collider
            entity.IsModified = false;

            // Act - Add shape to existing collider
            collider.AddBoxShape(1.0f, 2.0f, 3.0f);

            // Assert
            entity.IsModified.Should().BeTrue(
                "adding shapes to existing collider should mark entity as modified");
        }

        [Test]
        public void ComponentSetMultiValueProperty_ShouldSetIsModifiedFlag()
        {
            // Arrange
            var manager = new SceneManager(_sceneContent);
            var entity = manager.CreateEntity("TestEntity");
            var component = entity.AddComponent("ModelComponent");
            entity.IsModified = false;

            // Act
            component.SetMultiValueProperty("Skeleton", new Dictionary<string, object>
            {
                { "Enabled", true }
            });

            // Assert
            entity.IsModified.Should().BeTrue(
                "SetMultiValueProperty should mark entity as modified");
        }

        [Test]
        public void ComponentAddToList_ShouldSetIsModifiedFlag()
        {
            // Arrange
            var manager = new SceneManager(_sceneContent);
            var entity = manager.CreateEntity("TestEntity");
            var component = entity.AddComponent("ModelComponent");
            entity.IsModified = false;

            // Act
            component.AddToList("Materials", "material-guid:path");

            // Assert
            entity.IsModified.Should().BeTrue(
                "AddToList should mark entity as modified");
        }

        [Test]
        public void ComponentSetDictionary_ShouldSetIsModifiedFlag()
        {
            // Arrange
            var manager = new SceneManager(_sceneContent);
            var entity = manager.CreateEntity("TestEntity");
            var component = entity.AddComponent("ModelComponent");
            entity.IsModified = false;

            // Act
            component.SetDictionary("Tags", "key1", "value1");

            // Assert
            entity.IsModified.Should().BeTrue(
                "SetDictionary should mark entity as modified");
        }

        [Test]
        public void ComponentSetList_ShouldSetIsModifiedFlag()
        {
            // Arrange
            var manager = new SceneManager(_sceneContent);
            var entity = manager.CreateEntity("TestEntity");
            var component = entity.AddComponent("ModelComponent");
            entity.IsModified = false;

            // Act
            component.SetList("Materials", new[] { "material1", "material2" });

            // Assert
            entity.IsModified.Should().BeTrue(
                "SetList should mark entity as modified");
        }

        [Test]
        public void RealWorldScenario_ExistingEntityModifyTransform_ShouldPersist()
        {
            // This was the original failing scenario - modifying existing entity's transform

            // Arrange - Load scene with raw content
            var sceneContent = StrideYamlScene.ParseScene(_testScenePath);
            var existingEntity = sceneContent.Entities.FirstOrDefault();
            existingEntity.Should().NotBeNull();
            existingEntity!.IsModified = false;

            // Act - Modify transform using wrapper (common user operation)
            var transform = existingEntity.GetTransform();
            transform.SetPosition(50, 100, 150);

            // Assert
            existingEntity.IsModified.Should().BeTrue(
                "modifying existing entity's transform should mark it for surgical save");

            var modifiedEntities = sceneContent.Entities.Where(e => e.IsModified).ToList();
            modifiedEntities.Should().Contain(existingEntity);
        }
    }
}
