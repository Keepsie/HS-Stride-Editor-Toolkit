// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using NUnit.Framework;
using FluentAssertions;
using HS.Stride.Editor.Toolkit.Core;

namespace HS.Stride.Editor.Toolkit.Tests
{
    [TestFixture]
    public class MaterialAssetTests
    {
        private string _testMaterialPath;

        [SetUp]
        public void Setup()
        {
            _testMaterialPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Example Assets", "BG00.sdmat");
        }

        [Test]
        public void Load_ValidMaterialFile_ShouldLoadMaterial()
        {
            // Act
            var material = MaterialAsset.Load(_testMaterialPath);

            // Assert
            material.Should().NotBeNull();
            material.Id.Should().NotBeNullOrEmpty();
            material.FilePath.Should().Be(_testMaterialPath);
        }

        [Test]
        public void Load_NonExistentFile_ShouldThrowFileNotFoundException()
        {
            // Arrange
            var nonExistentPath = "NonExistent.sdmat";

            // Act
            Action act = () => MaterialAsset.Load(nonExistentPath);

            // Assert
            act.Should().Throw<FileNotFoundException>();
        }

        [Test]
        public void GetDiffuseTexture_ValidMaterial_ShouldReturnTextureReference()
        {
            // Arrange
            var material = MaterialAsset.Load(_testMaterialPath);

            // Act
            var textureRef = material.GetDiffuseTexture();

            // Assert
            textureRef.Should().NotBeNullOrEmpty();
            textureRef.Should().Contain(":");
        }

        [Test]
        public void SetDiffuseTexture_ValidReference_ShouldSetTexture()
        {
            // Arrange
            var material = MaterialAsset.Load(_testMaterialPath);
            var newTextureRef = "test-guid:test-path";

            // Act
            material.SetDiffuseTexture(newTextureRef);

            // Assert
            material.GetDiffuseTexture().Should().Be(newTextureRef);
        }

        [Test]
        public void GetUVScale_ValidMaterial_ShouldReturnScale()
        {
            // Arrange
            var material = MaterialAsset.Load(_testMaterialPath);

            // Act
            var scale = material.GetUVScale();

            // Assert
            scale.Should().NotBeNull();
            scale.Value.X.Should().BeGreaterOrEqualTo(0);
            scale.Value.Y.Should().BeGreaterOrEqualTo(0);
        }

        [Test]
        public void SetUVScale_ValidValues_ShouldSetScale()
        {
            // Arrange
            var material = MaterialAsset.Load(_testMaterialPath);

            // Act
            material.SetUVScale(2.0f, 3.0f);

            // Assert
            var scale = material.GetUVScale();
            scale.Should().NotBeNull();
            scale.Value.X.Should().Be(2.0f);
            scale.Value.Y.Should().Be(3.0f);
        }

        [Test]
        public void Get_ValidPropertyPath_ShouldReturnValue()
        {
            // Arrange
            var material = MaterialAsset.Load(_testMaterialPath);

            // Act - Access nested property using full path
            var texture = material.Get("Attributes.Diffuse.DiffuseMap.Texture");

            // Assert
            texture.Should().NotBeNull();
        }

        [Test]
        public void Set_ValidPropertyPath_ShouldSetValue()
        {
            // Arrange
            var material = MaterialAsset.Load(_testMaterialPath);
            var newValue = "test-value";

            // Act - Parser flattens nested type declarations
            material.Set("Attributes.Texture", newValue);

            // Assert
            material.Get("Attributes.Texture").Should().Be(newValue);
        }

