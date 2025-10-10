// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using NUnit.Framework;
using FluentAssertions;
using HS.Stride.Editor.Toolkit.Core;
using HS.Stride.Editor.Toolkit.Core.DataTypes;
using HS.Stride.Editor.Toolkit.Core.SceneEditing;

namespace HS.Stride.Editor.Toolkit.Tests
{
    [TestFixture]
    public class ReferenceDataTests
    {
        private string _testProjectPath;

        [SetUp]
        public void Setup()
        {
            _testProjectPath = @"E:\Github\HS-Stride-Editor-Toolkit\HS.Stride.Editor.Toolkit.Tests\Example Scenes\TestProject\";
        }

        [Test]
        public void EntityRefData_Parse_ShouldExtractGuid()
        {
            // Arrange
            var refString = "ref!! d594e60f-c93c-4ae2-add6-8f2fa6b7c170";

            // Act
            var entityRef = EntityRefData.Parse(refString);

            // Assert
            entityRef.Should().NotBeNull();
            entityRef!.Guid.Should().Be("d594e60f-c93c-4ae2-add6-8f2fa6b7c170");
            entityRef.Reference.Should().Be(refString);
        }

        [Test]
        public void EntityRefData_Parse_NullOrInvalid_ShouldReturnNull()
        {
            // Arrange & Act & Assert
            EntityRefData.Parse(null).Should().BeNull();
            EntityRefData.Parse("").Should().BeNull();
            EntityRefData.Parse("invalid").Should().BeNull();
            EntityRefData.Parse("guid-without-prefix").Should().BeNull();
        }

        [Test]
        public void AssetRefData_Parse_ShouldExtractGuidAndPath()
        {
            // Arrange
            var refString = "1c3eb155-b98b-4977-b089-6ef8ad8e3447:LootBox";

            // Act
            var assetRef = AssetRefData.Parse(refString);

            // Assert
            assetRef.Should().NotBeNull();
            assetRef!.Guid.Should().Be("1c3eb155-b98b-4977-b089-6ef8ad8e3447");
            assetRef.Path.Should().Be("LootBox");
            assetRef.Reference.Should().Be(refString);
        }

        [Test]
        public void AssetRefData_Parse_WithNestedPath_ShouldWork()
        {
            // Arrange
            var refString = "1a54db4e-d69e-47a1-893d-3542a8dbc90b:Models/Box3x1x1";

            // Act
            var assetRef = AssetRefData.Parse(refString);

            // Assert
            assetRef.Should().NotBeNull();
            assetRef!.Guid.Should().Be("1a54db4e-d69e-47a1-893d-3542a8dbc90b");
            assetRef.Path.Should().Be("Models/Box3x1x1");
        }

        [Test]
        public void AssetRefData_Parse_NullOrInvalid_ShouldReturnNull()
        {
            // Arrange & Act & Assert
            AssetRefData.Parse(null).Should().BeNull();
            AssetRefData.Parse("").Should().BeNull();
            AssetRefData.Parse("null").Should().BeNull();
            AssetRefData.Parse("invalid").Should().BeNull();
            AssetRefData.Parse("guid-only").Should().BeNull();
        }

        [Test]
        public void Component_GetEntityRefData_ShouldParseFromString()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var scene = project.LoadScene("Testing");
            var entity = scene.FindEntityByName("Testing NoNameSpace");
            var component = entity!.GetComponent("NoNameSpace");

            // The component should have a transformRef set to the TransformComponent's ID
            // Act
            var entityRef = component!.Get<EntityRefData>("transformRef");

            // Assert
            entityRef.Should().NotBeNull();
            entityRef!.Guid.Should().NotBeEmpty();
            entityRef.Reference.Should().StartWith("ref!! ");
        }

        [Test]
        public void Component_SetAndGetEntityRefData_FullWorkflow()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var scene = project.LoadScene("Testing");
            var entity1 = scene.FindEntityByName("Box1x1x1");
            var entity2 = scene.FindEntityByName("Testing NoNameSpace");
            var component = entity1!.AddComponent("NoNameSpace");

            // Act - Set entity reference using clean helper method
            component.SetEntityRef("singleEntity", entity2!);

            // Get it back as EntityRefData
            var entityRef = component.Get<EntityRefData>("singleEntity");

            // Assert
            entityRef.Should().NotBeNull();
            entityRef!.Guid.Should().Be(entity2.Id);

