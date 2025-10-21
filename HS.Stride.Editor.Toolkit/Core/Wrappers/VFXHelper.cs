// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using HS.Stride.Editor.Toolkit.Utilities;

namespace HS.Stride.Editor.Toolkit.Core.Wrappers
{
    /// <summary>
    /// Helper methods for creating and configuring VFX particle emitters with common patterns and presets.
    /// Provides fluent API for building effects like explosions, smoke, fire, sparks, trails, etc.
    /// </summary>
    public static class VFXHelper
    {
        #region Emitter Creation Helpers

        /// <summary>
        /// Creates a billboard emitter (particles always face the camera)
        /// </summary>
        public static VFXEmitter CreateBillboardEmitter(
            this ParticleSystemWrapper particleSystem,
            string? name = null,
            (float min, float max)? lifetime = null)
        {
            var emitter = new VFXEmitter
            {
                EmitterName = name,
                ParticleLifetime = lifetime ?? (1.0f, 1.0f)
            };

            // Set billboard shape
            emitter.SetShapeBuilder(VFXShapeType.Billboard, new Dictionary<string, object>
            {
                ["SamplerPosition"] = "null",
                ["SamplerSize"] = "null",
                ["SamplerRotation"] = "null"
            });

            return emitter;
        }

        /// <summary>
        /// Creates an oriented quad emitter (particles oriented by velocity)
        /// </summary>
        public static VFXEmitter CreateOrientedQuadEmitter(
            this ParticleSystemWrapper particleSystem,
            string? name = null,
            (float min, float max)? lifetime = null,
            bool scaleLength = true,
            float lengthFactor = 1.0f)
        {
            var emitter = new VFXEmitter
            {
                EmitterName = name,
                ParticleLifetime = lifetime ?? (0.5f, 1.5f)
            };

            emitter.SetShapeBuilder(VFXShapeType.OrientedQuad, new Dictionary<string, object>
            {
                ["SamplerPosition"] = "null",
                ["SamplerSize"] = "null",
                ["ScaleLength"] = scaleLength,
                ["LengthFactor"] = lengthFactor
            });

            return emitter;
        }

        /// <summary>
        /// Creates a ribbon emitter (for trails and beams)
        /// </summary>
        public static VFXEmitter CreateRibbonEmitter(
            this ParticleSystemWrapper particleSystem,
            string? name = null,
            (float min, float max)? lifetime = null,
            int segments = 15,
            VFXSmoothingPolicy smoothingPolicy = VFXSmoothingPolicy.Best,
            int maxParticles = 50)
        {
            var emitter = new VFXEmitter
            {
                EmitterName = name,
                ParticleLifetime = lifetime ?? (1.0f, 1.0f),
                MaxParticlesOverride = maxParticles,
                SortingPolicy = "ByOrder"
            };

            emitter.SetShapeBuilder(VFXShapeType.Ribbon, new Dictionary<string, object>
            {
                ["SmoothingPolicy"] = smoothingPolicy.ToString(),
                ["Segments"] = segments,
                ["TextureCoordinatePolicy"] = "Stretched",
                ["TexCoordsFactor"] = 1.0f,
                ["UVRotate"] = new Dictionary<string, object>
                {
                    ["FlipX"] = false,
                    ["FlipY"] = false,
                    ["UVClockwise"] = "Degree0"
                }
            });

            return emitter;
        }

        #endregion

        #region Material Helpers

        /// <summary>
        /// Sets a simple texture material with optional HDR multiplier and additive blending
        /// </summary>
        public static VFXEmitter SetTextureMaterial(
            this VFXEmitter emitter,
            AssetEditing.AssetReference textureAsset,
            float hdrMultiplier = 1.0f,
            float alphaAdditive = 0.0f)
        {
            return SetTextureMaterial(emitter, textureAsset.Reference, hdrMultiplier, alphaAdditive);
        }

