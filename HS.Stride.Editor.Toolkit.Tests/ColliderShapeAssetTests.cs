// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using NUnit.Framework;
using FluentAssertions;
using HS.Stride.Editor.Toolkit.Core;
using HS.Stride.Editor.Toolkit.Core.AssetEditing;
using HS.Stride.Editor.Toolkit.Core.SceneEditing;
using HS.Stride.Editor.Toolkit.Core.StrideYamlParser;

namespace HS.Stride.Editor.Toolkit.Tests
{
    [TestFixture]
    public class ColliderShapeAssetTests
    {
        private string _testAssetsPath;
        private SceneContent _sceneContent;

        [SetUp]
        public void Setup()
        {
            _testAssetsPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Example Assets");
            var testScenePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Example Scenes", "Scene.sdscene");
            _sceneContent = StrideYamlScene.ParseScene(testScenePath);
        }

        [Test]
        public void CreateConvexHull_WithAssetReference_ShouldCreateValidAsset()
        {
            // Arrange
            var modelAsset = new AssetReference
            {
                Id = "test-model-guid",
                Path = "TestModel",
                Type = AssetType.Model
            };

            // Act
            var colliderAsset = ColliderShapeAsset.CreateConvexHull(modelAsset, "TestColliderHull");

            // Assert
            colliderAsset.Should().NotBeNull();
            colliderAsset.Id.Should().NotBeNullOrEmpty();
        }

        [Test]
        public void CreateConvexHull_WithDifferentModelAsset_ShouldCreateValidAsset()
        {
            // Arrange
            var modelAsset = new AssetReference
            {
                Id = "different-model-guid",
                Path = "DifferentTestModel",
                Type = AssetType.Model
            };

            // Act
            var colliderAsset = ColliderShapeAsset.CreateConvexHull(modelAsset, "TestColliderHull");

            // Assert
            colliderAsset.Should().NotBeNull();
            colliderAsset.Id.Should().NotBeNullOrEmpty();
        }

