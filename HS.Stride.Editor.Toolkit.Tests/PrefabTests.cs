// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using NUnit.Framework;
using FluentAssertions;
using HS.Stride.Editor.Toolkit.Core;
using HS.Stride.Editor.Toolkit.Core.AssetEditing;
using HS.Stride.Editor.Toolkit.Core.SceneEditing;

namespace HS.Stride.Editor.Toolkit.Tests
{
    [TestFixture]
    public class PrefabTests
    {
        private string _testProjectPath;

        [SetUp]
        public void Setup()
        {
            _testProjectPath = @"E:\Github\HS-Stride-Editor-Toolkit\HS.Stride.Editor.Toolkit.Tests\Example Scenes\TestProject\";
        }

        [Test]
        public void LoadPrefabAsset_ShouldParseAllEntities()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);

            // Act
            var prefab = project.LoadPrefab("CubeGroupPrefab");

            // Assert
            prefab.Should().NotBeNull();
            prefab.AllEntities.Should().HaveCount(7, "CubeGroupPrefab has 7 entities");

            // Check root entity
            var rootEntity = prefab.AllEntities.FirstOrDefault(e => e.Name == "CubeGroupPrefab");
            rootEntity.Should().NotBeNull();
            rootEntity!.ParentPrefab.Should().BeNull("Root entity should not be a prefab instance");
        }

        [Test]
        public void LoadPrefabAsset_ShouldParseNestedPrefabInstances()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);

            // Act
            var prefab = project.LoadPrefab("CubeGroupPrefab");

            // Assert - Check that some entities are LootBox prefab instances
            var lootBoxInstances = prefab.AllEntities.Where(e => e.ParentPrefab != null && e.Name == "LootBox").ToList();
            lootBoxInstances.Should().HaveCountGreaterThan(0, "Should have LootBox prefab instances within CubeGroupPrefab");

            foreach (var instance in lootBoxInstances)
            {
                instance.ParentPrefab.Should().NotBeNull();
                instance.ParentPrefab!.PrefabSourcePath.Should().Contain("LootBox");
                instance.ParentPrefab.PrefabEntityId.Should().NotBeNullOrEmpty();
                instance.ParentPrefab.InstanceId.Should().NotBeNullOrEmpty();
            }
        }

        [Test]
        public void Scene_LoadPrefabInstance_ShouldHaveBaseData()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var scene = project.LoadScene("Testing");

            // Act - Find the CubeGroupPrefab instance in the scene
            var prefabInstance = scene.FindEntityByName("CubeGroupPrefab");

            // Assert
            prefabInstance.Should().NotBeNull();
            prefabInstance!.ParentPrefab.Should().NotBeNull("Entity instantiated from prefab should have Base data");
            prefabInstance.ParentPrefab!.PrefabSourcePath.Should().Be("28904097-13e5-4f96-86b4-0f5a42785fc3:CubeGroupPrefab");
            prefabInstance.ParentPrefab.PrefabEntityId.Should().Be("902c2f90-a323-4301-8c41-d541fc705517");
            prefabInstance.ParentPrefab.InstanceId.Should().Be("b55a19a6-e4e7-4575-abed-a7266bf7d397");
        }

        [Test]
        public void Scene_PrefabInstance_AllChildrenShareSameInstanceId()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var scene = project.LoadScene("Testing");

            // Act - Get all entities from the CubeGroupPrefab instance
            var prefabChildren = scene.AllEntities
                .Where(e => e.ParentPrefab != null &&
                           e.ParentPrefab.PrefabSourcePath.Contains("CubeGroupPrefab"))
                .ToList();

            // Assert
            prefabChildren.Should().HaveCountGreaterThan(0);

            // All children should share the same InstanceId
            var instanceIds = prefabChildren.Select(e => e.ParentPrefab!.InstanceId).Distinct().ToList();
            instanceIds.Should().HaveCount(1, "All entities from same prefab instance should share InstanceId");
            instanceIds.First().Should().Be("b55a19a6-e4e7-4575-abed-a7266bf7d397");
        }

        [Test]
        public void PrefabAsset_ModifyAndSave_ShouldPersist()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var prefab = project.LoadPrefab("CubeGroupPrefab");
            var rootEntity = prefab.AllEntities.First(e => e.Name == "CubeGroupPrefab");
            var transform = rootEntity.GetTransform();

            var testPrefabName = $"test_prefab_{Guid.NewGuid()}.sdprefab";
            var tempPath = Path.Combine(project.AssetsPath, testPrefabName);

            try
            {
                // Act - Modify prefab
                transform!.SetPosition(5.0f, 10.0f, 15.0f);

                prefab.SaveAs(testPrefabName);

                // Reload and verify
                var reloaded = project.LoadPrefab(tempPath);
                var reloadedRoot = reloaded.AllEntities.First(e => e.Name == "CubeGroupPrefab");
                var reloadedTransform = reloadedRoot.GetTransform();

                // Assert
                reloadedTransform.Should().NotBeNull();
                var position = reloadedTransform!.GetPosition();
                position.X.Should().Be(5.0f);
                position.Y.Should().Be(10.0f);
                position.Z.Should().Be(15.0f);
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        [Test]
        public void Scene_ModifyPrefabInstanceComponent_ShouldPreserveBaseData()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var scene = project.LoadScene("Testing");
            var prefabInstance = scene.FindEntityByName("CubeGroupPrefab");
            var transform = prefabInstance!.GetTransform();

            var testSceneName = $"test_scene_{Guid.NewGuid()}.sdscene";
            var tempPath = Path.Combine(project.AssetsPath, testSceneName);

            try
            {
                // Act - Modify the prefab instance
                transform!.SetPosition(100.0f, 0.0f, 0.0f);

                scene.SaveAs(testSceneName);

                // Reload and verify
                var reloaded = Scene.Load(tempPath);
                var reloadedInstance = reloaded.FindEntityByName("CubeGroupPrefab");

                // Assert - Component change should persist
                var reloadedTransform = reloadedInstance!.GetTransform();
                var reloadedPos = reloadedTransform!.GetPosition();
                reloadedPos.X.Should().Be(100.0f);

                // Base data should still be intact
                reloadedInstance.ParentPrefab.Should().NotBeNull();
                reloadedInstance.ParentPrefab!.PrefabSourcePath.Should().Be("28904097-13e5-4f96-86b4-0f5a42785fc3:CubeGroupPrefab");
                reloadedInstance.ParentPrefab.PrefabEntityId.Should().Be("902c2f90-a323-4301-8c41-d541fc705517");
                reloadedInstance.ParentPrefab.InstanceId.Should().Be("b55a19a6-e4e7-4575-abed-a7266bf7d397");
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }


        [Test]
        public void PrefabAsset_AddComponentToEntity_ShouldWork()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var prefab = project.LoadPrefab("CubeGroupPrefab");
            var cubeEntity = prefab.AllEntities.First(e => e.Name == "Cube (2)");

            var testPrefabName = $"test_prefab_component_{Guid.NewGuid()}.sdprefab";
            var tempPath = Path.Combine(project.AssetsPath, testPrefabName);

            try
            {
                // Act - Add a custom component
                var customComp = cubeEntity.AddComponent("SimpleScript");
                customComp.Set("health", 500);
                customComp.Set("speed", 9.5f);

                prefab.SaveAs(testPrefabName);

                // Reload and verify
                var reloaded = project.LoadPrefab(tempPath);
                var reloadedCube = reloaded.AllEntities.First(e => e.Name == "Cube (2)");
                var reloadedComp = reloadedCube.GetComponent("SimpleScript");

                // Assert
                reloadedComp.Should().NotBeNull();
                reloadedComp!.Get<int>("health").Should().Be(500);
                reloadedComp.Get<float>("speed").Should().Be(9.5f);
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        [Test]
        public void PrefabAsset_EntityHierarchy_ShouldBePreserved()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var prefab = project.LoadPrefab("CubeGroupPrefab");

            // Act - Get root entity and check its children references
            var rootEntity = prefab.AllEntities.First(e => e.Name == "CubeGroupPrefab");
            var transform = rootEntity.GetTransform();

            // Assert - Root should have children references
            var children = transform!.Component.Properties["Children"] as Dictionary<string, object>;
            children.Should().NotBeNull();
            children.Should().HaveCountGreaterThan(0, "Root entity should have children");

            // Each child reference should be in format "ref!! {guid}"
            foreach (var child in children.Values)
            {
                child.Should().BeOfType<string>();
                child.ToString().Should().StartWith("ref!! ");
            }
        }

        [Test]
        public void Scene_FindEntitiesInPrefabInstance_ByName_ShouldWork()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var scene = project.LoadScene("Testing");

            // Act - Find all LootBox entities (some are from CubeGroupPrefab instance)
            var lootBoxes = scene.FindEntitiesByName("LootBox").ToList();

            // Assert
            lootBoxes.Should().HaveCountGreaterThan(0);

            // Some should be from CubeGroupPrefab
            var fromPrefab = lootBoxes.Where(e =>
                e.ParentPrefab != null &&
                e.ParentPrefab.PrefabSourcePath.Contains("CubeGroupPrefab")).ToList();

            fromPrefab.Should().HaveCountGreaterThan(0, "Should find LootBox entities from CubeGroupPrefab instance");
        }

        [Test]
        public void PrefabAsset_WithCustomComponents_ShouldParseCorrectly()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var prefab = project.LoadPrefab("CubeGroupPrefab");

            // Act - Find entities with CrateScript (custom component)
            var entitiesWithCrate = prefab.AllEntities
                .Where(e => e.HasComponent("TopDownRPG.Gameplay.CrateScript"))
                .ToList();

            // Assert
            entitiesWithCrate.Should().HaveCountGreaterThan(0, "Prefab should have entities with CrateScript");

            foreach (var entity in entitiesWithCrate)
            {
                var crateScript = entity.GetComponent("CrateScript");
                crateScript.Should().NotBeNull();

                // Check it has expected properties
                crateScript!.Properties.Should().ContainKey("CoinGetEffect");
                crateScript.Properties.Should().ContainKey("SoundEffect");
                crateScript.Properties.Should().ContainKey("Trigger");
            }
        }

        [Test]
        public void PrefabAsset_FindEntitiesByName_WithWildcard_ShouldWork()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var prefab = project.LoadPrefab("CubeGroupPrefab");

            // Act - Find all entities starting with "Cube"
            var cubes = prefab.FindEntitiesByName("Cube*");

            // Assert
            cubes.Should().HaveCountGreaterThan(0);
            cubes.Should().OnlyContain(e => e.Name.StartsWith("Cube"));
        }

        [Test]
        public void PrefabAsset_FindEntitiesWithComponent_ShouldWork()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var prefab = project.LoadPrefab("CubeGroupPrefab");

            // Act - Find all entities with ModelComponent
            var entitiesWithModel = prefab.FindEntitiesWithComponent("ModelComponent");

            // Assert
            entitiesWithModel.Should().HaveCountGreaterThan(0);
            entitiesWithModel.Should().OnlyContain(e => e.HasComponent("ModelComponent"));
        }

        [Test]
        public void PrefabAsset_FindEntities_WithPredicate_ShouldWork()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var prefab = project.LoadPrefab("CubeGroupPrefab");

            // Act - Find entities with both Model and custom script
            var results = prefab.FindEntities(e =>
                e.HasComponent("ModelComponent") &&
                e.HasComponent("TopDownRPG.Gameplay.CrateScript"));

            // Assert
            results.Should().HaveCountGreaterThan(0);
            results.Should().OnlyContain(e =>
                e.HasComponent("ModelComponent") &&
                e.HasComponent("TopDownRPG.Gameplay.CrateScript"));
        }

        [Test]
        public void PrefabAsset_CreateEntity_WithFolder_ShouldWork()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var prefab = project.LoadPrefab("CubeGroupPrefab");
            var testPrefabName = $"test_prefab_folder_{Guid.NewGuid()}.sdprefab";
            var tempPath = Path.Combine(project.AssetsPath, testPrefabName);

            try
            {
                // Act - Create new entity with folder
                var newEntity = prefab.CreateEntity("TestEntity", "TestFolder");

                // Assert
                newEntity.Should().NotBeNull();
                newEntity.Name.Should().Be("TestEntity");
                newEntity.Folder.Should().Be("TestFolder");

                // Save and verify
                prefab.SaveAs(testPrefabName);
                var yaml = File.ReadAllText(tempPath);
                yaml.Should().Contain("Folder: TestFolder");
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        [Test]
        public void PrefabAsset_CreateEntity_WithNestedPath_ShouldCreateHierarchy()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var prefab = project.LoadPrefab("CubeGroupPrefab");
            var testPrefabName = $"test_prefab_nested_{Guid.NewGuid()}.sdprefab";
            var tempPath = Path.Combine(project.AssetsPath, testPrefabName);

            try
            {
                // Act - Create entity with nested parent path
                var child = prefab.CreateEntity("DeepChild", "Level1/Level2/Level3", ParentType.Entity);

                // Assert
                child.Should().NotBeNull();

                // Should create empty parent entities
                var level1 = prefab.FindEntityByName("Level1");
                var level2 = prefab.FindEntityByName("Level2");
                var level3 = prefab.FindEntityByName("Level3");

                level1.Should().NotBeNull("Level1 should be auto-created");
                level2.Should().NotBeNull("Level2 should be auto-created");
                level3.Should().NotBeNull("Level3 should be auto-created");

                // Verify hierarchy
                prefab.SaveAs(testPrefabName);
                var reloaded = project.LoadPrefab(tempPath);

                reloaded.FindEntityByName("Level1").Should().NotBeNull();
                reloaded.FindEntityByName("Level2").Should().NotBeNull();
                reloaded.FindEntityByName("Level3").Should().NotBeNull();
                reloaded.FindEntityByName("DeepChild").Should().NotBeNull();
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }


        [Test]
        public void PrefabAsset_GetChildren_ShouldReturnChildEntities()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var prefab = project.LoadPrefab("CubeGroupPrefab");

            // Act
            var root = prefab.GetRootEntity();
            var children = root!.GetChildren();

            // Assert
            children.Should().NotBeEmpty("CubeGroupPrefab root should have children");
            children.Count.Should().BeGreaterThan(0);

            // Verify all children have the root as parent
            foreach (var child in children)
            {
                var parent = child.GetParent();
                parent.Should().NotBeNull();
                parent!.Id.Should().Be(root.Id);
            }
        }

        [Test]
        public void PrefabAsset_FindChildByName_ShouldFindDirectChild()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var prefab = project.LoadPrefab("CubeGroupPrefab");
            var root = prefab.GetRootEntity();

            // Act - Find LootBox child
            var lootBox = root!.FindChildByName("LootBox");

            // Assert
            lootBox.Should().NotBeNull("Should find LootBox child");
            lootBox!.Name.Should().Be("LootBox");
            lootBox.GetParent()!.Id.Should().Be(root.Id);
        }

        [Test]
        public void PrefabAsset_GetComponentFromChild_ShouldWork()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var prefab = project.LoadPrefab("CubeGroupPrefab");
            var root = prefab.GetRootEntity();

            // Act - Get child and its components
            var lootBox = root!.FindChildByName("LootBox");
            lootBox.Should().NotBeNull();

            var modelComponent = lootBox!.GetModel();
            var colliderComponent = lootBox.GetStaticCollider();
            var crateScript = lootBox.GetComponent("CrateScript");

            // Assert
            modelComponent.Should().NotBeNull("LootBox should have ModelComponent");
            colliderComponent.Should().NotBeNull("LootBox should have StaticColliderComponent");
            crateScript.Should().NotBeNull("LootBox should have CrateScript");

            // Check component properties
            crateScript!.Properties.Should().ContainKey("CoinGetEffect");
            crateScript.Properties.Should().ContainKey("SoundEffect");
        }

        [Test]
        public void PrefabAsset_GetDescendants_ShouldIncludeNestedChildren()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var prefab = project.LoadPrefab("CubeGroupPrefab");
            var root = prefab.GetRootEntity();

            // Act
            var descendants = root!.GetDescendants();

            // Assert
            descendants.Should().NotBeEmpty();

            // Check for specific entities we know exist
            var lootBoxes = descendants.Where(e => e.Name == "LootBox").ToList();
            lootBoxes.Should().NotBeEmpty("Should have LootBox entities in descendants");

            // Check for nested children (LootBox has CollisionWall child)
            var collisionWalls = descendants.Where(e => e.Name == "CollisionWall").ToList();
            if (collisionWalls.Any())
            {
                collisionWalls.Should().OnlyContain(e => e.GetParent() != null,
                    "CollisionWall entities should have parents");
            }
        }

        [Test]
        public void PrefabAsset_NavigateHierarchy_FindChildThenGrandchild()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var prefab = project.LoadPrefab("CubeGroupPrefab");
            var root = prefab.GetRootEntity();

            // Act - Navigate down the hierarchy
            var lootBox = root!.FindChildByName("LootBox");
            lootBox.Should().NotBeNull();

            // LootBox should have children (like CollisionWall)
            if (lootBox!.HasChildren())
            {
                var grandchildren = lootBox.GetChildren();
                grandchildren.Should().NotBeEmpty();

                // Check grandchild has correct parent chain
                var grandchild = grandchildren.First();
                grandchild.GetParent()!.Id.Should().Be(lootBox.Id);
                grandchild.GetParent()!.GetParent()!.Id.Should().Be(root.Id);
            }
        }

        [Test]
        public void PrefabAsset_ModifyChildComponent_ShouldPersist()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var prefab = project.LoadPrefab("CubeGroupPrefab");
            var root = prefab.GetRootEntity();
            var testPrefabName = $"test_prefab_child_mod_{Guid.NewGuid()}.sdprefab";
            var tempPath = Path.Combine(project.AssetsPath, testPrefabName);

            try
            {
                // Act - Find child and modify its component
                var lootBox = root!.FindChildByName("LootBox");
                lootBox.Should().NotBeNull();

                var transform = lootBox!.GetTransform();
                transform!.SetPosition(10.0f, 20.0f, 30.0f);

                prefab.SaveAs(testPrefabName);

                // Reload and verify
                var reloaded = project.LoadPrefab(tempPath);
                var reloadedRoot = reloaded.GetRootEntity();
                var reloadedLootBox = reloadedRoot!.FindChildByName("LootBox");

                // Assert
                reloadedLootBox.Should().NotBeNull();
                var reloadedTransform = reloadedLootBox!.GetTransform();
                var pos = reloadedTransform!.GetPosition();
                pos.X.Should().Be(10.0f);
                pos.Y.Should().Be(20.0f);
                pos.Z.Should().Be(30.0f);
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        [Test]
        public void PrefabAsset_FindChildrenByName_WithPattern_ShouldMatchMultiple()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var prefab = project.LoadPrefab("CubeGroupPrefab");
            var root = prefab.GetRootEntity();

            // Act - Find all LootBox children (there might be multiple)
            var lootBoxes = root!.FindChildrenByName("LootBox*");

            // Assert
            lootBoxes.Should().NotBeEmpty();
            lootBoxes.Should().OnlyContain(e => e.Name.StartsWith("LootBox"));

            // All should be direct children of root
            lootBoxes.Should().OnlyContain(e => e.GetParent()!.Id == root.Id);
        }

        [Test]
        public void PrefabAsset_AddComponent_WithScriptScanning_ShouldHaveFullTypeName()
        {
            // This test verifies the fix for: "Issue with adding component to prefab entity says wrong name"
            // When AddComponent is called, it should use ScriptScanner to get the full type name
            // (namespace.ClassName,AssemblyName) rather than just the class name

            // Arrange
            var project = new StrideProject(_testProjectPath);
            var prefab = project.LoadPrefab("CubeGroupPrefab");
            var rootEntity = prefab.GetRootEntity();
            var testPrefabName = $"test_prefab_type_{Guid.NewGuid()}.sdprefab";
            var tempPath = Path.Combine(project.AssetsPath, testPrefabName);

            try
            {
                // Act - Add custom component
                var component = rootEntity!.AddComponent("SimpleScript");
                component.Set("health", 100);

                // Assert - Component Type should have full namespace and assembly
                component.Type.Should().Contain("TestNamespace.SimpleScript", "Should have full namespace");
                component.Type.Should().Contain(",", "Should have assembly name");
                component.Type.Should().NotBe("SimpleScript", "Should not be just the class name");
                component.Type.Should().NotBe("!Tester", "Should not be a bare tag name");

                // Save and verify the YAML is correct
                prefab.SaveAs(testPrefabName);
                var yaml = File.ReadAllText(tempPath);

                // YAML should contain the full type name
                yaml.Should().Contain("TestNamespace.SimpleScript", "YAML should have full namespace");
                yaml.Should().NotContain("!SimpleScript\n", "YAML should not have bare class name as tag");

                // Reload and verify it deserializes correctly
                var reloaded = project.LoadPrefab(tempPath);
                var reloadedRoot = reloaded.GetRootEntity();
                var reloadedComp = reloadedRoot!.GetComponent("SimpleScript");

                // Should reload successfully with proper type
                reloadedComp.Should().NotBeNull("Component should deserialize successfully");
                reloadedComp!.Type.Should().Contain("TestNamespace.SimpleScript");
                reloadedComp.Get<int>("health").Should().Be(100);
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        [Test]
        public void PrefabAsset_CreatePrefab_AddComponent_ShouldResolveTypeName()
        {
            // This test verifies CreatePrefab properly sets ParentProject for script scanning

            // Arrange
            var project = new StrideProject(_testProjectPath);
            var tempPath = Path.Combine(project.AssetsPath, "TestScenes", $"test_new_prefab_{Guid.NewGuid()}.sdprefab");

            try
            {
                // Act - Create NEW prefab and add component
                var prefab = project.CreatePrefab("TestPrefab");
                var rootEntity = prefab.GetRootEntity();

                var component = rootEntity!.AddComponent("NoNameSpace");
                component.Set("intValue", 42);

                // Assert - Type should have full type info (even without namespace, should have leading dot)
                component.Type.Should().StartWith(".", "Scripts without namespace should start with dot");
                component.Type.Should().Contain("NoNameSpace");
                component.Type.Should().Contain(",", "Should have assembly name");

                // Save and verify YAML
                prefab.SaveAs(tempPath);
                var yaml = File.ReadAllText(tempPath);

                yaml.Should().Contain(".NoNameSpace", "YAML should have type with leading dot");
                yaml.Should().NotContain("!NoNameSpace\n", "YAML should not have bare class name");

                // Reload and verify
                var reloaded = project.LoadPrefab(tempPath);
                var reloadedRoot = reloaded.GetRootEntity();
                var reloadedComp = reloadedRoot!.GetComponent("NoNameSpace");

                reloadedComp.Should().NotBeNull("Component should deserialize successfully");
                reloadedComp!.Get<int>("intValue").Should().Be(42);
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }
    }
}
