// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using HS.Stride.Editor.Toolkit.Core.DataTypes;

namespace HS.Stride.Editor.Toolkit.Core.Wrappers
{
    /// <summary>
    /// Wrapper for TransformComponent providing easy access to position, rotation, and scale
    /// </summary>
    public class TransformWrapper
    {
        public Component Component { get; private set; }

        public TransformWrapper(Component component)
        {
            Component = component;
        }

        /// <summary>
        /// Gets the current position as a Vector3Data
        /// </summary>
        public Vector3Data GetPosition()
        {
            return Vector3Data.FromMultiValueProperty(Component.GetMultiValueProperty("Position"));
        }

        /// <summary>
        /// Gets the current rotation as a QuaternionData
        /// </summary>
        public QuaternionData GetRotation()
        {
            return QuaternionData.FromMultiValueProperty(Component.GetMultiValueProperty("Rotation"));
        }

        /// <summary>
        /// Gets the current scale as a Vector3Data
        /// </summary>
        public Vector3Data GetScale()
        {
            return Vector3Data.FromMultiValueProperty(Component.GetMultiValueProperty("Scale"));
        }

        /// <summary>
        /// Sets the position in world space
        /// </summary>
        public void SetPosition(float x, float y, float z)
        {
            Component.Set("Position", new Vector3Data(x, y, z).ToMultiValueProperty());
        }

        /// <summary>
        /// Sets the rotation using quaternion components
        /// </summary>
        public void SetRotation(float x, float y, float z, float w)
        {
            Component.Set("Rotation", new QuaternionData(x, y, z, w).ToMultiValueProperty());
        }

        /// <summary>
        /// Sets the scale on each axis
        /// </summary>
        public void SetScale(float x, float y, float z)
        {
            Component.Set("Scale", new Vector3Data(x, y, z).ToMultiValueProperty());
        }

        /// <summary>
        /// Sets a uniform scale on all axes
        /// </summary>
        public void SetUniformScale(float scale)
        {
            Component.Set("Scale", new Vector3Data(scale, scale, scale).ToMultiValueProperty());
        }

        public Dictionary<string, object> GetChildren()
        {
            return Component.GetMultiValueProperty("Children") ?? new Dictionary<string, object>();
        }

        public void AddChild(string childEntityId)
        {
            var children = GetChildren();
            var childKey = Utilities.GuidHelper.NewGuidNoDashes();
            children[childKey] = $"ref!! {childEntityId}";
            Component.Set("Children", children);
        }

        public void RemoveChild(string childEntityId)
        {
            var children = GetChildren();
            var keyToRemove = children.FirstOrDefault(kvp =>
                kvp.Value is string strValue && strValue.Contains(childEntityId)
            ).Key;

            if (keyToRemove != null)
            {
                children.Remove(keyToRemove);
                Component.Set("Children", children);
            }
        }

        public bool HasChild(string childEntityId)
        {
            var children = GetChildren();
            return children.Any(kvp =>
                kvp.Value is string strValue && strValue.Contains(childEntityId)
            );
        }
    }
}
