// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using NUnit.Framework;
using FluentAssertions;
using HS.Stride.Editor.Toolkit.Core;
using HS.Stride.Editor.Toolkit.Core.AssetEditing;
using HS.Stride.Editor.Toolkit.Core.DataTypes;
using HS.Stride.Editor.Toolkit.Core.Wrappers;

namespace HS.Stride.Editor.Toolkit.Tests
{
    [TestFixture]
    public class StrideProjectTests
    {
        private StrideProject _project;

        [SetUp]
        public void Setup()
        {
            // Go up from bin/Release/net8.0 to project root
            var projectRoot = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", ".."));
            var testProjectPath = Path.Combine(projectRoot, "Example Scenes", "TestProject");
            _project = new StrideProject(testProjectPath);
        }

        [Test]
        public void Constructor_ValidProject_ShouldInitializeAndScan()
        {
            _project.Should().NotBeNull();
            _project.AssetsPath.Should().NotBeNullOrEmpty();
            _project.GetAllAssets().Should().NotBeEmpty();
        }

        [Test]
        public void Constructor_InvalidProject_ShouldThrow()
        {
            Action act = () => new StrideProject(@"C:\InvalidPath");
            act.Should().Throw<ArgumentException>();
        }

        [Test]
        public void LoadScene_ByName_ShouldLoadScene()
        {
            var scene = _project.LoadScene("MainScene");

            scene.Should().NotBeNull();
            scene.AllEntities.Should().NotBeEmpty();
        }

        [Test]
        public void LoadScene_ByRelativePath_ShouldLoadScene()
        {
            var scene = _project.LoadScene(@"C:\Users\Dave\Documents\Stride Projects\TopDownRPG\Assets\MainScene.sdscene");

            scene.Should().NotBeNull();
            scene.AllEntities.Should().NotBeEmpty();
        }

        [Test]
        public void LoadScene_NonExistent_ShouldThrow()
        {
            Action act = () => _project.LoadScene("NonExistentScene");
            act.Should().Throw<FileNotFoundException>();
        }

