// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using NUnit.Framework;
using FluentAssertions;
using HS.Stride.Editor.Toolkit.Core;
using HS.Stride.Editor.Toolkit.Core.PrefabEditing;

namespace HS.Stride.Editor.Toolkit.Tests
{
    [TestFixture]
    public class PrefabAssetTests
    {
        private string _testPrefabPath;
        private string _testProjectPath;

        [SetUp]
        public void Setup()
        {
            _testPrefabPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Example Assets", "Background.sdprefab");
            _testProjectPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Example Scenes", "TestProject");
        }

        [Test]
        public void Load_ValidPrefabFile_ShouldLoadPrefab()
        {
            // Act
            var prefab = Prefab.Load(_testPrefabPath);

            // Assert
            prefab.Should().NotBeNull();
            prefab.Id.Should().NotBeNullOrEmpty();
            prefab.FilePath.Should().Be(_testPrefabPath);
        }

        [Test]
        public void Load_NonExistentFile_ShouldThrowFileNotFoundException()
        {
            // Arrange
            var nonExistentPath = "NonExistent.sdprefab";

            // Act
            Action act = () => Prefab.Load(nonExistentPath);

            // Assert
            act.Should().Throw<FileNotFoundException>();
        }

        [Test]
        public void AllEntities_ShouldReturnAllEntities()
        {
            // Arrange
            var prefab = Prefab.Load(_testPrefabPath);

            // Act
            var entities = prefab.AllEntities;

            // Assert
            entities.Should().NotBeNull();
            entities.Should().NotBeEmpty();
        }

        [Test]
        public void GetRootEntity_ValidPrefab_ShouldReturnRootEntity()
        {
            // Arrange
            var prefab = Prefab.Load(_testPrefabPath);

            // Act
            var rootEntity = prefab.GetRootEntity();

            // Assert
            rootEntity.Should().NotBeNull();
            rootEntity.Id.Should().NotBeNullOrEmpty();
        }

        [Test]
        public void FindEntityById_ExistingEntity_ShouldReturnEntity()
        {
            // Arrange
            var prefab = Prefab.Load(_testPrefabPath);
            var expectedEntity = prefab.AllEntities.First();

            // Act
            var result = prefab.FindEntityById(expectedEntity.Id);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(expectedEntity);
        }

        [Test]
        public void FindEntityByName_ExistingEntity_ShouldReturnEntity()
        {
            // Arrange
            var prefab = Prefab.Load(_testPrefabPath);
            var expectedEntity = prefab.AllEntities.First();

            // Act
            var result = prefab.FindEntityByName(expectedEntity.Name);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(expectedEntity);
        }

        [Test]
        public void CreateEntity_ValidName_ShouldCreateEntity()
        {
            // Arrange
            var prefab = Prefab.Load(_testPrefabPath);

            // Act
            var entity = prefab.CreateEntity("NewEntity");

            // Assert
            entity.Should().NotBeNull();
            entity.Name.Should().Be("NewEntity");
            prefab.AllEntities.Should().Contain(entity);
        }

        [Test]
        public void RemoveEntity_ExistingEntity_ShouldRemoveEntity()
        {
            // Arrange
            var prefab = Prefab.Load(_testPrefabPath);
            var entity = prefab.CreateEntity("ToRemove");

            // Act
            prefab.RemoveEntity(entity);

            // Assert
            prefab.AllEntities.Should().NotContain(entity);
        }

        [Test]
        public void AddComponent_ValidParameters_ShouldAddComponent()
        {
            // Arrange
            var prefab = Prefab.Load(_testPrefabPath);
            var entity = prefab.CreateEntity("TestEntity");

            // Act
            var component = entity.AddComponent("TestComponent");

            // Assert
            component.Should().NotBeNull();
            component.Type.Should().Be("TestComponent");
            entity.Components.Should().Contain(kvp => kvp.Value == component);
        }

