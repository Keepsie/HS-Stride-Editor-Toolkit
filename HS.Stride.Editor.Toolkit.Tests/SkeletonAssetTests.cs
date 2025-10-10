// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using NUnit.Framework;
using FluentAssertions;
using HS.Stride.Editor.Toolkit.Core;

namespace HS.Stride.Editor.Toolkit.Tests
{
    [TestFixture]
    public class SkeletonAssetTests
    {
        private string _testSkeletonPath;

        [SetUp]
        public void Setup()
        {
            _testSkeletonPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Example Assets", "character_00 Skeleton.sdskel");
        }

        [Test]
        public void Load_ValidSkeletonFile_ShouldLoadSkeleton()
        {
            // Act
            var skeleton = SkeletonAsset.Load(_testSkeletonPath);

            // Assert
            skeleton.Should().NotBeNull();
            skeleton.Id.Should().NotBeNullOrEmpty();
            skeleton.FilePath.Should().Be(_testSkeletonPath);
        }

        [Test]
        public void Load_NonExistentFile_ShouldThrowFileNotFoundException()
        {
            // Arrange
            var nonExistentPath = "NonExistent.sdskel";

            // Act
            Action act = () => SkeletonAsset.Load(nonExistentPath);

            // Assert
            act.Should().Throw<FileNotFoundException>();
        }

        [Test]
        public void GetSource_ValidSkeleton_ShouldReturnSourcePath()
        {
            // Arrange
            var skeleton = SkeletonAsset.Load(_testSkeletonPath);

            // Act
            var source = skeleton.GetSource();

            // Assert
            source.Should().NotBeNullOrEmpty();
            source.Should().NotStartWith("!file");
        }

        [Test]
        public void SetSource_ValidPath_ShouldSetSource()
        {
            // Arrange
            var skeleton = SkeletonAsset.Load(_testSkeletonPath);
            var newSource = "../../Resources/NewSkeleton.FBX";

            // Act
            skeleton.SetSource(newSource);

            // Assert
            skeleton.GetSource().Should().Be(newSource);
        }

        [Test]
        public void Get_ValidPropertyName_ShouldReturnValue()
        {
            // Arrange
            var skeleton = SkeletonAsset.Load(_testSkeletonPath);

            // Act
            var nodes = skeleton.Get("Nodes");

            // Assert
            nodes.Should().NotBeNull();
        }

        [Test]
        public void Set_ValidPropertyName_ShouldSetValue()
        {
            // Arrange
            var skeleton = SkeletonAsset.Load(_testSkeletonPath);
            var testValue = "TestValue";

            // Act
            skeleton.Set("TestProperty", testValue);

            // Assert
            skeleton.Get("TestProperty").Should().Be(testValue);
        }

        [Test]
        public void GetAllProperties_ShouldReturnAllProperties()
        {
            // Arrange
            var skeleton = SkeletonAsset.Load(_testSkeletonPath);

            // Act
            var properties = skeleton.GetAllProperties();

            // Assert
            properties.Should().NotBeNull();
            properties.Should().NotBeEmpty();
            properties.Should().ContainKey("Nodes");
        }

        [Test]
        public void Save_ModifiedSkeleton_ShouldSaveToFile()
        {
            // Arrange
            var tempPath = Path.Combine(Path.GetTempPath(), $"test_skeleton_{Guid.NewGuid()}.sdskel");

            try
            {
                File.Copy(_testSkeletonPath, tempPath);
                var skeleton = SkeletonAsset.Load(tempPath);
                var newSource = "../../Resources/Modified.FBX";
                skeleton.SetSource(newSource);

                // Act
                skeleton.Save();

                // Assert
                var reloaded = SkeletonAsset.Load(tempPath);
                reloaded.GetSource().Should().Be(newSource);
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
            var skeleton = SkeletonAsset.Load(_testSkeletonPath);
            var tempPath = Path.Combine(Path.GetTempPath(), $"test_skeleton_{Guid.NewGuid()}.sdskel");

            try
            {
                var newSource = "../../Resources/New.FBX";
                skeleton.SetSource(newSource);

                // Act
                skeleton.SaveAs(tempPath);

                // Assert
                File.Exists(tempPath).Should().BeTrue();
                var loaded = SkeletonAsset.Load(tempPath);
                loaded.GetSource().Should().Be(newSource);
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }
    }
}
