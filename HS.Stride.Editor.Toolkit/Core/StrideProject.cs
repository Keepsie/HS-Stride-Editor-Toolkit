// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using HS.Stride.Editor.Toolkit.Core.AssetEditing;
using HS.Stride.Editor.Toolkit.Core.PrefabEditing;
using HS.Stride.Editor.Toolkit.Core.SceneEditing;
using HS.Stride.Editor.Toolkit.Core.UIPageEditing;
using HS.Stride.Editor.Toolkit.Utilities;

namespace HS.Stride.Editor.Toolkit.Core
{
    /// <summary>
    /// Load and use assets that are in the Stride project (Folder)
    /// </summary>
    public class StrideProject
    {
        private readonly ProjectScanner _scanner;
        private readonly string _projectPath;
        private readonly string _assetsPath;
        public string ProjectPath => _projectPath;
        public string AssetsPath => _assetsPath;

        /// <summary>
        /// Project validation mode - controls how the toolkit handles property validation
        /// </summary>
        public ProjectMode Mode { get; set; } = ProjectMode.Strict;


        /// <summary>
        /// Creates a new StrideProject instance and automatically scans for all assets.
        /// </summary>
        /// <param name="projectPath">Path to the Stride project</param>
        /// <param name="mode">Validation mode (Strict = validates properties, Loose = allows any property)</param>
        public StrideProject(string projectPath, ProjectMode mode = ProjectMode.Strict)
        {
            if (string.IsNullOrWhiteSpace(projectPath))
                throw new ArgumentNullException(nameof(projectPath));

            _scanner = new ProjectScanner(projectPath);
            _scanner.Scan();

            _projectPath = projectPath;
            var structure = ProjectStructureDetector.DetectTargetProjectStructure(projectPath);
            _assetsPath = Path.Combine(projectPath, structure.AssetsPath);
            Mode = mode;
        }
        
        /// <summary>
        /// Loads a scene by name or relative path (from Assets folder).
        /// </summary>
        /// <example>
        /// <code>
        /// var project = new StrideProject(@"C:\MyGame");
        /// var scene1 = project.LoadScene("Level1");
        /// var scene2 = project.LoadScene("Scenes/MainMenu");
        /// </code>
        /// </example>
        public Scene LoadScene(string sceneNameOrPath)
        {
            if (string.IsNullOrWhiteSpace(sceneNameOrPath))
                throw new ArgumentNullException(nameof(sceneNameOrPath));

            // Try to find scene by name or path
            var sceneAsset = _scanner.FindAsset(sceneNameOrPath, AssetType.Scene) ??
                           _scanner.FindAssetByPath(sceneNameOrPath);

            if (sceneAsset == null)
            {
                // Try with just the filename if it's a path
                var fileName = Path.GetFileNameWithoutExtension(sceneNameOrPath);
                sceneAsset = _scanner.FindAsset(fileName, AssetType.Scene);
            }

            if (sceneAsset == null)
                throw new FileNotFoundException($"Scene not found: {sceneNameOrPath}");

            var scene = Scene.Load(sceneAsset.FilePath);

            // Set ParentProject on scene (propagates to all entities)
            scene.SetParentProject(this);

            return scene;
        }

        /// <summary>
        /// Creates a new blank scene and saves it to the Assets folder.
        /// The scene is immediately saved to disk and returned ready for editing.
        /// The scene will be automatically registered in the project scanner after calling Rescan().
        /// </summary>
        /// <param name="name">Name of the scene</param>
        /// <param name="relativePath">Relative path from Assets folder (e.g., "Scenes/Level1" or just "Level1")</param>
        /// <returns>The created Scene ready for editing</returns>
        /// <example>
        /// <code>
        /// var project = new StrideProject(@"C:\MyGame");
        /// var scene = project.CreateScene("Level1", "Scenes/Level1");
        ///
        /// // Start editing immediately
        /// scene.CreateEntity("Player");
        /// scene.Save();
        ///
        /// // Rescan to register in project
        /// project.Rescan();
        /// </code>
        /// </example>
        public Scene CreateScene(string name, string? relativePath = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            // Build file path
            string filePath;
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                // Default: save in Assets root
                filePath = Path.Combine(_assetsPath, $"{name}.sdscene");
            }
            else
            {
                // Handle relative path
                var cleanPath = relativePath.Replace("/", Path.DirectorySeparatorChar.ToString());

                // If path doesn't end with .sdscene, treat it as a directory and add filename
                if (!cleanPath.EndsWith(".sdscene", StringComparison.OrdinalIgnoreCase))
                {
                    filePath = Path.Combine(_assetsPath, cleanPath, $"{name}.sdscene");
                }
                else
                {
                    filePath = Path.Combine(_assetsPath, cleanPath);
                }
            }

