// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using HS.Stride.Editor.Toolkit.Core.StrideYamlParser;
using HS.Stride.Editor.Toolkit.Utilities;

namespace HS.Stride.Editor.Toolkit.Core.AssetEditing
{
    /// <summary>
    /// Represents an editable Stride ColliderShape asset (.sdphy).
    /// Used for creating reusable collision shapes, especially convex hulls.
    /// </summary>
    public class ColliderShapeAsset : IStrideAsset
    {
        private readonly Asset _colliderShape;

        private ColliderShapeAsset(Asset colliderShape)
        {
            _colliderShape = colliderShape;
        }

        /// <summary>
        /// Loads a collider shape asset from the specified file path.
        /// </summary>
        public static ColliderShapeAsset Load(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            var assetData = StrideYamlAssetParser.ParseAsset(filePath);
            return new ColliderShapeAsset(assetData);
        }

        /// <summary>
        /// Internal method to create a new convex hull collider shape asset.
        /// Use StrideProject.CreateColliderShape() instead for proper path handling.
        /// </summary>
        internal static ColliderShapeAsset CreateConvexHull(AssetReference modelReference, string assetName = "ColliderHull")
        {
            if (modelReference == null)
                throw new ArgumentNullException(nameof(modelReference));
            if (string.IsNullOrWhiteSpace(assetName))
                throw new ArgumentNullException(nameof(assetName));

            var assetId = GuidHelper.NewGuid();
            var shapeGuid = GuidHelper.NewGuidNoDashes();

            var properties = new Dictionary<string, object>
            {
                ["ColliderShapes"] = new Dictionary<string, object>
                {
                    [shapeGuid] = new Dictionary<string, object>
                    {
                        ["!ConvexHullColliderShapeDesc"] = "",
                        ["ConvexHulls"] = "null",
                        ["ConvexHullsIndices"] = "null",
                        ["Model"] = $"{modelReference.Id}:{modelReference.Path}",
                        ["LocalOffset"] = new Dictionary<string, object>
                        {
                            ["X"] = 0.0f,
                            ["Y"] = 0.0f,
                            ["Z"] = 0.0f
                        },
                        ["LocalRotation"] = new Dictionary<string, object>
                        {
                            ["X"] = 0.0f,
                            ["Y"] = 0.0f,
                            ["Z"] = 0.0f,
                            ["W"] = 1.0f
                        },
                        ["Scaling"] = new Dictionary<string, object>
                        {
                            ["X"] = 1.0f,
                            ["Y"] = 1.0f,
                            ["Z"] = 1.0f
                        },
                        ["Margin"] = 0.04f,
                        ["Decomposition"] = new Dictionary<string, object>
                        {
                            ["Depth"] = 10,
                            ["PosSampling"] = 10,
                            ["AngleSampling"] = 10,
                            ["PosRefine"] = 5,
                            ["AngleRefine"] = 5,
                            ["Alpha"] = 0.01f,
                            ["Threshold"] = 0.01f,
                            ["Enabled"] = false
                        }
                    }
                }
            };

            var asset = new Asset
            {
                AssetTypeHeader = "!ColliderShapeAsset",
                Id = assetId,
                SerializedVersion = "{Stride: 3.0.0.0}",
                Tags = new List<string>(),
                FilePath = string.Empty, // Will be set when saved
                Properties = properties
            };

            return new ColliderShapeAsset(asset);
        }

        public string Id => _colliderShape.Id;
        public string FilePath => _colliderShape.FilePath;

        /// <summary>
        /// Saves the collider shape asset to its current file path.
        /// </summary>
        public void Save()
        {
            if (string.IsNullOrEmpty(_colliderShape.FilePath))
                throw new InvalidOperationException("FilePath must be set before calling Save(). Use Save(filePath) or SaveAs(filePath) instead.");

            var yaml = StrideYamlAsset.GenerateAssetYaml(_colliderShape);
            File.WriteAllText(_colliderShape.FilePath, yaml);
        }

        /// <summary>
        /// Saves the collider shape asset to a new path.
        /// </summary>
        /// <param name="filePath">Full path including filename and .sdphy extension</param>
        public void SaveAs(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (!filePath.EndsWith(".sdphy", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("ColliderShape assets must have .sdphy extension", nameof(filePath));

            _colliderShape.FilePath = filePath;
            var yaml = StrideYamlAsset.GenerateAssetYaml(_colliderShape);
            File.WriteAllText(filePath, yaml);
        }

        /// <summary>
        /// Saves the collider shape asset to the specified path.
        /// </summary>
        /// <param name="filePath">Full path including filename and .sdphy extension</param>
        public void Save(string filePath)
        {
            SaveAs(filePath);
        }

        /// <summary>
        /// Saves the collider shape asset to the project's Assets folder.
        /// </summary>
        /// <param name="project">The Stride project</param>
        /// <param name="relativePath">Relative path within Assets folder (e.g., "ColliderShapes/MyCollider")</param>
        public void SaveToProject(StrideProject project, string relativePath)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));
            if (string.IsNullOrWhiteSpace(relativePath))
                throw new ArgumentNullException(nameof(relativePath));

            // Ensure .sdphy extension
            if (!relativePath.EndsWith(".sdphy", StringComparison.OrdinalIgnoreCase))
                relativePath += ".sdphy";

            var fullPath = Path.Combine(project.AssetsPath, relativePath);

            // Create directory if it doesn't exist
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            Save(fullPath);
        }

        /// <summary>
        /// Gets an asset reference that can be used in scenes to reference this collider shape.
        /// </summary>
        /// <returns>AssetReference for use in ColliderShapeAssetDesc</returns>
        public AssetReference GetReference()
        {
            if (string.IsNullOrEmpty(FilePath))
                throw new InvalidOperationException("ColliderShape asset must be saved before getting a reference");

            // Extract relative path from Assets folder
            var fileName = Path.GetFileNameWithoutExtension(FilePath);
            var directory = Path.GetDirectoryName(FilePath) ?? "";

            // Try to find "Assets" in the path
            var assetsIndex = directory.IndexOf("Assets", StringComparison.OrdinalIgnoreCase);
            string relativePath;
            if (assetsIndex >= 0)
            {
                relativePath = directory.Substring(assetsIndex + "Assets".Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                relativePath = Path.Combine(relativePath, fileName).Replace('\\', '/');
            }
            else
            {
                relativePath = fileName;
            }

            return new AssetReference
            {
                Id = Id,
                Path = relativePath,
                Type = AssetType.ColliderShape
            };
        }
    }
}
