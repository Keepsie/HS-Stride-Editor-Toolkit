// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using NUnit.Framework;
using FluentAssertions;
using HS.Stride.Editor.Toolkit.Core;
using HS.Stride.Editor.Toolkit.Core.UIPageEditing;

namespace HS.Stride.Editor.Toolkit.Tests
{
    [TestFixture]
    public class UIPageTests
    {
        private string _testUIPagePath;
        private string _testProjectPath;

        [SetUp]
        public void Setup()
        {
            _testUIPagePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Example Scenes", "TestProject", "dev_console_page.sduipage");
            _testProjectPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Example Scenes", "TestProject");
        }

        [Test]
        public void Load_ValidUIPageFile_ShouldLoadUIPage()
        {
            // Act
            var page = UIPage.Load(_testUIPagePath);

            // Assert
            page.Should().NotBeNull();
            page.Id.Should().NotBeNullOrEmpty();
            page.FilePath.Should().Be(_testUIPagePath);
        }

        [Test]
        public void Load_NonExistentFile_ShouldThrowFileNotFoundException()
        {
            // Arrange
            var nonExistentPath = "NonExistent.sduipage";

            // Act
            Action act = () => UIPage.Load(nonExistentPath);

            // Assert
            act.Should().Throw<FileNotFoundException>();
        }

        [Test]
        public void AllElements_ShouldReturnAllUIElements()
        {
            // Arrange
            var page = UIPage.Load(_testUIPagePath);

            // Act
            var elements = page.AllElements;

            // Assert
            elements.Should().NotBeNull();
            elements.Should().NotBeEmpty();
        }

        [Test]
        public void RootElements_ShouldReturnRootUIElements()
        {
            // Arrange
            var page = UIPage.Load(_testUIPagePath);

            // Act
            var rootElements = page.RootElements;

            // Assert
            rootElements.Should().NotBeNull();
            rootElements.Should().NotBeEmpty();
            rootElements.First().Type.Should().Be("Grid");
        }

        [Test]
        public void FindElementById_ExistingElement_ShouldReturnElement()
        {
            // Arrange
            var page = UIPage.Load(_testUIPagePath);
            var expectedElement = page.AllElements.First();

            // Act
            var result = page.FindElementById(expectedElement.Id);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(expectedElement);
        }

        [Test]
        public void FindElementByName_ExistingElement_ShouldReturnElement()
        {
            // Arrange
            var page = UIPage.Load(_testUIPagePath);
            var expectedElement = page.AllElements.First(e => !string.IsNullOrEmpty(e.Name));

            // Act
            var result = page.FindElementByName(expectedElement.Name);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(expectedElement);
        }

        [Test]
        public void Create_NewUIPage_ShouldCreateEmptyPageWithRootGrid()
        {
            // Act
            var page = UIPage.Create("TestPage");

            // Assert
            page.Should().NotBeNull();
            page.Id.Should().NotBeNullOrEmpty();
            page.AllElements.Should().HaveCount(1, "should have one root Grid");

            var rootElement = page.RootElements.First();
            rootElement.Should().NotBeNull();
            rootElement.Name.Should().Be("TestPage");
            rootElement.Type.Should().Be("Grid");
        }