            // Ensure directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Generate blank scene YAML
            var sceneId = GuidHelper.NewGuid();
            var blankSceneYaml = $@"!SceneAsset
Id: {sceneId}
SerializedVersion: {{Stride: 3.1.0.1}}
Tags: []
ChildrenIds: []
Offset: {{X: 0.0, Y: 0.0, Z: 0.0}}
Hierarchy:
    RootParts: []
    Parts: []";

            // Save to disk immediately
            FileHelper.SaveFile(blankSceneYaml, filePath);

            // Load and return the scene
            var scene = Scene.Load(filePath);
            scene.SetParentProject(this);

            return scene;
        }
        
        /// <summary>
        /// Rescans the project for assets.
        /// Call this if assets were added/removed externally during runtime.
        /// </summary>
        public void Rescan()
        {
            _scanner.Scan();
        }
        
        
        /// <summary>
        /// Finds an asset by name or path and returns its reference.
        /// Use this to get asset references for manual assignment to components/scripts.
        /// </summary>
        private AssetReference? FindAssetByNameOrPath(string nameOrPath, AssetType type)
        {
            // Try exact name match first
            var asset = _scanner.FindAsset(nameOrPath, type);
            if (asset != null)
                return asset;

            // Try as path
            asset = _scanner.FindAssetByPath(nameOrPath);
            if (asset != null && asset.Type == type)
                return asset;

            // Try with just filename if it's a path
            var fileName = Path.GetFileNameWithoutExtension(nameOrPath);
            return _scanner.FindAsset(fileName, type);
        }
        
        /// <summary>
        /// Finds an asset by name (exact match).
        /// </summary>
        public AssetReference? FindAsset(string name, AssetType? type = null)
        {
            return _scanner.FindAsset(name, type);
        }

        /// <summary>
        /// Finds assets by name pattern (supports * and ?).
        /// </summary>
        public List<AssetReference> FindAssets(string pattern, AssetType? type = null)
        {
            return _scanner.FindAssets(pattern, type);
        }

        /// <summary>
        /// Finds an asset by its relative path (from Assets folder).
        /// </summary>
        public AssetReference? FindAssetByPath(string path)
        {
            return _scanner.FindAssetByPath(path);
        }

        /// <summary>
        /// Finds an asset by GUID
        /// </summary>
        public AssetReference? FindAssetByGuid(string guid)
        {
            return _scanner.FindAssetByGuid(guid);
        }

        
        ///================================ QUICK LOADS (Asset types) =====================================
        
        /// <summary>
        /// Loads a material asset by name or relative path.
        /// </summary>
        public MaterialAsset LoadMaterial(string nameOrPath)
        {
            if (string.IsNullOrWhiteSpace(nameOrPath))
                throw new ArgumentNullException(nameof(nameOrPath));

            var asset = FindAssetByNameOrPath(nameOrPath, AssetType.Material);
            if (asset == null)
                throw new FileNotFoundException($"Material not found: {nameOrPath}");

            return MaterialAsset.Load(asset.FilePath);
        }

        /// <summary>
        /// Loads a texture asset by name or relative path.
        /// </summary>
        public TextureAsset LoadTexture(string nameOrPath)
        {
            if (string.IsNullOrWhiteSpace(nameOrPath))
                throw new ArgumentNullException(nameof(nameOrPath));

            var asset = FindAssetByNameOrPath(nameOrPath, AssetType.Texture);
            if (asset == null)
                throw new FileNotFoundException($"Texture not found: {nameOrPath}");

            return TextureAsset.Load(asset.FilePath);
        }

        /// <summary>
        /// Loads an animation asset by name or relative path.
        /// </summary>
        public AnimationAsset LoadAnimation(string nameOrPath)
        {
            if (string.IsNullOrWhiteSpace(nameOrPath))
                throw new ArgumentNullException(nameof(nameOrPath));

            var asset = FindAssetByNameOrPath(nameOrPath, AssetType.Animation);
            if (asset == null)
                throw new FileNotFoundException($"Animation not found: {nameOrPath}");

            return AnimationAsset.Load(asset.FilePath);
        }

