// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using NUnit.Framework;
using FluentAssertions;
using HS.Stride.Editor.Toolkit.Core;

namespace HS.Stride.Editor.Toolkit.Tests
{
    [TestFixture]
    public class AnimationAssetTests
    {
        private string _testAnimationPath;

        [SetUp]
        public void Setup()
        {
            _testAnimationPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Example Assets", "ma00_1.sdanim");
        }

        [Test]
        public void Load_ValidAnimationFile_ShouldLoadAnimation()
        {
            // Act
            var animation = AnimationAsset.Load(_testAnimationPath);

            // Assert
            animation.Should().NotBeNull();
            animation.Id.Should().NotBeNullOrEmpty();
            animation.FilePath.Should().Be(_testAnimationPath);
        }

        [Test]
        public void Load_NonExistentFile_ShouldThrowFileNotFoundException()
        {
            // Arrange
            var nonExistentPath = "NonExistent.sdanim";

            // Act
            Action act = () => AnimationAsset.Load(nonExistentPath);

            // Assert
            act.Should().Throw<FileNotFoundException>();
        }

        [Test]
        public void GetSource_ValidAnimation_ShouldReturnSourcePath()
        {
            // Arrange
            var animation = AnimationAsset.Load(_testAnimationPath);

            // Act
            var source = animation.GetSource();

            // Assert
            source.Should().NotBeNullOrEmpty();
            source.Should().NotStartWith("!file");
        }

        [Test]
        public void SetSource_ValidPath_ShouldSetSource()
        {
            // Arrange
            var animation = AnimationAsset.Load(_testAnimationPath);
            var newSource = "../../Resources/NewAnimation.FBX";

            // Act
            animation.SetSource(newSource);

            // Assert
            animation.GetSource().Should().Be(newSource);
        }

        [Test]
        public void RepeatMode_ValidAnimation_ShouldReturnValue()
        {
            // Arrange
            var animation = AnimationAsset.Load(_testAnimationPath);

            // Act
            var repeatMode = animation.RepeatMode;

            // Assert
            repeatMode.Should().NotBeNullOrEmpty();
            repeatMode.Should().Be("LoopInfinite");
        }

        [Test]
        public void RepeatMode_SetValue_ShouldSetRepeatMode()
        {
            // Arrange
            var animation = AnimationAsset.Load(_testAnimationPath);

            // Act
            animation.RepeatMode = "PlayOnce";

            // Assert
            animation.RepeatMode.Should().Be("PlayOnce");
        }

        [Test]
        public void RootMotion_ValidAnimation_ShouldReturnValue()
        {
            // Arrange
            var animation = AnimationAsset.Load(_testAnimationPath);

            // Act
            var rootMotion = animation.RootMotion;

            // Assert
            rootMotion.Should().BeFalse();
        }

        [Test]
        public void RootMotion_SetValue_ShouldSetRootMotion()
        {
            // Arrange
            var animation = AnimationAsset.Load(_testAnimationPath);

            // Act
            animation.RootMotion = true;

            // Assert
            animation.RootMotion.Should().BeTrue();
        }

        [Test]
        public void GetSkeletonReference_ValidAnimation_ShouldReturnReference()
        {
            // Arrange
            var animation = AnimationAsset.Load(_testAnimationPath);

            // Act
            var skeletonRef = animation.GetSkeletonReference();

            // Assert
            skeletonRef.Should().NotBeNullOrEmpty();
            skeletonRef.Should().Contain(":");
        }

        [Test]
        public void SetSkeletonReference_ValidReference_ShouldSetSkeleton()
        {
            // Arrange
            var animation = AnimationAsset.Load(_testAnimationPath);
            var newSkeletonRef = "test-guid:test-skeleton";

            // Act
            animation.SetSkeletonReference(newSkeletonRef);

            // Assert
            animation.GetSkeletonReference().Should().Be(newSkeletonRef);
        }

        [Test]
        public void GetPreviewModel_ValidAnimation_ShouldReturnReference()
        {
            // Arrange
            var animation = AnimationAsset.Load(_testAnimationPath);

            // Act
            var previewModel = animation.GetPreviewModel();

            // Assert
            previewModel.Should().NotBeNullOrEmpty();
            previewModel.Should().Contain(":");
        }

        [Test]
        public void SetPreviewModel_ValidReference_ShouldSetPreviewModel()
        {
            // Arrange
            var animation = AnimationAsset.Load(_testAnimationPath);
            var newModelRef = "test-guid:test-model";

            // Act
            animation.SetPreviewModel(newModelRef);

            // Assert
            animation.GetPreviewModel().Should().Be(newModelRef);
        }

        [Test]
        public void Save_ModifiedAnimation_ShouldSaveToFile()
        {
            // Arrange
            var tempPath = Path.Combine(Path.GetTempPath(), $"test_animation_{Guid.NewGuid()}.sdanim");

            try
            {
                File.Copy(_testAnimationPath, tempPath);
                var animation = AnimationAsset.Load(tempPath);
                animation.RepeatMode = "PlayOnce";
                animation.RootMotion = true;

                // Act
                animation.Save();

                // Assert
                var reloaded = AnimationAsset.Load(tempPath);
                reloaded.RepeatMode.Should().Be("PlayOnce");
                reloaded.RootMotion.Should().BeTrue();
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
            var animation = AnimationAsset.Load(_testAnimationPath);
            var tempPath = Path.Combine(Path.GetTempPath(), $"test_animation_{Guid.NewGuid()}.sdanim");

            try
            {
                animation.RepeatMode = "PlayOnce";

                // Act
                animation.SaveAs(tempPath);

                // Assert
                File.Exists(tempPath).Should().BeTrue();
                var loaded = AnimationAsset.Load(tempPath);
                loaded.RepeatMode.Should().Be("PlayOnce");
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }
    }
}