        /// <summary>
        /// Sets a simple texture material using texture reference string
        /// </summary>
        internal static VFXEmitter SetTextureMaterial(
            this VFXEmitter emitter,
            string textureReference,
            float hdrMultiplier = 1.0f,
            float alphaAdditive = 0.0f)
        {
            var material = new Dictionary<string, object>
            {
                ["!ParticleMaterialComputeColor"] = "",
                ["AlphaAdditive"] = alphaAdditive,
                ["ComputeColor"] = new Dictionary<string, object>
                {
                    ["!ComputeBinaryColor"] = "",
                    ["Operator"] = "Multiply",
                    ["LeftChild"] = new Dictionary<string, object>
                    {
                        ["!ComputeFloat4"] = "",
                        ["Value"] = new Dictionary<string, object>
                        {
                            ["X"] = hdrMultiplier,
                            ["Y"] = hdrMultiplier,
                            ["Z"] = hdrMultiplier,
                            ["W"] = 1.0f
                        }
                    },
                    ["RightChild"] = new Dictionary<string, object>
                    {
                        ["!ComputeBinaryColor"] = "",
                        ["Operator"] = "Multiply",
                        ["LeftChild"] = new Dictionary<string, object>
                        {
                            ["!ComputeTextureColor"] = "",
                            ["Texture"] = textureReference,
                            ["FallbackValue"] = new Dictionary<string, object>
                            {
                                ["Value"] = new Dictionary<string, object>
                                {
                                    ["R"] = 1.0f,
                                    ["G"] = 1.0f,
                                    ["B"] = 1.0f,
                                    ["A"] = 1.0f
                                }
                            },
                            ["Scale"] = new Dictionary<string, object> { ["X"] = 1.0f, ["Y"] = 1.0f },
                            ["Offset"] = new Dictionary<string, object> { ["X"] = 0.0f, ["Y"] = 0.0f },
                            ["Swizzle"] = "null"
                        },
                        ["RightChild"] = new Dictionary<string, object>
                        {
                            ["!ComputeVertexStreamColor"] = "",
                            ["Stream"] = new Dictionary<string, object>
                            {
                                ["!ColorVertexStreamDefinition"] = ""
                            }
                        }
                    }
                },
                ["UVBuilder"] = "null",
                ["ForceTexCoords"] = false
            };

            emitter.SetMaterial(material);
            return emitter;
        }

        /// <summary>
        /// Adds flipbook animation to the material (sprite sheet animation)
        /// </summary>
        public static VFXEmitter SetFlipbookAnimation(
            this VFXEmitter emitter,
            int xDivisions = 8,
            int yDivisions = 8,
            int startingFrame = 0,
            int animationSpeed = 64)
        {
            // Get existing material
            var material = emitter.Get<Dictionary<string, object>>("Material");
            if (material == null) return emitter;

            material["UVBuilder"] = new Dictionary<string, object>
            {
                ["!UVBuilderFlipbook"] = "",
                ["XDivisions"] = xDivisions,
                ["YDivisions"] = yDivisions,
                ["StartingFrame"] = startingFrame,
                ["AnimationSpeed"] = animationSpeed
            };

            emitter.SetMaterial(material);
            return emitter;
        }

        #endregion

        #region Spawner Helpers

        /// <summary>
        /// Adds a burst spawner (one-time emission)
        /// </summary>
        public static VFXEmitter SetBurstSpawner(
            this VFXEmitter emitter,
            int spawnCount,
            bool oneShot = true,
            (float min, float max)? delay = null)
        {
            emitter.AddSpawner("Burst", new Dictionary<string, object>
            {
                ["LoopCondition"] = oneShot ? "OneShot" : "Looping",
                ["Delay"] = new Dictionary<string, object>
                {
                    ["X"] = delay?.min ?? 0.0f,
                    ["Y"] = delay?.max ?? 0.0f
                },
                ["Duration"] = new Dictionary<string, object> { ["X"] = 1.0f, ["Y"] = 1.0f },
                ["SpawnCount"] = spawnCount
            });

            return emitter;
        }

        /// <summary>
        /// Adds a per-second spawner (continuous emission)
        /// </summary>
        public static VFXEmitter SetPerSecondSpawner(
            this VFXEmitter emitter,
            float particlesPerSecond,
            bool looping = true)
        {
            emitter.AddSpawner("PerSecond", new Dictionary<string, object>
            {
                ["LoopCondition"] = looping ? "Looping" : "OneShot",
                ["Delay"] = new Dictionary<string, object> { ["X"] = 0.0f, ["Y"] = 0.0f },
                ["Duration"] = new Dictionary<string, object> { ["X"] = 1.0f, ["Y"] = 1.0f },
                ["SpawnCount"] = particlesPerSecond
            });

            return emitter;
        }

