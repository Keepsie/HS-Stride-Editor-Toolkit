// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

namespace HS.Stride.Editor.Toolkit.Core.Wrappers
{
    /// <summary>
    /// Standard VFX particle shape types used in Stride particle systems.
    /// </summary>
    public enum VFXShapeType
    {
        /// <summary>
        /// Billboard particles always face the camera
        /// </summary>
        Billboard,

        /// <summary>
        /// Quad particles with fixed orientation
        /// </summary>
        Quad,

        /// <summary>
        /// Oriented quad particles with custom rotation
        /// </summary>
        OrientedQuad,

        /// <summary>
        /// Ribbon/trail particles connecting sequential particles
        /// </summary>
        Ribbon
    }
}
