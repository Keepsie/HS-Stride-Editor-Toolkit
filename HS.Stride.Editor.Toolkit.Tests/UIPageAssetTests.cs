// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using NUnit.Framework;
using FluentAssertions;
using HS.Stride.Editor.Toolkit.Core;

namespace HS.Stride.Editor.Toolkit.Tests
{
    [TestFixture]
    public class UIPageAssetTests
    {
        private string _testUIPagePath;

        [SetUp]
        public void Setup()
        {
            _testUIPagePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Example Assets", "Page.sduipage");
        }

        [Test]
        public void Load_ValidUIPageFile_ShouldLoadUIPage()
        {
            // Act
            var uiPage = UIPageAsset.Load(_testUIPagePath);

            // Assert
            uiPage.Should().NotBeNull();
            uiPage.Id.Should().NotBeNullOrEmpty();
            uiPage.FilePath.Should().Be(_testUIPagePath);
        }

        [Test]
        public void Load_NonExistentFile_ShouldThrowFileNotFoundException()
        {
            // Arrange
            var nonExistentPath = "NonExistent.sduipage";

            // Act
            Action act = () => UIPageAsset.Load(nonExistentPath);

            // Assert
            act.Should().Throw<FileNotFoundException>();
        }

        [Test]
        public void GetDesignResolution_ValidUIPage_ShouldReturnResolution()
        {
            // Arrange
            var uiPage = UIPageAsset.Load(_testUIPagePath);

            // Act
            var resolution = uiPage.GetDesignResolution();

            // Assert
            resolution.Should().NotBeNull();
            resolution.Value.X.Should().BeGreaterThan(0);
            resolution.Value.Y.Should().BeGreaterThan(0);
            resolution.Value.Z.Should().BeGreaterThan(0);
        }

        [Test]
        public void SetDesignResolution_ValidValues_ShouldSetResolution()
        {
            // Arrange
            var uiPage = UIPageAsset.Load(_testUIPagePath);

            // Act
            uiPage.SetDesignResolution(1920f, 1080f, 1500f);

            // Assert
            var resolution = uiPage.GetDesignResolution();
            resolution.Should().NotBeNull();
            resolution.Value.X.Should().Be(1920f);
            resolution.Value.Y.Should().Be(1080f);
            resolution.Value.Z.Should().Be(1500f);
        }

        [Test]
        public void Get_ValidPropertyPath_ShouldReturnValue()
        {
            // Arrange
            var uiPage = UIPageAsset.Load(_testUIPagePath);

            // Act
            var designResolution = uiPage.Get("Design.Resolution");

            // Assert
            designResolution.Should().NotBeNull();
        }

        [Test]
        public void Set_ValidPropertyPath_ShouldSetValue()
        {
            // Arrange
            var uiPage = UIPageAsset.Load(_testUIPagePath);
            var newResolution = new Dictionary<string, object>
            {
                ["X"] = 1920f,
                ["Y"] = 1080f,
                ["Z"] = 1000f
            };

            // Act
            uiPage.Set("Design.Resolution", newResolution);

            // Assert
            uiPage.Get("Design.Resolution").Should().BeEquivalentTo(newResolution);
        }

        [Test]
        public void Save_ModifiedUIPage_ShouldSaveToFile()
        {
            // Arrange
            var tempPath = Path.Combine(Path.GetTempPath(), $"test_uipage_{Guid.NewGuid()}.sduipage");

            try
            {
                File.Copy(_testUIPagePath, tempPath);
                var uiPage = UIPageAsset.Load(tempPath);
                uiPage.SetDesignResolution(1024f, 768f, 500f);

                // Act
                uiPage.Save();

                // Assert
                var reloaded = UIPageAsset.Load(tempPath);
                var resolution = reloaded.GetDesignResolution();
                resolution.Value.X.Should().Be(1024f);
                resolution.Value.Y.Should().Be(768f);
                resolution.Value.Z.Should().Be(500f);
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
            var uiPage = UIPageAsset.Load(_testUIPagePath);
            var tempPath = Path.Combine(Path.GetTempPath(), $"test_uipage_{Guid.NewGuid()}.sduipage");

            try
            {
                uiPage.SetDesignResolution(800f, 600f, 1000f);

                // Act
                uiPage.SaveAs(tempPath);

                // Assert
                File.Exists(tempPath).Should().BeTrue();
                var loaded = UIPageAsset.Load(tempPath);
                var resolution = loaded.GetDesignResolution();
                resolution.Value.X.Should().Be(800f);
                resolution.Value.Y.Should().Be(600f);
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }
    }
}