        /// <summary>
        /// Adds a per-frame spawner (for trails/ribbons)
        /// </summary>
        public static VFXEmitter SetPerFrameSpawner(
            this VFXEmitter emitter,
            float particlesPerFrame,
            float framerate = 60.0f,
            bool looping = true)
        {
            emitter.AddSpawner("PerFrame", new Dictionary<string, object>
            {
                ["LoopCondition"] = looping ? "Looping" : "OneShot",
                ["Delay"] = new Dictionary<string, object> { ["X"] = 0.0f, ["Y"] = 0.0f },
                ["Duration"] = new Dictionary<string, object> { ["X"] = 1.0f, ["Y"] = 1.0f },
                ["SpawnCount"] = particlesPerFrame,
                ["Framerate"] = framerate
            });

            return emitter;
        }

        #endregion

        #region Initializer Helpers

        /// <summary>
        /// Adds initial position randomization
        /// </summary>
        public static VFXEmitter AddInitialPosition(
            this VFXEmitter emitter,
            (float x, float y, float z) posMin,
            (float x, float y, float z) posMax,
            int seedOffset = 0)
        {
            emitter.AddInitializer("PositionSeed", CreateBaseInitializer(new Dictionary<string, object>
            {
                ["PositionMin"] = new Dictionary<string, object>
                {
                    ["X"] = posMin.x,
                    ["Y"] = posMin.y,
                    ["Z"] = posMin.z
                },
                ["PositionMax"] = new Dictionary<string, object>
                {
                    ["X"] = posMax.x,
                    ["Y"] = posMax.y,
                    ["Z"] = posMax.z
                },
                ["Interpolate"] = false,
                ["SeedOffset"] = seedOffset
            }));

            return emitter;
        }

        /// <summary>
        /// Adds initial velocity randomization
        /// </summary>
        public static VFXEmitter AddInitialVelocity(
            this VFXEmitter emitter,
            (float x, float y, float z) velMin,
            (float x, float y, float z) velMax,
            int seedOffset = 0)
        {
            emitter.AddInitializer("VelocitySeed", CreateBaseInitializer(new Dictionary<string, object>
            {
                ["VelocityMin"] = new Dictionary<string, object>
                {
                    ["X"] = velMin.x,
                    ["Y"] = velMin.y,
                    ["Z"] = velMin.z
                },
                ["VelocityMax"] = new Dictionary<string, object>
                {
                    ["X"] = velMax.x,
                    ["Y"] = velMax.y,
                    ["Z"] = velMax.z
                },
                ["SeedOffset"] = seedOffset
            }));

            return emitter;
        }

        /// <summary>
        /// Adds initial size randomization
        /// </summary>
        public static VFXEmitter AddInitialSize(
            this VFXEmitter emitter,
            (float min, float max) sizeRange,
            float scaleUniform = 1.0f,
            int seedOffset = 0)
        {
            emitter.AddInitializer("SizeSeed", CreateBaseInitializer(new Dictionary<string, object>
            {
                ["RandomSize"] = new Dictionary<string, object>
                {
                    ["X"] = sizeRange.min,
                    ["Y"] = sizeRange.max
                },
                ["ScaleUniform"] = scaleUniform,
                ["SeedOffset"] = seedOffset
            }));

            return emitter;
        }

        /// <summary>
        /// Adds initial rotation randomization
        /// </summary>
        public static VFXEmitter AddInitialRotation(
            this VFXEmitter emitter,
            (float min, float max) angularRange,
            int seedOffset = 0)
        {
            emitter.AddInitializer("RotationSeed", CreateBaseInitializer(new Dictionary<string, object>
            {
                ["AngularRotation"] = new Dictionary<string, object>
                {
                    ["X"] = angularRange.min,
                    ["Y"] = angularRange.max
                },
                ["SeedOffset"] = seedOffset
            }));

            return emitter;
        }