            // Resolve to actual entity
            var resolvedEntity = entityRef.Resolve(scene);
            resolvedEntity.Should().NotBeNull();
            resolvedEntity!.Name.Should().Be("Testing NoNameSpace");
        }

        [Test]
        public void Component_SetAndGetAssetRefData_FullWorkflow()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var scene = project.LoadScene("Testing");
            var entity = scene.FindEntityByName("Box1x1x1");
            var component = entity!.AddComponent("NoNameSpace");

            // Find a prefab asset
            var prefabAsset = project.FindAsset("LootBox", Core.AssetEditing.AssetType.Prefab);
            prefabAsset.Should().NotBeNull("LootBox prefab should exist in test project");

            // Act - Set asset reference using clean helper method
            component.SetAssetRef("prefabRef", prefabAsset!);

            // Get it back as AssetRefData
            var assetRef = component.Get<AssetRefData>("prefabRef");

            // Assert
            assetRef.Should().NotBeNull();
            assetRef!.Guid.Should().Be(prefabAsset.Id);
            assetRef.Path.Should().Be(prefabAsset.Path);

            // Resolve to actual asset
            var resolvedAsset = assetRef.Resolve(project);
            resolvedAsset.Should().NotBeNull();
            resolvedAsset!.Name.Should().Be("LootBox");
        }

        [Test]
        public void Component_SetAndGetAssetRefData_RawAsset()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var scene = project.LoadScene("Testing");
            var entity = scene.FindEntityByName("Box1x1x1");
            var component = entity!.AddComponent("NoNameSpace");

            // Find raw asset
            var rawAsset = project.FindAsset("rawAssetTest", Core.AssetEditing.AssetType.RawAsset);
            rawAsset.Should().NotBeNull("rawAssetTest should exist in test project");

            // Act - Set RawAsset (UrlReference) reference using clean helper method
            component.SetAssetRef("RawAsset", rawAsset!);

            // Get it back as AssetRefData
            var assetRef = component.Get<AssetRefData>("RawAsset");

            // Assert
            assetRef.Should().NotBeNull();
            assetRef!.Guid.Should().Be(rawAsset.Id);
            assetRef.Path.Should().Be(rawAsset.Path);

            // Resolve to actual asset
            var resolvedAsset = assetRef.Resolve(project);
            resolvedAsset.Should().NotBeNull();
            resolvedAsset!.Name.Should().Be("rawAssetTest");
        }

        [Test]
        public void Component_SetAndGetAssetRefData_SaveAndReload()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var scene = project.LoadScene("Testing");
            var entity = scene.FindEntityByName("Box1x1x1");
            var component = entity!.AddComponent("NoNameSpace");
            var prefabAsset = project.FindAsset("LootBox", Core.AssetEditing.AssetType.Prefab);

            var tempPath = Path.Combine(Path.GetTempPath(), $"test_assetref_{Guid.NewGuid()}.sdscene");

            try
            {
                // Act - Set using clean helper method, save, reload
                component.SetAssetRef("prefabRef", prefabAsset!);
                scene.SaveAs(tempPath);

                var reloadedScene = Scene.Load(tempPath);
                var reloadedEntity = reloadedScene.FindEntityByName("Box1x1x1");
                var reloadedComponent = reloadedEntity!.GetComponent("NoNameSpace");

                // Assert - Should parse correctly after reload
                var assetRef = reloadedComponent!.Get<AssetRefData>("prefabRef");
                assetRef.Should().NotBeNull();
                assetRef!.Guid.Should().Be(prefabAsset.Id);
                assetRef.Path.Should().Be(prefabAsset.Path);

                // Verify YAML format
                var yaml = File.ReadAllText(tempPath);
                yaml.Should().Contain($"prefabRef: {prefabAsset.Reference}");
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        [Test]
        public void Component_SetAndGetEntityRefData_SaveAndReload()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var scene = project.LoadScene("Testing");
            var entity1 = scene.FindEntityByName("Box1x1x1");
            var entity2 = scene.FindEntityByName("Testing NoNameSpace");
            var component = entity1!.AddComponent("NoNameSpace");

            var tempPath = Path.Combine(Path.GetTempPath(), $"test_entityref_{Guid.NewGuid()}.sdscene");

            try
            {
                // Act - Set using clean helper method, save, reload
                component.SetEntityRef("singleEntity", entity2!);
                scene.SaveAs(tempPath);

                var reloadedScene = Scene.Load(tempPath);
                var reloadedEntity = reloadedScene.FindEntityByName("Box1x1x1");
                var reloadedComponent = reloadedEntity!.GetComponent("NoNameSpace");

                // Assert - Should parse correctly after reload
                var entityRef = reloadedComponent!.Get<EntityRefData>("singleEntity");
                entityRef.Should().NotBeNull();
                entityRef!.Guid.Should().Be(entity2.Id);

                // Resolve should work
                var resolvedEntity = entityRef.Resolve(reloadedScene);
                resolvedEntity.Should().NotBeNull();
                resolvedEntity!.Name.Should().Be("Testing NoNameSpace");

                // Verify YAML format
                var yaml = File.ReadAllText(tempPath);
                yaml.Should().Contain($"singleEntity: ref!! {entity2.Id}");
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        [Test]
        public void StrideProject_GetRawAssetSource_ShouldReturnSourceFilePath()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var rawAsset = project.FindAsset("rawAssetTest", Core.AssetEditing.AssetType.RawAsset);

            // Act
            var sourcePath = project.GetRawAssetSource(rawAsset!);

            // Assert
            sourcePath.Should().NotBeNull("RawAsset should have a source file");
            File.Exists(sourcePath).Should().BeTrue("Source file should exist");
            sourcePath.Should().EndWith("rawAssetTest.txt", "Should point to the actual .txt file");

            // Verify we can read the content
            var content = File.ReadAllText(sourcePath!);
            content.Should().Contain("just a rawasset test");
        }

        [Test]
        public void StrideProject_GetRawAssetSource_WithNonRawAsset_ShouldThrow()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var prefab = project.FindAsset("LootBox", Core.AssetEditing.AssetType.Prefab);

            // Act & Assert
            var act = () => project.GetRawAssetSource(prefab!);
            act.Should().Throw<ArgumentException>()
                .WithMessage("*RawAsset*");
        }
    }
}
