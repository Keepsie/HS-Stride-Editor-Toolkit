// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using NUnit.Framework;
using FluentAssertions;
using HS.Stride.Editor.Toolkit.Core;
using HS.Stride.Editor.Toolkit.Core.AssetEditing;
using HS.Stride.Editor.Toolkit.Core.Wrappers;
using HS.Stride.Editor.Toolkit.Core.SceneEditing;
using HS.Stride.Editor.Toolkit.Core.PrefabEditing;

namespace HS.Stride.Editor.Toolkit.Tests
{
    [TestFixture]
    public class VFXTests
    {
        private string _testScenePath;
        private string _vfxPrefabsPath;

        [SetUp]
        public void Setup()
        {
            // Go up from bin/Release/net8.0 to project root
            var projectRoot = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", ".."));
            _testScenePath = Path.Combine(projectRoot, "Example Scenes", "TestProject", "Assets", "Testing.sdscene");
            _vfxPrefabsPath = Path.Combine(projectRoot, "Example Assets", "VFXPrefabs");
        }

        [Test]
        public void CreateParticleSystemComponent_ShouldHaveDefaultProperties()
        {
            // Act
            var component = ParticleSystemWrapper.CreateComponent();

            // Assert
            component.Should().NotBeNull();
            component.Type.Should().Be("ParticleSystemComponent");
            component.Id.Should().NotBeNullOrEmpty();
            component.Properties.Should().ContainKey("Control");
            component.Properties.Should().ContainKey("Color");
            component.Properties.Should().ContainKey("Speed");
            component.Properties.Should().ContainKey("ParticleSystem");
        }

        [Test]
        public void ParticleSystemWrapper_ShouldWrapComponent()
        {
            // Arrange
            var component = ParticleSystemWrapper.CreateComponent();

            // Act
            var wrapper = new ParticleSystemWrapper(component);

            // Assert
            wrapper.Should().NotBeNull();
            wrapper.Control.Should().Be("Play");
            wrapper.Speed.Should().Be(1.0f);
            wrapper.ResetSeconds.Should().Be(5.0f);
        }

        [Test]
        public void ParticleSystemWrapper_SetProperties_ShouldUpdateComponent()
        {
            // Arrange
            var component = ParticleSystemWrapper.CreateComponent();
            var wrapper = new ParticleSystemWrapper(component);

            // Act
            wrapper.Speed = 2.0f;
            wrapper.ResetSeconds = 3.0f;
            wrapper.Control = "Pause";

            // Assert
            wrapper.Speed.Should().Be(2.0f);
            wrapper.ResetSeconds.Should().Be(3.0f);
            wrapper.Control.Should().Be("Pause");
        }

        [Test]
        public void CreateBillboardEmitter_ShouldCreateEmitterWithCorrectShape()
        {
            // Arrange
            var component = ParticleSystemWrapper.CreateComponent();
            var particleSystem = new ParticleSystemWrapper(component);

            // Act
            var emitter = particleSystem.CreateBillboardEmitter("test_emitter", (1.0f, 2.0f));

            // Assert
            emitter.Should().NotBeNull();
            emitter.EmitterName.Should().Be("test_emitter");
            emitter.ParticleLifetime.Should().Be((1.0f, 2.0f));
            emitter.Get<Dictionary<string, object>>("ShapeBuilder").Should().NotBeNull();
        }

        [Test]
        public void VFXEmitter_SetTextureMaterial_ShouldCreateMaterialStructure()
        {
            // Arrange
            var component = ParticleSystemWrapper.CreateComponent();
            var particleSystem = new ParticleSystemWrapper(component);
            var emitter = particleSystem.CreateBillboardEmitter("fire");

            // Act
            var textureRef = new AssetReference { Id = "test-guid", Path = "Textures/fire" };
            emitter.SetTextureMaterial(
                textureRef,
                hdrMultiplier: 10.0f,
                alphaAdditive: 0.8f);

            // Assert
            var material = emitter.Get<Dictionary<string, object>>("Material");
            material.Should().NotBeNull();
            material.Should().ContainKey("!ParticleMaterialComputeColor");

            var alphaAdditive = emitter.Get<float?>("Material.AlphaAdditive");
            alphaAdditive.Should().Be(0.8f);
        }

        [Test]
        public void VFXEmitter_AddBurstSpawner_ShouldAddSpawnerToEmitter()
        {
            // Arrange
            var emitter = new VFXEmitter { EmitterName = "test" };

            // Act
            emitter.SetBurstSpawner(10, oneShot: true);

            // Assert
            var spawners = emitter.Get<Dictionary<string, object>>("Spawners");
            spawners.Should().NotBeNull();
            spawners.Should().HaveCount(1);
        }

        [Test]
        public void VFXEmitter_AddInitializers_ShouldAddMultipleInitializers()
        {
            // Arrange
            var emitter = new VFXEmitter { EmitterName = "test" };

            // Act
            emitter.AddInitialPosition((-0.1f, 0.0f, -0.1f), (0.1f, 0.2f, 0.1f));
            emitter.AddInitialVelocity((-0.5f, 0.5f, -0.5f), (0.5f, 1.0f, 0.5f));
            emitter.AddInitialSize((0.7f, 1.5f));

            // Assert
            var initializers = emitter.Get<Dictionary<string, object>>("Initializers");
            initializers.Should().NotBeNull();
            initializers.Should().HaveCount(3);
        }

        [Test]
        public void VFXEmitter_AddColorFade_ShouldCreateColorCurve()
        {
            // Arrange
            var emitter = new VFXEmitter { EmitterName = "test" };

            // Act
            emitter.AddColorFade(
                (0.0f, 1.0f, 1.0f, 1.0f, 0.0f),
                (0.2f, 1.0f, 1.0f, 1.0f, 0.5f),
                (1.0f, 1.0f, 1.0f, 1.0f, 0.0f));

            // Assert
            var updaters = emitter.Get<Dictionary<string, object>>("Updaters");
            updaters.Should().NotBeNull();
            updaters.Should().HaveCount(1);
        }

        [Test]
        public void RecreateClickVFX_FromCode_ShouldMatchOriginalStructure()
        {
            // This recreates the vfx-Click.sdprefab from the Example Assets
            // Arrange
            var component = ParticleSystemWrapper.CreateComponent();
            var particleSystem = new ParticleSystemWrapper(component);

            // Set particle system properties
            particleSystem.ResetSeconds = 5.0f;
            particleSystem.Control = "Play";
            particleSystem.Speed = 1.0f;

            // Act - Create the emitter matching vfx-Click structure
            var emitter = particleSystem.CreateBillboardEmitter(null, (1.0f, 1.0f));
            emitter.MaxParticlesOverride = 1;

            // Set the material
            var textureRef = new AssetReference { Id = "8223c28b-5cb8-402a-b51f-aa6c239dbb9f", Path = "Stride Samples/Textures/circle02" };
            emitter.SetTextureMaterial(
                textureRef,
                hdrMultiplier: 10.0f,
                alphaAdditive: 1.0f);

            // Set spawner
            emitter.SetBurstSpawner(1, oneShot: true);

            // Set initializers
            emitter.AddInitialPosition((0.0f, 0.1f, 0.0f), (0.0f, 0.1f, 0.0f));

            // Add emitter to particle system
            particleSystem.AddEmitter(emitter);

            // Assert
            var emitters = particleSystem.GetEmitters();
            emitters.Should().HaveCount(1);

            var clickEmitter = emitters[0];
            clickEmitter.ParticleLifetime.Should().Be((1.0f, 1.0f));
            clickEmitter.MaxParticlesOverride.Should().Be(1);

            var material = clickEmitter.Get<Dictionary<string, object>>("Material");
            material.Should().NotBeNull();

            var spawners = clickEmitter.Get<Dictionary<string, object>>("Spawners");
            spawners.Should().NotBeNull();
            spawners.Should().HaveCount(1);

            var initializers = clickEmitter.Get<Dictionary<string, object>>("Initializers");
            initializers.Should().NotBeNull();
            initializers.Should().HaveCount(1);
        }