        /// <summary>
        /// Adds initial color randomization
        /// </summary>
        public static VFXEmitter AddInitialColor(
            this VFXEmitter emitter,
            (float r, float g, float b, float a) colorMin,
            (float r, float g, float b, float a) colorMax,
            int seedOffset = 0)
        {
            emitter.AddInitializer("ColorSeed", CreateBaseInitializer(new Dictionary<string, object>
            {
                ["ColorMin"] = new Dictionary<string, object>
                {
                    ["R"] = colorMin.r,
                    ["G"] = colorMin.g,
                    ["B"] = colorMin.b,
                    ["A"] = colorMin.a
                },
                ["ColorMax"] = new Dictionary<string, object>
                {
                    ["R"] = colorMax.r,
                    ["G"] = colorMax.g,
                    ["B"] = colorMax.b,
                    ["A"] = colorMax.a
                },
                ["SeedOffset"] = seedOffset
            }));

            return emitter;
        }

        /// <summary>
        /// Adds spawn order initializer (required for ribbons/trails)
        /// </summary>
        public static VFXEmitter AddSpawnOrder(this VFXEmitter emitter)
        {
            emitter.AddInitializer("SpawnOrder", CreateBaseInitializer(new Dictionary<string, object>()));
            return emitter;
        }

        #endregion

        #region Updater Helpers

        /// <summary>
        /// Adds color fade over time with keyframes
        /// </summary>
        public static VFXEmitter AddColorFade(
            this VFXEmitter emitter,
            params (float time, float r, float g, float b, float a)[] keyframes)
        {
            var curve = CreateAnimationCurveColor4(keyframes);

            emitter.AddUpdater("ColorOverTime", CreateBaseUpdater(new Dictionary<string, object>
            {
                ["SamplerMain"] = new Dictionary<string, object>
                {
                    ["!ComputeCurveSamplerColor4"] = "",
                    ["Curve"] = curve
                },
                ["SamplerOptional"] = "null",
                ["SeedOffset"] = 0
            }));

            return emitter;
        }

        /// <summary>
        /// Adds size change over time with keyframes
        /// </summary>
        public static VFXEmitter AddSizeOverTime(
            this VFXEmitter emitter,
            params (float time, float value)[] keyframes)
        {
            var curve = CreateAnimationCurveFloat(keyframes);

            emitter.AddUpdater("SizeOverTime", CreateBaseUpdater(new Dictionary<string, object>
            {
                ["SamplerMain"] = new Dictionary<string, object>
                {
                    ["!ComputeCurveSamplerFloat"] = "",
                    ["Curve"] = curve
                },
                ["SamplerOptional"] = "null",
                ["SeedOffset"] = 0
            }));

            return emitter;
        }

        /// <summary>
        /// Adds a force field (gravity, vortex, repulsion)
        /// </summary>
        public static VFXEmitter AddForceField(
            this VFXEmitter emitter,
            VFXFieldShape fieldShape = VFXFieldShape.Sphere,
            (float x, float y, float z)? scale = null,
            float forceDirected = 0.0f,
            float forceVortex = 0.0f,
            float forceRepulsive = 0.0f,
            float energyConservation = 0.2f)
        {
            var scaleVec = scale ?? (1.0f, 1.0f, 1.0f);

            var fieldShapeData = fieldShape switch
            {
                VFXFieldShape.Sphere => new Dictionary<string, object>
                {
                    ["!FieldShapeSphere"] = "",
                    ["Radius"] = 1.0f
                },
                VFXFieldShape.Cylinder => new Dictionary<string, object>
                {
                    ["!FieldShapeCylinder"] = "",
                    ["HalfHeight"] = 1.0f,
                    ["Radius"] = 1.0f
                },
                _ => new Dictionary<string, object>
                {
                    ["!FieldShapeSphere"] = "",
                    ["Radius"] = 1.0f
                }
            };

            emitter.AddUpdater("ForceField", CreateBaseUpdater(new Dictionary<string, object>
            {
                ["Scale"] = new Dictionary<string, object>
                {
                    ["X"] = scaleVec.x,
                    ["Y"] = scaleVec.y,
                    ["Z"] = scaleVec.z
                },
                ["DisplayParticlePosition"] = true,
                ["FieldShape"] = fieldShapeData,
                ["DisplayParticleRotation"] = true,
                ["DisplayParticleScale"] = true,
                ["DisplayParticleScaleUniform"] = false,
                ["FieldFalloff"] = new Dictionary<string, object>
                {
                    ["StrengthInside"] = 1.0f,
                    ["FalloffStart"] = 0.1f,
                    ["StrengthOutside"] = 0.0f,
                    ["FalloffEnd"] = 0.9f
                },
                ["EnergyConservation"] = energyConservation,
                ["ForceDirected"] = forceDirected,
                ["ForceVortex"] = forceVortex,
                ["ForceRepulsive"] = forceRepulsive,
                ["ForceFixed"] = new Dictionary<string, object>
                {
                    ["X"] = 0.0f,
                    ["Y"] = 0.0f,
                    ["Z"] = 0.0f
                }
            }));

            return emitter;
        }

