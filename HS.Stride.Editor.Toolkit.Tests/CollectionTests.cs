// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using FluentAssertions;
using HS.Stride.Editor.Toolkit.Core;
using HS.Stride.Editor.Toolkit.Core.AssetEditing;
using HS.Stride.Editor.Toolkit.Core.SceneEditing;
using NUnit.Framework;

namespace HS.Stride.Editor.Toolkit.Tests
{
    [TestFixture]
    public class CollectionTests
    {
        private string _testProjectPath;

        [SetUp]
        public void Setup()
        {
            _testProjectPath = @"E:\Github\HS-Stride-Editor-Toolkit\HS.Stride.Editor.Toolkit.Tests\Example Scenes\TestProject\";
        }

        [Test]
        public void AddToList_SingleEntity_ShouldCreateGuidKeyedEntry()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var scene = project.LoadScene("Testing");
            var entity = scene.FindEntityByName("Box1x1x1");
            var component = entity.AddComponent("NoNameSpace");

            var targetEntity = scene.CreateEntity("TargetEntity");
            var entityRef = $"ref!! {targetEntity.Id}";

            // Act
            component.AddToList("entityList", entityRef);

            // Assert
            var listDict = component.Properties["entityList"] as Dictionary<string, object>;
            listDict.Should().NotBeNull();
            listDict.Should().HaveCount(1);
            listDict!.Values.First().Should().Be(entityRef);
        }

        [Test]
        public void AddToList_MultipleEntities_ShouldCreateMultipleGuidKeyedEntries()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var scene = project.LoadScene("Testing");
            var entity = scene.FindEntityByName("Box1x1x1");
            var component = entity.AddComponent("NoNameSpace");

            var entity1 = scene.CreateEntity("Entity1");
            var entity2 = scene.CreateEntity("Entity2");
            var entity3 = scene.CreateEntity("Entity3");

            // Act
            component.AddToList("entityList", $"ref!! {entity1.Id}");
            component.AddToList("entityList", $"ref!! {entity2.Id}");
            component.AddToList("entityList", $"ref!! {entity3.Id}");

