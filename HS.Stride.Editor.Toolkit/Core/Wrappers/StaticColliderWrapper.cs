// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using HS.Stride.Editor.Toolkit.Core.AssetEditing;
using HS.Stride.Editor.Toolkit.Utilities;

namespace HS.Stride.Editor.Toolkit.Core.Wrappers
{
    /// <summary>
    /// Wrapper for StaticColliderComponent providing access to collider shapes and physics properties
    /// </summary>
    public class StaticColliderWrapper : ColliderWrapperBase
    {
        public StaticColliderWrapper(Component component) : base(component)
        {
        }

        /// <summary>
        /// Creates a new StaticColliderComponent with default values
        /// </summary>
        public static Component CreateComponent()
        {
            return new Component
            {
                Type = "StaticColliderComponent",
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
                    ["AlwaysUpdateNaviMeshCache"] = false,
                    ["ColliderShapes"] = new Dictionary<string, object>()
                }
            };
        }

        public bool CanSleep
        {
            get => Component.Get<bool?>("CanSleep") ?? false;
            set => Component.Set("CanSleep", value);
        }

        public float Restitution
        {
            get => Component.Get<float?>("Restitution") ?? 0.0f;
            set => Component.Set("Restitution", value);
        }

        public float Friction
        {
            get => Component.Get<float?>("Friction") ?? 0.5f;
            set => Component.Set("Friction", value);
        }

        public float RollingFriction
        {
            get => Component.Get<float?>("RollingFriction") ?? 0.0f;
            set => Component.Set("RollingFriction", value);
        }

        public bool IsTrigger
        {
            get => Component.Get<bool?>("IsTrigger") ?? false;
            set => Component.Set("IsTrigger", value);
        }

        // Override to add YAML marker normalization
        public new Dictionary<string, object> ColliderShapes
        {
            get
            {
                var shapes = Component.GetMultiValueProperty("ColliderShapes") ?? new Dictionary<string, object>();

                // Normalize shapes to ensure a YAML type marker key exists (e.g., "!BoxColliderShapeDesc")
                // This helps round-trip parsing so tests can detect the shape types reliably.
                var shapeKeys = new List<string>(shapes.Keys);
                foreach (var shapeKey in shapeKeys)
                {
                    if (shapes[shapeKey] is Dictionary<string, object> shapeDict)
                    {
                        // Check if any existing key starts with '!' (already has a marker)
                        bool hasMarker = false;
                        foreach (var k in shapeDict.Keys)
                        {
                            if (k.StartsWith("!"))
                            {
                                hasMarker = true;
                                break;
                            }
                        }

                        if (!hasMarker)
                        {
                            // Infer marker by characteristic properties
                            if (shapeDict.ContainsKey("Size"))
                            {
                                // Box has Size + LocalRotation typically
                                shapeDict["!BoxColliderShapeDesc"] = "";
                            }
                            else if (shapeDict.ContainsKey("Length") && shapeDict.ContainsKey("Radius") && shapeDict.ContainsKey("Orientation"))
                            {
                                // Capsule has Length + Radius + Orientation
                                shapeDict["!CapsuleColliderShapeDesc"] = "";
                            }
                            else if (shapeDict.ContainsKey("Height") && shapeDict.ContainsKey("Radius") && shapeDict.ContainsKey("Orientation"))
                            {
                                // Cylinder has Height + Radius + Orientation
                                shapeDict["!CylinderColliderShapeDesc"] = "";
                            }
                            else if (shapeDict.ContainsKey("Radius") && !shapeDict.ContainsKey("Length"))
                            {
                                // Sphere has Radius only (no Length)
                                shapeDict["!SphereColliderShapeDesc"] = "";
                            }
                            else if (shapeDict.ContainsKey("Normal") && shapeDict.ContainsKey("Offset"))
                            {
                                // Plane has Normal + Offset
                                shapeDict["!StaticPlaneColliderShapeDesc"] = "";
                            }
                            else if (shapeDict.ContainsKey("Model"))
                            {
                                // Mesh-like shapes have Model; default to StaticMesh if unknown
                                if (!shapeDict.ContainsKey("!ConvexHullColliderShapeDesc") && !shapeDict.ContainsKey("!StaticMeshColliderShapeDesc"))
                                {
                                    shapeDict["!StaticMeshColliderShapeDesc"] = "";
                                }
                            }

                            shapes[shapeKey] = shapeDict;
                        }
                    }
                }

                return shapes;
            }
            set => Component.Set("ColliderShapes", value);
        }
    }
}