        [Test]
        public void AddParticleSystemToEntity_ShouldWork()
        {
            // Arrange
            var scene = Scene.Load(_testScenePath);
            var entity = scene.CreateEntity("VFXTest");

            // Act
            var particleSystem = entity.AddParticleSystem();

            var emitter = particleSystem.CreateBillboardEmitter("explosion", (1.0f, 1.3f));
            var textureRef = new AssetReference { Id = "test-id", Path = "texture" };
            emitter.SetTextureMaterial(textureRef, 50.0f);
            emitter.SetBurstSpawner(10, oneShot: true);
            emitter.AddInitialPosition((-0.2f, 0.0f, -0.2f), (0.2f, 0.0f, 0.2f));
            emitter.AddInitialVelocity((-0.5f, -0.2f, -0.5f), (0.5f, 0.5f, 0.5f));

            particleSystem.AddEmitter(emitter);

            // Assert
            var retrievedSystem = entity.GetParticleSystem();
            retrievedSystem.Should().NotBeNull();

            var loadedEmitters = retrievedSystem!.GetEmitters();
            loadedEmitters.Should().HaveCount(1);
            loadedEmitters[0].EmitterName.Should().Be("explosion");
        }

        [Test]
        public void CreateRibbonEmitter_ForTrail_ShouldHaveCorrectProperties()
        {
            // Arrange
            var component = ParticleSystemWrapper.CreateComponent();
            var particleSystem = new ParticleSystemWrapper(component);

            // Act - Create ribbon emitter like Bullettrail
            var emitter = particleSystem.CreateRibbonEmitter(
                "trail",
                (1.0f, 1.0f),
                segments: 15,
                smoothingPolicy: VFXSmoothingPolicy.Best,
                maxParticles: 50);

            var textureRef = new AssetReference { Id = "texture-guid", Path = "Textures/bullettrail" };
            emitter.SetTextureMaterial(
                textureRef,
                hdrMultiplier: 5.0f,
                alphaAdditive: 0.5f);

            emitter.SetPerSecondSpawner(50.0f, looping: true);
            emitter.AddSpawnOrder();
            emitter.AddInitialVelocity((-0.1f, 0.4f, -0.1f), (0.1f, 0.4f, 0.1f));
            emitter.AddSizeOverTime((0.0f, 0.1f), (0.9f, 0.0f));
            emitter.AddColorFade(
                (0.0f, 1.0f, 1.0f, 1.0f, 0.5f),
                (1.0f, 1.0f, 1.0f, 1.0f, 0.0f));

            particleSystem.AddEmitter(emitter);

            // Assert
            var emitters = particleSystem.GetEmitters();
            emitters.Should().HaveCount(1);

            var trail = emitters[0];
            trail.EmitterName.Should().Be("trail");
            trail.SortingPolicy.Should().Be("ByOrder");
            trail.MaxParticlesOverride.Should().Be(50);

            var updaters = trail.Get<Dictionary<string, object>>("Updaters");
            updaters.Should().NotBeNull();
            updaters.Should().HaveCount(2); // SizeOverTime + ColorOverTime
        }

        [Test]
        public void CreateComplexExplosion_MultipleEmitters_ShouldWork()
        {
            // Arrange
            var component = ParticleSystemWrapper.CreateComponent();
            var particleSystem = new ParticleSystemWrapper(component);

            // Act - Create explosion with fire, smoke, and sparks
            // Fire emitter
            var fire = particleSystem.CreateBillboardEmitter("explosionfire", (1.0f, 1.3f));
            fire.DrawPriority = 5;
            var fireTexture = new AssetReference { Id = "texture", Path = "EXP001" };
            fire.SetTextureMaterial(fireTexture, 50.0f);
            fire.SetFlipbookAnimation(8, 8, 8, 56);
            fire.SetBurstSpawner(10, oneShot: true);
            fire.AddInitialSize((1.0f, 2.0f));
            fire.AddInitialPosition((-0.2f, 0.0f, -0.2f), (0.2f, 0.0f, 0.2f));
            fire.AddInitialVelocity((-0.5f, -0.2f, -0.5f), (0.5f, 0.5f, 0.5f));
            fire.AddInitialRotation((-360.0f, 360.0f));
            fire.AddColorFade(
                (0.0f, 1.0f, 1.0f, 1.0f, 1.0f),
                (0.5f, 0.3f, 0.3f, 0.3f, 0.25f),
                (1.0f, 0.0f, 0.0f, 0.0f, 0.0f));

            particleSystem.AddEmitter(fire);

            // Sparks emitter
            var sparks = particleSystem.CreateBillboardEmitter("sparks", (0.7f, 1.4f));
            var sparksTexture = new AssetReference { Id = "texture", Path = "dota" };
            sparks.SetTextureMaterial(sparksTexture, 50.0f);
            sparks.SetBurstSpawner(50, oneShot: true, delay: (0.5f, 0.5f));
            sparks.AddInitialVelocity((-0.5f, -0.5f, -0.5f), (0.5f, 0.5f, 0.5f));
            sparks.AddInitialSize((0.1f, 0.2f), scaleUniform: 0.1f);
            sparks.AddInitialPosition((-0.6f, -0.6f, -0.6f), (0.6f, 0.6f, 0.6f));
            sparks.AddInitialColor(
                (1.0f, 0.18187499f, 0.037500024f, 1.0f),
                (1.0f, 0.3f, 0.0f, 1.0f));
            sparks.AddForceField(
                fieldShape: VFXFieldShape.Sphere,
                scale: (2.0f, 2.0f, 2.0f),
                forceDirected: 0.3f,
                forceVortex: 0.5f,
                forceRepulsive: -0.3f,
                energyConservation: 0.5f);

            particleSystem.AddEmitter(sparks);

            // Assert
            var emitters = particleSystem.GetEmitters();
            emitters.Should().HaveCount(2);
            emitters.Should().Contain(e => e.EmitterName == "explosionfire");
            emitters.Should().Contain(e => e.EmitterName == "sparks");

            var fireEmitter = emitters.First(e => e.EmitterName == "explosionfire");
            fireEmitter.DrawPriority.Should().Be(5);

            var sparksEmitter = emitters.First(e => e.EmitterName == "sparks");
            var forceFieldUpdaters = sparksEmitter.Get<Dictionary<string, object>>("Updaters");
            forceFieldUpdaters.Should().NotBeNull();
            forceFieldUpdaters.Should().HaveCount(1); // ForceField updater
        }

        #region Round-Trip Tests (Load, Recreate, Compare)