            // Assert
            var listDict = component.Properties["entityList"] as Dictionary<string, object>;
            listDict.Should().NotBeNull();
            listDict!.Values.Should().Contain($"ref!! {entity1.Id}");
            listDict.Values.Should().Contain($"ref!! {entity2.Id}");
            listDict.Values.Should().Contain($"ref!! {entity3.Id}");
        }

        [Test]
        public void AddToList_AssetReferences_ShouldWork()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var scene = project.LoadScene("Testing");
            var entity = scene.FindEntityByName("Box1x1x1");
            var component = entity.AddComponent("NoNameSpace");

            var asset1 = new AssetReference { Id = "guid1", Path = "path1" };
            var asset2 = new AssetReference { Id = "guid2", Path = "path2" };

            // Act
            component.AddToList("prefabList", asset1.Reference);
            component.AddToList("prefabList", asset2.Reference);

            // Assert
            var listDict = component.Properties["prefabList"] as Dictionary<string, object>;
            listDict.Should().NotBeNull();
            listDict!.Values.Should().Contain(asset1.Reference);
            listDict.Values.Should().Contain(asset2.Reference);
        }

        [Test]
        public void AddToList_OnNonExistentProperty_LooseMode_ShouldCreateNewList()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath, ProjectMode.Loose);
            var scene = project.LoadScene("Testing");
            var entity = scene.FindEntityByName("Box1x1x1");
            var component = entity.AddComponent("NoNameSpace");

            // Act
            component.AddToList("newList", "value1");

            // Assert
            component.Properties.Should().ContainKey("newList");
            var listDict = component.Properties["newList"] as Dictionary<string, object>;
            listDict.Should().NotBeNull();
            listDict.Should().HaveCount(1);
        }

        [Test]
        public void SetDictionary_StringKey_ShouldCreateGuidTildeKeyEntry()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var scene = project.LoadScene("Testing");
            var entity = scene.FindEntityByName("Box1x1x1");
            var component = entity.AddComponent("NoNameSpace");

            var asset = new AssetReference { Id = "anim-guid", Path = "Animations/Walk" };

            // Act
            component.SetDictionary("AnimationClips", "Walk", asset.Reference);

            // Assert
            var dictData = component.Properties["AnimationClips"] as Dictionary<string, object>;
            dictData.Should().NotBeNull();

            var key = dictData!.Keys.First(k => k.Contains("~Walk"));
            key.Should().Contain("~Walk");
            dictData[key].Should().Be(asset.Reference);
        }

        [Test]
        public void SetDictionary_IntKey_ShouldWork()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var scene = project.LoadScene("Testing");
            var entity = scene.FindEntityByName("Box1x1x1");
            var component = entity.AddComponent("NoNameSpace");

            // Act
            component.SetDictionary("primClips", 123, "Dog");
            component.SetDictionary("primClips", 456, "Cat");

            // Assert
            var dictData = component.Properties["primClips"] as Dictionary<string, object>;
            dictData.Should().NotBeNull();

            var keys = dictData!.Keys.ToList();
            keys.Should().Contain(k => k.Contains("~123"));
            keys.Should().Contain(k => k.Contains("~456"));
        }

        [Test]
        public void SetDictionary_MultipleEntries_ShouldAllHaveUniqueGuids()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath, ProjectMode.Loose);
            var scene = project.LoadScene("Testing");
            var entity = scene.FindEntityByName("Box1x1x1");
            var component = entity.AddComponent("NoNameSpace");

            // Act
            component.SetDictionary("clips", "Walk", "walk-data");
            component.SetDictionary("clips", "Run", "run-data");
            component.SetDictionary("clips", "Jump", "jump-data");

            // Assert
            var dictData = component.Properties["clips"] as Dictionary<string, object>;
            dictData.Should().NotBeNull();

            var guidParts = dictData!.Keys.Select(k => k.Split('~')[0]).ToList();
            guidParts.Should().OnlyHaveUniqueItems();
        }

        [Test]
        public void SetList_ReplaceEntireList_ShouldWork()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var scene = project.LoadScene("Testing");
            var entity = scene.FindEntityByName("Box1x1x1");
            var component = entity.AddComponent("NoNameSpace");

            var entity1 = scene.CreateEntity("Entity1");
            var entity2 = scene.CreateEntity("Entity2");
            var entity3 = scene.CreateEntity("Entity3");

            var entityRefs = new[]
            {
                $"ref!! {entity1.Id}",
                $"ref!! {entity2.Id}",
                $"ref!! {entity3.Id}"
            };

            // Act
            component.SetList("entityList", entityRefs);

            // Assert
            var listDict = component.Properties["entityList"] as Dictionary<string, object>;
            listDict.Should().NotBeNull();
            listDict.Should().HaveCount(3);
            listDict!.Values.Should().Contain(entityRefs[0]);
            listDict.Values.Should().Contain(entityRefs[1]);
            listDict.Values.Should().Contain(entityRefs[2]);
        }

        [Test]
        public void SetList_OverwriteExistingList_ShouldReplaceAll()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath, ProjectMode.Loose);
            var scene = project.LoadScene("Testing");
            var entity = scene.FindEntityByName("Box1x1x1");
            var component = entity.AddComponent("NoNameSpace");

            // Add initial values
            component.AddToList("myList", "value1");
            component.AddToList("myList", "value2");

            var newValues = new[] { "newValue1", "newValue2", "newValue3" };

            // Act
            component.SetList("myList", newValues);

            // Assert
            var listDict = component.Properties["myList"] as Dictionary<string, object>;
            listDict.Should().NotBeNull();
            listDict.Should().HaveCount(3);
            listDict!.Values.Should().NotContain("value1");
            listDict.Values.Should().NotContain("value2");
            listDict.Values.Should().Contain("newValue1");
            listDict.Values.Should().Contain("newValue2");
            listDict.Values.Should().Contain("newValue3");
        }

        [Test]
        public void AddToList_SaveAndReload_ShouldPersist()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var scene = project.LoadScene("Testing");
            var entity = scene.FindEntityByName("Box1x1x1");
            var component = entity.AddComponent("NoNameSpace");

            var target1 = scene.CreateEntity("Target1");
            var target2 = scene.CreateEntity("Target2");

            component.AddToList("entityList", $"ref!! {target1.Id}");
            component.AddToList("entityList", $"ref!! {target2.Id}");

            var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.sdscene");

            try
            {
                // Act - Save and reload
                scene.SaveAs(tempPath);
                var reloaded = Scene.Load(tempPath);

                // Assert
                var reloadedEntity = reloaded.FindEntityByName("Box1x1x1");
                reloadedEntity.Should().NotBeNull();

                var reloadedComponent = reloadedEntity!.GetComponent("NoNameSpace");
                reloadedComponent.Should().NotBeNull();

                var listDict = reloadedComponent!.Properties["entityList"] as Dictionary<string, object>;
                listDict.Should().NotBeNull();
                // Initial dict had entries, plus we added 2
                listDict!.Values.Should().Contain($"ref!! {target1.Id}");
                listDict.Values.Should().Contain($"ref!! {target2.Id}");
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        [Test]
        public void SetDictionary_SaveAndReload_ShouldPersist()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var scene = project.LoadScene("Testing");
            var entity = scene.FindEntityByName("Box1x1x1");
            var component = entity.AddComponent("NoNameSpace");

            component.SetDictionary("AnimationClips", "Walk", "walk-asset");
            component.SetDictionary("AnimationClips", "Run", "run-asset");

            var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.sdscene");

            try
            {
                // Act - Save and reload
                scene.SaveAs(tempPath);
                var reloaded = Scene.Load(tempPath);

                // Assert
                var reloadedEntity = reloaded.FindEntityByName("Box1x1x1");
                reloadedEntity.Should().NotBeNull();

                var reloadedComponent = reloadedEntity!.GetComponent("NoNameSpace");
                reloadedComponent.Should().NotBeNull();

                var dictData = reloadedComponent!.Properties["AnimationClips"] as Dictionary<string, object>;
                dictData.Should().NotBeNull();

                var keys = dictData!.Keys.ToList();
                keys.Should().Contain(k => k.Contains("~Walk"));
                keys.Should().Contain(k => k.Contains("~Run"));
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }
    }
}
