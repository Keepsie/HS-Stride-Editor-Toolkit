// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using NUnit.Framework;
using FluentAssertions;
using HS.Stride.Editor.Toolkit.Core.SceneEditing;
using HS.Stride.Editor.Toolkit.Core.StrideYamlParser;

namespace HS.Stride.Editor.Toolkit.Tests
{
    [TestFixture]
    public class SceneParserTests
    {
        private string _testScenePath;

        [SetUp]
        public void Setup()
        {
            _testScenePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Example Scenes", "Scene.sdscene");
        }

        [Test]
        public void ParseScene_ValidSceneFile_ShouldParseSuccessfully()
        {
            // Act
            var sceneContent = StrideYamlScene.ParseScene(_testScenePath);

            // Assert
            sceneContent.Should().NotBeNull();
            sceneContent.Id.Should().NotBeNullOrEmpty();
            sceneContent.FilePath.Should().Be(_testScenePath);
            sceneContent.RawContent.Should().NotBeNullOrEmpty();
        }

        [Test]
        public void ParseScene_ValidScene_ShouldLazyLoadAllComponents()
        {
            // Arrange & Act
            var sceneContent = StrideYamlScene.ParseScene(_testScenePath);

            // Assert - All components should be lazy-loaded, Components should be empty initially
            sceneContent.Entities.Should().AllSatisfy(entity =>
            {
                entity.Components.Should().BeEmpty("All components should be lazy-loaded, not pre-parsed");
            });
        }

        [Test]
        public void ParseScene_ValidScene_ShouldParseSceneId()
        {
            // Act
            var sceneContent = StrideYamlScene.ParseScene(_testScenePath);

            // Assert
            sceneContent.Id.Should().NotBeNullOrEmpty();
            sceneContent.Id.Should().MatchRegex(@"^[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}$");
        }

        [Test]
        public void ParseScene_ValidScene_ShouldParseRootEntityIds()
        {
            // Act
            var sceneContent = StrideYamlScene.ParseScene(_testScenePath);

            // Assert
            sceneContent.RootEntityIds.Should().NotBeNull();
            sceneContent.RootEntityIds.Should().NotBeEmpty();
        }

        [Test]
        public void ParseScene_ValidScene_ShouldParseEntities()
        {
            // Act
            var sceneContent = StrideYamlScene.ParseScene(_testScenePath);

            // Assert
            sceneContent.Entities.Should().NotBeNull();
            sceneContent.Entities.Should().NotBeEmpty();
            sceneContent.Entities.Should().AllSatisfy(entity =>
            {
                entity.Id.Should().NotBeNullOrEmpty();
                entity.Name.Should().NotBeNullOrEmpty();
            });
        }


        [Test]
        public void ParseScene_ValidScene_ShouldEnableLazyLoading()
        {
            // Act
            var sceneContent = StrideYamlScene.ParseScene(_testScenePath);

            // Assert - Verify lazy loading works by accessing a component
            var entity = sceneContent.Entities.First();
            var component = entity.GetComponent("TransformComponent");
            component.Should().NotBeNull("Lazy loading should be enabled for entities");
        }

        [Test]
        public void ParseComponentFromRaw_ExistingComponent_ShouldParseComponent()
        {
            // Arrange
            var sceneContent = StrideYamlScene.ParseScene(_testScenePath);
            var entity = sceneContent.Entities.First();

            // Act - Try to parse a component that exists (like TransformComponent)
            var component = StrideYamlScene.ParseComponentFromRaw(sceneContent.RawContent, entity.Id, "TransformComponent");

            // Assert
            component.Should().NotBeNull();
            component.Type.Should().Contain("TransformComponent");
            component.Id.Should().NotBeNullOrEmpty();
        }

        [Test]
        public void ParseComponentFromRaw_NonExistentComponent_ShouldReturnNull()
        {
            // Arrange
            var sceneContent = StrideYamlScene.ParseScene(_testScenePath);
            var entity = sceneContent.Entities.First();

            // Act
            var component = StrideYamlScene.ParseComponentFromRaw(sceneContent.RawContent, entity.Id, "NonExistentComponent");

            // Assert
            component.Should().BeNull();
        }