        /// <summary>
        /// Loads a prefab asset by name or relative path.
        /// </summary>
        public Prefab LoadPrefab(string nameOrPath)
        {
            if (string.IsNullOrWhiteSpace(nameOrPath))
                throw new ArgumentNullException(nameof(nameOrPath));

            var asset = FindAssetByNameOrPath(nameOrPath, AssetType.Prefab);
            if (asset == null)
            {
                // Fallback: check if it's a direct file path
                if (File.Exists(nameOrPath) && nameOrPath.EndsWith(".sdprefab", StringComparison.OrdinalIgnoreCase))
                {
                    var prefab = Prefab.Load(nameOrPath);
                    prefab.SetParentProject(this);
                    return prefab;
                }
                throw new FileNotFoundException($"Prefab not found: {nameOrPath}");
            }

            var loadedPrefab = Prefab.Load(asset.FilePath);

            // Set ParentProject on prefab (propagates to all entities)
            loadedPrefab.SetParentProject(this);

            return loadedPrefab;
        }

        /// <summary>
        /// Creates a new prefab and saves it to the Assets folder.
        /// The prefab will be automatically registered in the project scanner after calling Rescan().
        /// </summary>
        /// <param name="name">Name of the prefab (and root entity)</param>
        /// <param name="relativePath">Relative path from Assets folder (e.g., "Prefabs/MyPrefab" or just "MyPrefab")</param>
        /// <returns>A new PrefabAsset ready for editing</returns>
        /// <example>
        /// <code>
        /// var project = new StrideProject(@"C:\MyGame");
        /// var prefab = project.CreatePrefab("Crate", "Prefabs/Crate");
        ///
        /// // Add entities and components
        /// var root = prefab.GetRootEntity();
        /// root.AddModel();
        /// root.AddStaticCollider().AddBoxShape(1, 1, 1);
        ///
        /// // Save to disk
        /// prefab.Save();
        ///
        /// // Rescan to register in project
        /// project.Rescan();
        /// </code>
        /// </example>
        public Prefab CreatePrefab(string name, string? relativePath = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            // Build file path
            string filePath;
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                // Default: save in Assets root
                filePath = Path.Combine(_assetsPath, $"{name}.sdprefab");
            }
            else
            {
                // Handle relative path
                var cleanPath = relativePath.Replace("/", Path.DirectorySeparatorChar.ToString());

                // If path doesn't end with .sdprefab, treat it as a directory and add filename
                if (!cleanPath.EndsWith(".sdprefab", StringComparison.OrdinalIgnoreCase))
                {
                    filePath = Path.Combine(_assetsPath, cleanPath, $"{name}.sdprefab");
                }
                else
                {
                    filePath = Path.Combine(_assetsPath, cleanPath);
                }
            }

            // Ensure directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Create the prefab
            var prefab = Prefab.Create(name, filePath);

            // Set ParentProject on prefab (propagates to all entities)
            prefab.SetParentProject(this);

            // Save to disk immediately
            prefab.Save();