        [Test]
        public void Save_WithValidPath_ShouldCreateFile()
        {
            // Arrange
            var modelAsset = new AssetReference
            {
                Id = "test-model-guid",
                Path = "TestModel",
                Type = AssetType.Model
            };
            var colliderAsset = ColliderShapeAsset.CreateConvexHull(modelAsset);
            var tempPath = Path.Combine(Path.GetTempPath(), $"TestCollider_{Guid.NewGuid()}.sdphy");

            try
            {
                // Act
                colliderAsset.Save(tempPath);

                // Assert
                File.Exists(tempPath).Should().BeTrue();

                var content = File.ReadAllText(tempPath);
                content.Should().Contain("!ColliderShapeAsset");
                content.Should().Contain("!ConvexHullColliderShapeDesc");
                content.Should().Contain("test-model-guid:TestModel");
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        [Test]
        public void Save_WithoutSdphyExtension_ShouldThrowException()
        {
            // Arrange
            var modelAsset = new AssetReference
            {
                Id = "test-guid",
                Path = "test-model",
                Type = AssetType.Model
            };
            var colliderAsset = ColliderShapeAsset.CreateConvexHull(modelAsset);
            var invalidPath = Path.Combine(Path.GetTempPath(), "TestCollider.txt");

            // Act & Assert
            Assert.Throws<ArgumentException>(() => colliderAsset.Save(invalidPath));
        }

        [Test]
        public void SavedAsset_ShouldContainAllRequiredFields()
        {
            // Arrange
            var modelAsset = new AssetReference
            {
                Id = "test-model-guid",
                Path = "TestModel",
                Type = AssetType.Model
            };
            var colliderAsset = ColliderShapeAsset.CreateConvexHull(modelAsset);
            var tempPath = Path.Combine(Path.GetTempPath(), $"TestCollider_{Guid.NewGuid()}.sdphy");

            try
            {
                // Act
                colliderAsset.Save(tempPath);
                var content = File.ReadAllText(tempPath);

                // Assert - Check for all required fields
                content.Should().Contain("LocalOffset:");
                content.Should().Contain("LocalRotation:");
                content.Should().Contain("Scaling:");
                content.Should().Contain("Margin: 0.04");
                content.Should().Contain("Decomposition:");
                content.Should().Contain("Depth: 10");
                content.Should().Contain("ConvexHulls: null");
                content.Should().Contain("ConvexHullsIndices: null");
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        [Test]
        public void GetReference_AfterSave_ShouldReturnValidReference()
        {
            // Arrange
            var modelAsset = new AssetReference
            {
                Id = "test-model-guid",
                Path = "TestModel",
                Type = AssetType.Model
            };
            var colliderAsset = ColliderShapeAsset.CreateConvexHull(modelAsset, "TestColliderHull");
            var tempDir = Path.Combine(Path.GetTempPath(), $"TestAssets_{Guid.NewGuid()}");
            var tempPath = Path.Combine(tempDir, "Assets", "TestCollider.sdphy");

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(tempPath));
                colliderAsset.Save(tempPath);

                // Act
                var reference = colliderAsset.GetReference();

                // Assert
                reference.Should().NotBeNull();
                reference.Id.Should().Be(colliderAsset.Id);
                reference.Type.Should().Be(AssetType.ColliderShape);
                reference.Path.Should().NotBeNullOrEmpty();
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Test]
        public void GetReference_BeforeSave_ShouldThrowException()
        {
            // Arrange
            var modelAsset = new AssetReference
            {
                Id = "test-guid",
                Path = "test-model",
                Type = AssetType.Model
            };
            var colliderAsset = ColliderShapeAsset.CreateConvexHull(modelAsset);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => colliderAsset.GetReference());
        }

        [Test]
        public void AddColliderShapeAsset_ToStaticCollider_ShouldAddReference()
        {
            // Arrange
            var manager = new SceneManager(_sceneContent);
            var entity = manager.CreateEntity("TestEntity");
            var collider = entity.AddStaticCollider();

            var colliderShapeRef = new AssetReference
            {
                Id = "test-collider-guid",
                Path = "TestColliderHull",
                Type = AssetType.ColliderShape
            };

            // Act
            collider.AddColliderShapeAsset(colliderShapeRef);

            // Assert
            collider.ColliderShapes.Should().NotBeEmpty();
            var shape = collider.ColliderShapes.First().Value as Dictionary<string, object>;
            shape.Should().NotBeNull();
            shape.Should().ContainKey("!ColliderShapeAssetDesc");
            shape["Shape"].Should().Be("test-collider-guid:TestColliderHull");
        }

        [Test]
        public void AddColliderShapeAsset_ToRigidbody_ShouldAddReference()
        {
            // Arrange
            var manager = new SceneManager(_sceneContent);
            var entity = manager.CreateEntity("TestEntity");
            var rigidbody = entity.AddRigidbody();

            var colliderShapeRef = new AssetReference
            {
                Id = "test-collider-guid",
                Path = "TestColliderHull",
                Type = AssetType.ColliderShape
            };

            // Act
            rigidbody.AddColliderShapeAsset(colliderShapeRef);

            // Assert
            rigidbody.ColliderShapes.Should().NotBeEmpty();
            var shape = rigidbody.ColliderShapes.First().Value as Dictionary<string, object>;
            shape.Should().NotBeNull();
            shape.Should().ContainKey("!ColliderShapeAssetDesc");
        }

        [Test]
        public void AddColliderShapeAsset_WithGuidAndPath_ShouldAddReference()
        {
            // Arrange
            var manager = new SceneManager(_sceneContent);
            var entity = manager.CreateEntity("TestEntity");
            var collider = entity.AddStaticCollider();

            // Act
            collider.AddColliderShapeAsset("test-collider-guid", "TestColliderHull");

            // Assert
            collider.ColliderShapes.Should().NotBeEmpty();
            var shape = collider.ColliderShapes.First().Value as Dictionary<string, object>;
            shape.Should().NotBeNull();
            shape["Shape"].Should().Be("test-collider-guid:TestColliderHull");
        }

        [Test]
        public void CompleteWorkflow_CreateAssetAndAddToScene_ShouldWork()
        {
            // Arrange
            var modelAsset = new AssetReference
            {
                Id = "test-model-guid",
                Path = "TestModel",
                Type = AssetType.Model
            };
            var tempDir = Path.Combine(Path.GetTempPath(), $"TestWorkflow_{Guid.NewGuid()}");
            var tempAssetPath = Path.Combine(tempDir, "Assets", "TestCollider.sdphy");
            var tempScenePath = Path.Combine(tempDir, "TestScene.sdscene");

            try
            {
                // Create collider asset
                var colliderAsset = ColliderShapeAsset.CreateConvexHull(modelAsset, "TestColliderHull");
                Directory.CreateDirectory(Path.GetDirectoryName(tempAssetPath));
                colliderAsset.Save(tempAssetPath);

                // Create scene and add reference
                var manager = new SceneManager(_sceneContent);
                var entity = manager.CreateEntity("TestObject");
                entity.AddStaticCollider().AddColliderShapeAsset(colliderAsset.GetReference());

                // Save scene
                var yaml = StrideYamlScene.GenerateSceneYaml(_sceneContent);
                File.WriteAllText(tempScenePath, yaml);

                // Assert
                File.Exists(tempAssetPath).Should().BeTrue();
                File.Exists(tempScenePath).Should().BeTrue();

                var sceneContent = File.ReadAllText(tempScenePath);
                sceneContent.Should().Contain("!ColliderShapeAssetDesc");
                sceneContent.Should().Contain(colliderAsset.Id);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Test]
        public void LoadExistingColliderAsset_ShouldWork()
        {
            // Arrange - Use existing test asset
            var existingAssetPath = Path.Combine(_testAssetsPath, "ColliderCone.sdphy");

            // Act
            var loadedAsset = ColliderShapeAsset.Load(existingAssetPath);

            // Assert
            loadedAsset.Should().NotBeNull();
            loadedAsset.Id.Should().Be("e8baa721-503d-4fba-bc06-eaaadf8da091");
            loadedAsset.FilePath.Should().Be(existingAssetPath);
        }

        [Test]
        public void AddColliderShapeAsset_MarksEntityAsModified()
        {
            // This ensures the IsModified bug fix applies to collider shape assets too

            // Arrange
            var manager = new SceneManager(_sceneContent);
            var entity = manager.CreateEntity("TestEntity");
            var collider = entity.AddStaticCollider();
            entity.IsModified = false; // Reset after creation

            // Act
            collider.AddColliderShapeAsset("test-guid", "TestCollider");

            // Assert
            entity.IsModified.Should().BeTrue(
                "adding collider shape asset should mark entity as modified for surgical save");
        }
    }
}
