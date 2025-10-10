// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using NUnit.Framework;
using HS.Stride.Editor.Toolkit.Core.UIPageEditing;

namespace HS.Stride.Editor.Toolkit.Tests
{
    [TestFixture]
    public class UIPageParserDebugTest
    {
        [Test]
        public void Debug_ParseSimpleUIPage()
        {
            // Create a minimal UI page YAML
            var yaml = @"!UIPageAsset
Id: test-id-123
SerializedVersion: {Stride: 2.1.0.1}
Tags: []
Design:
    Resolution: {X: 1280.0, Y: 720.0, Z: 1000.0}
Hierarchy:
    RootParts:
        - !Grid ref!! root-grid-id
    Parts:
        -   UIElement: !Grid
                Id: root-grid-id
                DependencyProperties: {}
                BackgroundColor: {R: 0, G: 0, B: 0, A: 0}
                Margin: {}
                MaximumWidth: 3.4028235E+38
                MaximumHeight: 3.4028235E+38
                MaximumDepth: 3.4028235E+38
                Name: TestGrid
                Children: {}
                RowDefinitions: {}
                ColumnDefinitions: {}
                LayerDefinitions: {}
        -   UIElement: !TextBlock
                Id: text-block-id
                DependencyProperties: {}
                BackgroundColor: {R: 0, G: 0, B: 0, A: 0}
                HorizontalAlignment: Center
                VerticalAlignment: Center
                Margin: {}
                MaximumWidth: 3.4028235E+38
                MaximumHeight: 3.4028235E+38
                MaximumDepth: 3.4028235E+38
                Name: TestText
                Text: Hello World
                Font: c90f3988-0544-4cbe-993f-13af7d9c23c6:StrideDefaultFont
                TextSize: 20.0
                TextColor: {R: 240, G: 240, B: 240, A: 255}
                OutlineColor: {R: 0, G: 0, B: 0, A: 255}
                OutlineThickness: 0.0
";

            var tempPath = Path.Combine(Path.GetTempPath(), "test_debug.sduipage");
            File.WriteAllText(tempPath, yaml);

            try
            {
                // Parse it
                var page = UIPage.Load(tempPath);

                // Debug output
                TestContext.WriteLine($"Page ID: {page.Id}");
                TestContext.WriteLine($"Elements Count: {page.AllElements.Count}");

                foreach (var element in page.AllElements)
                {
                    TestContext.WriteLine($"  Element: {element.Type} - {element.Name} ({element.Id})");
                }

                TestContext.WriteLine($"Root Elements Count: {page.RootElements.Count}");

                // Assertions
                Assert.That(page.AllElements.Count, Is.EqualTo(2), "Should have 2 elements");
                Assert.That(page.RootElements.Count, Is.EqualTo(1), "Should have 1 root element");
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }
    }
}
