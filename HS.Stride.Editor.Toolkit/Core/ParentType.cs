// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

namespace HS.Stride.Editor.Toolkit.Core
{
    /// <summary>
    /// Defines how an entity should be organized in the scene hierarchy.
    /// </summary>
    public enum ParentType
    {
        /// <summary>
        /// Entity uses a Folder for organization (metadata only, no transform hierarchy).
        /// Similar to Unity folders - entities appear grouped in the editor but have no parent transform.
        /// </summary>
        Folder,

        /// <summary>
        /// Entity is parented under another entity (affects transform hierarchy).
        /// Child transforms are relative to parent. Moving parent moves all children.
        /// </summary>
        Entity
    }
}
