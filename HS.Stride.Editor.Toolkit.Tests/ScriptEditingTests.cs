// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using NUnit.Framework;
using FluentAssertions;
using HS.Stride.Editor.Toolkit.Core;
using HS.Stride.Editor.Toolkit.Core.ScriptEditing;
using HS.Stride.Editor.Toolkit.Core.SceneEditing;

namespace HS.Stride.Editor.Toolkit.Tests
{
    [TestFixture]
    public class ScriptEditingTests
    {
        private string _testProjectPath;

        [SetUp]
        public void Setup()
        {
            // Use the TestProject directory inside Example Scenes (proper Stride project structure)
            _testProjectPath = @"E:\Github\HS-Stride-Editor-Toolkit\HS.Stride.Editor.Toolkit.Tests\Example Scenes\TestProject\";
        }

        [Test]
        public void ScriptScanner_FindSimpleScript_ShouldExtractNamespaceAndProperties()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);

            // Act
            var scriptInfo = ScriptScanner.FindScript(project, "SimpleScript");

            // Assert
            scriptInfo.Should().NotBeNull("SimpleScript.cs should be found in Example Scenes");
            scriptInfo.Namespace.Should().Be("TestNamespace", "Namespace should be extracted from the file");
            scriptInfo.ClassName.Should().Be("SimpleScript");
            scriptInfo.AssemblyName.Should().NotBeEmpty("Assembly name should be extracted from .csproj");

            // Check public members were extracted
            scriptInfo.PublicMembers.Should().ContainKey("health");
            scriptInfo.PublicMembers.Should().ContainKey("speed");
            scriptInfo.PublicMembers.Should().ContainKey("isActive");
            scriptInfo.PublicMembers.Should().ContainKey("playerName");