        [Test]
        public void RoundTrip_LoadClickVFX_RecreateInCode_ComparePrefabs()
        {
            // This is the GOLD STANDARD test: Load actual VFX, recreate with our API, compare them

            // Arrange - Load original prefab
            var originalPrefabPath = Path.Combine(_vfxPrefabsPath, "vfx-Click.sdprefab");
            var originalPrefab = Prefab.Load(originalPrefabPath);
            var originalEntity = originalPrefab.GetRootEntity();
            originalEntity.Should().NotBeNull();

            var originalParticleComponent = originalEntity!.GetComponent("ParticleSystemComponent");
            originalParticleComponent.Should().NotBeNull("Original should have ParticleSystemComponent");

            // Act - Recreate the VFX using our API
            var recreatedPrefab = Prefab.Create("Click_Recreated");
            var recreatedEntity = recreatedPrefab.GetRootEntity();
            var vfx = recreatedEntity!.AddParticleSystem();

            // Match the original settings
            vfx.ResetSeconds = 5.0f;
            vfx.Control = "Play";
            vfx.Speed = 1.0f;
            vfx.SetColor(1.0f, 1.0f, 1.0f, 1.0f);

            // Create emitter matching original (Click uses Quad, not Billboard!)
            var emitter = new VFXEmitter
            {
                ParticleLifetime = (1.0f, 1.0f),
                MaxParticlesOverride = 1
            };

            // Set Quad shape (not Billboard)
            emitter.SetShapeBuilder(VFXShapeType.Quad, new Dictionary<string, object>
            {
                ["SamplerPosition"] = "null",
                ["SamplerRotation"] = "null"
            });

            // Set material with size curve
            var textureRef = new AssetReference { Id = "8223c28b-5cb8-402a-b51f-aa6c239dbb9f", Path = "Stride Samples/Textures/circle02" };
            emitter.SetTextureMaterial(
                textureRef,
                hdrMultiplier: 10.0f,
                alphaAdditive: 1.0f);

            // Note: The original has a size curve in ShapeBuilder, we need to add that manually
            var shapeBuilder = emitter.Get<Dictionary<string, object>>("ShapeBuilder");
            if (shapeBuilder != null)
            {
                shapeBuilder["SamplerSize"] = new Dictionary<string, object>
                {
                    ["!ComputeCurveSamplerFloat"] = "",
                    ["Curve"] = new Dictionary<string, object>
                    {
                        ["!ComputeAnimationCurveFloat"] = "",
                        ["KeyFrames"] = new Dictionary<string, object>
                        {
                            [Utilities.GuidHelper.NewGuid()] = new Dictionary<string, object>
                            {
                                ["Key"] = 0.0f,
                                ["Value"] = 0.0f,
                                ["TangentType"] = "Linear"
                            },
                            [Utilities.GuidHelper.NewGuid()] = new Dictionary<string, object>
                            {
                                ["Key"] = 0.4f,
                                ["Value"] = 1.0f,
                                ["TangentType"] = "Linear"
                            },
                            [Utilities.GuidHelper.NewGuid()] = new Dictionary<string, object>
                            {
                                ["Key"] = 1.0f,
                                ["Value"] = 1.0f,
                                ["TangentType"] = "Linear"
                            }
                        }
                    }
                };
            }

            emitter.SetBurstSpawner(1, oneShot: true);
            emitter.AddInitialPosition((0.0f, 0.1f, 0.0f), (0.0f, 0.1f, 0.0f));

            vfx.AddEmitter(emitter);

            // Save both to temp files
            var tempOriginalPath = Path.Combine(Path.GetTempPath(), $"vfx-click-original-{Guid.NewGuid()}.sdprefab");
            var tempRecreatedPath = Path.Combine(Path.GetTempPath(), $"vfx-click-recreated-{Guid.NewGuid()}.sdprefab");

            try
            {
                originalPrefab.SaveAs(tempOriginalPath);
                recreatedPrefab.SaveAs(tempRecreatedPath);

                // Assert - Compare structures
                var originalWrapper = new ParticleSystemWrapper(originalParticleComponent!);
                var recreatedWrapper = vfx;

                // Compare top-level properties
                recreatedWrapper.Control.Should().Be(originalWrapper.Control);
                recreatedWrapper.Speed.Should().Be(originalWrapper.Speed);
                recreatedWrapper.ResetSeconds.Should().Be(originalWrapper.ResetSeconds);

                // Compare emitter counts
                var originalEmitters = originalWrapper.GetEmitters();
                var recreatedEmitters = recreatedWrapper.GetEmitters();

                recreatedEmitters.Should().HaveCount(originalEmitters.Count, "Should have same number of emitters");

                // Compare first emitter properties
                var origEmitter = originalEmitters[0];
                var recEmitter = recreatedEmitters[0];

                recEmitter.ParticleLifetime.Should().Be(origEmitter.ParticleLifetime);
                recEmitter.MaxParticlesOverride.Should().Be(origEmitter.MaxParticlesOverride);

                // Verify spawners exist
                var origSpawners = origEmitter.Get<Dictionary<string, object>>("Spawners");
                var recSpawners = recEmitter.Get<Dictionary<string, object>>("Spawners");

                origSpawners.Should().NotBeNull();
                recSpawners.Should().NotBeNull();
                recSpawners!.Should().HaveCount(origSpawners!.Count);

                // Verify initializers exist
                var origInitializers = origEmitter.Get<Dictionary<string, object>>("Initializers");
                var recInitializers = recEmitter.Get<Dictionary<string, object>>("Initializers");

                origInitializers.Should().NotBeNull();
                recInitializers.Should().NotBeNull();
                recInitializers!.Should().HaveCount(origInitializers!.Count);

                TestContext.WriteLine("✅ Click VFX recreated successfully!");
                TestContext.WriteLine($"Original emitters: {originalEmitters.Count}");
                TestContext.WriteLine($"Recreated emitters: {recreatedEmitters.Count}");
            }
            finally
            {
                if (File.Exists(tempOriginalPath)) File.Delete(tempOriginalPath);
                if (File.Exists(tempRecreatedPath)) File.Delete(tempRecreatedPath);
            }
        }

        [Test]
        public void RoundTrip_LoadSmokeVFX_RecreateInCode_ComparePrefabs()
        {
            // Smoke is more complex: has velocity, size, rotation, color fade, force field

            // Arrange - Load original
            var originalPrefabPath = Path.Combine(_vfxPrefabsPath, "vfx-Smoke.sdprefab");
            var originalPrefab = Prefab.Load(originalPrefabPath);
            var originalEntity = originalPrefab.GetRootEntity();
            var originalComponent = originalEntity!.GetComponent("ParticleSystemComponent");
            originalComponent.Should().NotBeNull();

            // Act - Recreate
            var recreatedPrefab = Prefab.Create("Smoke_Recreated");
            var recreatedEntity = recreatedPrefab.GetRootEntity();
            var vfx = recreatedEntity!.AddParticleSystem();

            vfx.ResetSeconds = 0.0f;
            vfx.Control = "Play";
            vfx.Speed = 1.0f;

            // Create smoke emitter
            var emitter = vfx.CreateBillboardEmitter("Smoke", (5.0f, 7.0f));

            // Set material
            var textureRef = new AssetReference { Id = "fa1018e4-c74d-4495-9535-134f5a06e4cd", Path = "Stride Samples/Textures/SMO001" };
            emitter.SetTextureMaterial(
                textureRef,
                hdrMultiplier: 1.0f,
                alphaAdditive: 0.5f);

            emitter.SetFlipbookAnimation(8, 8, 0, 64);

            // Add size and rotation curves to shape builder
            var shapeBuilder = emitter.Get<Dictionary<string, object>>("ShapeBuilder");
            if (shapeBuilder != null)
            {
                // Size curve
                shapeBuilder["SamplerSize"] = new Dictionary<string, object>
                {
                    ["!ComputeCurveSamplerFloat"] = "",
                    ["Curve"] = new Dictionary<string, object>
                    {
                        ["!ComputeAnimationCurveFloat"] = "",
                        ["KeyFrames"] = new Dictionary<string, object>
                        {
                            [Utilities.GuidHelper.NewGuid()] = new Dictionary<string, object>
                            {
                                ["Key"] = 0.0f,
                                ["Value"] = 1.0f,
                                ["TangentType"] = "Linear"
                            },
                            [Utilities.GuidHelper.NewGuid()] = new Dictionary<string, object>
                            {
                                ["Key"] = 0.5f,
                                ["Value"] = 1.2f,
                                ["TangentType"] = "Linear"
                            },
                            [Utilities.GuidHelper.NewGuid()] = new Dictionary<string, object>
                            {
                                ["Key"] = 1.0f,
                                ["Value"] = 2.0f,
                                ["TangentType"] = "Linear"
                            }
                        }
                    }
                };

                // Rotation curve
                shapeBuilder["SamplerRotation"] = new Dictionary<string, object>
                {
                    ["!ComputeCurveSamplerFloat"] = "",
                    ["Curve"] = new Dictionary<string, object>
                    {
                        ["!ComputeAnimationCurveFloat"] = "",
                        ["KeyFrames"] = new Dictionary<string, object>
                        {
                            [Utilities.GuidHelper.NewGuid()] = new Dictionary<string, object>
                            {
                                ["Key"] = 0.0f,
                                ["Value"] = 0.5133011f,
                                ["TangentType"] = "Linear"
                            },
                            [Utilities.GuidHelper.NewGuid()] = new Dictionary<string, object>
                            {
                                ["Key"] = 1.0f,
                                ["Value"] = -60.0f,
                                ["TangentType"] = "Linear"
                            }
                        }
                    }
                };
            }

            emitter.SetPerSecondSpawner(10.0f, looping: true);
            emitter.AddInitialSize((0.7f, 1.5f));
            emitter.AddInitialPosition((-0.1f, 0.0f, -0.1f), (0.1f, 0.2f, 0.1f));
            emitter.AddInitialVelocity((-0.1f, 0.5f, -0.1f), (0.1f, 0.7f, 0.1f));
            emitter.AddInitialRotation((-360.0f, 360.0f));

            emitter.AddColorFade(
                (0.0f, 1.0f, 1.0f, 1.0f, 0.0f),
                (0.2f, 1.0f, 1.0f, 1.0f, 0.1f),
                (0.5f, 1.0f, 1.0f, 1.0f, 0.5f),
                (1.0f, 1.0f, 1.0f, 1.0f, 0.0f));

            emitter.AddForceField(
                fieldShape: VFXFieldShape.Cylinder,
                scale: (2.0f, 10.0f, 2.0f),
                forceDirected: -0.1f,
                forceVortex: 0.1f,
                forceRepulsive: 0.0f,
                energyConservation: 0.2f);

            vfx.AddEmitter(emitter);

            // Assert - Compare
            var originalWrapper = new ParticleSystemWrapper(originalComponent!);
            var recreatedWrapper = vfx;

            var originalEmitters = originalWrapper.GetEmitters();
            var recreatedEmitters = recreatedWrapper.GetEmitters();

            recreatedEmitters.Should().HaveCount(originalEmitters.Count);

            var origEmitter = originalEmitters[0];
            var recEmitter = recreatedEmitters[0];

            recEmitter.EmitterName.Should().Be(origEmitter.EmitterName);
            recEmitter.ParticleLifetime.Should().Be(origEmitter.ParticleLifetime);

            // Check updaters (ColorOverTime + ForceField)
            var origUpdaters = origEmitter.Get<Dictionary<string, object>>("Updaters");
            var recUpdaters = recEmitter.Get<Dictionary<string, object>>("Updaters");

            origUpdaters.Should().NotBeNull();
            recUpdaters.Should().NotBeNull();
            recUpdaters!.Should().HaveCount(origUpdaters!.Count, "Should have same number of updaters (ColorOverTime + ForceField)");

            TestContext.WriteLine("✅ Smoke VFX recreated successfully!");
            TestContext.WriteLine($"Updaters: {recUpdaters.Count} (ColorOverTime + ForceField)");
        }

