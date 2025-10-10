// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using NUnit.Framework;
using FluentAssertions;
using HS.Stride.Editor.Toolkit.Core;

namespace HS.Stride.Editor.Toolkit.Tests
{
    [TestFixture]
    public class TextureAssetTests
    {
        private string _testTexturePath;

        [SetUp]
        public void Setup()
        {
            _testTexturePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Example Assets", "BG_00 Texture.sdtex");
        }

        [Test]
        public void Load_ValidTextureFile_ShouldLoadTexture()
        {
            // Act
            var texture = TextureAsset.Load(_testTexturePath);

            // Assert
            texture.Should().NotBeNull();
            texture.Id.Should().NotBeNullOrEmpty();
            texture.FilePath.Should().Be(_testTexturePath);
        }

        [Test]
        public void Load_NonExistentFile_ShouldThrowFileNotFoundException()
        {
            // Arrange
            var nonExistentPath = "NonExistent.sdtex";

            // Act
            Action act = () => TextureAsset.Load(nonExistentPath);

            // Assert
            act.Should().Throw<FileNotFoundException>();
        }

        [Test]
        public void GetSource_ValidTexture_ShouldReturnSourcePath()
        {
            // Arrange
            var texture = TextureAsset.Load(_testTexturePath);

            // Act
            var source = texture.GetSource();

            // Assert
            source.Should().NotBeNullOrEmpty();
            source.Should().NotStartWith("!file");
        }

        [Test]
        public void SetSource_ValidPath_ShouldSetSource()
        {
            // Arrange
            var texture = TextureAsset.Load(_testTexturePath);
            var newSource = "../../Resources/NewTexture.png";

            // Act
            texture.SetSource(newSource);

            // Assert
            texture.GetSource().Should().Be(newSource);
        }

        [Test]
        public void IsStreamable_ValidTexture_ShouldReturnValue()
        {
            // Arrange
            var texture = TextureAsset.Load(_testTexturePath);

            // Act
            var isStreamable = texture.IsStreamable;

            // Assert
            isStreamable.Should().BeFalse();
        }

        [Test]
        public void IsStreamable_SetValue_ShouldSetStreamable()
        {
            // Arrange
            var texture = TextureAsset.Load(_testTexturePath);

            // Act
            texture.IsStreamable = true;

            // Assert
            texture.IsStreamable.Should().BeTrue();
        }

        [Test]
        public void PremultiplyAlpha_ValidTexture_ShouldReturnValue()
        {
            // Arrange
            var texture = TextureAsset.Load(_testTexturePath);

            // Act
            var premultiplyAlpha = texture.PremultiplyAlpha;

            // Assert
            premultiplyAlpha.Should().NotBeNull();
            premultiplyAlpha.Value.Should().BeFalse();
        }

        [Test]
        public void PremultiplyAlpha_SetValue_ShouldSetPremultiplyAlpha()
        {
            // Arrange
            var texture = TextureAsset.Load(_testTexturePath);

            // Act
            texture.PremultiplyAlpha = true;

            // Assert
            texture.PremultiplyAlpha.Should().BeTrue();
        }

        [Test]
        public void Save_ModifiedTexture_ShouldSaveToFile()
        {
            // Arrange
            var tempPath = Path.Combine(Path.GetTempPath(), $"test_texture_{Guid.NewGuid()}.sdtex");

            try
            {
                File.Copy(_testTexturePath, tempPath);
                var texture = TextureAsset.Load(tempPath);
                texture.IsStreamable = true;
                texture.PremultiplyAlpha = true;

                // Act
                texture.Save();

                // Assert
                var reloaded = TextureAsset.Load(tempPath);
                reloaded.IsStreamable.Should().BeTrue();
                reloaded.PremultiplyAlpha.Should().BeTrue();
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
            var texture = TextureAsset.Load(_testTexturePath);
            var tempPath = Path.Combine(Path.GetTempPath(), $"test_texture_{Guid.NewGuid()}.sdtex");

            try
            {
                texture.IsStreamable = true;

                // Act
                texture.SaveAs(tempPath);

                // Assert
                File.Exists(tempPath).Should().BeTrue();
                var loaded = TextureAsset.Load(tempPath);
                loaded.IsStreamable.Should().BeTrue();
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }
    }
}
