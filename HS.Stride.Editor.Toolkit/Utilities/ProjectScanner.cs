// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0


using HS.Stride.Editor.Toolkit.Core.AssetEditing;

namespace HS.Stride.Editor.Toolkit.Utilities
{
    /// <summary>
    /// Scans a Stride project to find all assets (prefabs, models, materials, etc)
    /// </summary>
    public class ProjectScanner
    {
        private readonly string _projectPath;
        private readonly string _assetsPath;
        private List<AssetReference> _cachedAssets = new();

        public ProjectScanner(string projectPath)
        {
            if (!PathHelper.IsStrideProject(projectPath))
                throw new ArgumentException($"Not a valid Stride project: {projectPath}");

            _projectPath = projectPath;
            var structure = ProjectStructureDetector.DetectTargetProjectStructure(projectPath);
            _assetsPath = Path.Combine(projectPath, structure.AssetsPath);
        }

        /// <summary>
        /// Scans the project for all assets
        /// </summary>
        public void Scan()
        {
            _cachedAssets.Clear();

            if (!Directory.Exists(_assetsPath))
                return;

            // Scan .sd* asset files
            var assetFiles = Directory.GetFiles(_assetsPath, "*.sd*", SearchOption.AllDirectories);

            foreach (var file in assetFiles)
            {
                var assetRef = ParseAssetFile(file);
                if (assetRef != null)
                    _cachedAssets.Add(assetRef);
            }

            // Scan .cs script files
            var scriptFiles = Directory.GetFiles(_projectPath, "*.cs", SearchOption.AllDirectories);

            foreach (var file in scriptFiles)
            {
                // Skip files in obj, bin, packages folders
                if (file.Contains("\\obj\\") || file.Contains("\\bin\\") ||
                    file.Contains("\\packages\\") || file.Contains("/obj/") ||
                    file.Contains("/bin/") || file.Contains("/packages/"))
                    continue;

                var scriptRef = ParseScriptFile(file);
                if (scriptRef != null)
                    _cachedAssets.Add(scriptRef);
            }
        }

        /// <summary>
        /// Get all assets of a specific type
        /// </summary>
        public List<AssetReference> GetAssets(AssetType type)
        {
            return _cachedAssets.Where(a => a.Type == type).ToList();
        }

        /// <summary>
        /// Get all prefabs
        /// </summary>
        public List<AssetReference> GetPrefabs()
        {
            return GetAssets(AssetType.Prefab);
        }

        /// <summary>
        /// Get all models
        /// </summary>
        public List<AssetReference> GetModels()
        {
            return GetAssets(AssetType.Model);
        }

        /// <summary>
        /// Get all materials
        /// </summary>
        public List<AssetReference> GetMaterials()
        {
            return GetAssets(AssetType.Material);
        }

        /// <summary>
        /// Get all scenes
        /// </summary>
        public List<AssetReference> GetScenes()
        {
            return GetAssets(AssetType.Scene);
        }

        /// <summary>
        /// Get all animations
        /// </summary>
        public List<AssetReference> GetAnimations()
        {
            return GetAssets(AssetType.Animation);
        }

        /// <summary>
        /// Get all skeletons
        /// </summary>
        public List<AssetReference> GetSkeletons()
        {
            return GetAssets(AssetType.Skeleton);
        }

        /// <summary>
        /// Get all sprite sheets
        /// </summary>
        public List<AssetReference> GetSpriteSheets()
        {
            return GetAssets(AssetType.SpriteSheet);
        }

        /// <summary>
        /// Get all effects
        /// </summary>
        public List<AssetReference> GetEffects()
        {
            return GetAssets(AssetType.Effect);
        }

        /// <summary>
        /// Get all UI pages
        /// </summary>
        public List<AssetReference> GetUIPages()
        {
            return GetAssets(AssetType.UIPage);
        }
        
        /// <summary>
        /// Get all sounds
        /// </summary>
        public List<AssetReference> GetSounds()
        {
            return GetAssets(AssetType.Sound);
        }

        /// <summary>
        /// Get all textures
        /// </summary>
        public List<AssetReference> GetTextures()
        {
            return GetAssets(AssetType.Texture);
        }

        /// <summary>
        /// Get all scripts
        /// </summary>
        public List<AssetReference> GetScripts()
        {
            return GetAssets(AssetType.Script);
        }

        /// <summary>
        /// Find asset by name (exact match)
        /// </summary>
        public AssetReference? FindAsset(string name, AssetType? type = null)
        {
            var query = _cachedAssets.Where(a => a.Name == name);
            if (type.HasValue)
                query = query.Where(a => a.Type == type.Value);
            return query.FirstOrDefault();
        }