            return prefab;
        }

        /// <summary>
        /// Loads a UI page asset by name or relative path.
        /// </summary>
        public UIPage LoadUIPage(string nameOrPath)
        {
            if (string.IsNullOrWhiteSpace(nameOrPath))
                throw new ArgumentNullException(nameof(nameOrPath));

            var asset = FindAssetByNameOrPath(nameOrPath, AssetType.UIPage);
            if (asset == null)
            {
                // Fallback: check if it's a direct file path
                if (File.Exists(nameOrPath) && nameOrPath.EndsWith(".sduipage", StringComparison.OrdinalIgnoreCase))
                {
                    var page = UIPage.Load(nameOrPath);
                    page.SetParentProject(this);
                    return page;
                }
                throw new FileNotFoundException($"UI page not found: {nameOrPath}");
            }

            var loadedPage = UIPage.Load(asset.FilePath);
            loadedPage.SetParentProject(this);
            return loadedPage;
        }

        /// <summary>
        /// Creates a new UI page and saves it to the Assets folder.
        /// The UI page will be automatically registered in the project scanner after calling Rescan().
        /// </summary>
        /// <param name="name">Name of the UI page (and root Grid element)</param>
        /// <param name="relativePath">Relative path from Assets folder (e.g., "UI/MyMenu" or just "MyMenu")</param>
        /// <returns>A new UIPage ready for editing</returns>
        /// <example>
        /// <code>
        /// var project = new StrideProject(@"C:\MyGame");
        /// var page = project.CreateUIPage("MainMenu", "UI/MainMenu");
        ///
        /// // Create UI elements
        /// var canvas = page.CreateCanvas("menu_canvas");
        /// var button = page.CreateButton("start_button", "Start Game", canvas);
        /// button.SetMargin(left: 100, top: 200);
        ///
        /// // Save to disk
        /// page.Save();
        ///
        /// // Rescan to register in project
        /// project.Rescan();
        /// </code>
        /// </example>
        public UIPage CreateUIPage(string name, string? relativePath = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            // Build file path
            string filePath;
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                // Default: save in Assets root
                filePath = Path.Combine(_assetsPath, $"{name}.sduipage");
            }
            else
            {
                // Handle relative path
                var cleanPath = relativePath.Replace("/", Path.DirectorySeparatorChar.ToString());

                // If path doesn't end with .sduipage, treat it as a directory and add filename
                if (!cleanPath.EndsWith(".sduipage", StringComparison.OrdinalIgnoreCase))
                {
                    filePath = Path.Combine(_assetsPath, cleanPath, $"{name}.sduipage");
                }
                else
                {
                    filePath = Path.Combine(_assetsPath, cleanPath);
                }
            }

            // Ensure directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Create the UI page
            var page = UIPage.Create(name, filePath);

            // Set ParentProject on UI page
            page.SetParentProject(this);

            return page;
        }

        /// <summary>
        /// Loads a sound asset by name or relative path.
        /// </summary>
        public SoundAsset LoadSound(string nameOrPath)
        {
            if (string.IsNullOrWhiteSpace(nameOrPath))
                throw new ArgumentNullException(nameof(nameOrPath));

            var asset = FindAssetByNameOrPath(nameOrPath, AssetType.Sound);
            if (asset == null)
                throw new FileNotFoundException($"Sound not found: {nameOrPath}");

            return SoundAsset.Load(asset.FilePath);
        }

        /// <summary>
        /// Loads a skeleton asset by name or relative path.
        /// </summary>
        public SkeletonAsset LoadSkeleton(string nameOrPath)
        {
            if (string.IsNullOrWhiteSpace(nameOrPath))
                throw new ArgumentNullException(nameof(nameOrPath));

            var asset = FindAssetByNameOrPath(nameOrPath, AssetType.Skeleton);
            if (asset == null)
                throw new FileNotFoundException($"Skeleton not found: {nameOrPath}");

            return SkeletonAsset.Load(asset.FilePath);
        }

        /// <summary>
        /// Loads a sprite sheet asset by name or relative path.
        /// </summary>
        public SpriteSheetAsset LoadSpriteSheet(string nameOrPath)
        {
            if (string.IsNullOrWhiteSpace(nameOrPath))
                throw new ArgumentNullException(nameof(nameOrPath));

            var asset = FindAssetByNameOrPath(nameOrPath, AssetType.SpriteSheet);
            if (asset == null)
                throw new FileNotFoundException($"Sprite sheet not found: {nameOrPath}");

            return SpriteSheetAsset.Load(asset.FilePath);
        }

        /// <summary>
        /// Loads an effect asset by name or relative path.
        /// </summary>
        public EffectAsset LoadEffect(string nameOrPath)
        {
            if (string.IsNullOrWhiteSpace(nameOrPath))
                throw new ArgumentNullException(nameof(nameOrPath));

            var asset = FindAssetByNameOrPath(nameOrPath, AssetType.Effect);
            if (asset == null)
                throw new FileNotFoundException($"Effect not found: {nameOrPath}");

            return EffectAsset.Load(asset.FilePath);
        }

        
        
        ///================================ QUICK GETS (Asset References by type) =====================================
        
        
        /// <summary>
        /// Gets all prefabs in the project.
        /// </summary>
        public List<AssetReference> GetPrefabs() => _scanner.GetPrefabs();

        /// <summary>
        /// Gets all models in the project.
        /// </summary>
        public List<AssetReference> GetModels() => _scanner.GetModels();

        /// <summary>
        /// Gets all materials in the project.
        /// </summary>
        public List<AssetReference> GetMaterials() => _scanner.GetMaterials();

        /// <summary>
        /// Gets all textures in the project.
        /// </summary>
        public List<AssetReference> GetTextures() => _scanner.GetTextures();

        /// <summary>
        /// Gets all scenes in the project.
        /// </summary>
        public List<AssetReference> GetScenes() => _scanner.GetScenes();

        /// <summary>
        /// Gets all animations in the project.
        /// </summary>
        public List<AssetReference> GetAnimations() => _scanner.GetAnimations();

        /// <summary>
        /// Gets all skeletons in the project.
        /// </summary>
        public List<AssetReference> GetSkeletons() => _scanner.GetSkeletons();

        /// <summary>
        /// Gets all sounds in the project.
        /// </summary>
        public List<AssetReference> GetSounds() => _scanner.GetSounds();

        /// <summary>
        /// Gets all UI pages in the project.
        /// </summary>
        public List<AssetReference> GetUIPages() => _scanner.GetUIPages();

        /// <summary>
        /// Gets all sprite sheets in the project.
        /// </summary>
        public List<AssetReference> GetSpriteSheets() => _scanner.GetSpriteSheets();

        /// <summary>
        /// Gets all effects in the project.
        /// </summary>
        public List<AssetReference> GetEffects() => _scanner.GetEffects();

        /// <summary>
        /// Gets all scripts in the project.
        /// </summary>
        public List<AssetReference> GetScripts() => _scanner.GetScripts();

        /// <summary>
        /// Gets all assets of a specific type.
        /// </summary>
        public List<AssetReference> GetAssets(AssetType type) => _scanner.GetAssets(type);

        /// <summary>
        /// Gets all assets in the project.
        /// </summary>
        public List<AssetReference> GetAllAssets() => _scanner.GetAllAssets();


        /// <summary>
        /// Gets the source file path for a RawAsset (.sdraw file points to actual content file in Resources/)
        /// </summary>
        /// <param name="rawAssetReference">The RawAsset reference</param>
        /// <returns>Full path to the source file (JSON/TXT/XML/CSV), or null if not found</returns>
        /// <exception cref="ArgumentNullException">If rawAssetReference is null</exception>
        /// <exception cref="ArgumentException">If asset is not of type RawAsset</exception>
        public string? GetRawAssetSource(AssetReference rawAssetReference)
        {
            if (rawAssetReference == null)
                throw new ArgumentNullException(nameof(rawAssetReference));

            if (rawAssetReference.Type != AssetType.RawAsset)
                throw new ArgumentException("Asset must be of type RawAsset", nameof(rawAssetReference));

            // Read the .sdraw file to get the Source property
            if (!File.Exists(rawAssetReference.FilePath))
                return null;

            var sdrawContent = File.ReadAllText(rawAssetReference.FilePath);

            // Parse YAML to find Source line
            // Format: "Source: !file ../../../Resources/Databases/DialogSystem_db/merchant_dialog.json"
            var lines = sdrawContent.Split('\n');
            foreach (var line in lines)
            {
                var trimmedLine = line.TrimStart();
                if (trimmedLine.StartsWith("Source:", StringComparison.OrdinalIgnoreCase))
                {
                    var sourcePath = trimmedLine.Substring("Source:".Length).Trim();

                    // Remove !file tag if present
                    if (sourcePath.StartsWith("!file ", StringComparison.OrdinalIgnoreCase))
                    {
                        sourcePath = sourcePath.Substring("!file ".Length).Trim();
                    }

                    // Resolve relative path from .sdraw location
                    var sdrawDir = Path.GetDirectoryName(rawAssetReference.FilePath);
                    if (sdrawDir == null)
                        return null;

                    var fullPath = Path.GetFullPath(Path.Combine(sdrawDir, sourcePath));

                    if (File.Exists(fullPath))
                        return fullPath;

                    // Fallback: try to find file in Resources folder by name
                    return FindResourceFileByName(Path.GetFileName(sourcePath));
                }
            }

            return null;
        }

        private string? FindResourceFileByName(string fileName)
        {
            var searchPaths = new[]
            {
                Path.Combine(_projectPath, "Resources"),
                Path.Combine(_projectPath, "Assets", "Resources"),
                _projectPath
            };

            foreach (var searchPath in searchPaths)
            {
                if (Directory.Exists(searchPath))
                {
                    var foundFile = Directory.GetFiles(searchPath, fileName, SearchOption.AllDirectories)
                        .FirstOrDefault();
                    if (foundFile != null)
                        return foundFile;
                }
            }

            return null;
        }


    }
}
