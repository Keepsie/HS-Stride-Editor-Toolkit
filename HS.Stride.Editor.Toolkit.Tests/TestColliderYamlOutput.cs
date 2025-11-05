using HS.Stride.Editor.Toolkit.Core;
using HS.Stride.Editor.Toolkit.Core.SceneEditing;
using HS.Stride.Editor.Toolkit.Core.AssetEditing;
using NUnit.Framework;

namespace HS.Stride.Editor.Toolkit.Tests
{
    /// <summary>
    /// Quick test to output YAML and verify ColliderShapes formatting
    /// </summary>
    [TestFixture]
    public class TestColliderYamlOutput
    {
        [Test]
        public void OutputPrefabYaml()
        {
            var testProjectPath = @"E:\Github\HS-Stride-Editor-Toolkit\HS.Stride.Editor.Toolkit.Tests\Example Scenes\TestProject\";
            var project = new StrideProject(testProjectPath);
            var scene = project.LoadScene("Testing");
            var prefabAsset = project.FindAsset("LootBox", AssetType.Prefab);
            
            // Instantiate a prefab with collider shapes
            var newInstance = scene.InstantiatePrefab(prefabAsset!);

            // Get the YAML
            var testSceneName = "test_prefab_output.sdscene";
            var tempPath = Path.Combine(project.AssetsPath, testSceneName);
            scene.SaveAs(testSceneName);
            var yaml = File.ReadAllText(tempPath);
            
            // Output to console so we can see it
            TestContext.WriteLine("========== SCENE YAML WITH PREFAB ==========");
            TestContext.WriteLine(yaml);
            TestContext.WriteLine("===========================================");
            
            TestContext.WriteLine($"\nYAML written to: {tempPath}");
            
            // Check for the bad format
            if (yaml.Contains("ColliderShapes: {"))
            {
                TestContext.WriteLine("\n❌ ERROR: Found bad inline ColliderShapes format!");
            }
            else
            {
                TestContext.WriteLine("\n✓ Good: No inline ColliderShapes format found");
            }
            
            // Check for proper nested format
            if (yaml.Contains("ColliderShapes:") && yaml.Contains("!BoxColliderShapeDesc"))
            {
                TestContext.WriteLine("✓ Good: Proper nested ColliderShapes format found");
            }
            else
            {
                TestContext.WriteLine("❌ ERROR: Missing proper ColliderShapes format");
            }
        }
    }
}