        [Test]
        public void RoundTrip_LoadBullettrailVFX_RecreateInCode_VerifyRibbon()
        {
            // Bullettrail uses Ribbon shape - different from billboards

            // Arrange - Load original
            var originalPrefabPath = Path.Combine(_vfxPrefabsPath, "vfx-Bullettrail.sdprefab");
            var originalPrefab = Prefab.Load(originalPrefabPath);
            var originalEntity = originalPrefab.GetRootEntity();
            var originalComponent = originalEntity!.GetComponent("ParticleSystemComponent");
            originalComponent.Should().NotBeNull();

            // Act - Recreate
            var recreatedPrefab = Prefab.Create("Bullettrail_Recreated");
            var recreatedEntity = recreatedPrefab.GetRootEntity();
            var vfx = recreatedEntity!.AddParticleSystem();

            vfx.ResetSeconds = 3.0f;
            vfx.Control = "Play";
            vfx.Speed = 1.0f;

            // Create ribbon emitter
            var emitter = vfx.CreateRibbonEmitter(
                "trail",
                (1.0f, 1.0f),
                segments: 15,
                smoothingPolicy: VFXSmoothingPolicy.Best,
                maxParticles: 50);

            var textureRef = new AssetReference { Id = "2c8e60e4-e070-4124-a1fa-f36cc74de76c", Path = "Stride Samples/Textures/Bullettrail01" };
            emitter.SetTextureMaterial(
                textureRef,
                hdrMultiplier: 5.0f,
                alphaAdditive: 0.5f);

            emitter.SetPerSecondSpawner(50.0f, looping: true);
            emitter.AddSpawnOrder(); // Critical for ribbons!
            emitter.AddInitialVelocity((-0.1f, 0.4f, -0.1f), (0.1f, 0.4f, 0.1f));

            emitter.AddSizeOverTime(
                (0.0f, 0.1f),
                (0.9f, 0.0f));

            emitter.AddColorFade(
                (0.0f, 1.0f, 1.0f, 1.0f, 0.5f),
                (1.0f, 1.0f, 1.0f, 1.0f, 0.0f));

            vfx.AddEmitter(emitter);

            // Assert - Compare
            var originalWrapper = new ParticleSystemWrapper(originalComponent!);
            var recreatedWrapper = vfx;

            var originalEmitters = originalWrapper.GetEmitters();
            var recreatedEmitters = recreatedWrapper.GetEmitters();

            recreatedEmitters.Should().HaveCount(originalEmitters.Count);

            var origEmitter = originalEmitters[0];
            var recEmitter = recreatedEmitters[0];

            // Verify ribbon-specific properties
            recEmitter.SortingPolicy.Should().Be(origEmitter.SortingPolicy, "Ribbons require SortingPolicy: ByOrder");
            recEmitter.MaxParticlesOverride.Should().Be(origEmitter.MaxParticlesOverride);

            // Verify shape is ribbon
            var shapeBuilder = recEmitter.Get<Dictionary<string, object>>("ShapeBuilder");
            shapeBuilder.Should().NotBeNull();
            shapeBuilder.Should().ContainKey("!ShapeBuilderRibbon");

            // Verify spawn order initializer exists (required for ribbons)
            var initializers = recEmitter.Get<Dictionary<string, object>>("Initializers");
            initializers.Should().NotBeNull();

            var hasSpawnOrder = initializers!.Values.Any(v =>
                v is Dictionary<string, object> dict && dict.ContainsKey("!InitialSpawnOrder"));
            hasSpawnOrder.Should().BeTrue("Ribbons require InitialSpawnOrder initializer");

            TestContext.WriteLine("✅ Bullettrail (Ribbon) VFX recreated successfully!");
            TestContext.WriteLine($"Shape: Ribbon, SortingPolicy: {recEmitter.SortingPolicy}");
        }

        [Test]
        public void CompareYAML_OriginalVsRecreated_ShouldBeSimilar()
        {
            // This test verifies the YAML structure is equivalent (ignoring GUIDs)

            // Arrange - Load and recreate Click VFX
            var originalPrefabPath = Path.Combine(_vfxPrefabsPath, "vfx-Click.sdprefab");
            var originalPrefab = Prefab.Load(originalPrefabPath);

            var recreatedPrefab = Prefab.Create("Click_Recreated");
            var entity = recreatedPrefab.GetRootEntity();
            var vfx = entity!.AddParticleSystem();

            vfx.ResetSeconds = 5.0f;
            // Click VFX uses Quad shape, not Billboard
            var emitter = new VFXEmitter
            {
                ParticleLifetime = (1.0f, 1.0f),
                MaxParticlesOverride = 1
            };
            emitter.SetShapeBuilder(VFXShapeType.Quad, new Dictionary<string, object>
            {
                ["SamplerPosition"] = "null",
                ["SamplerRotation"] = "null"
            });
            var circleTexture = new AssetReference { Id = "8223c28b-5cb8-402a-b51f-aa6c239dbb9f", Path = "Stride Samples/Textures/circle02" };
            emitter.SetTextureMaterial(circleTexture, 10.0f, 1.0f);
            emitter.SetBurstSpawner(1, oneShot: true);
            emitter.AddInitialPosition((0.0f, 0.1f, 0.0f), (0.0f, 0.1f, 0.0f));
            vfx.AddEmitter(emitter);

            // Act - Save to temp files
            var tempOriginalPath = Path.Combine(Path.GetTempPath(), $"original-{Guid.NewGuid()}.sdprefab");
            var tempRecreatedPath = Path.Combine(Path.GetTempPath(), $"recreated-{Guid.NewGuid()}.sdprefab");

            try
            {
                originalPrefab.SaveAs(tempOriginalPath);
                recreatedPrefab.SaveAs(tempRecreatedPath);

                // Read YAML content
                var originalYaml = File.ReadAllText(tempOriginalPath);
                var recreatedYaml = File.ReadAllText(tempRecreatedPath);

                // Assert - Basic structure checks
                originalYaml.Should().Contain("!PrefabAsset");
                recreatedYaml.Should().Contain("!PrefabAsset");

                originalYaml.Should().Contain("!ParticleSystemComponent");
                recreatedYaml.Should().Contain("!ParticleSystemComponent");

                originalYaml.Should().Contain("!ShapeBuilderQuad");
                recreatedYaml.Should().Contain("!ShapeBuilderQuad");

                originalYaml.Should().Contain("!SpawnerBurst");
                recreatedYaml.Should().Contain("!SpawnerBurst");

                originalYaml.Should().Contain("!InitialPositionSeed");
                recreatedYaml.Should().Contain("!InitialPositionSeed");

                // Write to test output for manual inspection
                TestContext.WriteLine("=== ORIGINAL YAML (first 500 chars) ===");
                TestContext.WriteLine(originalYaml.Substring(0, Math.Min(500, originalYaml.Length)));
                TestContext.WriteLine("\n=== RECREATED YAML (first 500 chars) ===");
                TestContext.WriteLine(recreatedYaml.Substring(0, Math.Min(500, recreatedYaml.Length)));

                TestContext.WriteLine("\n✅ YAML structure comparison passed!");
            }
            finally
            {
                if (File.Exists(tempOriginalPath)) File.Delete(tempOriginalPath);
                if (File.Exists(tempRecreatedPath)) File.Delete(tempRecreatedPath);
            }
        }

