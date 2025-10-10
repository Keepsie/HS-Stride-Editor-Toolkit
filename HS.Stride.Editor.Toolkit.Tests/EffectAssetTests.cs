// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using NUnit.Framework;
using FluentAssertions;
using HS.Stride.Editor.Toolkit.Core;
using HS.Stride.Editor.Toolkit.Core.AssetEditing;

namespace HS.Stride.Editor.Toolkit.Tests
{
    [TestFixture]
    public class EffectAssetTests
    {
        private string _testEffectPath;

        [SetUp]
        public void Setup()
        {
            // Check if effect asset exists in test data
            var possiblePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Example Assets");
            var effectFiles = Directory.GetFiles(possiblePath, "*.sdfx", SearchOption.AllDirectories);

            if (effectFiles.Length > 0)
            {
                _testEffectPath = effectFiles[0];
            }
        }

        [Test]
        public void Load_ValidEffectFile_ShouldLoadEffect()
        {
            // Arrange
            if (string.IsNullOrEmpty(_testEffectPath))
            {
                Assert.Inconclusive("No .sdfx test file available");
                return;
            }

            // Act
            var effect = EffectAsset.Load(_testEffectPath);

            // Assert
            effect.Should().NotBeNull();
            effect.Id.Should().NotBeNullOrEmpty();
            effect.FilePath.Should().Be(_testEffectPath);
        }

        [Test]
        public void Load_NonExistentFile_ShouldThrowFileNotFoundException()
        {
            // Arrange
            var nonExistentPath = "NonExistent.sdfx";

            // Act
            Action act = () => EffectAsset.Load(nonExistentPath);

            // Assert
            act.Should().Throw<FileNotFoundException>();
        }

        [Test]
        public void Get_ValidPropertyName_ShouldReturnValue()
        {
            // Arrange
            if (string.IsNullOrEmpty(_testEffectPath))
            {
                Assert.Inconclusive("No .sdfx test file available");
                return;
            }

            var effect = EffectAsset.Load(_testEffectPath);

            // Act
            var properties = effect.GetAllProperties();

            // Assert
            properties.Should().NotBeNull();
        }

        [Test]
        public void Set_ValidPropertyName_ShouldSetValue()
        {
            // Arrange
            if (string.IsNullOrEmpty(_testEffectPath))
            {
                Assert.Inconclusive("No .sdfx test file available");
                return;
            }

            var effect = EffectAsset.Load(_testEffectPath);
            var testValue = "TestValue";

            // Act
            effect.Set("TestProperty", testValue);

            // Assert
            effect.Get("TestProperty").Should().Be(testValue);
        }

        [Test]
        public void GetAllProperties_ShouldReturnAllProperties()
        {
            // Arrange
            if (string.IsNullOrEmpty(_testEffectPath))
            {
                Assert.Inconclusive("No .sdfx test file available");
                return;
            }

            var effect = EffectAsset.Load(_testEffectPath);

            // Act
            var properties = effect.GetAllProperties();

            // Assert
            properties.Should().NotBeNull();
            properties.Should().NotBeEmpty();
        }

        [Test]
        public void Save_ModifiedEffect_ShouldSaveToFile()
        {
            // Arrange
            if (string.IsNullOrEmpty(_testEffectPath))
            {
                Assert.Inconclusive("No .sdfx test file available");
                return;
            }

            var tempPath = Path.Combine(Path.GetTempPath(), $"test_effect_{Guid.NewGuid()}.sdfx");

            try
            {
                File.Copy(_testEffectPath, tempPath);
                var effect = EffectAsset.Load(tempPath);
                effect.Set("TestProperty", "ModifiedValue");

                // Act
                effect.Save();

                // Assert
                var reloaded = EffectAsset.Load(tempPath);
                reloaded.Get("TestProperty").Should().Be("ModifiedValue");
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
            if (string.IsNullOrEmpty(_testEffectPath))
            {
                Assert.Inconclusive("No .sdfx test file available");
                return;
            }

            var effect = EffectAsset.Load(_testEffectPath);
            var tempPath = Path.Combine(Path.GetTempPath(), $"test_effect_{Guid.NewGuid()}.sdfx");

            try
            {
                effect.Set("TestProperty", "NewValue");

                // Act
                effect.SaveAs(tempPath);

                // Assert
                File.Exists(tempPath).Should().BeTrue();
                var loaded = EffectAsset.Load(tempPath);
                loaded.Get("TestProperty").Should().Be("NewValue");
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }
    }
}
