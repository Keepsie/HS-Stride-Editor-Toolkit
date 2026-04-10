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
            var project = new StrideProject(_testProjectPath);
            var page = project.CreateUIPage("TestPage", "UI/TestPage");

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
            var project = new StrideProject(_testProjectPath);
            var testPageName = $"test_new_page_{Guid.NewGuid()}.sduipage";
            var tempPath = Path.Combine(project.AssetsPath, testPageName);

            try
            {
                // Act - Create and save with full path
                var page = project.CreateUIPage("MyMenu", tempPath);
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
            var project = new StrideProject(_testProjectPath);
            var page = project.CreateUIPage("TestPage", "UI/TestPage");

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
            var project = new StrideProject(_testProjectPath);
            var page = project.CreateUIPage("TestPage", "UI/TestPage");
            var canvas = page.CreateElement("Canvas", "test_canvas");

            // Act
            var textBlock = page.CreateElement("TextBlock", "test_text", canvas);

            // Assert
            canvas.Children.Should().ContainValue(textBlock);
            textBlock.Parent.Should().Be(canvas);
        }

        [Test]
        public void AddChild_ShouldAssignSequentialZIndices()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var page = project.CreateUIPage("TestPage", "UI/TestPage");
            var root = page.RootElements.First();
            var canvas = page.CreateCanvas("z_canvas", parent: root);

            // Act
            var first = page.CreateTextBlock("first", "First", canvas);
            var second = page.CreateTextBlock("second", "Second", canvas);

            // Assert
            first.GetZIndex().Should().Be(0);
            second.GetZIndex().Should().Be(1);
            UIPageManager.GetZIndex(first).Should().Be(0);
            UIPageManager.GetZIndex(second).Should().Be(1);
        }

        [Test]
        public void RemoveChild_ShouldReindexRemainingChildrenZIndices()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var page = project.CreateUIPage("TestPage", "UI/TestPage");
            var root = page.RootElements.First();
            var canvas = page.CreateCanvas("remove_z_canvas", parent: root);
            var first = page.CreateTextBlock("first", "First", canvas);
            var middle = page.CreateTextBlock("middle", "Middle", canvas);
            var last = page.CreateTextBlock("last", "Last", canvas);

            // Act
            var removed = canvas.RemoveChild(middle);

            // Assert
            removed.Should().BeTrue();
            middle.Parent.Should().BeNull();
            first.GetZIndex().Should().Be(0);
            last.GetZIndex().Should().Be(1);
        }

        [Test]
        public void AddChild_ShouldReparentAndReindexOldAndNewParents()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var page = project.CreateUIPage("TestPage", "UI/TestPage");
            var root = page.RootElements.First();
            var source = page.CreateCanvas("source_canvas", parent: root);
            var target = page.CreateCanvas("target_canvas", parent: root);

            var sourceChild1 = page.CreateTextBlock("source_child_1", "A", source);
            var sourceChild2 = page.CreateTextBlock("source_child_2", "B", source);
            var targetChild1 = page.CreateTextBlock("target_child_1", "C", target);

            // Act
            target.AddChild(sourceChild1);

            // Assert
            sourceChild1.Parent.Should().Be(target);
            source.Children.Should().NotContainValue(sourceChild1);
            source.Children.Values.Should().ContainSingle().Which.Should().Be(sourceChild2);

            target.GetChildren().Should().ContainInOrder(targetChild1, sourceChild1);

            sourceChild2.GetZIndex().Should().Be(0);
            targetChild1.GetZIndex().Should().Be(0);
            sourceChild1.GetZIndex().Should().Be(1);
        }

        [Test]
        public void SetZIndex_Extension_ShouldWritePanelZIndexAndClearLegacyAliases()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var page = project.CreateUIPage("TestPage", "UI/TestPage");
            var element = page.CreateElement("TextBlock", "z_test");

            element.Set("Canvas.ZIndex", 9);
            element.Set("Grid.ZIndex", 7);

            // Act
            UIPageManager.SetZIndex(element, 4);

            // Assert
            element.GetZIndex().Should().Be(4);
            UIPageManager.GetZIndex(element).Should().Be(4);
            element.Properties.ContainsKey("Canvas.ZIndex").Should().BeFalse();
            element.Properties.ContainsKey("Grid.ZIndex").Should().BeFalse();
        }

        [Test]
        public void GetZIndex_Extension_ShouldFallbackToLegacyCanvasZIndex_WhenPanelZIndexMissing()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var page = project.CreateUIPage("TestPage", "UI/TestPage");
            var element = page.CreateElement("TextBlock", "legacy_z");
            element.SetZIndex(0); // Ensure Panel.ZIndex is absent
            element.Set("Canvas.ZIndex", 6);

            // Act
            var zIndex = UIPageManager.GetZIndex(element);

            // Assert
            zIndex.Should().Be(6);
        }

        [Test]
        public void Load_ShouldNormalizeHierarchyZIndices_FromSavedOutOfOrderValues()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var testPageName = $"test_zindex_normalize_{Guid.NewGuid()}.sduipage";
            var tempPath = Path.Combine(project.AssetsPath, testPageName);

            try
            {
                var page = project.CreateUIPage("ZIndexNormalize", tempPath);
                var root = page.RootElements.First();
                var canvas = page.CreateCanvas("normalize_canvas", parent: root);
                var first = page.CreateTextBlock("normalize_first", "First", canvas);
                var second = page.CreateTextBlock("normalize_second", "Second", canvas);

                // Force out-of-order values prior to save
                root.SetZIndex(99);
                canvas.SetZIndex(42);
                first.SetZIndex(17);
                second.SetZIndex(3);

                page.Save();

                // Act
                var loaded = UIPage.Load(tempPath);
                var loadedRoot = loaded.RootElements.First();
                var loadedCanvas = loaded.FindElementByName("normalize_canvas");

                // Assert
                loadedCanvas.Should().NotBeNull();
                loadedRoot.GetZIndex().Should().Be(0);

                var loadedChildren = loadedCanvas!.GetChildren();
                loadedChildren.Should().HaveCount(2);
                loadedChildren[0].Name.Should().Be("normalize_first");
                loadedChildren[1].Name.Should().Be("normalize_second");
                loadedChildren[0].GetZIndex().Should().Be(0);
                loadedChildren[1].GetZIndex().Should().Be(1);
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        [Test]
        public void RemoveElement_ExistingElement_ShouldRemoveElement()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var page = project.CreateUIPage("TestPage", "UI/TestPage");
            var element = page.CreateElement("TextBlock", "to_remove");

            // Act
            page.RemoveElement(element);

            // Assert
            page.AllElements.Should().NotContain(element);
        }


        [Test]
        public void CreateButton_ShouldCreateButtonWithTextContent()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var page = project.CreateUIPage("TestPage", "UI/TestPage");

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
            var project = new StrideProject(_testProjectPath);
            var page = project.CreateUIPage("TestPage", "UI/TestPage");

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
            var project = new StrideProject(_testProjectPath);
            var page = project.CreateUIPage("TestPage", "UI/TestPage");

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
            var project = new StrideProject(_testProjectPath);
            var page = project.CreateUIPage("TestPage", "UI/TestPage");
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
            var project = new StrideProject(_testProjectPath);
            var page = project.CreateUIPage("TestPage", "UI/TestPage");
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
            var project = new StrideProject(_testProjectPath);
            var testPageName = $"test_complex_page_{Guid.NewGuid()}.sduipage";
            var tempPath = Path.Combine(project.AssetsPath, testPageName);

            try
            {
                // Act - Create complex UI page with full path
                var page = project.CreateUIPage("MainMenu", tempPath);

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
            var testPageName = $"test_project_page_{Guid.NewGuid()}.sduipage";
            var tempPath = Path.Combine(project.AssetsPath, testPageName);

            try
            {
                // Act - Create UI page through project
                var page = project.CreateUIPage("TestMenu");
                page.SaveAs(testPageName);

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

        [Test]
        public void GetDesignResolution_ShouldReturnResolution()
        {
            // Arrange
            var page = UIPage.Load(_testUIPagePath);

            // Act
            var resolution = page.GetDesignResolution();

            // Assert
            resolution.Should().NotBeNull();
            resolution.Value.X.Should().BeGreaterThan(0);
            resolution.Value.Y.Should().BeGreaterThan(0);
            resolution.Value.Z.Should().BeGreaterThan(0);
        }

        [Test]
        public void SetDesignResolution_ShouldSetResolution()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var page = project.CreateUIPage("TestPage");

            // Act
            page.SetDesignResolution(1920f, 1080f, 1500f);

            // Assert
            var resolution = page.GetDesignResolution();
            resolution.Should().NotBeNull();
            resolution.Value.X.Should().Be(1920f);
            resolution.Value.Y.Should().Be(1080f);
            resolution.Value.Z.Should().Be(1500f);
        }

        [Test]
        public void CreateEditText_ShouldCreateEditTextElement()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var page = project.CreateUIPage("TestPage");

            // Act
            var editText = page.CreateEditText("username_input", placeholder: "Enter name", width: 300f);

            // Assert
            editText.Should().NotBeNull();
            editText.Type.Should().Be("EditText");
            editText.Get<float>("Width").Should().Be(300f);
        }

        [Test]
        public void CreateSlider_ShouldCreateSliderElement()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var page = project.CreateUIPage("TestPage");

            // Act
            var slider = page.CreateSlider("volume_slider", min: 0f, max: 100f, value: 75f);

            // Assert
            slider.Should().NotBeNull();
            slider.Type.Should().Be("Slider");
            slider.Get<float>("Minimum").Should().Be(0f);
            slider.Get<float>("Maximum").Should().Be(100f);
            slider.Get<float>("Value").Should().Be(75f);
        }

        [Test]
        public void CreateToggleButton_ShouldCreateToggleElement()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var page = project.CreateUIPage("TestPage");

            // Act
            var toggle = page.CreateToggleButton("vsync_toggle", text: "VSync", isChecked: true);

            // Assert
            toggle.Should().NotBeNull();
            toggle.Type.Should().Be("ToggleButton");
            toggle.Get<string>("State").Should().Be("Checked");
        }

        [Test]
        public void CreateScrollBar_ShouldCreateScrollBarElement()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var page = project.CreateUIPage("TestPage");

            // Act
            var scrollBar = page.CreateScrollBar("custom_scroll", isVertical: true);

            // Assert
            scrollBar.Should().NotBeNull();
            scrollBar.Type.Should().Be("ScrollBar");
        }

        // ===== Margin/Position Parsing Tests =====

        [Test]
        public void GetPosition_FromLoadedPage_ShouldReturnCorrectValues()
        {
            // Arrange - Load a real page with elements that have Margin set
            var page = UIPage.Load(_testUIPagePath);

            // Find an element with a margin (most elements in dev_console_page have margins)
            var elementWithMargin = page.AllElements.FirstOrDefault(e =>
                e.Properties.ContainsKey("Margin") &&
                e.Properties["Margin"] is Dictionary<string, object> margin &&
                margin.ContainsKey("Left"));

            // Skip if no suitable element found
            if (elementWithMargin == null)
            {
                Assert.Inconclusive("No element with Left margin found in test file");
                return;
            }

            // Act
            var (x, y) = elementWithMargin.GetPosition();

            // Assert - Position should be readable (not 0,0 if margin was set)
            // We just verify it doesn't throw and returns the parsed values
            x.Should().BeGreaterThanOrEqualTo(0);
            y.Should().BeGreaterThanOrEqualTo(0);
        }

        [Test]
        public void GetMargin_FromLoadedPage_ShouldParseInlineFormat()
        {
            // Arrange - Load page with all UI elements
            var allElementsPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Example Assets", "all-ui-elements.sduipage");
            var page = UIPage.Load(allElementsPath);

            // Find ToggleButton which has Margin: {Top: 232.0, Right: 58.0}
            var toggleButton = page.AllElements.FirstOrDefault(e => e.Type == "ToggleButton");
            toggleButton.Should().NotBeNull("all-ui-elements.sduipage should contain a ToggleButton");

            // Act
            var margin = toggleButton!.GetMargin();

            // Assert - Should have parsed the inline {Top: 232.0, Right: 58.0} format
            margin.Top.Should().Be(232.0f);
            margin.Right.Should().Be(58.0f);
        }

        [Test]
        public void SetPosition_ThenGetPosition_ShouldRoundTrip()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var page = project.CreateUIPage("TestPage");
            var element = page.CreateElement("TextBlock", "test");

            // Act
            element.SetPosition(150.5f, 200.0f);
            var (x, y) = element.GetPosition();

            // Assert
            x.Should().Be(150.5f);
            y.Should().Be(200.0f);
        }

        [Test]
        public void GetMargin_AllFourValues_ShouldReturnCorrectly()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var page = project.CreateUIPage("TestPage");
            var element = page.CreateElement("TextBlock", "test");

            // Act
            element.SetMargin(left: 10f, top: 20f, right: 30f, bottom: 40f);
            var margin = element.GetMargin();

            // Assert
            margin.Left.Should().Be(10f);
            margin.Top.Should().Be(20f);
            margin.Right.Should().Be(30f);
            margin.Bottom.Should().Be(40f);
        }

        // ===== Sprite/Texture Detection Tests =====

        [Test]
        public void IsSpriteFromSheet_WithSpriteSheet_ShouldReturnTrue()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var page = project.CreateUIPage("TestPage");
            var image = page.CreateImage("test_image");

            // Act - Set a sprite sheet source
            image.Set("Source", new Dictionary<string, object>
            {
                ["!SpriteFromSheet"] = "",
                ["Sheet"] = "some-guid:SomeSheet",
                ["CurrentFrame"] = 0
            });

            // Assert
            image.IsSpriteFromSheet("Source").Should().BeTrue();
            image.IsSpriteFromTexture("Source").Should().BeFalse();
            image.IsImageSpriteSheet().Should().BeTrue();
            image.IsImageTexture().Should().BeFalse();
        }

        [Test]
        public void IsSpriteFromTexture_WithTexture_ShouldReturnTrue()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var page = project.CreateUIPage("TestPage");
            var image = page.CreateImage("test_image");

            // Act - Set a texture source
            image.Set("Source", new Dictionary<string, object>
            {
                ["!SpriteFromTexture"] = "",
                ["Texture"] = "some-guid:SomeTexture"
            });

            // Assert
            image.IsSpriteFromTexture("Source").Should().BeTrue();
            image.IsSpriteFromSheet("Source").Should().BeFalse();
            image.IsImageTexture().Should().BeTrue();
            image.IsImageSpriteSheet().Should().BeFalse();
        }

        [Test]
        public void GetSpriteSheet_ShouldReturnAssetAndFrame()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var page = project.CreateUIPage("TestPage");
            var image = page.CreateImage("test_image");

            image.Set("Source", new Dictionary<string, object>
            {
                ["!SpriteFromSheet"] = "",
                ["Sheet"] = "abc123:MySheet",
                ["CurrentFrame"] = 5
            });

            // Act
            var result = image.GetSpriteSheet("Source");

            // Assert
            result.Should().NotBeNull();
            result!.Value.AssetReference.Should().Be("abc123:MySheet");
            result.Value.Frame.Should().Be(5);
        }

        [Test]
        public void GetTextureSource_ShouldReturnTextureReference()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var page = project.CreateUIPage("TestPage");
            var image = page.CreateImage("test_image");

            image.Set("Source", new Dictionary<string, object>
            {
                ["!SpriteFromTexture"] = "",
                ["Texture"] = "xyz789:MyTexture"
            });

            // Act
            var result = image.GetTextureSource("Source");

            // Assert
            result.Should().Be("xyz789:MyTexture");
        }

        [Test]
        public void HasSpriteSource_WithValidAsset_ShouldReturnTrue()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var page = project.CreateUIPage("TestPage");
            var image = page.CreateImage("test_image");

            image.Set("Source", new Dictionary<string, object>
            {
                ["!SpriteFromSheet"] = "",
                ["Sheet"] = "valid-guid:ValidSheet",
                ["CurrentFrame"] = 0
            });

            // Assert
            image.HasSpriteSource("Source").Should().BeTrue();
        }

        [Test]
        public void HasSpriteSource_WithNullSheet_ShouldReturnFalse()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var page = project.CreateUIPage("TestPage");
            var image = page.CreateImage("test_image");

            image.Set("Source", new Dictionary<string, object>
            {
                ["!SpriteFromSheet"] = "",
                ["Sheet"] = "null",
                ["CurrentFrame"] = 0
            });

            // Assert
            image.HasSpriteSource("Source").Should().BeFalse();
        }

        [Test]
        public void Button_SpriteDetection_ShouldWorkForAllImages()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var page = project.CreateUIPage("TestPage");
            var button = page.CreateButton("test_btn", "Test");

            // Set button images using sprite sheet
            button.Set("PressedImage", new Dictionary<string, object>
            {
                ["!SpriteFromSheet"] = "",
                ["Sheet"] = "btn-sheet:Buttons",
                ["CurrentFrame"] = 1
            });

            // Assert
            button.IsButtonUsingSpriteSheet().Should().BeTrue();
            button.IsButtonUsingTexture().Should().BeFalse();

            var pressedSprite = button.GetPressedImageSprite();
            pressedSprite.Should().NotBeNull();
            pressedSprite!.Value.AssetReference.Should().Be("btn-sheet:Buttons");
            pressedSprite.Value.Frame.Should().Be(1);
        }

        [Test]
        public void LoadedPage_SpriteSheetReferences_ShouldBeReadable()
        {
            // Arrange - Load the all-ui-elements page which has sprites
            var allElementsPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Example Assets", "all-ui-elements.sduipage");
            var page = UIPage.Load(allElementsPath);

            // Find the button
            var button = page.AllElements.FirstOrDefault(e => e.Type == "Button");
            button.Should().NotBeNull();

            // Act
            var isSheet = button!.IsButtonUsingSpriteSheet();
            var pressedSprite = button.GetPressedImageSprite();

            // Assert
            isSheet.Should().BeTrue();
            pressedSprite.Should().NotBeNull();
            pressedSprite!.Value.AssetReference.Should().Contain("StrideUIDesigns");
        }

        // ===== SetContent Tests =====

        [Test]
        public void SetContent_ShouldSetReferenceFormat()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var page = project.CreateUIPage("TestPage");
            var button = page.CreateElement("Button", "test_btn");
            var textBlock = page.CreateTextBlock("btn_text", "Click Me", autoAttach: false);

            // Act
            button.SetContent(textBlock);

            // Assert
            var content = button.Get<string>("Content");
            content.Should().NotBeNull();
            content.Should().Contain("!TextBlock");
            content.Should().Contain("ref!!");
            content.Should().Contain(textBlock.Id);
        }

        [Test]
        public void GetContent_ShouldReturnReferencedElement()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var page = project.CreateUIPage("TestPage");
            var button = page.CreateElement("Button", "test_btn");
            var textBlock = page.CreateTextBlock("btn_text", "Click Me", autoAttach: false);
            button.SetContent(textBlock);

            // Act
            var content = button.GetContent();

            // Assert
            content.Should().NotBeNull();
            content.Should().Be(textBlock);
        }

        [Test]
        public void SetContent_OnNonContentElement_ShouldThrow()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var page = project.CreateUIPage("TestPage");
            var textBlock = page.CreateTextBlock("test", "Hello");
            var anotherText = page.CreateTextBlock("other", "World", autoAttach: false);

            // Act
            Action act = () => textBlock.SetContent(anotherText);

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }

        // ===== New Element Type Tests =====

        [Test]
        public void CreateModalElement_ShouldCreateModalElement()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var page = project.CreateUIPage("TestPage");

            // Act
            var modal = page.CreateModalElement("dialog", width: 400f, height: 300f);

            // Assert
            modal.Should().NotBeNull();
            modal.Type.Should().Be("ModalElement");
            modal.Get<float>("Width").Should().Be(400f);
            modal.Get<float>("Height").Should().Be(300f);
        }

        [Test]
        public void CreateBorder_ShouldCreateBorderElement()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var page = project.CreateUIPage("TestPage");

            // Act
            var border = page.CreateBorder("frame", width: 200f, height: 100f);

            // Assert
            border.Should().NotBeNull();
            border.Type.Should().Be("Border");
        }

        [Test]
        public void CreateScrollingText_ShouldCreateScrollingTextElement()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var page = project.CreateUIPage("TestPage");

            // Act
            var scrollingText = page.CreateScrollingText("ticker", "Breaking News!");

            // Assert
            scrollingText.Should().NotBeNull();
            scrollingText.Type.Should().Be("ScrollingText");
            scrollingText.Get<string>("Text").Should().Be("Breaking News!");
        }

        [Test]
        public void CreateUniformGrid_ShouldCreateUniformGridElement()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var page = project.CreateUIPage("TestPage");

            // Act
            var grid = page.CreateUniformGrid("inventory_grid");

            // Assert
            grid.Should().NotBeNull();
            grid.Type.Should().Be("UniformGrid");
        }

        // ===== Color Property Tests =====

        [Test]
        public void GetBackgroundColor_ShouldReturnParsedColor()
        {
            // Arrange - Load page with elements that have background colors
            var allElementsPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Example Assets", "all-ui-elements.sduipage");
            var page = UIPage.Load(allElementsPath);

            var element = page.AllElements.First();

            // Act
            var color = element.GetBackgroundColor();

            // Assert - Should return parsed values (default is 0,0,0,0 for transparent)
            color.R.Should().BeGreaterThanOrEqualTo(0);
            color.G.Should().BeGreaterThanOrEqualTo(0);
            color.B.Should().BeGreaterThanOrEqualTo(0);
            color.A.Should().BeGreaterThanOrEqualTo(0);
        }

        [Test]
        public void SetBackgroundColor_ThenGet_ShouldRoundTrip()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var page = project.CreateUIPage("TestPage");
            var element = page.CreateElement("Canvas", "test");

            // Act
            element.SetBackgroundColor(100, 150, 200, 255);
            var color = element.GetBackgroundColor();

            // Assert
            color.R.Should().Be(100);
            color.G.Should().Be(150);
            color.B.Should().Be(200);
            color.A.Should().Be(255);
        }

        // ===== Behavior Property Tests =====

        [Test]
        public void GetSetIsEnabled_ShouldRoundTrip()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var page = project.CreateUIPage("TestPage");
            var button = page.CreateButton("test_btn", "Test");

            // Act
            button.SetIsEnabled(false);
            var isEnabled = button.GetIsEnabled();

            // Assert
            isEnabled.Should().BeFalse();
        }

        [Test]
        public void GetSetClickMode_ShouldRoundTrip()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var page = project.CreateUIPage("TestPage");
            var button = page.CreateButton("test_btn", "Test");

            // Act
            button.SetClickMode("Press");
            var clickMode = button.GetClickMode();

            // Assert
            clickMode.Should().Be("Press");
        }

        [Test]
        public void GetSetWrapText_ShouldRoundTrip()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var page = project.CreateUIPage("TestPage");
            var textBlock = page.CreateTextBlock("test", "Hello World");

            // Act
            textBlock.SetWrapText(true);
            var wrapText = textBlock.GetWrapText();

            // Assert
            wrapText.Should().BeTrue();
        }

        // ===== Save/Load Round-Trip with New Properties =====

        [Test]
        public void SaveLoad_WithMarginAndProperties_ShouldPreserveData()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var testPageName = $"test_roundtrip_{Guid.NewGuid()}.sduipage";
            var tempPath = Path.Combine(project.AssetsPath, testPageName);

            try
            {
                // Create page with various properties
                var page = project.CreateUIPage("RoundTripTest", tempPath);
                var root = page.RootElements.First();

                var button = page.CreateButton("test_btn", "Click", root, width: 200f, height: 50f);
                button.SetPosition(100f, 200f);
                button.SetBackgroundColor(60, 60, 80, 255);

                var textBlock = page.CreateTextBlock("test_text", "Hello", root);
                textBlock.SetMargin(left: 50f, top: 75f, right: 10f, bottom: 5f);
                textBlock.SetWrapText(true);

                // Save
                page.Save();

                // Load
                var loaded = UIPage.Load(tempPath);

                // Assert button
                var loadedButton = loaded.FindElementByName("test_btn");
                loadedButton.Should().NotBeNull();

                var btnPos = loadedButton!.GetPosition();
                btnPos.X.Should().Be(100f);
                btnPos.Y.Should().Be(200f);

                // Assert textblock
                var loadedText = loaded.FindElementByName("test_text");
                loadedText.Should().NotBeNull();

                var textMargin = loadedText!.GetMargin();
                textMargin.Left.Should().Be(50f);
                textMargin.Top.Should().Be(75f);
                textMargin.Right.Should().Be(10f);
                textMargin.Bottom.Should().Be(5f);
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }
    }
}