            scriptInfo.PublicMembers["health"].Should().Be("int");
            scriptInfo.PublicMembers["speed"].Should().Be("float");
            scriptInfo.PublicMembers["isActive"].Should().Be("bool");
            scriptInfo.PublicMembers["playerName"].Should().Be("string");
        }

        [Test]
        public void ScriptScanner_FindNoNameSpaceScript_ShouldHandleMissingNamespace()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);

            // Act
            var scriptInfo = ScriptScanner.FindScript(project, "NoNameSpace");

            // Assert
            scriptInfo.Should().NotBeNull("NoNameSpace.cs should be found");
            scriptInfo.Namespace.Should().BeEmpty("Script has no namespace declaration");
            scriptInfo.ClassName.Should().Be("NoNameSpace");

            // Check it extracted various property types
            scriptInfo.PublicMembers.Should().ContainKey("intValue");
            scriptInfo.PublicMembers.Should().ContainKey("floatValue");
            scriptInfo.PublicMembers.Should().ContainKey("singleEntity");
            scriptInfo.PublicMembers.Should().ContainKey("prefabRef");
        }

        [Test]
        public void ScriptScanner_NoNameSpace_ShouldExtractAllMemberTypes()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);

            // Act
            var scriptInfo = ScriptScanner.FindScript(project, "NoNameSpace");

            // Assert
            scriptInfo.Should().NotBeNull();

            // Primitives
            scriptInfo.PublicMembers.Should().ContainKey("intValue");
            scriptInfo.PublicMembers["intValue"].Should().Be("int");

            scriptInfo.PublicMembers.Should().ContainKey("floatValue");
            scriptInfo.PublicMembers["floatValue"].Should().Be("float");

            scriptInfo.PublicMembers.Should().ContainKey("boolValue");
            scriptInfo.PublicMembers["boolValue"].Should().Be("bool");

            scriptInfo.PublicMembers.Should().ContainKey("stringValue");
            scriptInfo.PublicMembers["stringValue"].Should().Be("string");

            // Entity references
            scriptInfo.PublicMembers.Should().ContainKey("singleEntity");
            scriptInfo.PublicMembers["singleEntity"].Should().Be("Entity");

            scriptInfo.PublicMembers.Should().ContainKey("entityList");
            scriptInfo.PublicMembers["entityList"].Should().Contain("List");
            scriptInfo.PublicMembers["entityList"].Should().Contain("Entity");

            // Asset references
            scriptInfo.PublicMembers.Should().ContainKey("prefabRef");
            scriptInfo.PublicMembers["prefabRef"].Should().Be("Prefab");

            scriptInfo.PublicMembers.Should().ContainKey("prefabList");
            scriptInfo.PublicMembers["prefabList"].Should().Contain("List");
            scriptInfo.PublicMembers["prefabList"].Should().Contain("Prefab");

            scriptInfo.PublicMembers.Should().ContainKey("modelRef");
            scriptInfo.PublicMembers["modelRef"].Should().Be("Model");

            scriptInfo.PublicMembers.Should().ContainKey("materialRef");
            scriptInfo.PublicMembers["materialRef"].Should().Be("Material");

            scriptInfo.PublicMembers.Should().ContainKey("RawAsset");
            scriptInfo.PublicMembers["RawAsset"].Should().Be("UrlReference");

            // Arrays
            scriptInfo.PublicMembers.Should().ContainKey("intArray");
            scriptInfo.PublicMembers["intArray"].Should().Contain("int");
            scriptInfo.PublicMembers["intArray"].Should().Contain("[]");

            scriptInfo.PublicMembers.Should().ContainKey("entityArray");
            scriptInfo.PublicMembers["entityArray"].Should().Contain("Entity");
            scriptInfo.PublicMembers["entityArray"].Should().Contain("[]");

            // Lists
            scriptInfo.PublicMembers.Should().ContainKey("Stuff2");
            scriptInfo.PublicMembers["Stuff2"].Should().Contain("List");
            scriptInfo.PublicMembers["Stuff2"].Should().Contain("int");

            // Dictionaries
            scriptInfo.PublicMembers.Should().ContainKey("AnimationClips");
            scriptInfo.PublicMembers["AnimationClips"].Should().Contain("Dictionary");
            scriptInfo.PublicMembers["AnimationClips"].Should().Contain("string");
            scriptInfo.PublicMembers["AnimationClips"].Should().Contain("AnimationClip");

            scriptInfo.PublicMembers.Should().ContainKey("primClips");
            scriptInfo.PublicMembers["primClips"].Should().Contain("Dictionary");
            scriptInfo.PublicMembers["primClips"].Should().Contain("int");
            scriptInfo.PublicMembers["primClips"].Should().Contain("string");

            // Component reference
            scriptInfo.PublicMembers.Should().ContainKey("transformRef");
            scriptInfo.PublicMembers["transformRef"].Should().Be("TransformComponent");

            TestContext.WriteLine($"Total members extracted: {scriptInfo.PublicMembers.Count}");
            foreach (var member in scriptInfo.PublicMembers)
            {
                TestContext.WriteLine($"  {member.Key}: {member.Value}");
            }
        }

        [Test]
        public void ScriptInfo_GetFullTypeName_WithNamespace_ShouldFormatCorrectly()
        {
            // Arrange
            var scriptInfo = new ScriptInfo
            {
                Namespace = "TestNamespace",
                ClassName = "SimpleScript",
                AssemblyName = "TestAssembly.Game"
            };

            // Act
            var fullTypeName = scriptInfo.GetFullTypeName();

            // Assert
            fullTypeName.Should().Be("TestNamespace.SimpleScript,TestAssembly.Game");
        }

        [Test]
        public void ScriptInfo_GetFullTypeName_WithoutNamespace_ShouldIncludeLeadingDot()
        {
            // Arrange
            var scriptInfo = new ScriptInfo
            {
                Namespace = "",
                ClassName = "NoNameSpace",
                AssemblyName = "TestAssembly.Game"
            };

            // Act
            var fullTypeName = scriptInfo.GetFullTypeName();

            // Assert
            fullTypeName.Should().Be(".NoNameSpace,TestAssembly.Game");
        }

        [Test]
        public void ScriptToComponent_Create_ShouldInitializePropertiesWithDefaults()
        {
            // Arrange
            var scriptInfo = new ScriptInfo
            {
                Namespace = "TestNamespace",
                ClassName = "SimpleScript",
                AssemblyName = "TestAssembly.Game",
                PublicMembers = new Dictionary<string, string>
                {
                    { "health", "int" },
                    { "speed", "float" },
                    { "isActive", "bool" },
                    { "playerName", "string" },
                    { "targetEntity", "Entity" },
                    { "weaponPrefab", "Prefab" }
                }
            };

            // Act
            var component = ScriptToComponent.Create(scriptInfo);

            // Assert
            component.Should().NotBeNull();
            component.Type.Should().Be("TestNamespace.SimpleScript,TestAssembly.Game");
            component.Key.Should().NotBeEmpty();
            component.Id.Should().NotBeEmpty();

            // Check property initialization
            component.Properties["health"].Should().Be(0);
            component.Properties["speed"].Should().Be(0.0f);
            component.Properties["isActive"].Should().Be(false);
            component.Properties["playerName"].Should().Be("null");
            component.Properties["targetEntity"].Should().Be("null");
            component.Properties["weaponPrefab"].Should().Be("null");
        }

        [Test]
        public void Entity_AddComponent_WithScriptScanning_ShouldCreateFullComponent()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var scene = project.LoadScene("Testing");
            var entity = scene.FindEntityByName("Box1x1x1");

            // Act
            var component = entity.AddComponent("SimpleScript");

            // Assert
            component.Should().NotBeNull();
            component.Type.Should().Contain("TestNamespace.SimpleScript", "Should have full namespace");
            component.Type.Should().Contain(",", "Should have assembly name");

            // Verify properties were initialized
            component.Properties.Should().ContainKey("health");
            component.Properties.Should().ContainKey("speed");
            component.Properties.Should().ContainKey("isActive");
            component.Properties.Should().ContainKey("playerName");
        }

        [Test]
        public void Entity_AddComponent_NoNameSpace_ShouldUseLeadingDot()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var scene = project.LoadScene("Testing");
            var entity = scene.FindEntityByName("Box1x1x1");

            // Act
            var component = entity.AddComponent("NoNameSpace");

            // Assert
            component.Should().NotBeNull();
            component.Type.Should().StartWith(".", "Scripts without namespace should start with dot");
            component.Type.Should().Contain("NoNameSpace");
        }

        [Test]
        public void Entity_AddComponent_AndSave_ShouldWriteValidYAML()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var scene = project.LoadScene("Testing");
            var entity = scene.FindEntityByName("Box1x1x1");
            var tempPath = Path.Combine(Path.GetTempPath(), $"test_script_add_{Guid.NewGuid()}.sdscene");

            try
            {
                // Act
                var component = entity.AddComponent("SimpleScript");
                component.Set("health", 100);
                component.Set("speed", 5.5f);
                component.Set("isActive", true);
                component.Set("playerName", "TestPlayer");

                scene.SaveAs(tempPath);

                // Assert - Verify YAML contains all properties
                var yaml = File.ReadAllText(tempPath);
                yaml.Should().Contain("TestNamespace.SimpleScript", "Full namespace should be in YAML");

                // Properties we set
                yaml.Should().Contain("health: 100");
                yaml.Should().Contain("speed: 5.5");
                yaml.Should().Contain("isActive: true");
                yaml.Should().Contain("playerName: TestPlayer");

                // Verify it can be reloaded
                var reloadedScene = Scene.Load(tempPath);
                var reloadedEntity = reloadedScene.FindEntityByName("Box1x1x1");
                var reloadedComponent = reloadedEntity.GetComponent("SimpleScript");
                reloadedComponent.Should().NotBeNull();

                // Verify all properties persist correctly
                reloadedComponent.Get<int>("health").Should().Be(100);
                reloadedComponent.Get<float>("speed").Should().Be(5.5f);
                reloadedComponent.Get<bool>("isActive").Should().Be(true);
                reloadedComponent.Get<string>("playerName").Should().Be("TestPlayer");
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        [Test]
        public void Entity_AddComponent_GetSetEveryProperty_SimpleScript()
        {
            // Verify Get/Set works for EVERY property individually
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var scene = project.LoadScene("Testing");
            var entity = scene.FindEntityByName("Box1x1x1");

            // Act
            var component = entity.AddComponent("SimpleScript");

            // Test each property individually
            // int property
            component.Set("health", 150);
            component.Get<int>("health").Should().Be(150, "int Get/Set should work");

            // float property
            component.Set("speed", 7.5f);
            component.Get<float>("speed").Should().Be(7.5f, "float Get/Set should work");

            // bool property
            component.Set("isActive", true);
            component.Get<bool>("isActive").Should().Be(true, "bool Get/Set should work");

            // string property
            component.Set("playerName", "TestName");
            component.Get<string>("playerName").Should().Be("TestName", "string Get/Set should work");

            TestContext.WriteLine("✅ All 4 properties from SimpleScript can Get/Set correctly");
        }

        [Test]
        public void Entity_AddComponent_TrySetFakeProperty_LooseMode_ShouldNotCrash()
        {
            // Verify that in loose mode, setting properties that don't exist doesn't crash
            // Arrange
            var project = new StrideProject(_testProjectPath, ProjectMode.Loose);
            var scene = project.LoadScene("Testing");
            var entity = scene.FindEntityByName("Box1x1x1");
            var component = entity.AddComponent("SimpleScript");

            // Act - Try to set a property that doesn't exist in SimpleScript
            component.Set("fakeProperty", 999);
            component.Set("nonExistentField", "test");

            // Assert - Component should still be valid
            component.Should().NotBeNull();
            component.Type.Should().Contain("SimpleScript");

            // The fake properties will be in the dictionary but won't be in the script
            component.Properties.Should().ContainKey("fakeProperty");
            component.Properties.Should().ContainKey("nonExistentField");

            // Real properties should still work
            component.Set("health", 100);
            component.Get<int>("health").Should().Be(100);

            TestContext.WriteLine("✅ Loose mode: Setting fake properties doesn't crash - they're just added to the dictionary");
        }

        [Test]
        public void Entity_AddComponent_TrySetFakeProperty_StrictMode_ShouldThrow()
        {
            // Verify that in strict mode, setting properties that don't exist throws an exception
            // Arrange
            var project = new StrideProject(_testProjectPath, ProjectMode.Strict);
            var scene = project.LoadScene("Testing");
            var entity = scene.FindEntityByName("Box1x1x1");
            var component = entity.AddComponent("SimpleScript");

            // Act & Assert - Should throw when setting fake property
            var act = () => component.Set("fakeProperty", 999);
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*fakeProperty*does not exist in script*SimpleScript*");

            // Real properties should still work
            component.Set("health", 100);
            component.Get<int>("health").Should().Be(100);

            TestContext.WriteLine("✅ Strict mode: Setting fake properties throws clear error with available properties");
        }

        [Test]
        public void Entity_AddComponent_LooseMode_AllowsAnyProperty_EvenNotInScript()
        {
            // Verify that loose mode allows setting properties even if they weren't scanned from the script
            // Arrange
            var project = new StrideProject(_testProjectPath, ProjectMode.Loose);
            var scene = project.LoadScene("Testing");
            var entity = scene.FindEntityByName("Box1x1x1");
            var component = entity.AddComponent("SimpleScript");

            // Act - Set a property that definitely doesn't exist in SimpleScript
            component.Set("customDynamicProperty", "dynamic value");
            component.Set("anotherFakeOne", 12345);

            // Assert - Properties should be set even though they're not in the script
            component.Properties.Should().ContainKey("customDynamicProperty");
            component.Properties.Should().ContainKey("anotherFakeOne");
            component.Get<string>("customDynamicProperty").Should().Be("dynamic value");
            component.Get<int>("anotherFakeOne").Should().Be(12345);

            // Verify they get written to YAML (even though they won't be recognized by Stride)
            var tempPath = Path.Combine(Path.GetTempPath(), $"test_loose_{Guid.NewGuid()}.sdscene");
            try
            {
                scene.SaveAs(tempPath);
                var yaml = File.ReadAllText(tempPath);

                // The properties will be in the YAML (user's responsibility if Stride doesn't recognize them)
                yaml.Should().Contain("customDynamicProperty");
                yaml.Should().Contain("anotherFakeOne");

                TestContext.WriteLine("✅ Loose mode: Allows any property - user controls what they set");
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        [Test]
        public void Entity_AddComponent_TryGetFakeProperty_ShouldReturnDefault()
        {
            // Verify that trying to get a property that doesn't exist returns default
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var scene = project.LoadScene("Testing");
            var entity = scene.FindEntityByName("Box1x1x1");
            var component = entity.AddComponent("SimpleScript");

            // Act & Assert - Get non-existent properties
            var fakeInt = component.Get<int>("fakeProperty");
            fakeInt.Should().Be(0, "Non-existent int should return default(int)");

            var fakeString = component.Get<string>("nonExistentField");
            fakeString.Should().BeNull("Non-existent string should return null");

            var fakeBool = component.Get<bool>("notReal");
            fakeBool.Should().Be(false, "Non-existent bool should return default(bool)");

            TestContext.WriteLine("✅ Getting fake properties returns default values, doesn't crash");
        }

        [Test]
        public void Entity_AddComponent_ShouldInitializeAllPropertiesWithDefaults()
        {
            // This test verifies that ALL properties from the script are initialized, not just the ones we set
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var scene = project.LoadScene("Testing");
            var entity = scene.FindEntityByName("Box1x1x1");
            var tempPath = Path.Combine(Path.GetTempPath(), $"test_defaults_{Guid.NewGuid()}.sdscene");

            try
            {
                // Act - Add component WITHOUT setting any properties
                var component = entity.AddComponent("SimpleScript");
                scene.SaveAs(tempPath);

                // Assert - ALL properties should be in YAML with default values
                var yaml = File.ReadAllText(tempPath);
                TestContext.WriteLine("=== Generated YAML (no properties set) ===");
                TestContext.WriteLine(yaml);

                // All 4 properties from SimpleScript should be present with defaults
                yaml.Should().Contain("health: 0", "int should default to 0");
                yaml.Should().Contain("speed: 0", "float should default to 0.0");
                yaml.Should().Contain("isActive: false", "bool should default to false");
                yaml.Should().Contain("playerName: null", "string should default to null");

                // Reload and verify defaults persist
                var reloadedScene = Scene.Load(tempPath);
                var reloadedEntity = reloadedScene.FindEntityByName("Box1x1x1");
                var reloadedComponent = reloadedEntity.GetComponent("SimpleScript");

                reloadedComponent.Should().NotBeNull();
                reloadedComponent.Get<int>("health").Should().Be(0, "int default should persist");
                reloadedComponent.Get<float>("speed").Should().Be(0.0f, "float default should persist");
                reloadedComponent.Get<bool>("isActive").Should().Be(false, "bool default should persist");
                // Note: string "null" is a literal string, not C# null
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        [Test]
        public void FullWorkflow_AddComponentSetPropertiesSaveReload_AllPropertyTypes()
        {
            // This is the CRITICAL end-to-end test for all property types
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var scene = project.LoadScene("Testing");
            var entity = scene.FindEntityByName("Box1x1x1");
            var tempPath = Path.Combine(Path.GetTempPath(), $"test_full_workflow_{Guid.NewGuid()}.sdscene");

            try
            {
                // ACT 1: Add component with script scanning
                var component = entity.AddComponent("NoNameSpace");

                // Assert component was created with full type
                component.Should().NotBeNull();
                component.Type.Should().StartWith(".", "NoNameSpace should have leading dot");
                component.Type.Should().Contain("NoNameSpace");

                // Assert all properties were initialized
                component.Properties.Should().ContainKey("intValue");
                component.Properties.Should().ContainKey("floatValue");
                component.Properties.Should().ContainKey("boolValue");
                component.Properties.Should().ContainKey("stringValue");
                component.Properties.Should().ContainKey("singleEntity");
                component.Properties.Should().ContainKey("entityList");
                component.Properties.Should().ContainKey("prefabRef");
                component.Properties.Should().ContainKey("modelRef");
                component.Properties.Should().ContainKey("materialRef");
                component.Properties.Should().ContainKey("RawAsset");
                component.Properties.Should().ContainKey("transformRef");

                // ACT 2: Set values for each property type
                component.Set("intValue", 42);
                component.Set("floatValue", 3.14f);
                component.Set("boolValue", true);
                component.Set("stringValue", "Hello World");

                // ACT 3: Get values to verify Set worked
                component.Get<int>("intValue").Should().Be(42);
                component.Get<float>("floatValue").Should().Be(3.14f);
                component.Get<bool>("boolValue").Should().Be(true);
                component.Get<string>("stringValue").Should().Be("Hello World");

                // ACT 4: Save to YAML
                scene.SaveAs(tempPath);

                // Assert YAML contains correct values
                var yaml = File.ReadAllText(tempPath);
                TestContext.WriteLine("=== Generated YAML ===");
                TestContext.WriteLine(yaml);

                yaml.Should().Contain(".NoNameSpace", "Type should have leading dot");
                yaml.Should().Contain("intValue: 42");
                yaml.Should().Contain("floatValue: 3.14");
                yaml.Should().Contain("boolValue: true");
                yaml.Should().Contain("stringValue: Hello World");

                // Properties that weren't set should be null
                yaml.Should().Contain("singleEntity: null");
                yaml.Should().Contain("entityList: null");
                yaml.Should().Contain("prefabRef: null");

                // ACT 5: Reload scene
                var reloadedScene = Scene.Load(tempPath);
                var reloadedEntity = reloadedScene.FindEntityByName("Box1x1x1");
                reloadedEntity.Should().NotBeNull("Entity should exist after reload");

                var reloadedComponent = reloadedEntity.GetComponent("NoNameSpace");
                reloadedComponent.Should().NotBeNull("Component should exist after reload");

                // ACT 6: Get values from reloaded component
                reloadedComponent.Get<int>("intValue").Should().Be(42, "intValue should persist");
                reloadedComponent.Get<float>("floatValue").Should().Be(3.14f, "floatValue should persist");
                reloadedComponent.Get<bool>("boolValue").Should().Be(true, "boolValue should persist");
                reloadedComponent.Get<string>("stringValue").Should().Be("Hello World", "stringValue should persist");

                // Verify properties we DIDN'T set still have their default values and persisted
                reloadedComponent.Properties.Should().ContainKey("singleEntity", "Unset Entity ref should still be in YAML");
                reloadedComponent.Properties.Should().ContainKey("entityList", "Unset List should still be in YAML");
                reloadedComponent.Properties.Should().ContainKey("prefabRef", "Unset Prefab ref should still be in YAML");
                reloadedComponent.Properties.Should().ContainKey("modelRef", "Unset Model ref should still be in YAML");
                reloadedComponent.Properties.Should().ContainKey("materialRef", "Unset Material ref should still be in YAML");
                reloadedComponent.Properties.Should().ContainKey("RawAsset", "Unset RawAsset should still be in YAML");
                reloadedComponent.Properties.Should().ContainKey("transformRef", "Unset Component ref should still be in YAML");
                reloadedComponent.Properties.Should().ContainKey("intArray", "Unset array should still be in YAML");
                reloadedComponent.Properties.Should().ContainKey("entityArray", "Unset Entity array should still be in YAML");
                reloadedComponent.Properties.Should().ContainKey("Stuff2", "Unset List<int> should still be in YAML");
                reloadedComponent.Properties.Should().ContainKey("AnimationClips", "Unset Dictionary should still be in YAML");
                reloadedComponent.Properties.Should().ContainKey("primClips", "Unset primitive Dictionary should still be in YAML");

                // Verify type is still correct
                reloadedComponent.Type.Should().StartWith(".", "Type should still have leading dot after reload");
                reloadedComponent.Type.Should().Contain("NoNameSpace");

                TestContext.WriteLine("✅ FULL WORKFLOW PASSED: Extract → Initialize → Set → Get → Save → Reload → Get");
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        [Test]
        public void Entity_AddComponent_SetWrongType_StrictMode_ShouldThrow_IntAsFloat()
        {
            // Verify that strict mode catches type mismatches (int property set with float)
            // Arrange
            var project = new StrideProject(_testProjectPath, ProjectMode.Strict);
            var scene = project.LoadScene("Testing");
            var entity = scene.FindEntityByName("Box1x1x1");
            var component = entity.AddComponent("SimpleScript");

            // Act & Assert - Should throw when setting int property with float value
            var act = () => component.Set("health", 100f);  // health is int, but passing float
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*Type mismatch*health*Expected: int*Got: Single*");

            TestContext.WriteLine("✅ Strict mode: Type mismatch (int vs float) throws clear error");
        }

        [Test]
        public void Entity_AddComponent_SetWrongType_StrictMode_ShouldThrow_FloatAsInt()
        {
            // Verify that strict mode catches type mismatches (float property set with int)
            // Arrange
            var project = new StrideProject(_testProjectPath, ProjectMode.Strict);
            var scene = project.LoadScene("Testing");
            var entity = scene.FindEntityByName("Box1x1x1");
            var component = entity.AddComponent("SimpleScript");

            // Act & Assert - Should throw when setting float property with int value
            var act = () => component.Set("speed", 100);  // speed is float, but passing int
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*Type mismatch*speed*Expected: float*Got: Int32*");

            TestContext.WriteLine("✅ Strict mode: Type mismatch (float vs int) throws clear error");
        }

        [Test]
        public void Entity_AddComponent_SetWrongType_StrictMode_ShouldThrow_StringAsInt()
        {
            // Verify that strict mode catches type mismatches (string property set with int)
            // Arrange
            var project = new StrideProject(_testProjectPath, ProjectMode.Strict);
            var scene = project.LoadScene("Testing");
            var entity = scene.FindEntityByName("Box1x1x1");
            var component = entity.AddComponent("SimpleScript");

            // Act & Assert - Should throw when setting string property with int value
            var act = () => component.Set("playerName", 123);  // playerName is string, but passing int
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*Type mismatch*playerName*Expected: string*Got: Int32*");

            TestContext.WriteLine("✅ Strict mode: Type mismatch (string vs int) throws clear error");
        }

        [Test]
        public void Entity_AddComponent_SetCorrectTypes_StrictMode_ShouldNotThrow()
        {
            // Verify that strict mode ALLOWS correct types
            // Arrange
            var project = new StrideProject(_testProjectPath, ProjectMode.Strict);
            var scene = project.LoadScene("Testing");
            var entity = scene.FindEntityByName("Box1x1x1");
            var component = entity.AddComponent("SimpleScript");

            // Act & Assert - Should NOT throw when using correct types
            var act1 = () => component.Set("health", 100);  // int - correct
            act1.Should().NotThrow();

            var act2 = () => component.Set("speed", 5.5f);  // float - correct
            act2.Should().NotThrow();

            var act3 = () => component.Set("isActive", true);  // bool - correct
            act3.Should().NotThrow();

            var act4 = () => component.Set("playerName", "Hero");  // string - correct
            act4.Should().NotThrow();

            // Verify values were set correctly
            component.Get<int>("health").Should().Be(100);
            component.Get<float>("speed").Should().Be(5.5f);
            component.Get<bool>("isActive").Should().Be(true);
            component.Get<string>("playerName").Should().Be("Hero");

            TestContext.WriteLine("✅ Strict mode: Correct types work perfectly");
        }

        [Test]
        public void Entity_AddComponent_SetWrongType_LooseMode_ShouldNotThrow()
        {
            // Verify that loose mode ALLOWS type mismatches (user's responsibility)
            // Arrange
            var project = new StrideProject(_testProjectPath, ProjectMode.Loose);
            var scene = project.LoadScene("Testing");
            var entity = scene.FindEntityByName("Box1x1x1");
            var component = entity.AddComponent("SimpleScript");

            // Act & Assert - Should NOT throw even with wrong types
            var act1 = () => component.Set("health", 100f);  // int property, float value - loose mode allows
            act1.Should().NotThrow();

            var act2 = () => component.Set("speed", 100);  // float property, int value - loose mode allows
            act2.Should().NotThrow();

            var act3 = () => component.Set("playerName", 123);  // string property, int value - loose mode allows
            act3.Should().NotThrow();

            // Values will be set, but may cause issues in Stride (user's responsibility)
            component.Get<float>("health").Should().Be(100f);
            component.Get<int>("speed").Should().Be(100);
            component.Get<int>("playerName").Should().Be(123);

            TestContext.WriteLine("✅ Loose mode: Type mismatches allowed - user controls correctness");
        }

        [Test]
        public void Entity_AddComponent_SetEntityRef_StrictMode_ShouldAcceptString()
        {
            // Verify that Entity references accept string format (ref!! guid)
            // Arrange
            var project = new StrideProject(_testProjectPath, ProjectMode.Strict);
            var scene = project.LoadScene("Testing");
            var entity = scene.FindEntityByName("Box1x1x1");
            var component = entity.AddComponent("NoNameSpace");

            // Act & Assert - Should accept string for Entity reference
            var act = () => component.Set("singleEntity", "ref!! abc-123-def");
            act.Should().NotThrow("Entity references should accept strings");

            component.Get<string>("singleEntity").Should().Be("ref!! abc-123-def");

            TestContext.WriteLine("✅ Strict mode: Entity references accept string format");
        }

        [Test]
        public void Entity_AddComponent_SetEntityRef_StrictMode_ShouldRejectInt()
        {
            // Verify that Entity references reject non-string types
            // Arrange
            var project = new StrideProject(_testProjectPath, ProjectMode.Strict);
            var scene = project.LoadScene("Testing");
            var entity = scene.FindEntityByName("Box1x1x1");
            var component = entity.AddComponent("NoNameSpace");

            // Act & Assert - Should throw when setting Entity reference with int
            var act = () => component.Set("singleEntity", 123);
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*Type mismatch*singleEntity*Expected: Entity*Got: Int32*");

            TestContext.WriteLine("✅ Strict mode: Entity references reject integers");
        }

        [Test]
        public void Entity_AddComponent_SetPrefabRef_StrictMode_ShouldAcceptString()
        {
            // Verify that Prefab references accept string format (guid:path)
            // Arrange
            var project = new StrideProject(_testProjectPath, ProjectMode.Strict);
            var scene = project.LoadScene("Testing");
            var entity = scene.FindEntityByName("Box1x1x1");
            var component = entity.AddComponent("NoNameSpace");

            // Act & Assert - Should accept string for Prefab reference
            var act = () => component.Set("prefabRef", "abc-123-def:path/to/prefab");
            act.Should().NotThrow("Prefab references should accept strings");

            component.Get<string>("prefabRef").Should().Be("abc-123-def:path/to/prefab");

            TestContext.WriteLine("✅ Strict mode: Prefab references accept string format");
        }

        [Test]
        public void Entity_AddComponent_SetPrefabRef_StrictMode_ShouldRejectInt()
        {
            // Verify that Prefab references reject non-string types
            // Arrange
            var project = new StrideProject(_testProjectPath, ProjectMode.Strict);
            var scene = project.LoadScene("Testing");
            var entity = scene.FindEntityByName("Box1x1x1");
            var component = entity.AddComponent("NoNameSpace");

            // Act & Assert - Should throw when setting Prefab reference with int
            var act = () => component.Set("prefabRef", 456);
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*Type mismatch*prefabRef*Expected: Prefab*Got: Int32*");

            TestContext.WriteLine("✅ Strict mode: Prefab references reject integers");
        }

        [Test]
        public void Entity_AddComponent_SetModelRef_StrictMode_ShouldAcceptString()
        {
            // Verify that Model references accept string format
            // Arrange
            var project = new StrideProject(_testProjectPath, ProjectMode.Strict);
            var scene = project.LoadScene("Testing");
            var entity = scene.FindEntityByName("Box1x1x1");
            var component = entity.AddComponent("NoNameSpace");

            // Act & Assert - Should accept string for Model reference
            var act = () => component.Set("modelRef", "abc-123:path/to/model");
            act.Should().NotThrow("Model references should accept strings");

            TestContext.WriteLine("✅ Strict mode: Model references accept string format");
        }

        [Test]
        public void Entity_AddComponent_SetModelRef_StrictMode_ShouldRejectFloat()
        {
            // Verify that Model references reject non-string types
            // Arrange
            var project = new StrideProject(_testProjectPath, ProjectMode.Strict);
            var scene = project.LoadScene("Testing");
            var entity = scene.FindEntityByName("Box1x1x1");
            var component = entity.AddComponent("NoNameSpace");

            // Act & Assert - Should throw when setting Model reference with float
            var act = () => component.Set("modelRef", 3.14f);
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*Type mismatch*modelRef*Expected: Model*Got: Single*");

            TestContext.WriteLine("✅ Strict mode: Model references reject floats");
        }

        [Test]
        public void Entity_AddComponent_SetMaterialRef_StrictMode_ShouldAcceptString()
        {
            // Verify that Material references accept string format
            // Arrange
            var project = new StrideProject(_testProjectPath, ProjectMode.Strict);
            var scene = project.LoadScene("Testing");
            var entity = scene.FindEntityByName("Box1x1x1");
            var component = entity.AddComponent("NoNameSpace");

            // Act & Assert - Should accept string for Material reference
            var act = () => component.Set("materialRef", "xyz-789:materials/main");
            act.Should().NotThrow("Material references should accept strings");

            TestContext.WriteLine("✅ Strict mode: Material references accept string format");
        }

        [Test]
        public void Entity_AddComponent_SetRawAssetRef_StrictMode_ShouldAcceptString()
        {
            // Verify that RawAsset (UrlReference) accepts string format
            // Arrange
            var project = new StrideProject(_testProjectPath, ProjectMode.Strict);
            var scene = project.LoadScene("Testing");
            var entity = scene.FindEntityByName("Box1x1x1");
            var component = entity.AddComponent("NoNameSpace");

            // Act & Assert - Should accept string for RawAsset reference
            var act = () => component.Set("RawAsset", "def-456:resources/data");
            act.Should().NotThrow("RawAsset references should accept strings");

            TestContext.WriteLine("✅ Strict mode: RawAsset references accept string format");
        }

        [Test]
        public void Entity_AddComponent_SetHelperMethods_StrictMode_ShouldWork()
        {
            // Verify that helper methods (SetEntityRef, SetAssetRef) work correctly
            // Arrange
            var project = new StrideProject(_testProjectPath, ProjectMode.Strict);
            var scene = project.LoadScene("Testing");
            var entity = scene.FindEntityByName("Box1x1x1");

            // Create a target entity to reference
            var targetEntity = scene.CreateEntity("TargetEntity");
            var component = entity.AddComponent("NoNameSpace");

            // Act & Assert - SetEntityRef should work
            var act1 = () => component.SetEntityRef("singleEntity", targetEntity);
            act1.Should().NotThrow("SetEntityRef helper should work in strict mode");

            // Verify it set the correct string format
            var refString = component.Get<string>("singleEntity");
            refString.Should().Contain("ref!!");
            refString.Should().Contain(targetEntity.Id);

            TestContext.WriteLine("✅ Strict mode: SetEntityRef helper works correctly");
        }

        [Test]
        public void FullWorkflow_WithNamespace_AllSteps()
        {
            // Same comprehensive test but with a namespaced script
            var project = new StrideProject(_testProjectPath);
            var scene = project.LoadScene("Testing");
            var entity = scene.FindEntityByName("Box1x1x1");
            var tempPath = Path.Combine(Path.GetTempPath(), $"test_namespaced_{Guid.NewGuid()}.sdscene");

            try
            {
                // Add component
                var component = entity.AddComponent("SimpleScript");

                // Verify type has full namespace
                component.Type.Should().Contain("TestNamespace.SimpleScript");
                component.Type.Should().NotStartWith(".", "Namespaced scripts should NOT have leading dot");

                // Set and verify all properties
                component.Set("health", 999);
                component.Set("speed", 12.5f);
                component.Set("isActive", true);
                component.Set("playerName", "TestHero");

                component.Get<int>("health").Should().Be(999);
                component.Get<float>("speed").Should().Be(12.5f);
                component.Get<bool>("isActive").Should().Be(true);
                component.Get<string>("playerName").Should().Be("TestHero");

                // Save
                scene.SaveAs(tempPath);

                // Verify YAML
                var yaml = File.ReadAllText(tempPath);
                yaml.Should().Contain("TestNamespace.SimpleScript");
                yaml.Should().Contain("health: 999");
                yaml.Should().Contain("speed: 12.5");
                yaml.Should().Contain("isActive: true");
                yaml.Should().Contain("playerName: TestHero");

                // Reload and verify
                var reloadedScene = Scene.Load(tempPath);
                var reloadedEntity = reloadedScene.FindEntityByName("Box1x1x1");
                var reloadedComponent = reloadedEntity.GetComponent("SimpleScript");

                reloadedComponent.Should().NotBeNull();
                reloadedComponent.Get<int>("health").Should().Be(999);
                reloadedComponent.Get<float>("speed").Should().Be(12.5f);
                reloadedComponent.Get<bool>("isActive").Should().Be(true);
                reloadedComponent.Get<string>("playerName").Should().Be("TestHero");

                TestContext.WriteLine("✅ NAMESPACED WORKFLOW PASSED");
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }
    }
}