        [Test]
        public void LoadScene_NullName_ShouldThrow()
        {
            Action act = () => _project.LoadScene(null);
            act.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void LoadMaterial_ByName_ShouldLoadMaterial()
        {
            var materials = _project.GetMaterials();
            if (!materials.Any())
            {
                Assert.Inconclusive("No materials in test project");
                return;
            }

            var firstMat = materials.First();
            var material = _project.LoadMaterial(firstMat.Name);

            material.Should().NotBeNull();
            material.Id.Should().Be(firstMat.Id);
        }

        [Test]
        public void LoadTexture_ByName_ShouldLoadTexture()
        {
            var textures = _project.GetTextures();
            if (!textures.Any())
            {
                Assert.Inconclusive("No textures in test project");
                return;
            }

            var firstTex = textures.First();
            var texture = _project.LoadTexture(firstTex.Name);

            texture.Should().NotBeNull();
            texture.Id.Should().Be(firstTex.Id);
        }

        [Test]
        public void LoadAnimation_ByName_ShouldLoadAnimation()
        {
            var animations = _project.GetAnimations();
            if (!animations.Any())
            {
                Assert.Inconclusive("No animations in test project");
                return;
            }

            var firstAnim = animations.First();
            var animation = _project.LoadAnimation(firstAnim.Name);

            animation.Should().NotBeNull();
            animation.Id.Should().Be(firstAnim.Id);
        }

        [Test]
        public void LoadPrefab_ByName_ShouldLoadPrefab()
        {
            var prefabs = _project.GetPrefabs();
            if (!prefabs.Any())
            {
                Assert.Inconclusive("No prefabs in test project");
                return;
            }

            var firstPrefab = prefabs.First();
            var prefab = _project.LoadPrefab(firstPrefab.Name);

            prefab.Should().NotBeNull();
            prefab.Id.Should().Be(firstPrefab.Id);
        }

        [Test]
        public void FindAsset_ByName_ShouldReturnAsset()
        {
            var sceneRef = _project.FindAsset("MainScene", AssetType.Scene);

            sceneRef.Should().NotBeNull();
            sceneRef.Type.Should().Be(AssetType.Scene);
            sceneRef.Name.Should().Be("MainScene");
        }

        [Test]
        public void FindAssets_ByPattern_ShouldReturnMatchingAssets()
        {
            var scenes = _project.FindAssets("*Scene", AssetType.Scene);

            scenes.Should().NotBeEmpty();
            scenes.Should().AllSatisfy(s => s.Name.Should().EndWith("Scene"));
        }

        [Test]
        public void GetScenes_ShouldReturnAllScenes()
        {
            var scenes = _project.GetScenes();

            scenes.Should().NotBeEmpty();
            scenes.Should().AllSatisfy(s => s.Type.Should().Be(AssetType.Scene));
        }

        [Test]
        public void GetPrefabs_ShouldReturnAllPrefabs()
        {
            var prefabs = _project.GetPrefabs();

            // May be empty in test project, just ensure no exception
            prefabs.Should().NotBeNull();
            prefabs.Should().AllSatisfy(p => p.Type.Should().Be(AssetType.Prefab));
        }

        [Test]
        public void GetMaterials_ShouldReturnAllMaterials()
        {
            var materials = _project.GetMaterials();

            materials.Should().NotBeNull();
            materials.Should().AllSatisfy(m => m.Type.Should().Be(AssetType.Material));
        }

        [Test]
        public void CreateScene_WithNameOnly_ShouldCreateSceneInAssetsRoot()
        {
            // Arrange
            var sceneName = $"TestScene_{Guid.NewGuid()}";

            try
            {
                // Act
                var scene = _project.CreateScene(sceneName);

                // Assert
                scene.Should().NotBeNull();
                scene.FilePath.Should().Contain(sceneName);
                scene.FilePath.Should().EndWith(".sdscene");
                File.Exists(scene.FilePath).Should().BeTrue();
                scene.AllEntities.Should().BeEmpty();
            }
            finally
            {
                // Cleanup
                var scenePath = Path.Combine(_project.AssetsPath, $"{sceneName}.sdscene");
                if (File.Exists(scenePath))
                    File.Delete(scenePath);
            }
        }

        [Test]
        public void CreateScene_WithRelativePath_ShouldCreateSceneInSubfolder()
        {
            // Arrange
            var sceneName = $"TestScene_{Guid.NewGuid()}";
            var relativePath = $"TestScenes/{sceneName}";

            try
            {
                // Act
                var scene = _project.CreateScene(sceneName, relativePath);

                // Assert
                scene.Should().NotBeNull();
                scene.FilePath.Should().Contain("TestScenes");
                scene.FilePath.Should().Contain(sceneName);
                scene.FilePath.Should().EndWith(".sdscene");
                File.Exists(scene.FilePath).Should().BeTrue();
            }
            finally
            {
                // Cleanup
                var scenePath = Path.Combine(_project.AssetsPath, "TestScenes", $"{sceneName}.sdscene");
                if (File.Exists(scenePath))
                    File.Delete(scenePath);

                var folderPath = Path.Combine(_project.AssetsPath, "TestScenes");
                if (Directory.Exists(folderPath) && !Directory.EnumerateFileSystemEntries(folderPath).Any())
                    Directory.Delete(folderPath);
            }
        }

        [Test]
        public void CreateScene_WithFullRelativePathAndExtension_ShouldCreateScene()
        {
            // Arrange
            var sceneName = $"TestScene_{Guid.NewGuid()}";
            var relativePath = $"TestScenes/{sceneName}.sdscene";

            try
            {
                // Act
                var scene = _project.CreateScene(sceneName, relativePath);

                // Assert
                scene.Should().NotBeNull();
                scene.FilePath.Should().EndWith($"TestScenes{Path.DirectorySeparatorChar}{sceneName}.sdscene");
                File.Exists(scene.FilePath).Should().BeTrue();
            }
            finally
            {
                // Cleanup
                var scenePath = Path.Combine(_project.AssetsPath, "TestScenes", $"{sceneName}.sdscene");
                if (File.Exists(scenePath))
                    File.Delete(scenePath);

                var folderPath = Path.Combine(_project.AssetsPath, "TestScenes");
                if (Directory.Exists(folderPath) && !Directory.EnumerateFileSystemEntries(folderPath).Any())
                    Directory.Delete(folderPath);
            }
        }

        [Test]
        public void CreateScene_NullName_ShouldThrow()
        {
            // Act
            Action act = () => _project.CreateScene(null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void CreateScene_EmptyName_ShouldThrow()
        {
            // Act
            Action act = () => _project.CreateScene("");

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void CreateScene_ShouldBeEditableImmediately()
        {
            // Arrange
            var sceneName = $"TestScene_{Guid.NewGuid()}";

            try
            {
                // Act
                var scene = _project.CreateScene(sceneName);
                var entity = scene.CreateEntity("TestEntity");
                scene.Save();

                // Assert
                scene.AllEntities.Should().ContainSingle();
                scene.AllEntities.First().Name.Should().Be("TestEntity");

                // Reload and verify persistence
                scene.Reload();
                scene.AllEntities.Should().ContainSingle();
                scene.AllEntities.First().Name.Should().Be("TestEntity");
            }
            finally
            {
                // Cleanup
                var scenePath = Path.Combine(_project.AssetsPath, $"{sceneName}.sdscene");
                if (File.Exists(scenePath))
                    File.Delete(scenePath);
            }
        }

        [Test]
        public void CreateScene_ShouldHaveValidSceneStructure()
        {
            // Arrange
            var sceneName = $"TestScene_{Guid.NewGuid()}";

            try
            {
                // Act
                var scene = _project.CreateScene(sceneName);

                // Assert
                scene.Id.Should().NotBeNullOrEmpty();
                scene.FilePath.Should().NotBeNullOrEmpty();
                scene.AllEntities.Should().BeEmpty();

                // Check YAML structure
                var yaml = File.ReadAllText(scene.FilePath);
                yaml.Should().Contain("!SceneAsset");
                yaml.Should().Contain("Id:");
                yaml.Should().Contain("SerializedVersion:");
                yaml.Should().Contain("Hierarchy:");
                yaml.Should().Contain("RootParts: []");
                yaml.Should().Contain("Parts: []");
            }
            finally
            {
                // Cleanup
                var scenePath = Path.Combine(_project.AssetsPath, $"{sceneName}.sdscene");
                if (File.Exists(scenePath))
                    File.Delete(scenePath);
            }
        }

        [Test]
        public void CreateScene_ShouldBeLoadableAfterRescan()
        {
            // Arrange
            var sceneName = $"TestScene_{Guid.NewGuid()}";

            try
            {
                // Act
                var createdScene = _project.CreateScene(sceneName);
                _project.Rescan();

                var loadedScene = _project.LoadScene(sceneName);

                // Assert
                loadedScene.Should().NotBeNull();
                loadedScene.Id.Should().Be(createdScene.Id);
                loadedScene.FilePath.Should().Be(createdScene.FilePath);
            }
            finally
            {
                // Cleanup
                var scenePath = Path.Combine(_project.AssetsPath, $"{sceneName}.sdscene");
                if (File.Exists(scenePath))
                    File.Delete(scenePath);
            }
        }

        [Test]
        public void IntegrationTest_LoadSceneAndModify()
        {
            // Load scene
            var scene = _project.LoadScene("MainScene");

            // Modify entity
            var entity = scene.AllEntities.FirstOrDefault();
            if (entity != null)
            {
                var transform = entity.GetTransform();
                if (transform != null)
                {
                    transform.SetPosition(100, 200, 300);

                    // Verify change
                    var pos = transform.GetPosition();
                    pos.X.Should().Be(100);
                }
            }

            // Demonstrates the workflow - scene loaded from project
            scene.Should().NotBeNull();
        }

        [Test]
        public void IntegrationTest_UseAssetReferencesWithScene()
        {
            var scene = _project.LoadScene("MainScene");
            var prefabs = _project.GetPrefabs();

            if (prefabs.Any())
            {
                var prefabRef = prefabs.First();

                // Should be able to use asset reference from project with scene
                // (This tests that ProjectScanner and Scene integration works)
                prefabRef.Should().NotBeNull();
                prefabRef.Reference.Should().Contain(":");
            }
        }

        [Test]
        public void Rescan_ShouldUpdateAssetList()
        {
            var originalCount = _project.GetAllAssets().Count;

            // Rescan
            _project.Rescan();

            var newCount = _project.GetAllAssets().Count;

            // Count should be same (no assets added/removed)
            newCount.Should().Be(originalCount);
        }
    }
}