        [Test]
        public void Save_ModifiedPrefab_ShouldSaveToFile()
        {
            // Arrange
            var tempPath = Path.Combine(Path.GetTempPath(), $"test_prefab_{Guid.NewGuid()}.sdprefab");

            try
            {
                File.Copy(_testPrefabPath, tempPath);
                var prefab = Prefab.Load(tempPath);
                var originalCount = prefab.AllEntities.Count;

                // Modify prefab
                prefab.CreateEntity("NewEntity");

                // Act
                prefab.Save();

                // Assert - Reload and verify
                var reloadedPrefab = Prefab.Load(tempPath);
                reloadedPrefab.AllEntities.Count.Should().Be(originalCount + 1);
                reloadedPrefab.FindEntityByName("NewEntity").Should().NotBeNull();
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
            var prefab = Prefab.Load(_testPrefabPath);
            var tempPath = Path.Combine(Path.GetTempPath(), $"test_prefab_{Guid.NewGuid()}.sdprefab");

            try
            {
                prefab.CreateEntity("NewEntity");

                // Act
                prefab.SaveAs(tempPath);

                // Assert
                File.Exists(tempPath).Should().BeTrue();
                var loadedPrefab = Prefab.Load(tempPath);
                loadedPrefab.FindEntityByName("NewEntity").Should().NotBeNull();
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        [Test]
        public void Create_NewPrefab_ShouldCreateEmptyPrefabWithRootEntity()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);

            // Act
            var prefab = project.CreatePrefab("TestPrefab", "Prefabs/TestPrefab");

            // Assert
            prefab.Should().NotBeNull();
            prefab.Id.Should().NotBeNullOrEmpty();
            prefab.AllEntities.Should().HaveCount(1, "should have one root entity");

            var root = prefab.GetRootEntity();
            root.Should().NotBeNull();
            root!.Name.Should().Be("TestPrefab");
            root.HasComponent("TransformComponent").Should().BeTrue("root entity should have TransformComponent");
        }

        [Test]
        public void Create_ThenSave_ShouldWriteValidPrefabFile()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var testPrefabName = $"test_new_prefab_{Guid.NewGuid()}.sdprefab";
            var tempPath = Path.Combine(project.AssetsPath, testPrefabName);

            try
            {
                // Act - Create and save with relative path
                var prefab = project.CreatePrefab("MyCrate", testPrefabName);
                prefab.Save();

                // Assert - File should exist and be loadable
                File.Exists(tempPath).Should().BeTrue();

                var loaded = Prefab.Load(tempPath);
                loaded.Should().NotBeNull();
                loaded.AllEntities.Should().HaveCount(1);
                loaded.GetRootEntity()!.Name.Should().Be("MyCrate");
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        [Test]
        public void Create_AddEntities_ThenSave_ShouldPersistAllEntities()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var testPrefabName = $"test_prefab_with_entities_{Guid.NewGuid()}.sdprefab";
            var tempPath = Path.Combine(project.AssetsPath, testPrefabName);

            try
            {
                // Act - Create prefab with relative path
                var prefab = project.CreatePrefab("House", testPrefabName);

                var root = prefab.GetRootEntity();
                root.Should().NotBeNull();

                // Add child entities
                var door = prefab.CreateEntity("Door", root!.Name, ParentType.Entity);
                var window1 = prefab.CreateEntity("Window1", root.Name, ParentType.Entity);
                var window2 = prefab.CreateEntity("Window2", root.Name, ParentType.Entity);

                // Add components to entities
                door.AddModel();
                door.AddStaticCollider().AddBoxShape(1.0f, 2.0f, 0.1f);

                window1.AddModel();
                window2.AddModel();

                // Save
                prefab.Save();

                // Assert - Reload and verify
                var loaded = Prefab.Load(tempPath);
                loaded.Should().NotBeNull();
                loaded.AllEntities.Should().HaveCount(4, "root + 3 children");

                var loadedRoot = loaded.GetRootEntity();
                loadedRoot.Should().NotBeNull();
                loadedRoot!.Name.Should().Be("House");

                var loadedDoor = loaded.FindEntityByName("Door");
                loadedDoor.Should().NotBeNull();
                loadedDoor!.HasComponent("ModelComponent").Should().BeTrue();
                loadedDoor.HasComponent("StaticColliderComponent").Should().BeTrue();

                loaded.FindEntityByName("Window1").Should().NotBeNull();
                loaded.FindEntityByName("Window2").Should().NotBeNull();
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }
    }
}
