// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using HS.Stride.Editor.Toolkit.Core.StrideYamlParser;
using HS.Stride.Editor.Toolkit.Utilities;

namespace HS.Stride.Editor.Toolkit.Core
{
    /// <summary>
    /// Represents an editable Stride Animation asset (.sdanim).
    /// </summary>
    public class AnimationAsset : IStrideAsset
    {
        private readonly Asset _animation;

        private AnimationAsset(Asset animation)
        {
            _animation = animation;
        }

        /// <summary>
        /// Loads an animation asset from the specified file path.
        /// </summary>
        public static AnimationAsset Load(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            var animationData = StrideYamlAssetParser.ParseAsset(filePath);
            return new AnimationAsset(animationData);
        }

        public string Id => _animation.Id;
        public string FilePath => _animation.FilePath;

        public string? GetSource()
        {
            if (_animation.Properties.TryGetValue("Source", out var source))
                return source.ToString().Replace("!file ", "");
            return null;
        }

        public void SetSource(string sourcePath)
        {
            _animation.Properties["Source"] = $"!file {sourcePath}";
        }

        public string RepeatMode
        {
            get => _animation.Properties.TryGetValue("RepeatMode", out var mode) ? mode.ToString() : "LoopInfinite";
            set => _animation.Properties["RepeatMode"] = value;
        }

        public bool RootMotion
        {
            get => _animation.Properties.TryGetValue("RootMotion", out var rootMotion) && Convert.ToBoolean(rootMotion);
            set => _animation.Properties["RootMotion"] = value;
        }
        
        public string? GetSkeletonReference()
        {
            return _animation.Properties.TryGetValue("Skeleton", out var skeleton) ? skeleton?.ToString() : null;
        }

        public void SetSkeletonReference(string skeletonRef)
        {
            _animation.Properties["Skeleton"] = skeletonRef;
        }

        public string? GetPreviewModel()
        {
            return _animation.Properties.TryGetValue("PreviewModel", out var model) ? model?.ToString() : null;
        }

        public void SetPreviewModel(string modelRef)
        {
            _animation.Properties["PreviewModel"] = modelRef;
        }

        /// <summary>
        /// Saves the animation's current state back to its original file.
        /// </summary>
        public void Save()
        {
            var yaml = StrideYamlAsset.GenerateAssetYaml(_animation);
            FileHelper.SaveFile(yaml, _animation.FilePath);
        }

        /// <summary>
        /// Saves the animation's current state to a new file.
        /// </summary>
        public void SaveAs(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            var yaml = StrideYamlAsset.GenerateAssetYaml(_animation);
            FileHelper.SaveFile(yaml, filePath);
        }
    }
}
