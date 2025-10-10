// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using NUnit.Framework;
using FluentAssertions;
using HS.Stride.Editor.Toolkit.Core;

namespace HS.Stride.Editor.Toolkit.Tests
{
    [TestFixture]
    public class SpriteSheetAssetTests
    {
        private string _testSpriteSheetPath;

        [SetUp]
        public void Setup()
        {
            _testSpriteSheetPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Example Assets", "UIImages.sdsheet");
        }

        [Test]
        public void Load_ValidSpriteSheetFile_ShouldLoadSpriteSheet()
        {
            // Act
            var spriteSheet = SpriteSheetAsset.Load(_testSpriteSheetPath);

            // Assert
            spriteSheet.Should().NotBeNull();
            spriteSheet.Id.Should().NotBeNullOrEmpty();
            spriteSheet.FilePath.Should().Be(_testSpriteSheetPath);
        }

        [Test]
        public void Load_NonExistentFile_ShouldThrowFileNotFoundException()
        {
            // Arrange
            var nonExistentPath = "NonExistent.sdsheet";

            // Act
            Action act = () => SpriteSheetAsset.Load(nonExistentPath);

            // Assert
            act.Should().Throw<FileNotFoundException>();
        }

        [Test]
        public void Get_ValidPropertyName_ShouldReturnValue()
        {
            // Arrange
            var spriteSheet = SpriteSheetAsset.Load(_testSpriteSheetPath);

            // Act
            var type = spriteSheet.Get("Type");

            // Assert
            type.Should().NotBeNull();
            type.Should().Be("Sprite2D");
        }

        [Test]
        public void Get_SpritesProperty_ShouldReturnSprites()
        {
            // Arrange
            var spriteSheet = SpriteSheetAsset.Load(_testSpriteSheetPath);

            // Act
            var sprites = spriteSheet.Get("Sprites");

            // Assert
            sprites.Should().NotBeNull();
            sprites.Should().BeOfType<Dictionary<string, object>>();
            var spritesDict = sprites as Dictionary<string, object>;
            spritesDict.Should().NotBeEmpty();
        }

        [Test]
        public void Set_ValidPropertyName_ShouldSetValue()
        {
            // Arrange
            var spriteSheet = SpriteSheetAsset.Load(_testSpriteSheetPath);
            var newType = "Sprite3D";

            // Act
            spriteSheet.Set("Type", newType);

            // Assert
            spriteSheet.Get("Type").Should().Be(newType);
        }

        [Test]
        public void GetAllProperties_ShouldReturnAllProperties()
        {
            // Arrange
            var spriteSheet = SpriteSheetAsset.Load(_testSpriteSheetPath);

            // Act
            var properties = spriteSheet.GetAllProperties();

            // Assert
            properties.Should().NotBeNull();
            properties.Should().NotBeEmpty();
            properties.Should().ContainKey("Type");
            properties.Should().ContainKey("Sprites");
        }

        [Test]
        public void Save_ModifiedSpriteSheet_ShouldSaveToFile()
        {
            // Arrange
            var tempPath = Path.Combine(Path.GetTempPath(), $"test_spritesheet_{Guid.NewGuid()}.sdsheet");

            try
            {
                File.Copy(_testSpriteSheetPath, tempPath);
                var spriteSheet = SpriteSheetAsset.Load(tempPath);
                spriteSheet.Set("Type", "ModifiedType");

                // Act
                spriteSheet.Save();

                // Assert
                var reloaded = SpriteSheetAsset.Load(tempPath);
                reloaded.Get("Type").Should().Be("ModifiedType");
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
            var spriteSheet = SpriteSheetAsset.Load(_testSpriteSheetPath);
            var tempPath = Path.Combine(Path.GetTempPath(), $"test_spritesheet_{Guid.NewGuid()}.sdsheet");

            try
            {
                spriteSheet.Set("Type", "NewType");

                // Act
                spriteSheet.SaveAs(tempPath);

                // Assert
                File.Exists(tempPath).Should().BeTrue();
                var loaded = SpriteSheetAsset.Load(tempPath);
                loaded.Get("Type").Should().Be("NewType");
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }
    }
}
