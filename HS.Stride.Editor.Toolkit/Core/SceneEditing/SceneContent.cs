// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

namespace HS.Stride.Editor.Toolkit.Core.SceneEditing
{
    /// <summary>
    /// Internal representation of scene file content
    /// </summary>
    public class SceneContent
    {
        public string Id { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string RawContent { get; set; } = string.Empty;
        public List<string> RootEntityIds { get; set; } = new();
        public List<Entity> Entities { get; set; } = new();
        public StrideProject? ParentProject { get; set; }
    }
}
