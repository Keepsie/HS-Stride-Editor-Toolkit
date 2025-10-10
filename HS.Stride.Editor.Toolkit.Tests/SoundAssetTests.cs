// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using NUnit.Framework;
using FluentAssertions;
using HS.Stride.Editor.Toolkit.Core;

namespace HS.Stride.Editor.Toolkit.Tests
{
    [TestFixture]
    public class SoundAssetTests
    {
        private string _testSoundPath;

        [SetUp]
        public void Setup()
        {
            // Check if sound asset exists in test data
            var possiblePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Example Assets");
            var soundFiles = Directory.GetFiles(possiblePath, "*.sdsnd", SearchOption.AllDirectories);

            if (soundFiles.Length > 0)
            {
                _testSoundPath = soundFiles[0];
            }
        }

        [Test]
        public void Load_ValidSoundFile_ShouldLoadSound()
        {
            // Arrange
            if (string.IsNullOrEmpty(_testSoundPath))
            {
                Assert.Inconclusive("No .sdsnd test file available");
                return;
            }

            // Act
            var sound = SoundAsset.Load(_testSoundPath);

            // Assert
            sound.Should().NotBeNull();
            sound.Id.Should().NotBeNullOrEmpty();
            sound.FilePath.Should().Be(_testSoundPath);
        }

        [Test]
        public void Load_NonExistentFile_ShouldThrowFileNotFoundException()
        {
            // Arrange
            var nonExistentPath = "NonExistent.sdsnd";

            // Act
            Action act = () => SoundAsset.Load(nonExistentPath);

            // Assert
            act.Should().Throw<FileNotFoundException>();
        }

        [Test]
        public void Get_ValidPropertyName_ShouldReturnValue()
        {
            // Arrange
            if (string.IsNullOrEmpty(_testSoundPath))
            {
                Assert.Inconclusive("No .sdsnd test file available");
                return;
            }

            var sound = SoundAsset.Load(_testSoundPath);

            // Act
            var source = sound.Get("Source");

            // Assert
            source.Should().NotBeNull();
        }

        [Test]
        public void Set_ValidPropertyName_ShouldSetValue()
        {
            // Arrange
            if (string.IsNullOrEmpty(_testSoundPath))
            {
                Assert.Inconclusive("No .sdsnd test file available");
                return;
            }

            var sound = SoundAsset.Load(_testSoundPath);
            var testValue = "TestValue";

            // Act
            sound.Set("TestProperty", testValue);

            // Assert
            sound.Get("TestProperty").Should().Be(testValue);
        }

        [Test]
        public void GetAllProperties_ShouldReturnAllProperties()
        {
            // Arrange
            if (string.IsNullOrEmpty(_testSoundPath))
            {
                Assert.Inconclusive("No .sdsnd test file available");
                return;
            }

            var sound = SoundAsset.Load(_testSoundPath);

            // Act
            var properties = sound.GetAllProperties();

            // Assert
            properties.Should().NotBeNull();
            properties.Should().NotBeEmpty();
        }

        [Test]
        public void Save_ModifiedSound_ShouldSaveToFile()
        {
            // Arrange
            if (string.IsNullOrEmpty(_testSoundPath))
            {
                Assert.Inconclusive("No .sdsnd test file available");
                return;
            }

            var tempPath = Path.Combine(Path.GetTempPath(), $"test_sound_{Guid.NewGuid()}.sdsnd");

            try
            {
                File.Copy(_testSoundPath, tempPath);
                var sound = SoundAsset.Load(tempPath);
                sound.Set("TestProperty", "ModifiedValue");

                // Act
                sound.Save();

                // Assert
                var reloaded = SoundAsset.Load(tempPath);
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
            if (string.IsNullOrEmpty(_testSoundPath))
            {
                Assert.Inconclusive("No .sdsnd test file available");
                return;
            }

            var sound = SoundAsset.Load(_testSoundPath);
            var tempPath = Path.Combine(Path.GetTempPath(), $"test_sound_{Guid.NewGuid()}.sdsnd");

            try
            {
                sound.Set("TestProperty", "NewValue");

                // Act
                sound.SaveAs(tempPath);

                // Assert
                File.Exists(tempPath).Should().BeTrue();
                var loaded = SoundAsset.Load(tempPath);
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
