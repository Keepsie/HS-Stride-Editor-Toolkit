// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using HS.Stride.Editor.Toolkit.Core.AssetEditing;
using HS.Stride.Editor.Toolkit.Core.DataTypes;
using HS.Stride.Editor.Toolkit.Utilities;

namespace HS.Stride.Editor.Toolkit.Core.Wrappers
{
    /// <summary>
    /// Wrapper for RigidbodyComponent providing access to physics properties and collider shapes
    /// </summary>
    public class RigidbodyWrapper : ColliderWrapperBase
    {
        public RigidbodyWrapper(Component component) : base(component)
        {
        }

        /// <summary>
        /// Creates a new RigidbodyComponent with specified mass and kinematic settings
        /// </summary>
        public static Component CreateComponent(float mass = 1.0f, bool isKinematic = false)
        {
            return new Component
            {
                Type = "RigidbodyComponent",
                Id = GuidHelper.NewGuid(),
                Properties = new Dictionary<string, object>
                {
                    ["CanSleep"] = false,
                    ["Restitution"] = 0.0f,
                    ["Friction"] = 0.5f,
                    ["RollingFriction"] = 0.0f,
                    ["CcdMotionThreshold"] = 0.0f,
                    ["CcdSweptSphereRadius"] = 0.0f,
                    ["IsTrigger"] = false,
                    ["IsKinematic"] = isKinematic,
                    ["Mass"] = mass,
                    ["LinearDamping"] = 0.0f,
                    ["AngularDamping"] = 0.0f,
                    ["OverrideGravity"] = false,
                    ["Gravity"] = new Dictionary<string, object>
                    {
                        ["X"] = 0.0f,
                        ["Y"] = 0.0f,
                        ["Z"] = 0.0f
                    },
                    ["NodeName"] = null,
                    ["ColliderShapes"] = new Dictionary<string, object>()
                }
            };
        }

        public bool CanSleep
        {
            get => Component.Get<bool?>("CanSleep") ?? false;
            set => Component.Set("CanSleep", value);
        }

        public bool IsKinematic
        {
            get => Component.Get<bool?>("IsKinematic") ?? false;
            set => Component.Set("IsKinematic", value);
        }

        public float Mass
        {
            get => Component.Get<float?>("Mass") ?? 1.0f;
            set => Component.Set("Mass", value);
        }

        public float LinearDamping
        {
            get => Component.Get<float?>("LinearDamping") ?? 0.0f;
            set => Component.Set("LinearDamping", value);
        }

        public float AngularDamping
        {
            get => Component.Get<float?>("AngularDamping") ?? 0.0f;
            set => Component.Set("AngularDamping", value);
        }

        public bool OverrideGravity
        {
            get => Component.Get<bool?>("OverrideGravity") ?? false;
            set => Component.Set("OverrideGravity", value);
        }

        /// <summary>
        /// Gets the custom gravity vector
        /// </summary>
        public Vector3Data GetGravity()
        {
            return Vector3Data.FromMultiValueProperty(Component.GetMultiValueProperty("Gravity"));
        }

        /// <summary>
        /// Sets a custom gravity vector
        /// </summary>
        public void SetGravity(float x, float y, float z)
        {
            Component.Set("Gravity", new Vector3Data(x, y, z).ToMultiValueProperty());
        }
    }
}
