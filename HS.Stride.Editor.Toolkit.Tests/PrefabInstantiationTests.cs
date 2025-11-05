// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using NUnit.Framework;
using FluentAssertions;
using HS.Stride.Editor.Toolkit.Core;
using HS.Stride.Editor.Toolkit.Core.SceneEditing;
using HS.Stride.Editor.Toolkit.Core.AssetEditing;
using HS.Stride.Editor.Toolkit.Core.DataTypes;

namespace HS.Stride.Editor.Toolkit.Tests
{
    [TestFixture]
    public class PrefabInstantiationTests
    {
        private string _testProjectPath;

        [SetUp]
        public void Setup()
        {
            _testProjectPath = @"E:\Github\HS-Stride-Editor-Toolkit\HS.Stride.Editor.Toolkit.Tests\Example Scenes\TestProject\";
        }

       

        [Test]
        public void InstantiatePrefab_WithoutFolder_ShouldHaveCorrectIndentation()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var scene = project.LoadScene("Testing");
            var prefabAsset = project.FindAsset("LootBox", AssetType.Prefab);

            var testSceneName = $"test_prefab_no_folder_{Guid.NewGuid()}.sdscene";
            var tempPath = Path.Combine(project.AssetsPath, testSceneName);

            try
            {
                // Act - Instantiate prefab without a folder
                var newInstance = scene.InstantiatePrefab(prefabAsset!, new Vector3Data(10.0f, 0.0f, 0.0f));

                // Save the scene
                scene.SaveAs(testSceneName);

                // Assert - Read the raw YAML and verify Base section indentation
                var yamlContent = File.ReadAllText(tempPath);
                
                // Without folder, pattern should be:
                // -   Entity:
                //         Id: ...
                //         Name: LootBox
                //         Components:
                //             ...
                //         Base:              <-- Should be at same level as Id, Name, Components
                //             BasePartAsset: ...
                
                yamlContent.Should().Contain("            Base:");  // 12 spaces before Base
                yamlContent.Should().Contain("                BasePartAsset:");  // 16 spaces before BasePartAsset
                yamlContent.Should().Contain("                BasePartId:");  // 16 spaces before BasePartId
                yamlContent.Should().Contain("                InstanceId:");  // 16 spaces before InstanceId

                // Verify the scene can be reloaded
                var reloadedScene = Scene.Load(tempPath);
                reloadedScene.Should().NotBeNull();
                
                // Verify we can find the prefab instances
                var instances = reloadedScene.FindEntitiesByName("LootBox").ToList();
                instances.Should().HaveCountGreaterThan(0);
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        

        [Test]
        public void Scene_WithPrefabInstances_ShouldMatchStrideFormat()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            
            // Load a scene we know Stride can load (created by Stride itself)
            var workingScene = project.LoadScene("Testing");

            var testSceneName = $"test_stride_format_{Guid.NewGuid()}.sdscene";
            var tempPath = Path.Combine(project.AssetsPath, testSceneName);

            try
            {
                // Act - Save and reload to ensure our format matches
                workingScene.SaveAs(testSceneName);
                var reloaded = Scene.Load(tempPath);

                // Assert - Verify all prefab instances maintained their data
                var originalPrefabs = workingScene.AllEntities.Where(e => e.ParentPrefab != null).ToList();
                var reloadedPrefabs = reloaded.AllEntities.Where(e => e.ParentPrefab != null).ToList();

                reloadedPrefabs.Should().HaveSameCount(originalPrefabs);

                foreach (var originalPrefab in originalPrefabs)
                {
                    var matching = reloadedPrefabs.FirstOrDefault(e => e.Id == originalPrefab.Id);
                    matching.Should().NotBeNull($"Prefab instance {originalPrefab.Name} should exist after reload");
                    
                    matching!.ParentPrefab.Should().NotBeNull();
                    matching.ParentPrefab!.PrefabSourcePath.Should().Be(originalPrefab.ParentPrefab!.PrefabSourcePath);
                    matching.ParentPrefab.PrefabEntityId.Should().Be(originalPrefab.ParentPrefab.PrefabEntityId);
                    matching.ParentPrefab.InstanceId.Should().Be(originalPrefab.ParentPrefab.InstanceId);
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
