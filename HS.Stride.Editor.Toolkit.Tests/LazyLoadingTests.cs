// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using NUnit.Framework;
using FluentAssertions;
using HS.Stride.Editor.Toolkit.Core.SceneEditing;
using HS.Stride.Editor.Toolkit.Core.StrideYamlParser;

namespace HS.Stride.Editor.Toolkit.Tests
{
    [TestFixture]
    public class LazyLoadingTests
    {
        private string _testScenePath;
        private SceneContent _sceneContent;

        [SetUp]
        public void Setup()
        {
            _testScenePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Example Scenes", "Shooting Range.sdscene");
            _sceneContent = StrideYamlScene.ParseScene(_testScenePath);
        }

        [Test]
        public void ParseScene_InitialLoad_ShouldOnlyParseTransformComponent()
        {
            // Arrange
            var entity = _sceneContent.Entities.First();

            // Assert - ALL components are lazy-loaded, so initially Components should be empty
            entity.Components.Should().BeEmpty("All components should be lazy-loaded, not pre-parsed");
        }

        [Test]
        public void GetComponent_NonTransformComponent_ShouldLazyLoadFromRaw()
        {
            // Arrange
            var entity = _sceneContent.Entities.FirstOrDefault(e =>
            {
                // Find entity that has a non-transform component in raw YAML
                var rawContent = _sceneContent.RawContent;
                return rawContent.Contains(e.Id) &&
                       rawContent.Contains("ModelComponent") &&
                       rawContent.Substring(rawContent.IndexOf(e.Id))
                           .IndexOf("ModelComponent") < rawContent.Substring(rawContent.IndexOf(e.Id))
                           .IndexOf("Entity:");
            });

            if (entity == null)
            {
                Assert.Inconclusive("No entity with ModelComponent found in test scene");
                return;
            }

            var initialComponentCount = entity.Components.Count;

            // Act - Access ModelComponent (should trigger lazy load)
            var modelComponent = entity.GetComponent("ModelComponent");

            // Assert
            modelComponent.Should().NotBeNull("Component should be lazy-loaded from raw YAML");
            entity.Components.Count.Should().Be(initialComponentCount + 1, "Lazy-loaded component should be cached");
            entity.Components.Values.Should().Contain(modelComponent, "Lazy-loaded component should be in Components dictionary");
        }

        [Test]
        public void GetComponent_SecondAccess_ShouldReturnCachedComponent()
        {
            // Arrange
            var entity = _sceneContent.Entities.First();

            // Access component first time (lazy load)
            var firstAccess = entity.GetComponent("TransformComponent");
            firstAccess.Should().NotBeNull();

            // Act - Access same component second time (should return cached)
            var secondAccess = entity.GetComponent("TransformComponent");

            // Assert
            secondAccess.Should().BeSameAs(firstAccess, "Second access should return same cached instance");
        }

        [Test]
        public void HasComponent_NonLoadedComponent_ShouldCheckRawYAML()
        {
            // Arrange
            var entity = _sceneContent.Entities.FirstOrDefault(e =>
            {
                var rawContent = _sceneContent.RawContent;
                return rawContent.Contains(e.Id) &&
                       rawContent.Contains("ModelComponent") &&
                       rawContent.Substring(rawContent.IndexOf(e.Id))
                           .IndexOf("ModelComponent") < rawContent.Substring(rawContent.IndexOf(e.Id))
                           .IndexOf("Entity:");
            });

            if (entity == null)
            {
                Assert.Inconclusive("No entity with ModelComponent found in test scene");
                return;
            }

            // Ensure ModelComponent is not loaded yet
            entity.Components.Values.Should().NotContain(c => c.Type.Contains("ModelComponent"));

            // Act - Check if component exists (should check raw YAML)
            var hasComponent = entity.HasComponent("ModelComponent");

            // Assert
            hasComponent.Should().BeTrue("Should find component in raw YAML even if not loaded");
            // Component should still not be loaded after HasComponent check
            entity.Components.Values.Should().NotContain(c => c.Type.Contains("ModelComponent"),
                "HasComponent should not load the component, only check existence");
        }

        [Test]
        public void SceneWriter_WithLazyLoadedComponents_ShouldWriteAccessedComponents()
        {
            // Arrange
            var entity = _sceneContent.Entities.FirstOrDefault(e =>
            {
                var rawContent = _sceneContent.RawContent;
                return rawContent.Contains(e.Id) &&
                       rawContent.Contains("ModelComponent") &&
                       rawContent.Substring(rawContent.IndexOf(e.Id))
                           .IndexOf("ModelComponent") < rawContent.Substring(rawContent.IndexOf(e.Id))
                           .IndexOf("Entity:");
            });

            if (entity == null)
            {
                Assert.Inconclusive("No entity with ModelComponent found in test scene");
                return;
            }

            // Access ModelComponent to load it
            var modelComponent = entity.GetComponent("ModelComponent");
            modelComponent.Should().NotBeNull();

            // Act - Generate YAML
            var yaml = StrideYamlScene.GenerateSceneYaml(_sceneContent);

            // Assert - Generated YAML should include the accessed ModelComponent
            yaml.Should().Contain("ModelComponent", "Accessed component should be written to YAML");
        }

        [Test]
        public void SceneWriter_WithUntouchedComponents_ShouldPreserveFromRaw()
        {
            // Arrange
            var entity = _sceneContent.Entities.FirstOrDefault(e =>
            {
                var rawContent = _sceneContent.RawContent;
                // Find entity with multiple components
                var entitySection = rawContent.Substring(
                    rawContent.IndexOf($"Id: {e.Id}"),
                    rawContent.IndexOf("Entity:", rawContent.IndexOf($"Id: {e.Id}") + 1) -
                    rawContent.IndexOf($"Id: {e.Id}")
                );
                return entitySection.Split(new[] { ": !" }, StringSplitOptions.None).Length > 2;
            });

            if (entity == null)
            {
                Assert.Inconclusive("No entity with multiple components found");
                return;
            }

            // Don't access any non-transform components (keep them untouched)
            var initialComponentCount = entity.Components.Count;

            // Act - Generate YAML
            var yaml = StrideYamlScene.GenerateSceneYaml(_sceneContent);

            // Assert - All components from raw YAML should be preserved
            var rawEntitySection = _sceneContent.RawContent.Substring(
                _sceneContent.RawContent.IndexOf($"Id: {entity.Id}"),
                _sceneContent.RawContent.IndexOf("Entity:", _sceneContent.RawContent.IndexOf($"Id: {entity.Id}") + 1) -
                _sceneContent.RawContent.IndexOf($"Id: {entity.Id}")
            );

            // Count components in raw (lines with ": !")
            var rawComponentCount = rawEntitySection.Split(new[] { ": !" }, StringSplitOptions.None).Length - 1;

            // Count components in generated YAML for this entity
            var yamlEntitySection = yaml.Substring(
                yaml.IndexOf($"Id: {entity.Id}"),
                yaml.IndexOf("Entity:", yaml.IndexOf($"Id: {entity.Id}") + 1) -
                yaml.IndexOf($"Id: {entity.Id}")
            );
            var yamlComponentCount = yamlEntitySection.Split(new[] { ": !" }, StringSplitOptions.None).Length - 1;

            yamlComponentCount.Should().Be(rawComponentCount,
                "All components should be preserved in generated YAML, even untouched ones");
        }
    }
}