        #endregion

        #region Live Comparison Tests

        [Test]
        public void RecreateExplosionAndMuzzleFlash_InTestScene_ForComparison()
        {
            // This test recreates Explosion and MuzzleFlash VFX in the test scene for side-by-side comparison

            // Arrange - Load the test scene
            var testScenePath = Path.Combine(_vfxPrefabsPath, "vfx_test_scene.sdscene");
            var scene = Scene.Load(testScenePath);

            // Act 1 - Recreate Explosion VFX
            var explosionEntity = scene.CreateEntity("Explosion_Recreated");
            var explosionVFX = explosionEntity.AddParticleSystem();

            explosionVFX.ResetSeconds = 3.0f;
            explosionVFX.Control = "Play";
            explosionVFX.Speed = 1.0f;
            explosionVFX.SetColor(1.0f, 1.0f, 1.0f, 1.0f);

            // Emitter 1: explosionfire
            var explosionFire = explosionVFX.CreateBillboardEmitter("explosionfire", (1.0f, 1.3f));
            explosionFire.DrawPriority = 5;
            var fireTexture = new AssetReference { Id = "24a1fa27-5ec5-4eb6-bf4d-918005b0fb24", Path = "Stride Samples/Textures/EXP001" };
            explosionFire.SetTextureMaterial(fireTexture, 50.0f);
            explosionFire.SetFlipbookAnimation(8, 8, 8, 56);
            explosionFire.SetBurstSpawner(10, oneShot: true);
            explosionFire.AddInitialSize((1.0f, 2.0f));
            explosionFire.AddInitialPosition((-0.2f, 0.0f, -0.2f), (0.2f, 0.0f, 0.2f));
            explosionFire.AddInitialVelocity((-0.5f, -0.2f, -0.5f), (0.5f, 0.5f, 0.5f));
            explosionFire.AddInitialRotation((-360.0f, 360.0f));
            explosionFire.AddColorFade(
                (0.0f, 1.0f, 1.0f, 1.0f, 1.0f),
                (0.5f, 0.3f, 0.3f, 0.3f, 0.25f),
                (1.0f, 0.0f, 0.0f, 0.0f, 0.0f));
            explosionVFX.AddEmitter(explosionFire);

            // Emitter 2: explosionsmoke01 (has size curve in ShapeBuilder)
            var smoke01 = explosionVFX.CreateBillboardEmitter("explosionsmoke01", (2.0f, 2.0f));
            smoke01.DrawPriority = 10;
            var smoke02Texture = new AssetReference { Id = "8ab2edfe-0ead-489f-a899-97a8112807f7", Path = "Stride Samples/Textures/Smoke02" };
            smoke01.SetTextureMaterial(smoke02Texture, hdrR: 30.0f, hdrG: 25.0f, hdrB: 20.0f);

            // Add size curve to ShapeBuilder
            var smoke01Shape = smoke01.Get<Dictionary<string, object>>("ShapeBuilder");
            if (smoke01Shape != null)
            {
                smoke01Shape["SamplerSize"] = new Dictionary<string, object>
                {
                    ["!ComputeCurveSamplerFloat"] = "",
                    ["Curve"] = new Dictionary<string, object>
                    {
                        ["!ComputeAnimationCurveFloat"] = "",
                        ["KeyFrames"] = new Dictionary<string, object>
                        {
                            [Utilities.GuidHelper.NewGuid()] = new Dictionary<string, object>
                            {
                                ["Key"] = 0.0f,
                                ["Value"] = 1.0f,
                                ["TangentType"] = "Linear"
                            },
                            [Utilities.GuidHelper.NewGuid()] = new Dictionary<string, object>
                            {
                                ["Key"] = 0.1f,
                                ["Value"] = 2.0f,
                                ["TangentType"] = "Linear"
                            },
                            [Utilities.GuidHelper.NewGuid()] = new Dictionary<string, object>
                            {
                                ["Key"] = 1.0f,
                                ["Value"] = 3.0f,
                                ["TangentType"] = "Linear"
                            }
                        }
                    }
                };
            }

            smoke01.SetBurstSpawner(5, oneShot: true);
            smoke01.AddInitialPosition((-0.15f, -0.15f, -0.15f), (0.15f, 0.15f, 0.15f));
            smoke01.AddInitialVelocity((-0.5f, -0.5f, -0.5f), (0.5f, 0.5f, 0.5f));
            smoke01.AddColorFade(
                (0.0f, 1.0f, 0.5f, 0.2f, 1.0f),
                (0.08f, 1.0f, 0.5f, 0.2f, 0.01f),
                (1.0f, 0.5f, 0.2f, 0.1f, 0.0f));
            explosionVFX.AddEmitter(smoke01);

            // Emitter 3: explosionsmoke02
            var smoke02 = explosionVFX.CreateBillboardEmitter("explosionsmoke02", (1.0f, 2.0f));
            var smo001Texture = new AssetReference { Id = "fa1018e4-c74d-4495-9535-134f5a06e4cd", Path = "Stride Samples/Textures/SMO001" };
            smoke02.SetTextureMaterial(smo001Texture, 1.0f);
            smoke02.SetFlipbookAnimation(8, 8, 0, 64);

            // Add size curve
            var smoke02Shape = smoke02.Get<Dictionary<string, object>>("ShapeBuilder");
            if (smoke02Shape != null)
            {
                smoke02Shape["SamplerSize"] = new Dictionary<string, object>
                {
                    ["!ComputeCurveSamplerFloat"] = "",
                    ["Curve"] = new Dictionary<string, object>
                    {
                        ["!ComputeAnimationCurveFloat"] = "",
                        ["KeyFrames"] = new Dictionary<string, object>
                        {
                            [Utilities.GuidHelper.NewGuid()] = new Dictionary<string, object>
                            {
                                ["Key"] = 0.0f,
                                ["Value"] = 1.0f,
                                ["TangentType"] = "Linear"
                            },
                            [Utilities.GuidHelper.NewGuid()] = new Dictionary<string, object>
                            {
                                ["Key"] = 1.0f,
                                ["Value"] = 2.0f,
                                ["TangentType"] = "Linear"
                            }
                        }
                    }
                };
            }

            smoke02.SetBurstSpawner(10, oneShot: true);
            smoke02.AddInitialVelocity((-0.4f, -0.4f, -0.4f), (0.4f, 0.4f, 0.4f));
            smoke02.AddInitialRotation((-360.0f, 360.0f));
            smoke02.AddInitialPosition((-0.3f, -0.3f, -0.3f), (0.3f, 0.3f, 0.3f));
            smoke02.AddColorFade(
                (0.0f, 0.5f, 0.25f, 0.1f, 1.0f),
                (0.5f, 0.5f, 0.2f, 0.1f, 0.7f),
                (1.0f, 0.1f, 0.1f, 0.1f, 0.0f));
            explosionVFX.AddEmitter(smoke02);

            // Emitter 4: sparks
            var sparks = explosionVFX.CreateBillboardEmitter("sparks", (0.7f, 1.4f));
            var dotaTexture = new AssetReference { Id = "91119660-137e-4be0-bcb9-32f8aeee6ca8", Path = "Stride Samples/Textures/dota" };
            sparks.SetTextureMaterial(dotaTexture, hdrR: 50.0f, hdrG: 20.0f, hdrB: 20.0f);

            // Add size curve
            var sparksShape = sparks.Get<Dictionary<string, object>>("ShapeBuilder");
            if (sparksShape != null)
            {
                sparksShape["SamplerSize"] = new Dictionary<string, object>
                {
                    ["!ComputeCurveSamplerFloat"] = "",
                    ["Curve"] = new Dictionary<string, object>
                    {
                        ["!ComputeAnimationCurveFloat"] = "",
                        ["KeyFrames"] = new Dictionary<string, object>
                        {
                            [Utilities.GuidHelper.NewGuid()] = new Dictionary<string, object>
                            {
                                ["Key"] = 0.0f,
                                ["Value"] = 1.0f,
                                ["TangentType"] = "Linear"
                            },
                            [Utilities.GuidHelper.NewGuid()] = new Dictionary<string, object>
                            {
                                ["Key"] = 1.0f,
                                ["Value"] = 0.1f,
                                ["TangentType"] = "Linear"
                            }
                        }
                    }
                };
            }

            sparks.SetBurstSpawner(50, oneShot: true, delay: (0.5f, 0.5f));
            sparks.AddInitialVelocity((-0.5f, -0.5f, -0.5f), (0.5f, 0.5f, 0.5f));
            sparks.AddInitialSize((0.1f, 0.2f), scaleUniform: 0.1f);
            sparks.AddInitialPosition((-0.6f, -0.6f, -0.6f), (0.6f, 0.6f, 0.6f));
            sparks.AddInitialColor((1.0f, 0.18187499f, 0.037500024f, 1.0f), (1.0f, 0.3f, 0.0f, 1.0f));
            sparks.AddForceField(
                fieldShape: VFXFieldShape.Sphere,
                scale: (2.0f, 2.0f, 2.0f),
                forceDirected: 0.3f,
                forceVortex: 0.5f,
                forceRepulsive: -0.3f,
                energyConservation: 0.5f);
            explosionVFX.AddEmitter(sparks);

            // Emitter 5: explosionsmoke03 (has both size and rotation curves)
            var smoke03 = explosionVFX.CreateBillboardEmitter("explosionsmoke03", (1.0f, 1.5f));
            var smo001_3Texture = new AssetReference { Id = "8dbe2382-769c-45fe-9604-98c5ae5c434c", Path = "Stride Samples/Textures/SMO001_3" };
            smoke03.SetTextureMaterial(smo001_3Texture, hdrR: 50.0f, hdrG: 11.0f, hdrB: 0.5f, alphaAdditive: 1.0f);

            // Add size and rotation curves
            var smoke03Shape = smoke03.Get<Dictionary<string, object>>("ShapeBuilder");
            if (smoke03Shape != null)
            {
                smoke03Shape["SamplerSize"] = new Dictionary<string, object>
                {
                    ["!ComputeCurveSamplerFloat"] = "",
                    ["Curve"] = new Dictionary<string, object>
                    {
                        ["!ComputeAnimationCurveFloat"] = "",
                        ["KeyFrames"] = new Dictionary<string, object>
                        {
                            [Utilities.GuidHelper.NewGuid()] = new Dictionary<string, object>
                            {
                                ["Key"] = 0.0f,
                                ["Value"] = 1.0f,
                                ["TangentType"] = "Linear"
                            },
                            [Utilities.GuidHelper.NewGuid()] = new Dictionary<string, object>
                            {
                                ["Key"] = 1.0f,
                                ["Value"] = 2.0f,
                                ["TangentType"] = "Linear"
                            }
                        }
                    }
                };

                smoke03Shape["SamplerRotation"] = new Dictionary<string, object>
                {
                    ["!ComputeCurveSamplerFloat"] = "",
                    ["Curve"] = new Dictionary<string, object>
                    {
                        ["!ComputeAnimationCurveFloat"] = "",
                        ["KeyFrames"] = new Dictionary<string, object>
                        {
                            [Utilities.GuidHelper.NewGuid()] = new Dictionary<string, object>
                            {
                                ["Key"] = 0.0f,
                                ["Value"] = 0.0f,
                                ["TangentType"] = "Linear"
                            },
                            [Utilities.GuidHelper.NewGuid()] = new Dictionary<string, object>
                            {
                                ["Key"] = 1.0f,
                                ["Value"] = 30.0f,
                                ["TangentType"] = "Linear"
                            }
                        }
                    }
                };
            }

            smoke03.SetBurstSpawner(5, oneShot: true);
            smoke03.AddInitialRotation((-360.0f, 360.0f));
            smoke03.AddInitialPosition((-0.2f, -0.2f, -0.2f), (0.2f, 0.2f, 0.2f));
            smoke03.AddInitialVelocity((-0.2f, -0.2f, -0.2f), (0.2f, 0.2f, 0.2f));
            smoke03.AddColorFade(
                (0.0f, 1.0f, 1.0f, 1.0f, 0.0f),
                (0.5f, 1.0f, 1.0f, 1.0f, 0.03f),
                (1.0f, 1.0f, 1.0f, 1.0f, 0.0f));
            explosionVFX.AddEmitter(smoke03);

            // Act 2 - Recreate MuzzleFlash VFX
            var muzzleFlashEntity = scene.CreateEntity("MuzzleFlash_Recreated");
            var muzzleFlashVFX = muzzleFlashEntity.AddParticleSystem();

            muzzleFlashVFX.ResetSeconds = 1.0f;
            muzzleFlashVFX.Control = "Play";
            muzzleFlashVFX.Speed = 1.0f;
            muzzleFlashVFX.SetColor(1.0f, 1.0f, 1.0f, 1.0f);

            // Emitter 1: flash (has SimulationSpace: Local, SortingPolicy: ByDepth)
            var flash = muzzleFlashVFX.CreateBillboardEmitter("flash", (0.05f, 0.05f));
            flash.MaxParticlesOverride = 10;
            flash.DrawPriority = 5;
            flash.SimulationSpace = "Local";
            flash.SortingPolicy = "ByDepth";

            var hit01Texture = new AssetReference { Id = "712fda70-50dc-47b2-9d41-1f4011945cac", Path = "Stride Samples/Textures/Hit01" };
            flash.SetTextureMaterial(hit01Texture, 20.0f, alphaAdditive: 1.0f);

            // Add size curve
            var flashShape = flash.Get<Dictionary<string, object>>("ShapeBuilder");
            if (flashShape != null)
            {
                flashShape["SamplerSize"] = new Dictionary<string, object>
                {
                    ["!ComputeCurveSamplerFloat"] = "",
                    ["Curve"] = new Dictionary<string, object>
                    {
                        ["!ComputeAnimationCurveFloat"] = "",
                        ["KeyFrames"] = new Dictionary<string, object>
                        {
                            [Utilities.GuidHelper.NewGuid()] = new Dictionary<string, object>
                            {
                                ["Key"] = 0.0f,
                                ["Value"] = 1.0f,
                                ["TangentType"] = "Linear"
                            },
                            [Utilities.GuidHelper.NewGuid()] = new Dictionary<string, object>
                            {
                                ["Key"] = 1.0f,
                                ["Value"] = 2.0f,
                                ["TangentType"] = "Linear"
                            }
                        }
                    }
                };
            }

            flash.SetBurstSpawner(3, oneShot: true);
            flash.AddInitialRotation((0.0f, 360.0f));
            flash.AddInitialSize((1.0f, 1.5f), scaleUniform: 0.2f);

            // TODO: Add InitialVelocityParent initializer (not currently supported in API)

            flash.AddColorFade(
                (0.24688779f, 1.0f, 1.0f, 1.0f, 0.2f),
                (1.0f, 1.0f, 1.0f, 1.0f, 0.0f));
            muzzleFlashVFX.AddEmitter(flash);

            // Emitter 2: sparks (OrientedQuad + UpdaterSpeedToDirection)
            var muzzleSparks = new VFXEmitter
            {
                EmitterName = "sparks",
                ParticleLifetime = (0.05f, 0.2f),
                MaxParticlesOverride = 50,
                DrawPriority = 3,
                SimulationSpace = "Local",
                SortingPolicy = "ByDepth"
            };

            // Set OrientedQuad shape
            muzzleSparks.SetShapeBuilder(VFXShapeType.OrientedQuad, new Dictionary<string, object>
            {
                ["SamplerPosition"] = "null",
                ["ScaleLength"] = false,
                ["LengthFactor"] = 0.05f,
                ["SamplerSize"] = new Dictionary<string, object>
                {
                    ["!ComputeCurveSamplerFloat"] = "",
                    ["Curve"] = new Dictionary<string, object>
                    {
                        ["!ComputeAnimationCurveFloat"] = "",
                        ["KeyFrames"] = new Dictionary<string, object>
                        {
                            [Utilities.GuidHelper.NewGuid()] = new Dictionary<string, object>
                            {
                                ["Key"] = 0.0f,
                                ["Value"] = 1.0f,
                                ["TangentType"] = "Linear"
                            },
                            [Utilities.GuidHelper.NewGuid()] = new Dictionary<string, object>
                            {
                                ["Key"] = 1.0f,
                                ["Value"] = 0.0f,
                                ["TangentType"] = "Linear"
                            }
                        }
                    }
                }
            });

            muzzleSparks.SetTextureMaterial(dotaTexture, 5.0f);
            muzzleSparks.SetPerSecondSpawner(500.0f, looping: false, delay: (0.05f, 0.05f), duration: (0.1f, 0.1f));
            muzzleSparks.AddInitialSize((0.5f, 1.0f), scaleUniform: 0.03f);
            muzzleSparks.AddInitialVelocity((-1.0f, 3.0f, -1.0f), (1.0f, 7.0f, 1.0f));
            muzzleSparks.AddInitialColor((1.0f, 0.8329202f, 0.41875f, 1.0f), (0.9921568f, 0.29764706f, 0.0f, 1.0f));

            // Add UpdaterSpeedToDirection
            muzzleSparks.AddUpdater("UpdaterSpeedToDirection", new Dictionary<string, object>
            {
                ["!UpdaterSpeedToDirection"] = "",
                ["InheritPosition"] = true,
                ["Position"] = new Dictionary<string, object> { ["X"] = 0.0f, ["Y"] = 0.0f, ["Z"] = 0.0f },
                ["InheritRotation"] = true,
                ["Rotation"] = new Dictionary<string, object> { ["X"] = 0.0f, ["Y"] = 0.0f, ["Z"] = 0.0f, ["W"] = 1.0f },
                ["InheritScale"] = true,
                ["Scale"] = new Dictionary<string, object> { ["X"] = 1.0f, ["Y"] = 1.0f, ["Z"] = 1.0f },
                ["ScaleUniform"] = 1.0f,
                ["DisplayParticlePosition"] = false,
                ["DisplayParticleRotation"] = false,
                ["DisplayParticleScale"] = false,
                ["DisplayParticleScaleUniform"] = false
            });

            muzzleFlashVFX.AddEmitter(muzzleSparks);

            // Emitter 3: smoke (has UpdaterGravity)
            var muzzleSmoke = muzzleFlashVFX.CreateBillboardEmitter("smoke", (0.8f, 1.2f));
            muzzleSmoke.SetTextureMaterial(smo001_3Texture, 1.0f);

            // Add size curve
            var muzzleSmokeShape = muzzleSmoke.Get<Dictionary<string, object>>("ShapeBuilder");
            if (muzzleSmokeShape != null)
            {
                muzzleSmokeShape["SamplerSize"] = new Dictionary<string, object>
                {
                    ["!ComputeCurveSamplerFloat"] = "",
                    ["Curve"] = new Dictionary<string, object>
                    {
                        ["!ComputeAnimationCurveFloat"] = "",
                        ["KeyFrames"] = new Dictionary<string, object>
                        {
                            [Utilities.GuidHelper.NewGuid()] = new Dictionary<string, object>
                            {
                                ["Key"] = 0.0f,
                                ["Value"] = 0.5f,
                                ["TangentType"] = "Linear"
                            },
                            [Utilities.GuidHelper.NewGuid()] = new Dictionary<string, object>
                            {
                                ["Key"] = 1.0f,
                                ["Value"] = 1.0f,
                                ["TangentType"] = "Linear"
                            }
                        }
                    }
                };
            }

            muzzleSmoke.SetPerSecondSpawner(200.0f, looping: false, delay: (0.0f, 0.0f), duration: (0.1f, 0.15f));
            muzzleSmoke.AddInitialRotation((-360.0f, 360.0f));
            muzzleSmoke.AddInitialPosition((-0.1f, 0.0f, -0.1f), (0.1f, 0.01f, 0.1f));
            muzzleSmoke.AddInitialSize((1.4f, 2.0f), scaleUniform: 0.5f);

            // Add velocity with InheritRotation: false
            var velocityInit = new Dictionary<string, object>
            {
                ["!InitialVelocitySeed"] = "",
                ["InheritPosition"] = true,
                ["Position"] = new Dictionary<string, object> { ["X"] = 0.0f, ["Y"] = 0.0f, ["Z"] = 0.0f },
                ["InheritRotation"] = false,  // Key difference!
                ["Rotation"] = new Dictionary<string, object> { ["X"] = 0.0f, ["Y"] = 0.0f, ["Z"] = 0.0f, ["W"] = 1.0f },
                ["InheritScale"] = true,
                ["Scale"] = new Dictionary<string, object> { ["X"] = 1.0f, ["Y"] = 1.0f, ["Z"] = 1.0f },
                ["ScaleUniform"] = 1.0f,
                ["SeedOffset"] = 0,
                ["DisplayParticlePosition"] = false,
                ["DisplayParticleRotation"] = true,
                ["DisplayParticleScale"] = false,
                ["DisplayParticleScaleUniform"] = true,
                ["VelocityMin"] = new Dictionary<string, object> { ["X"] = -0.15f, ["Y"] = 0.4f, ["Z"] = -0.15f },
                ["VelocityMax"] = new Dictionary<string, object> { ["X"] = 0.15f, ["Y"] = 1.0f, ["Z"] = 0.15f }
            };

            var initializers = muzzleSmoke.Get<Dictionary<string, object>>("Initializers");
            if (initializers != null)
            {
                initializers[Utilities.GuidHelper.NewGuid()] = velocityInit;
            }

            muzzleSmoke.AddColorFade(
                (0.0f, 1.5f, 0.5f, 0.5f, 0.0f),
                (0.1f, 10.0f, 8.0f, 4.0f, 0.02f),
                (0.3f, 1.0f, 0.8f, 0.5f, 0.1f),
                (1.0f, 0.0f, 0.0f, 0.0f, 0.0f));

            // Add UpdaterGravity
            muzzleSmoke.AddUpdater("UpdaterGravity", new Dictionary<string, object>
            {
                ["!UpdaterGravity"] = "",
                ["InheritPosition"] = true,
                ["Position"] = new Dictionary<string, object> { ["X"] = 0.0f, ["Y"] = 0.0f, ["Z"] = 0.0f },
                ["InheritRotation"] = true,
                ["Rotation"] = new Dictionary<string, object> { ["X"] = 0.0f, ["Y"] = 0.0f, ["Z"] = 0.0f, ["W"] = 1.0f },
                ["InheritScale"] = true,
                ["Scale"] = new Dictionary<string, object> { ["X"] = 1.0f, ["Y"] = 1.0f, ["Z"] = 1.0f },
                ["ScaleUniform"] = 1.0f,
                ["GravitationalAcceleration"] = new Dictionary<string, object> { ["X"] = 0.0f, ["Y"] = 1.0f, ["Z"] = 0.0f },
                ["DisplayParticlePosition"] = false,
                ["DisplayParticleRotation"] = false,
                ["DisplayParticleScale"] = false,
                ["DisplayParticleScaleUniform"] = false
            });

            muzzleFlashVFX.AddEmitter(muzzleSmoke);

            // Save the scene
            scene.Save();

            // Assert
            TestContext.WriteLine("✅ Created Explosion_Recreated and MuzzleFlash_Recreated in test scene");
            TestContext.WriteLine("Compare the YAML in vfx_test_scene.sdscene to find differences");
        }