        [Test]
        public void Create_ThenSave_ShouldWriteValidUIPageFile()
        {
            // Arrange
            var tempPath = Path.Combine(Path.GetTempPath(), $"test_new_page_{Guid.NewGuid()}.sduipage");

            try
            {
                // Act - Create and save
                var page = UIPage.Create("MyMenu", tempPath);
                TestContext.WriteLine($"Created page, root name: '{page.RootElements.First().Name}'");

                page.Save();

                // Debug: Print generated YAML
                var yaml = File.ReadAllText(tempPath);
                TestContext.WriteLine("=== GENERATED YAML ===");
                TestContext.WriteLine(yaml);

                // Assert - File should exist and be loadable
                File.Exists(tempPath).Should().BeTrue();

                var loaded = UIPage.Load(tempPath);
                loaded.Should().NotBeNull();
                loaded.AllElements.Should().HaveCount(1);

                TestContext.WriteLine($"Loaded page, root name: '{loaded.RootElements.First().Name}'");
                loaded.RootElements.First().Name.Should().Be("MyMenu");
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        [Test]
        public void CreateElement_ValidParameters_ShouldCreateElement()
        {
            // Arrange
            var page = UIPage.Create("TestPage");

            // Act
            var element = page.CreateElement("TextBlock", "test_text");

            // Assert
            element.Should().NotBeNull();
            element.Name.Should().Be("test_text");
            element.Type.Should().Be("TextBlock");
            page.AllElements.Should().Contain(element);
        }

        [Test]
        public void CreateElement_WithParent_ShouldAddToParentChildren()
        {
            // Arrange
            var page = UIPage.Create("TestPage");
            var canvas = page.CreateElement("Canvas", "test_canvas");

            // Act
            var textBlock = page.CreateElement("TextBlock", "test_text", canvas);

            // Assert
            canvas.Children.Should().ContainValue(textBlock);
            textBlock.Parent.Should().Be(canvas);
        }

        [Test]
        public void RemoveElement_ExistingElement_ShouldRemoveElement()
        {
            // Arrange
            var page = UIPage.Create("TestPage");
            var element = page.CreateElement("TextBlock", "to_remove");

            // Act
            page.RemoveElement(element);

            // Assert
            page.AllElements.Should().NotContain(element);
        }

        [Test]
        public void CreateTextBlock_ShouldCreateTextBlockWithDefaults()
        {
            // Arrange
            var page = UIPage.Create("TestPage");

            // Act
            var textBlock = page.CreateTextBlock("title", "Welcome");

            // Assert
            textBlock.Should().NotBeNull();
            textBlock.Type.Should().Be("TextBlock");
            textBlock.Get<string>("Text").Should().Be("Welcome");
            textBlock.Get<string>("HorizontalAlignment").Should().Be("Center");
            textBlock.Get<string>("VerticalAlignment").Should().Be("Center");
        }

        [Test]
        public void CreateButton_ShouldCreateButtonWithTextContent()
        {
            // Arrange
            var page = UIPage.Create("TestPage");

            // Act
            var button = page.CreateButton("start_btn", "Start Game");

            // Assert
            button.Should().NotBeNull();
            button.Type.Should().Be("Button");
            button.Get<float>("Width").Should().Be(200.0f);
            button.Get<float>("Height").Should().Be(50.0f);

            // Should have created a TextBlock for content
            var contentRef = button.Get<string>("Content");
            contentRef.Should().NotBeNull();
            contentRef.Should().Contain("TextBlock");
        }

        [Test]
        public void CreateImage_ShouldCreateImageElementWithDefaults()
        {
            // Arrange
            var page = UIPage.Create("TestPage");

            // Act
            var image = page.CreateImage("logo");

            // Assert
            image.Should().NotBeNull();
            image.Type.Should().Be("ImageElement");
            image.Get<float>("Width").Should().Be(100.0f);
            image.Get<float>("Height").Should().Be(100.0f);
        }

        [Test]
        public void CreateCanvas_ShouldCreateCanvasContainer()
        {
            // Arrange
            var page = UIPage.Create("TestPage");

            // Act
            var canvas = page.CreateCanvas("menu_canvas", width: 640.0f, height: 480.0f);

            // Assert
            canvas.Should().NotBeNull();
            canvas.Type.Should().Be("Canvas");
            canvas.Get<float>("Width").Should().Be(640.0f);
            canvas.Get<float>("Height").Should().Be(480.0f);
        }

        [Test]
        public void SetMargin_ShouldSetMarginProperties()
        {
            // Arrange
            var page = UIPage.Create("TestPage");
            var element = page.CreateElement("TextBlock", "test");

            // Act
            element.SetMargin(left: 100.0f, top: 50.0f);

            // Assert
            element.Properties.Should().ContainKey("Margin");
            var margin = element.Properties["Margin"] as Dictionary<string, object>;
            margin.Should().NotBeNull();
            margin.Should().ContainKey("Left");
            margin.Should().ContainKey("Top");
        }

        [Test]
        public void SetSize_ShouldSetWidthAndHeight()
        {
            // Arrange
            var page = UIPage.Create("TestPage");
            var element = page.CreateElement("ImageElement", "test");

            // Act
            element.SetSize(200.0f, 150.0f);

            // Assert
            element.Get<float>("Width").Should().Be(200.0f);
            element.Get<float>("Height").Should().Be(150.0f);
        }

        [Test]
        public void Create_ComplexUIPage_ThenSave_ShouldPersistAllElements()
        {
            // Arrange
            var tempPath = Path.Combine(Path.GetTempPath(), $"test_complex_page_{Guid.NewGuid()}.sduipage");

            try
            {
                // Act - Create complex UI page
                var page = UIPage.Create("MainMenu", tempPath);

                var rootGrid = page.RootElements.First();

                // Create canvas for menu
                var menuCanvas = page.CreateCanvas("menu_canvas", parent: rootGrid);
                menuCanvas.SetSize(400.0f, 600.0f);

                // Create title text
                var title = page.CreateTextBlock("title", "Main Menu", menuCanvas, fontSize: 40.0f);
                title.SetMargin(top: 50.0f);

                // Create buttons
                var startButton = page.CreateButton("start_btn", "Start Game", menuCanvas, width: 300.0f);
                startButton.SetMargin(left: 50.0f, top: 150.0f);

                var settingsButton = page.CreateButton("settings_btn", "Settings", menuCanvas, width: 300.0f);
                settingsButton.SetMargin(left: 50.0f, top: 220.0f);

                var quitButton = page.CreateButton("quit_btn", "Quit", menuCanvas, width: 300.0f);
                quitButton.SetMargin(left: 50.0f, top: 290.0f);

                // Save
                page.Save();

                // Assert - Reload and verify
                var loaded = UIPage.Load(tempPath);
                loaded.Should().NotBeNull();

                // Should have: 1 Grid + 1 Canvas + 1 Title TextBlock + 3 Buttons + 3 Button TextBlocks = 9 elements
                loaded.AllElements.Should().HaveCountGreaterOrEqualTo(7);

                var loadedCanvas = loaded.FindElementByName("menu_canvas");
                loadedCanvas.Should().NotBeNull();
                loadedCanvas.Type.Should().Be("Canvas");

                var loadedTitle = loaded.FindElementByName("title");
                loadedTitle.Should().NotBeNull();
                loadedTitle.Get<string>("Text").Should().Be("Main Menu");

                var loadedStartButton = loaded.FindElementByName("start_btn");
                loadedStartButton.Should().NotBeNull();
                loadedStartButton.Type.Should().Be("Button");
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        [Test]
        public void StrideProject_CreateUIPage_ShouldCreateAndSaveUIPage()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var tempPath = Path.Combine(Path.GetTempPath(), $"test_project_page_{Guid.NewGuid()}.sduipage");

            try
            {
                // Act - Create UI page through project
                var page = project.CreateUIPage("TestMenu");
                page.SaveAs(tempPath);

                // Assert
                File.Exists(tempPath).Should().BeTrue();

                var loaded = UIPage.Load(tempPath);
                loaded.Should().NotBeNull();
                loaded.RootElements.First().Name.Should().Be("TestMenu");
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }
    }
}