        [Test]
        public void Save_ModifiedMaterial_ShouldSaveToFile()
        {
            // Arrange
            var tempPath = Path.Combine(Path.GetTempPath(), $"test_material_{Guid.NewGuid()}.sdmat");

            try
            {
                File.Copy(_testMaterialPath, tempPath);
                var material = MaterialAsset.Load(tempPath);
                material.SetUVScale(5.0f, 5.0f);

                // Act
                material.Save();

                // Assert
                var reloaded = MaterialAsset.Load(tempPath);
                var scale = reloaded.GetUVScale();
                scale.Value.X.Should().Be(5.0f);
                scale.Value.Y.Should().Be(5.0f);
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
            var material = MaterialAsset.Load(_testMaterialPath);
            var tempPath = Path.Combine(Path.GetTempPath(), $"test_material_{Guid.NewGuid()}.sdmat");

            try
            {
                material.SetUVScale(4.0f, 4.0f);

                // Act
                material.SaveAs(tempPath);

                // Assert
                File.Exists(tempPath).Should().BeTrue();
                var loaded = MaterialAsset.Load(tempPath);
                var scale = loaded.GetUVScale();
                scale.Value.X.Should().Be(4.0f);
                scale.Value.Y.Should().Be(4.0f);
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        [Test]
        public void Load_EmptyFile_ShouldHandleGracefully()
        {
            // Arrange
            var tempPath = Path.Combine(Path.GetTempPath(), $"empty_material_{Guid.NewGuid()}.sdmat");

            try
            {
                File.WriteAllText(tempPath, string.Empty);

                // Act
                var material = MaterialAsset.Load(tempPath);

                // Assert - The implementation handles empty files gracefully
                material.Should().NotBeNull();
                material.FilePath.Should().Be(tempPath);
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        [Test]
        public void Load_MalformedYaml_ShouldHandleGracefully()
        {
            // Arrange
            var tempPath = Path.Combine(Path.GetTempPath(), $"malformed_material_{Guid.NewGuid()}.sdmat");

            try
            {
                // Create intentionally malformed YAML
                File.WriteAllText(tempPath, "!MaterialAsset\nId: invalid\n  broken: yaml: structure");

                // Act
                var material = MaterialAsset.Load(tempPath);

                // Assert - The implementation handles malformed YAML gracefully
                material.Should().NotBeNull();
                material.FilePath.Should().Be(tempPath);
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        [Test]
        public void GetDiffuseTexture_MaterialWithNoTexture_ShouldReturnNullOrEmpty()
        {
            // Arrange
            var tempPath = Path.Combine(Path.GetTempPath(), $"no_texture_material_{Guid.NewGuid()}.sdmat");

            try
            {
                // Create a minimal material without texture
                var minimalMaterial = @"!MaterialAsset
Id: 12345678-1234-1234-1234-123456789012
SerializedVersion: {Stride: 3.1.0.1}
Tags: []
Attributes:
    Surface: !MaterialNormalMapFeature {}";
                File.WriteAllText(tempPath, minimalMaterial);

                var material = MaterialAsset.Load(tempPath);

                // Act
                var texture = material.GetDiffuseTexture();

                // Assert
                texture.Should().BeNullOrEmpty();
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        [Test]
        public void Get_NonExistentProperty_ShouldReturnNull()
        {
            // Arrange
            var material = MaterialAsset.Load(_testMaterialPath);

            // Act
            var value = material.Get("NonExistent.Property.Path");

            // Assert
            value.Should().BeNull();
        }

        [Test]
        public void GetUVScale_MaterialWithoutUVScale_ShouldReturnNull()
        {
            // Arrange
            var tempPath = Path.Combine(Path.GetTempPath(), $"no_uvscale_material_{Guid.NewGuid()}.sdmat");

            try
            {
                // Create a minimal material without UV scale
                var minimalMaterial = @"!MaterialAsset
Id: 12345678-1234-1234-1234-123456789012
SerializedVersion: {Stride: 3.1.0.1}
Tags: []
Attributes:
    Surface: !MaterialNormalMapFeature {}";
                File.WriteAllText(tempPath, minimalMaterial);

                var material = MaterialAsset.Load(tempPath);

                // Act
                var scale = material.GetUVScale();

                // Assert
                scale.Should().BeNull();
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        [Test]
        public void SetUVScale_NegativeValues_ShouldSetNegativeValues()
        {
            // Arrange
            var material = MaterialAsset.Load(_testMaterialPath);

            // Act - Negative values might be valid in some contexts (mirroring)
            material.SetUVScale(-1.0f, -2.0f);

            // Assert
            var scale = material.GetUVScale();
            scale.Should().NotBeNull();
            scale.Value.X.Should().Be(-1.0f);
            scale.Value.Y.Should().Be(-2.0f);
        }

        [Test]
        public void SetUVScale_ZeroValues_ShouldSetZeroValues()
        {
            // Arrange
            var material = MaterialAsset.Load(_testMaterialPath);

            // Act
            material.SetUVScale(0.0f, 0.0f);

            // Assert
            var scale = material.GetUVScale();
            scale.Should().NotBeNull();
            scale.Value.X.Should().Be(0.0f);
            scale.Value.Y.Should().Be(0.0f);
        }

        [Test]
        public void SetDiffuseTexture_NullOrEmptyReference_ShouldClearTexture()
        {
            // Arrange
            var material = MaterialAsset.Load(_testMaterialPath);

            // Act
            material.SetDiffuseTexture(null);

            // Assert
            material.GetDiffuseTexture().Should().BeNullOrEmpty();
        }
    }
}