        #endregion

        #region Regression Tests

        [Test]
        public void VFXMaterial_ShouldSerializeColorVertexStreamDefinitionCorrectly()
        {
            // This test prevents the bug where ColorVertexStreamDefinition was missing {}
            // Issue: Stride expects "Stream: !ColorVertexStreamDefinition {}" but we were generating without {}

            // Arrange - Use existing test scene
            var scene = Scene.Load(_testScenePath);
            var entity = scene.CreateEntity("TestVFX_ColorVertex");
            var vfx = entity.AddParticleSystem();

            // Act - Create a simple emitter with texture material
            var emitter = vfx.CreateBillboardEmitter("test", (1.0f, 1.0f));
            var texture = new AssetReference { Id = "fa1018e4-c74d-4495-9535-134f5a06e4cd", Path = "Textures/SMO001" };
            emitter.SetTextureMaterial(texture, 1.0f);
            vfx.AddEmitter(emitter);
            scene.Save();

            // Assert - Verify the YAML contains the correct format
            var yaml = File.ReadAllText(_testScenePath);

            // Should contain the properly formatted ColorVertexStreamDefinition
            Assert.That(yaml, Does.Contain("Stream: !ColorVertexStreamDefinition {}"),
                "ColorVertexStreamDefinition must be serialized with {} for Stride to parse it correctly");

            // Should NOT contain incomplete format
            Assert.That(yaml, Does.Not.Contain("Stream: !ColorVertexStreamDefinition\n"),
                "ColorVertexStreamDefinition without {} will cause Stride parsing errors");

            // Cleanup - remove the test entity
            scene = Scene.Load(_testScenePath);
            var testEntity = scene.FindEntityByName("TestVFX_ColorVertex");
            if (testEntity != null)
            {
                scene.RemoveEntity(testEntity);
                scene.Save();
            }
        }