        [Test]
        public void ParseScene_NonExistentFile_ShouldThrowFileNotFoundException()
        {
            // Arrange
            var nonExistentPath = "NonExistent.sdscene";

            // Act
            Action act = () => StrideYamlScene.ParseScene(nonExistentPath);

            // Assert
            act.Should().Throw<FileNotFoundException>();
        }

        [Test]
        public void ParseScene_EmptyFile_ShouldHandleGracefully()
        {
            // Arrange
            var tempPath = Path.Combine(Path.GetTempPath(), $"empty_scene_{Guid.NewGuid()}.sdscene");

            try
            {
                File.WriteAllText(tempPath, string.Empty);

                // Act
                var sceneContent = StrideYamlScene.ParseScene(tempPath);

                // Assert - The implementation handles empty files gracefully
                sceneContent.Should().NotBeNull();
                sceneContent.FilePath.Should().Be(tempPath);
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        [Test]
        public void ParseScene_MalformedYaml_ShouldHandleGracefully()
        {
            // Arrange
            var tempPath = Path.Combine(Path.GetTempPath(), $"malformed_scene_{Guid.NewGuid()}.sdscene");

            try
            {
                // Create intentionally malformed YAML
                File.WriteAllText(tempPath, "!SceneAsset\nId: invalid\n  broken: yaml: structure");

                // Act
                var sceneContent = StrideYamlScene.ParseScene(tempPath);

                // Assert - The implementation handles malformed YAML gracefully
                sceneContent.Should().NotBeNull();
                sceneContent.FilePath.Should().Be(tempPath);
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        [Test]
        public void ParseScene_SceneWithNoEntities_ShouldReturnEmptyEntityList()
        {
            // Arrange
            var tempPath = Path.Combine(Path.GetTempPath(), $"empty_entities_scene_{Guid.NewGuid()}.sdscene");

            try
            {
                // Create a minimal valid scene with no entities
                var minimalScene = @"!SceneAsset
Id: 12345678-1234-1234-1234-123456789012
SerializedVersion: {Stride: 3.1.0.1}
Tags: []
ChildrenIds: []
Offset: {X: 0.0, Y: 0.0, Z: 0.0}
Hierarchy:
    RootParts: []
    Parts: []";
                File.WriteAllText(tempPath, minimalScene);

                // Act
                var sceneContent = StrideYamlScene.ParseScene(tempPath);

                // Assert
                sceneContent.Should().NotBeNull();
                sceneContent.Entities.Should().BeEmpty();
                sceneContent.RootEntityIds.Should().BeEmpty();
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        [Test]
        public void ParseScene_EntityWithNoComponents_ShouldCreateEntityWithEmptyComponents()
        {
            // Arrange
            var tempPath = Path.Combine(Path.GetTempPath(), $"no_components_scene_{Guid.NewGuid()}.sdscene");

            try
            {
                // Create a scene with an entity that has no components
                var sceneWithNoComponents = @"!SceneAsset
Id: 12345678-1234-1234-1234-123456789012
SerializedVersion: {Stride: 3.1.0.1}
Tags: []
ChildrenIds: []
Offset: {X: 0.0, Y: 0.0, Z: 0.0}
Hierarchy:
    RootParts:
        - ref!! 11111111-1111-1111-1111-111111111111
    Parts:
        - Entity:
            Id: 11111111-1111-1111-1111-111111111111
            Name: EmptyEntity
            Components: {}";
                File.WriteAllText(tempPath, sceneWithNoComponents);

                // Act
                var sceneContent = StrideYamlScene.ParseScene(tempPath);

                // Assert
                sceneContent.Should().NotBeNull();
                sceneContent.Entities.Should().ContainSingle();
                sceneContent.Entities.First().Components.Should().BeEmpty();
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }
    }
}