        #endregion

        #region Helper Methods

        private static Dictionary<string, object> CreateBaseInitializer(Dictionary<string, object> additionalProps)
        {
            var baseInit = new Dictionary<string, object>
            {
                ["InheritPosition"] = true,
                ["Position"] = new Dictionary<string, object> { ["X"] = 0.0f, ["Y"] = 0.0f, ["Z"] = 0.0f },
                ["InheritRotation"] = true,
                ["Rotation"] = new Dictionary<string, object> { ["X"] = 0.0f, ["Y"] = 0.0f, ["Z"] = 0.0f, ["W"] = 1.0f },
                ["InheritScale"] = true,
                ["Scale"] = new Dictionary<string, object> { ["X"] = 1.0f, ["Y"] = 1.0f, ["Z"] = 1.0f },
                ["ScaleUniform"] = 1.0f,
                ["DisplayParticlePosition"] = false,
                ["DisplayParticleRotation"] = additionalProps.ContainsKey("AngularRotation") ? false : true,
                ["DisplayParticleScale"] = false,
                ["DisplayParticleScaleUniform"] = additionalProps.ContainsKey("RandomSize") ? true : false
            };

            foreach (var kvp in additionalProps)
                baseInit[kvp.Key] = kvp.Value;

            return baseInit;
        }

        private static Dictionary<string, object> CreateBaseUpdater(Dictionary<string, object> additionalProps)
        {
            var baseUpdater = new Dictionary<string, object>
            {
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
            };

            foreach (var kvp in additionalProps)
                baseUpdater[kvp.Key] = kvp.Value;

            return baseUpdater;
        }

        private static Dictionary<string, object> CreateAnimationCurveFloat(params (float time, float value)[] keyframes)
        {
            var keyframeDict = new Dictionary<string, object>();

            foreach (var (time, value) in keyframes)
            {
                var key = GuidHelper.NewGuid();
                keyframeDict[key] = new Dictionary<string, object>
                {
                    ["Key"] = time,
                    ["Value"] = value,
                    ["TangentType"] = "Linear"
                };
            }

            return new Dictionary<string, object>
            {
                ["!ComputeAnimationCurveFloat"] = "",
                ["KeyFrames"] = keyframeDict
            };
        }

        private static Dictionary<string, object> CreateAnimationCurveColor4(params (float time, float r, float g, float b, float a)[] keyframes)
        {
            var keyframeDict = new Dictionary<string, object>();

            foreach (var (time, r, g, b, a) in keyframes)
            {
                var key = GuidHelper.NewGuid();
                keyframeDict[key] = new Dictionary<string, object>
                {
                    ["Key"] = time,
                    ["Value"] = new Dictionary<string, object>
                    {
                        ["R"] = r,
                        ["G"] = g,
                        ["B"] = b,
                        ["A"] = a
                    },
                    ["TangentType"] = "Linear"
                };
            }

            return new Dictionary<string, object>
            {
                ["!ComputeAnimationCurveColor4"] = "",
                ["KeyFrames"] = keyframeDict
            };
        }

        #endregion
    }
}