        [Test]
        public void VFXKeyframes_ShouldHaveUniqueGUIDs()
        {
            // This test prevents duplicate GUID bugs in animation curves
            // Issue: Stride throws "An item with the same key has already been added"

            // Arrange
            var scene = Scene.Load(_testScenePath);
            var entity = scene.CreateEntity("TestVFX_Keyframes");
            var vfx = entity.AddParticleSystem();
            var emitter = vfx.CreateBillboardEmitter("test", (1.0f, 1.0f));

            // Act - Add color fade with multiple keyframes
            emitter.AddColorFade(
                (0.0f, 1.0f, 1.0f, 1.0f, 0.0f),
                (0.2f, 1.0f, 1.0f, 1.0f, 0.5f),
                (0.5f, 1.0f, 1.0f, 1.0f, 1.0f),
                (1.0f, 1.0f, 1.0f, 1.0f, 0.0f));

            vfx.AddEmitter(emitter);
            scene.Save();

            // Assert - Check YAML for duplicate GUIDs
            var yaml = File.ReadAllText(_testScenePath);

            // Extract all GUID keys from KeyFrames section
            var keyframeSection = yaml.Substring(yaml.IndexOf("KeyFrames:"));
            var guidMatches = System.Text.RegularExpressions.Regex.Matches(
                keyframeSection,
                @"([a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}):");

            var guids = new HashSet<string>();
            var duplicates = new List<string>();

            foreach (System.Text.RegularExpressions.Match match in guidMatches)
            {
                var guid = match.Groups[1].Value;
                if (!guids.Add(guid))
                {
                    duplicates.Add(guid);
                }
            }

            Assert.That(duplicates, Is.Empty,
                $"Found duplicate GUIDs in KeyFrames: {string.Join(", ", duplicates)}. This causes Stride deserialization errors.");

            // Also verify no all-zero GUIDs
            Assert.That(guids, Does.Not.Contain("00000000-0000-0000-0000-000000000000"),
                "All-zero GUID found - this causes 'duplicate key' errors in Stride");

            // Cleanup
            scene = Scene.Load(_testScenePath);
            var testEntity = scene.FindEntityByName("TestVFX_Keyframes");
            if (testEntity != null)
            {
                scene.RemoveEntity(testEntity);
                scene.Save();
            }
        }

