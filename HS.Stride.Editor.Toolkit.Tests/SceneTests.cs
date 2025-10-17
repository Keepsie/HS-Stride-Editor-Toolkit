// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using NUnit.Framework;
using FluentAssertions;
using HS.Stride.Editor.Toolkit.Core.SceneEditing;

namespace HS.Stride.Editor.Toolkit.Tests
{
    [TestFixture]
    public class SceneTests
    {
        private string _testScenePath;

        [SetUp]
        public void Setup()
        {
            _testScenePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Example Scenes", "Scene.sdscene");
        }

        [Test]
        public void Load_ValidSceneFile_ShouldLoadScene()
        {
            // Act
            var scene = Scene.Load(_testScenePath);

            // Assert
            scene.Should().NotBeNull();
            scene.Id.Should().NotBeNullOrEmpty();
            scene.FilePath.Should().Be(_testScenePath);
        }

        [Test]
        public void Load_NonExistentFile_ShouldThrowFileNotFoundException()
        {
            // Arrange
            var nonExistentPath = "NonExistent.sdscene";

            // Act
            Action act = () => Scene.Load(nonExistentPath);

            // Assert
            act.Should().Throw<FileNotFoundException>();
        }

        [Test]
        public void AllEntities_ShouldReturnAllEntities()
        {
            // Arrange
            var scene = Scene.Load(_testScenePath);

            // Act
            var entities = scene.AllEntities;

            // Assert
            entities.Should().NotBeNull();
            entities.Should().NotBeEmpty();
        }

        [Test]
        public void FindEntityById_ExistingEntity_ShouldReturnEntity()
        {
            // Arrange
            var scene = Scene.Load(_testScenePath);
            var expectedEntity = scene.AllEntities.First();

            // Act
            var result = scene.FindEntityById(expectedEntity.Id);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(expectedEntity);
        }

        [Test]
        public void FindEntityByName_ExistingEntity_ShouldReturnEntity()
        {
            // Arrange
            var scene = Scene.Load(_testScenePath);
            var expectedEntity = scene.AllEntities.First();

            // Act
            var result = scene.FindEntityByName(expectedEntity.Name);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(expectedEntity);
        }

        [Test]
        public void CreateEntity_ValidName_ShouldCreateEntity()
        {
            // Arrange
            var scene = Scene.Load(_testScenePath);

            // Act
            var entity = scene.CreateEntity("NewEntity");

            // Assert
            entity.Should().NotBeNull();
            entity.Name.Should().Be("NewEntity");
            scene.AllEntities.Should().Contain(entity);
        }

        [Test]
        public void RemoveEntity_ExistingEntity_ShouldRemoveEntity()
        {
            // Arrange
            var scene = Scene.Load(_testScenePath);
            var entity = scene.CreateEntity("ToRemove");

            // Act
            scene.RemoveEntity(entity);

            // Assert
            scene.AllEntities.Should().NotContain(entity);
        }

        [Test]
        public void AddComponent_ValidParameters_ShouldAddComponent()
        {
            // Arrange
            var scene = Scene.Load(_testScenePath);
            var entity = scene.CreateEntity("TestEntity");

            // Act
            var component = entity.AddComponent("TestComponent");

            // Assert
            component.Should().NotBeNull();
            component.Type.Should().Be("TestComponent");
            entity.Components.Should().Contain(kvp => kvp.Value == component);
        }

        [Test]
        public void Save_ModifiedScene_ShouldSaveToFile()
        {
            // Arrange
            var tempPath = Path.Combine(Path.GetTempPath(), $"test_scene_{Guid.NewGuid()}.sdscene");

            try
            {
                // Copy original to temp location
                File.Copy(_testScenePath, tempPath);
                var scene = Scene.Load(tempPath);
                var originalCount = scene.AllEntities.Count;

                // Modify scene
                scene.CreateEntity("NewEntity");

                // Act
                scene.Save();

                // Assert - Reload and verify
                var reloadedScene = Scene.Load(tempPath);
                reloadedScene.AllEntities.Count.Should().Be(originalCount + 1);
                reloadedScene.FindEntityByName("NewEntity").Should().NotBeNull();
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        [Test]
        public void SaveAs_NewPath_ShouldSaveToNewFile()
        {
            // Arrange
            var scene = Scene.Load(_testScenePath);
            var tempPath = Path.Combine(Path.GetTempPath(), $"test_scene_{Guid.NewGuid()}.sdscene");

            try
            {
                scene.CreateEntity("NewEntity");

                // Act
                scene.SaveAs(tempPath);

                // Assert
                File.Exists(tempPath).Should().BeTrue();
                var loadedScene = Scene.Load(tempPath);
                loadedScene.FindEntityByName("NewEntity").Should().NotBeNull();
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        [Test]
        public void Reload_ModifiedScene_ShouldReloadFromDisk()
        {
            // Arrange
            var tempPath = Path.Combine(Path.GetTempPath(), $"test_scene_{Guid.NewGuid()}.sdscene");

            try
            {
                File.Copy(_testScenePath, tempPath);
                var scene = Scene.Load(tempPath);
                var originalCount = scene.AllEntities.Count;

                // Modify scene in memory (don't save)
                scene.CreateEntity("TemporaryEntity");
                scene.AllEntities.Count.Should().Be(originalCount + 1);

                // Act - Reload from disk
                scene.Reload();

                // Assert - Should revert to original state
                scene.AllEntities.Count.Should().Be(originalCount);
                scene.FindEntityByName("TemporaryEntity").Should().BeNull();
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        [Test]
        public void SaveAndReload_IntegerProperties_ShouldPreserveIntegerType()
        {
            // Arrange
            var tempPath = Path.Combine(Path.GetTempPath(), $"test_scene_{Guid.NewGuid()}.sdscene");

            try
            {
                File.Copy(_testScenePath, tempPath);
                var scene = Scene.Load(tempPath);
                var entity = scene.CreateEntity("TestEntity");
                var component = entity.AddComponent("TestComponent");

                // Set an integer property
                component.Set("intProperty", 3);
                component.Set("floatProperty", 3.5f);

                // Act - Save and reload
                scene.Save();
                var reloadedScene = Scene.Load(tempPath);
                var reloadedEntity = reloadedScene.FindEntityByName("TestEntity");
                var reloadedComponent = reloadedEntity!.GetComponent("TestComponent");

                // Assert - Integer should stay as int, not become float
                var intValue = reloadedComponent!.Get<int>("intProperty");
                var floatValue = reloadedComponent.Get<float>("floatProperty");

                intValue.Should().Be(3);
                floatValue.Should().Be(3.5f);

                // Verify in raw YAML that integer doesn't have decimal point
                var yamlContent = File.ReadAllText(tempPath);
                yamlContent.Should().Contain("intProperty: 3");
                yamlContent.Should().NotContain("intProperty: 3.0");
                yamlContent.Should().Contain("floatProperty: 3.5");
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }
    }
}
