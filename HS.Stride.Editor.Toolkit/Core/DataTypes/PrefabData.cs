// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using HS.Stride.Editor.Toolkit.Utilities;

namespace HS.Stride.Editor.Toolkit.Core.DataTypes
{
    /// <summary>
    /// Represents prefab instance data including source path and instance ID
    /// </summary>
    public class PrefabData
    {
        public string PrefabSourcePath { get; set; } = string.Empty;
        public string PrefabEntityId { get; set; } = string.Empty;
        public string InstanceId { get; set; }

        public PrefabData()
        {
            InstanceId = GuidHelper.NewGuid();
        }
    }
}

