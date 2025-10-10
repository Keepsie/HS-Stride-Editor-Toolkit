// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using NUnit.Framework;
using FluentAssertions;
using HS.Stride.Editor.Toolkit.Core.SceneEditing;
using HS.Stride.Editor.Toolkit.Core.AssetEditing;
using HS.Stride.Editor.Toolkit.Core.StrideYamlParser;

namespace HS.Stride.Editor.Toolkit.Tests
{
    [TestFixture]
    public class ColliderTests
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
        public void AddBoxShape_ValidParameters_ShouldAddBoxColliderShape()
        {
            // Arrange
            var manager = new SceneManager(_sceneContent);
            var entity = manager.CreateEntity("BoxColliderTest");

            // Act
            var collider = entity.AddStaticCollider();
            collider.AddBoxShape(2.0f, 3.0f, 4.0f);

            // Assert
            collider.ColliderShapes.Should().ContainSingle();
            var shape = collider.ColliderShapes.First().Value as Dictionary<string, object>;
            shape.Should().NotBeNull();
            shape.Should().ContainKey("!BoxColliderShapeDesc");

            var size = shape["Size"] as Dictionary<string, object>;
            size.Should().NotBeNull();
            size["X"].Should().Be(2.0f);
            size["Y"].Should().Be(3.0f);
            size["Z"].Should().Be(4.0f);
        }

        [Test]
        public void AddSphereShape_ValidParameters_ShouldAddSphereColliderShape()
        {
            // Arrange
            var manager = new SceneManager(_sceneContent);
            var entity = manager.CreateEntity("SphereColliderTest");

            // Act
            var collider = entity.AddStaticCollider();
            collider.AddSphereShape(radius: 1.5f);

            // Assert
            collider.ColliderShapes.Should().ContainSingle();
            var shape = collider.ColliderShapes.First().Value as Dictionary<string, object>;
            shape.Should().NotBeNull();
            shape.Should().ContainKey("!SphereColliderShapeDesc");
            shape["Radius"].Should().Be(1.5f);
        }

        [Test]
        public void AddCapsuleShape_ValidParameters_ShouldAddCapsuleColliderShape()
        {
            // Arrange
            var manager = new SceneManager(_sceneContent);
            var entity = manager.CreateEntity("CapsuleColliderTest");

            // Act
            var collider = entity.AddStaticCollider();
            collider.AddCapsuleShape(length: 2.0f, radius: 0.5f, orientation: "UpY");

            // Assert
            collider.ColliderShapes.Should().ContainSingle();
            var shape = collider.ColliderShapes.First().Value as Dictionary<string, object>;
            shape.Should().NotBeNull();
            shape.Should().ContainKey("!CapsuleColliderShapeDesc");
            shape["Length"].Should().Be(2.0f);
            shape["Radius"].Should().Be(0.5f);
            shape["Orientation"].Should().Be("UpY");
        }

        [Test]
        public void AddCylinderShape_ValidParameters_ShouldAddCylinderColliderShape()
        {
            // Arrange
            var manager = new SceneManager(_sceneContent);
            var entity = manager.CreateEntity("CylinderColliderTest");

            // Act
            var collider = entity.AddStaticCollider();
            collider.AddCylinderShape(height: 3.0f, radius: 1.0f);

            // Assert
            collider.ColliderShapes.Should().ContainSingle();
            var shape = collider.ColliderShapes.First().Value as Dictionary<string, object>;
            shape.Should().NotBeNull();
            shape.Should().ContainKey("!CylinderColliderShapeDesc");
            shape["Height"].Should().Be(3.0f);
            shape["Radius"].Should().Be(1.0f);
        }

        [Test]
        public void AddConeShape_ValidParameters_ShouldAddConeColliderShape()
        {
            // Arrange
            var manager = new SceneManager(_sceneContent);
            var entity = manager.CreateEntity("ConeColliderTest");

            // Act
            var collider = entity.AddStaticCollider();
            collider.AddConeShape(height: 2.5f, radius: 1.2f);

            // Assert
            collider.ColliderShapes.Should().ContainSingle();
            var shape = collider.ColliderShapes.First().Value as Dictionary<string, object>;
            shape.Should().NotBeNull();
            shape.Should().ContainKey("!ConeColliderShapeDesc");
            shape["Height"].Should().Be(2.5f);
            shape["Radius"].Should().Be(1.2f);
        }

        [Test]
        public void AddMeshShape_ValidAssetReference_ShouldAddStaticMeshColliderShape()
        {
            // Arrange
            var manager = new SceneManager(_sceneContent);
            var entity = manager.CreateEntity("MeshColliderTest");

            var modelAsset = new AssetReference
            {
                Id = "8eb7ebe2-c50f-4048-b944-b755cbd38808",
                Path = "Models/Box1x1x1",
                Type = AssetType.Model
            };

            // Act
            var collider = entity.AddStaticCollider();
            collider.AddMeshShape(modelAsset);

            // Assert
            collider.ColliderShapes.Should().ContainSingle();
            var shape = collider.ColliderShapes.First().Value as Dictionary<string, object>;
            shape.Should().NotBeNull();
            shape.Should().ContainKey("!StaticMeshColliderShapeDesc");
            shape["Model"].Should().Be("8eb7ebe2-c50f-4048-b944-b755cbd38808:Models/Box1x1x1");
        }

