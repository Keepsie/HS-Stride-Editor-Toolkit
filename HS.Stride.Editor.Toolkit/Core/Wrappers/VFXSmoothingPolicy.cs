// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

namespace HS.Stride.Editor.Toolkit.Core.Wrappers
{
    /// <summary>
    /// Smoothing policies for ribbon/trail particles
    /// </summary>
    public enum VFXSmoothingPolicy
    {
        /// <summary>
        /// No smoothing (sharp corners)
        /// </summary>
        None,

        /// <summary>
        /// Fast smoothing (lower quality, better performance)
        /// </summary>
        Fast,

        /// <summary>
        /// Best quality smoothing (higher quality, more expensive)
        /// </summary>
        Best
    }
}