        /// <summary>
        /// Find assets by name pattern (supports * and ?)
        /// </summary>
        public List<AssetReference> FindAssets(string pattern, AssetType? type = null)
        {
            var regex = "^" + System.Text.RegularExpressions.Regex.Escape(pattern)
                .Replace("\\*", ".*")
                .Replace("\\?", ".") + "$";

            var query = _cachedAssets.Where(a =>
                System.Text.RegularExpressions.Regex.IsMatch(a.Name, regex));

            if (type.HasValue)
                query = query.Where(a => a.Type == type.Value);

            return query.ToList();
        }

        /// <summary>
        /// Find asset by GUID
        /// </summary>
        public AssetReference? FindAssetByGuid(string guid)
        {
            return _cachedAssets.FirstOrDefault(a => a.Id == guid);
        }

        /// <summary>
        /// Find asset by path (relative to Assets folder)
        /// </summary>
        public AssetReference? FindAssetByPath(string path)
        {
            return _cachedAssets.FirstOrDefault(a =>
                a.Path.Equals(path, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Get all assets
        /// </summary>
        public List<AssetReference> GetAllAssets()
        {
            return _cachedAssets.ToList();
        }
        

        private AssetReference? ParseAssetFile(string filePath)
        {
            try
            {
                var extension = Path.GetExtension(filePath).ToLower();
                var assetType = GetAssetTypeFromExtension(extension);

                if (assetType == AssetType.Unknown)
                    return null;

                // Read first 20 lines to get ID
                var lines = File.ReadLines(filePath).Take(20).ToList();
                var idLine = lines.FirstOrDefault(l => l.Trim().StartsWith("Id: "));

                if (idLine == null)
                    return null;

                var id = idLine.Substring(idLine.IndexOf("Id: ") + 4).Trim();
                var name = Path.GetFileNameWithoutExtension(filePath);
                var relativePath = Path.GetRelativePath(_assetsPath, filePath);

                // Convert to forward slashes for asset path and remove the extension
                var assetPath = relativePath.Replace('\\', '/');
                var lastDot = assetPath.LastIndexOf('.');
                if (lastDot != -1)
                {
                    assetPath = assetPath.Substring(0, lastDot);
                }

                return new AssetReference
                {
                    Id = id,
                    Name = name,
                    Path = assetPath,
                    FilePath = filePath,
                    Type = assetType
                };
            }
            catch (IOException)
            {
                return null;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        private AssetType GetAssetTypeFromExtension(string extension)
        {
            return extension switch
            {
                ".sdprefab" => AssetType.Prefab,
                ".sdm3d" => AssetType.Model,
                ".sdmat" => AssetType.Material,
                ".sdtex" => AssetType.Texture,
                ".sdscene" => AssetType.Scene,
                ".sdsnd" => AssetType.Sound,
                ".sdanim" => AssetType.Animation,
                ".sdskel" => AssetType.Skeleton,
                ".sdsheet" => AssetType.SpriteSheet,
                ".sdsprite" => AssetType.Sprite,
                ".sdfx" => AssetType.Effect,
                ".sdpage" => AssetType.UIPage,
                ".sduilib" => AssetType.UILibrary,
                ".sdspritefnt" => AssetType.SpriteFont,
                ".sdfnt" => AssetType.SpriteFont,
                ".sdskybox" => AssetType.Skybox,
                ".sdvideo" => AssetType.Video,
                ".sdrendertex" => AssetType.RenderTexture,
                ".sdgamesettings" => AssetType.GameSettings,
                ".sdgfxcomp" => AssetType.GraphicsCompositor,
                ".sdarch" => AssetType.Archetype,
                ".sdphys" => AssetType.ColliderShape,
                ".sdconvex" => AssetType.ConvexHull,
                ".sdraw" => AssetType.RawAsset,
                ".cs" => AssetType.Script,
                _ => AssetType.Unknown
            };
        }

        private AssetReference? ParseScriptFile(string filePath)
        {
            try
            {
                var content = File.ReadAllText(filePath);

                // Look for class definition - extract class name
                var classMatch = System.Text.RegularExpressions.Regex.Match(content, @"class\s+(\w+)\s*(?::|{)");
                if (!classMatch.Success)
                    return null;

                var className = classMatch.Groups[1].Value;

                // Scripts don't have IDs like assets, so use file path hash or empty
                var id = string.Empty;

                return new AssetReference
                {
                    Id = id,
                    Name = className,
                    Path = Path.GetRelativePath(_projectPath, filePath).Replace('\\', '/'),
                    FilePath = filePath,
                    Type = AssetType.Script
                };
            }
            catch (IOException)
            {
                return null;
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }
        }

    }
}
