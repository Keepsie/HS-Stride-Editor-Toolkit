// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using NUnit.Framework;
using FluentAssertions;
using HS.Stride.Editor.Toolkit.Core.SceneEditing;

namespace HS.Stride.Editor.Toolkit.Tests
{
    [TestFixture]
    public class CustomComponentTests
    {
        private string _testScenePath;

        [SetUp]
        public void Setup()
        {
            _testScenePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Example Scenes", "MainScene.sdscene");
        }

        [Test]
        public void GetComponent_CustomBackgroundComponent_ShouldLoadAndAccessProperties()
        {
            // Arrange
            var scene = Scene.Load(_testScenePath);
            var entity = scene.FindEntities(e => e.HasComponent("BackgroundComponent")).FirstOrDefault();

            if (entity == null)
            {
                Assert.Inconclusive("No entity with BackgroundComponent found in test scene");
                return;
            }

            // Act
            var backgroundComponent = entity.GetComponent("BackgroundComponent");

            // Assert
            backgroundComponent.Should().NotBeNull();
            backgroundComponent.Type.Should().Be("BackgroundComponent");

            // Test property access
            var textureProperty = backgroundComponent.Get<string>("Texture");
            textureProperty.Should().NotBeNull("Custom component should have accessible properties");
            textureProperty.Should().Contain(":", "Texture property should be in guid:path format");
        }

        [Test]
        public void GetComponent_CustomCharacterComponent_ShouldLoadAndAccessProperties()
        {
            // Arrange
            var scene = Scene.Load(_testScenePath);
            var entity = scene.FindEntities(e => e.HasComponent("CharacterComponent")).FirstOrDefault();

            if (entity == null)
            {
                Assert.Inconclusive("No entity with CharacterComponent found in test scene");
                return;
            }

            // Act
            var characterComponent = entity.GetComponent("CharacterComponent");

            // Assert
            characterComponent.Should().NotBeNull();
            characterComponent.Type.Should().Be("CharacterComponent");

            // Test accessing various property types
            var fallSpeed = characterComponent.Get<float>("FallSpeed");
            fallSpeed.Should().Be(10.0f, "Should read float property correctly");

            var friction = characterComponent.Get<float>("Friction");
            friction.Should().Be(0.5f, "Should read friction property correctly");

            var canSleep = characterComponent.Get<bool>("CanSleep");
            canSleep.Should().BeFalse("Should read boolean property correctly");

            var collisionGroup = characterComponent.Get<string>("CollisionGroup");
            collisionGroup.Should().Be("CharacterFilter", "Should read string property correctly");
        }

        [Test]
        public void SetComponent_CustomCharacterComponent_ShouldModifyProperties()
        {
            // Arrange
            var scene = Scene.Load(_testScenePath);
            var entity = scene.FindEntities(e => e.HasComponent("CharacterComponent")).FirstOrDefault();

            if (entity == null)
            {
                Assert.Inconclusive("No entity with CharacterComponent found in test scene");
                return;
            }

            var characterComponent = entity.GetComponent("CharacterComponent");

            // Act
            characterComponent.Set("FallSpeed", 15.0f);
            characterComponent.Set("Friction", 0.8f);
            characterComponent.Set("CollisionGroup", "CustomFilter");

            // Assert
            characterComponent.Get<float>("FallSpeed").Should().Be(15.0f);
            characterComponent.Get<float>("Friction").Should().Be(0.8f);
            characterComponent.Get<string>("CollisionGroup").Should().Be("CustomFilter");
        }

        [Test]
        public void SaveAndReload_ModifiedCustomComponent_ShouldPersistChanges()
        {
            // Arrange
            var tempPath = Path.Combine(Path.GetTempPath(), $"test_custom_component_{Guid.NewGuid()}.sdscene");

            try
            {
                File.Copy(_testScenePath, tempPath);
                var scene = Scene.Load(tempPath);
                var entity = scene.FindEntities(e => e.HasComponent("CharacterComponent")).FirstOrDefault();

                if (entity == null)
                {
                    Assert.Inconclusive("No entity with CharacterComponent found");
                    return;
                }

                var characterComponent = entity.GetComponent("CharacterComponent");
                characterComponent.Set("FallSpeed", 20.0f);

                // Act
                scene.Save();

                // Assert
                var reloadedScene = Scene.Load(tempPath);
                var reloadedEntity = reloadedScene.FindEntityById(entity.Id);
                var reloadedComponent = reloadedEntity.GetComponent("CharacterComponent");

                reloadedComponent.Get<float>("FallSpeed").Should().Be(20.0f,
                    "Modified custom component property should persist after save/reload");
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        [Test]
        public void FindEntitiesWithComponent_CustomComponent_ShouldFindEntities()
        {
            // Arrange
            var scene = Scene.Load(_testScenePath);

            // Act
            var entitiesWithBackground = scene.FindEntitiesWithComponent("BackgroundComponent");
            var entitiesWithCharacter = scene.FindEntitiesWithComponent("CharacterComponent");

            // Assert
            entitiesWithBackground.Should().NotBeEmpty("Should find entities with BackgroundComponent");
            entitiesWithCharacter.Should().NotBeEmpty("Should find entities with CharacterComponent");

            foreach (var entity in entitiesWithBackground)
            {
                entity.HasComponent("BackgroundComponent").Should().BeTrue();
            }

            foreach (var entity in entitiesWithCharacter)
            {
                entity.HasComponent("CharacterComponent").Should().BeTrue();
            }
        }

        [Test]
        public void CustomComponent_FullNamespacePreservation_ShouldMaintainFullTypeTag()
        {
            // Arrange
            var testScenePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Example Scenes", "Testing_Original.sdscene");

            if (!File.Exists(testScenePath))
            {
                Assert.Inconclusive("Testing_Original.sdscene not found in Example Scenes");
                return;
            }

            var tempPath = Path.Combine(Path.GetTempPath(), $"test_namespace_{Guid.NewGuid()}.sdscene");

            try
            {
                File.Copy(testScenePath, tempPath);
                var scene = Scene.Load(tempPath);

                // Act - Find entity with Tester component
                var entity = scene.FindEntitiesWithComponent("Tester").FirstOrDefault();
                entity.Should().NotBeNull("Should find entity with Tester component");

                var testerComponent = entity.GetComponent("Tester");
                testerComponent.Should().NotBeNull();

                // Modify property
                testerComponent.Set("health", 1000.0f);

                // Save
                scene.Save();

                // Assert - Read raw YAML and verify namespace is preserved
                var savedYaml = File.ReadAllText(tempPath);

                // Should contain the FULL type tag with namespace and assembly
                savedYaml.Should().Contain("!TopDownRPG.Tester,TopDownRPG.Game",
                    "Full namespace and assembly should be preserved in YAML");

                // Should NOT contain just the short name
                savedYaml.Should().NotContain(": !Tester\n",
                    "Should not strip namespace to just short name");

                // Verify the health value was changed
                savedYaml.Should().Contain("health: 1000.0",
                    "Health property should be updated to 1000.0");
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }
    }
}