        [Test]
        public void VFXInitializers_ShouldHaveUniqueGUIDs()
        {
            // This test prevents duplicate GUID bugs in initializers
            // Issue: Multiple initializers were getting the same GUID

            // Arrange
            var scene = Scene.Load(_testScenePath);
            var entity = scene.CreateEntity("TestVFX_Initializers");
            var vfx = entity.AddParticleSystem();
            var emitter = vfx.CreateBillboardEmitter("test", (1.0f, 1.0f));

            // Act - Add multiple initializers
            emitter.AddInitialSize((0.5f, 1.0f));
            emitter.AddInitialPosition((-1.0f, -1.0f, -1.0f), (1.0f, 1.0f, 1.0f));
            emitter.AddInitialVelocity((-0.5f, 0.0f, -0.5f), (0.5f, 1.0f, 0.5f));
            emitter.AddInitialRotation((-180.0f, 180.0f));
            emitter.AddInitialColor((1.0f, 1.0f, 1.0f, 0.5f), (1.0f, 1.0f, 1.0f, 1.0f));

            vfx.AddEmitter(emitter);
            scene.Save();

            // Assert - Check YAML for duplicate GUIDs in Initializers
            var yaml = File.ReadAllText(_testScenePath);

            // Extract Initializers section
            var initializersStart = yaml.IndexOf("Initializers:");
            if (initializersStart == -1)
            {
                Assert.Fail("No Initializers section found in YAML");
                return;
            }

            var initializersEnd = yaml.IndexOf("Updaters:", initializersStart);
            if (initializersEnd == -1) initializersEnd = yaml.Length;

            var initializersSection = yaml.Substring(initializersStart, initializersEnd - initializersStart);

            // Extract all GUID keys
            var guidMatches = System.Text.RegularExpressions.Regex.Matches(
                initializersSection,
                @"([a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}):");

            var guids = new HashSet<string>();
            var duplicates = new List<string>();

            foreach (System.Text.RegularExpressions.Match match in guidMatches)
            {
                var guid = match.Groups[1].Value;
                if (!guids.Add(guid))
                {
                    duplicates.Add(guid);
                }
            }

            Assert.That(duplicates, Is.Empty,
                $"Found duplicate GUIDs in Initializers: {string.Join(", ", duplicates)}");

            Assert.That(guids, Does.Not.Contain("00000000-0000-0000-0000-000000000000"),
                "All-zero GUID found in Initializers");

            // Verify we have the expected number of initializers (5)
            Assert.That(guids.Count, Is.EqualTo(5),
                "Should have 5 unique GUIDs for 5 initializers");

            // Cleanup
            scene = Scene.Load(_testScenePath);
            var testEntity = scene.FindEntityByName("TestVFX_Initializers");
            if (testEntity != null)
            {
                scene.RemoveEntity(testEntity);
                scene.Save();
            }
        }

        #endregion
    }
}