        [Test]
        public void AddConvexHullShape_ValidAssetReference_ShouldAddConvexHullColliderShape()
        {
            // Arrange
            var manager = new SceneManager(_sceneContent);
            var entity = manager.CreateEntity("ConvexHullColliderTest");

            var modelAsset = new AssetReference
            {
                Id = "8eb7ebe2-c50f-4048-b944-b755cbd38808",
                Path = "Models/Box1x1x1",
                Type = AssetType.Model
            };

            // Act
            var collider = entity.AddStaticCollider();
            collider.AddConvexHullShape(modelAsset);

            // Assert
            collider.ColliderShapes.Should().ContainSingle();
            var shape = collider.ColliderShapes.First().Value as Dictionary<string, object>;
            shape.Should().NotBeNull();
            shape.Should().ContainKey("!ConvexHullColliderShapeDesc");
            shape["Model"].Should().Be("8eb7ebe2-c50f-4048-b944-b755cbd38808:Models/Box1x1x1");
        }

        [Test]
        public void AddPlaneShape_ValidParameters_ShouldAddStaticPlaneColliderShape()
        {
            // Arrange
            var manager = new SceneManager(_sceneContent);
            var entity = manager.CreateEntity("PlaneColliderTest");

            // Act
            var collider = entity.AddStaticCollider();
            collider.AddPlaneShape(normalY: 1.0f, offset: 0.0f);

            // Assert
            collider.ColliderShapes.Should().ContainSingle();
            var shape = collider.ColliderShapes.First().Value as Dictionary<string, object>;
            shape.Should().NotBeNull();
            shape.Should().ContainKey("!StaticPlaneColliderShapeDesc");

            var normal = shape["Normal"] as Dictionary<string, object>;
            normal.Should().NotBeNull();
            normal["Y"].Should().Be(1.0f);
        }

        [Test]
        public void AddMultipleShapes_ValidParameters_ShouldAddAllShapesToSingleCollider()
        {
            // Arrange
            var manager = new SceneManager(_sceneContent);
            var entity = manager.CreateEntity("MultiShapeColliderTest");

            var modelAsset = new AssetReference
            {
                Id = "test-guid",
                Path = "Models/TestModel",
                Type = AssetType.Model
            };

            // Act
            var collider = entity.AddStaticCollider();
            collider.AddBoxShape(1.0f, 1.0f, 1.0f);
            collider.AddSphereShape(0.5f);
            collider.AddMeshShape(modelAsset);

            // Assert
            collider.ColliderShapes.Should().HaveCount(3);

            var shapes = collider.ColliderShapes.Values.Cast<Dictionary<string, object>>().ToList();
            shapes.Should().Contain(s => s.ContainsKey("!BoxColliderShapeDesc"));
            shapes.Should().Contain(s => s.ContainsKey("!SphereColliderShapeDesc"));
            shapes.Should().Contain(s => s.ContainsKey("!StaticMeshColliderShapeDesc"));
        }


        [Test]
        public void RigidbodyCollider_AllShapes_ShouldWorkIdentically()
        {
            // Arrange
            var manager = new SceneManager(_sceneContent);
            var entity = manager.CreateEntity("RigidbodyColliderTest");

            var modelAsset = new AssetReference
            {
                Id = "rigidbody-model",
                Path = "Models/PhysicsObject",
                Type = AssetType.Model
            };

            // Act
            var rigidbody = entity.AddRigidbody(mass: 10.0f);
            rigidbody.AddBoxShape(1.0f, 1.0f, 1.0f);
            rigidbody.AddSphereShape(0.5f);
            rigidbody.AddConvexHullShape(modelAsset);

            // Assert
            rigidbody.ColliderShapes.Should().HaveCount(3);
            rigidbody.Mass.Should().Be(10.0f);
            rigidbody.IsKinematic.Should().BeFalse();

            var shapes = rigidbody.ColliderShapes.Values.Cast<Dictionary<string, object>>().ToList();
            shapes.Should().Contain(s => s.ContainsKey("!BoxColliderShapeDesc"));
            shapes.Should().Contain(s => s.ContainsKey("!SphereColliderShapeDesc"));
            shapes.Should().Contain(s => s.ContainsKey("!ConvexHullColliderShapeDesc"));
        }

        [Test]
        public void BatchAddColliders_MultipleEntities_ShouldAddToAll()
        {
            // Arrange
            var tempPath = Path.Combine(Path.GetTempPath(), $"batch_colliders_{Guid.NewGuid()}.sdscene");

            try
            {
                var manager = new SceneManager(_sceneContent);

                // Create multiple entities
                for (int i = 0; i < 10; i++)
                {
                    var entity = manager.CreateEntity($"BatchEntity_{i}");
                    var collider = entity.AddStaticCollider();
                    collider.AddBoxShape(1.0f, 1.0f, 1.0f);
                }

                // Act - Save
                var yaml = StrideYamlScene.GenerateSceneYaml(_sceneContent);
                File.WriteAllText(tempPath, yaml);

                // Reload and verify
                var reloaded = StrideYamlScene.ParseScene(tempPath);

                // Assert
                for (int i = 0; i < 10; i++)
                {
                    var reloadedEntity = reloaded.Entities.First(e => e.Name == $"BatchEntity_{i}");
                    var collider = reloadedEntity.GetStaticCollider();
                    collider.Should().NotBeNull();
                    collider!.ColliderShapes.Should().ContainSingle();
                }
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }
    }
}
