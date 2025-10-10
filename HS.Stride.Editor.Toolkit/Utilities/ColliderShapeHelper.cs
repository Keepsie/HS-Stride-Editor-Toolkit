// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using HS.Stride.Editor.Toolkit.Core.AssetEditing;

namespace HS.Stride.Editor.Toolkit.Utilities
{
    /// <summary>
    /// Helper class for creating collider shape dictionaries for StaticColliderComponent and RigidbodyComponent.
    /// </summary>
    public static class ColliderShapeHelper
    {
        /// <summary>
        /// Creates a BoxColliderShapeDesc.
        /// </summary>
        /// <param name="sizeX">Width (X dimension)</param>
        /// <param name="sizeY">Height (Y dimension)</param>
        /// <param name="sizeZ">Depth (Z dimension)</param>
        /// <param name="is2D">Whether this is a 2D collider</param>
        /// <param name="offsetX">Local offset X</param>
        /// <param name="offsetY">Local offset Y</param>
        /// <param name="offsetZ">Local offset Z</param>
        public static Dictionary<string, object> CreateBoxShape(
            float sizeX = 1.0f,
            float sizeY = 1.0f,
            float sizeZ = 1.0f,
            bool is2D = false,
            float offsetX = 0.0f,
            float offsetY = 0.0f,
            float offsetZ = 0.0f)
        {
            return new Dictionary<string, object>
            {
                ["!BoxColliderShapeDesc"] = "",
                ["Is2D"] = is2D,
                ["Size"] = new Dictionary<string, object>
                {
                    ["X"] = sizeX,
                    ["Y"] = sizeY,
                    ["Z"] = sizeZ
                },
                ["LocalOffset"] = new Dictionary<string, object>
                {
                    ["X"] = offsetX,
                    ["Y"] = offsetY,
                    ["Z"] = offsetZ
                },
                ["LocalRotation"] = new Dictionary<string, object>
                {
                    ["X"] = 0.0f,
                    ["Y"] = 0.0f,
                    ["Z"] = 0.0f,
                    ["W"] = 1.0f
                }
            };
        }

        /// <summary>
        /// Creates a SphereColliderShapeDesc.
        /// </summary>
        /// <param name="radius">Sphere radius</param>
        /// <param name="is2D">Whether this is a 2D collider</param>
        /// <param name="offsetX">Local offset X</param>
        /// <param name="offsetY">Local offset Y</param>
        /// <param name="offsetZ">Local offset Z</param>
        public static Dictionary<string, object> CreateSphereShape(
            float radius = 0.5f,
            bool is2D = false,
            float offsetX = 0.0f,
            float offsetY = 0.0f,
            float offsetZ = 0.0f)
        {
            return new Dictionary<string, object>
            {
                ["!SphereColliderShapeDesc"] = "",
                ["Is2D"] = is2D,
                ["Radius"] = radius,
                ["LocalOffset"] = new Dictionary<string, object>
                {
                    ["X"] = offsetX,
                    ["Y"] = offsetY,
                    ["Z"] = offsetZ
                }
            };
        }

        /// <summary>
        /// Creates a CapsuleColliderShapeDesc.
        /// </summary>
        /// <param name="length">Capsule length</param>
        /// <param name="radius">Capsule radius</param>
        /// <param name="orientation">Orientation (UpX, UpY, or UpZ)</param>
        /// <param name="offsetX">Local offset X</param>
        /// <param name="offsetY">Local offset Y</param>
        /// <param name="offsetZ">Local offset Z</param>
        public static Dictionary<string, object> CreateCapsuleShape(
            float length = 1.0f,
            float radius = 0.5f,
            string orientation = "UpY",
            float offsetX = 0.0f,
            float offsetY = 0.0f,
            float offsetZ = 0.0f)
        {
            return new Dictionary<string, object>
            {
                ["!CapsuleColliderShapeDesc"] = "",
                ["Length"] = length,
                ["Radius"] = radius,
                ["Orientation"] = orientation,
                ["LocalOffset"] = new Dictionary<string, object>
                {
                    ["X"] = offsetX,
                    ["Y"] = offsetY,
                    ["Z"] = offsetZ
                },
                ["LocalRotation"] = new Dictionary<string, object>
                {
                    ["X"] = 0.0f,
                    ["Y"] = 0.0f,
                    ["Z"] = 0.0f,
                    ["W"] = 1.0f
                }
            };
        }

        /// <summary>
        /// Creates a CylinderColliderShapeDesc.
        /// </summary>
        /// <param name="height">Cylinder height</param>
        /// <param name="radius">Cylinder radius</param>
        /// <param name="orientation">Orientation (UpX, UpY, or UpZ)</param>
        /// <param name="offsetX">Local offset X</param>
        /// <param name="offsetY">Local offset Y</param>
        /// <param name="offsetZ">Local offset Z</param>
        public static Dictionary<string, object> CreateCylinderShape(
            float height = 1.0f,
            float radius = 0.5f,
            string orientation = "UpY",
            float offsetX = 0.0f,
            float offsetY = 0.0f,
            float offsetZ = 0.0f)
        {
            return new Dictionary<string, object>
            {
                ["!CylinderColliderShapeDesc"] = "",
                ["Height"] = height,
                ["Radius"] = radius,
                ["Orientation"] = orientation,
                ["LocalOffset"] = new Dictionary<string, object>
                {
                    ["X"] = offsetX,
                    ["Y"] = offsetY,
                    ["Z"] = offsetZ
                },
                ["LocalRotation"] = new Dictionary<string, object>
                {
                    ["X"] = 0.0f,
                    ["Y"] = 0.0f,
                    ["Z"] = 0.0f,
                    ["W"] = 1.0f
                }
            };
        }

        /// <summary>
        /// Creates a ConeColliderShapeDesc.
        /// </summary>
        /// <param name="height">Cone height</param>
        /// <param name="radius">Cone base radius</param>
        /// <param name="orientation">Orientation (UpX, UpY, or UpZ)</param>
        /// <param name="offsetX">Local offset X</param>
        /// <param name="offsetY">Local offset Y</param>
        /// <param name="offsetZ">Local offset Z</param>
        public static Dictionary<string, object> CreateConeShape(
            float height = 1.0f,
            float radius = 0.5f,
            string orientation = "UpY",
            float offsetX = 0.0f,
            float offsetY = 0.0f,
            float offsetZ = 0.0f)
        {
            return new Dictionary<string, object>
            {
                ["!ConeColliderShapeDesc"] = "",
                ["Height"] = height,
                ["Radius"] = radius,
                ["Orientation"] = orientation,
                ["LocalOffset"] = new Dictionary<string, object>
                {
                    ["X"] = offsetX,
                    ["Y"] = offsetY,
                    ["Z"] = offsetZ
                },
                ["LocalRotation"] = new Dictionary<string, object>
                {
                    ["X"] = 0.0f,
                    ["Y"] = 0.0f,
                    ["Z"] = 0.0f,
                    ["W"] = 1.0f
                }
            };
        }

        /// <summary>
        /// Creates a StaticMeshColliderShapeDesc using another model as the collision mesh.
        /// CRITICAL: This allows using a simplified collision mesh instead of the visual model.
        /// </summary>
        /// <param name="modelAsset">Asset reference from ProjectAssetScanner</param>
        public static Dictionary<string, object> CreateStaticMeshShape(AssetReference modelAsset)
        {
            if (modelAsset == null)
                throw new ArgumentNullException(nameof(modelAsset));

            return new Dictionary<string, object>
            {
                ["!StaticMeshColliderShapeDesc"] = "",
                ["Model"] = $"{modelAsset.Id}:{modelAsset.Path}"
            };
        }

        /// <summary>
        /// Creates a StaticMeshColliderShapeDesc using another model as the collision mesh.
        /// CRITICAL: This allows using a simplified collision mesh instead of the visual model.
        /// </summary>
        /// <param name="modelGuid">GUID of the model to use for collision</param>
        /// <param name="modelPath">Path to the model asset (e.g., "Models/CollisionMesh")</param>
        public static Dictionary<string, object> CreateStaticMeshShape(
            string modelGuid,
            string modelPath)
        {
            if (string.IsNullOrWhiteSpace(modelGuid))
                throw new ArgumentNullException(nameof(modelGuid));
            if (string.IsNullOrWhiteSpace(modelPath))
                throw new ArgumentNullException(nameof(modelPath));

            return new Dictionary<string, object>
            {
                ["!StaticMeshColliderShapeDesc"] = "",
                ["Model"] = $"{modelGuid}:{modelPath}"
            };
        }

        /// <summary>
        /// Creates a ConvexHullColliderShapeDesc using another model.
        /// Creates a convex hull around the model's vertices.
        /// </summary>
        /// <param name="modelAsset">Asset reference from ProjectAssetScanner</param>
        public static Dictionary<string, object> CreateConvexHullShape(AssetReference modelAsset)
        {
            if (modelAsset == null)
                throw new ArgumentNullException(nameof(modelAsset));

            return new Dictionary<string, object>
            {
                ["!ConvexHullColliderShapeDesc"] = "",
                ["Model"] = $"{modelAsset.Id}:{modelAsset.Path}"
            };
        }

        /// <summary>
        /// Creates a ConvexHullColliderShapeDesc using another model.
        /// Creates a convex hull around the model's vertices.
        /// </summary>
        /// <param name="modelGuid">GUID of the model to use</param>
        /// <param name="modelPath">Path to the model asset</param>
        public static Dictionary<string, object> CreateConvexHullShape(
            string modelGuid,
            string modelPath)
        {
            if (string.IsNullOrWhiteSpace(modelGuid))
                throw new ArgumentNullException(nameof(modelGuid));
            if (string.IsNullOrWhiteSpace(modelPath))
                throw new ArgumentNullException(nameof(modelPath));

            return new Dictionary<string, object>
            {
                ["!ConvexHullColliderShapeDesc"] = "",
                ["Model"] = $"{modelGuid}:{modelPath}"
            };
        }

        /// <summary>
        /// Creates a StaticPlaneColliderShapeDesc.
        /// Represents an infinite plane (useful for ground).
        /// </summary>
        /// <param name="normalX">Plane normal X component</param>
        /// <param name="normalY">Plane normal Y component</param>
        /// <param name="normalZ">Plane normal Z component</param>
        /// <param name="offset">Offset along the normal</param>
        public static Dictionary<string, object> CreateStaticPlaneShape(
            float normalX = 0.0f,
            float normalY = 1.0f,
            float normalZ = 0.0f,
            float offset = 0.0f)
        {
            return new Dictionary<string, object>
            {
                ["!StaticPlaneColliderShapeDesc"] = "",
                ["Normal"] = new Dictionary<string, object>
                {
                    ["X"] = normalX,
                    ["Y"] = normalY,
                    ["Z"] = normalZ
                },
                ["Offset"] = offset
            };
        }

        /// <summary>
        /// Creates a complete StaticColliderComponent with one or more collider shapes.
        /// </summary>
        /// <param name="shapes">One or more collider shapes (use Create*Shape methods)</param>
        public static Dictionary<string, object> CreateStaticColliderComponent(params Dictionary<string, object>[] shapes)
        {
            if (shapes == null || shapes.Length == 0)
                throw new ArgumentException("At least one collider shape is required", nameof(shapes));

            var colliderShapes = new Dictionary<string, object>();
            foreach (var shape in shapes)
            {
                var shapeGuid = GuidHelper.NewGuidNoDashes();
                colliderShapes[shapeGuid] = shape;
            }

            return new Dictionary<string, object>
            {
                ["CanSleep"] = false,
                ["Restitution"] = 0.0f,
                ["Friction"] = 0.5f,
                ["RollingFriction"] = 0.0f,
                ["CcdMotionThreshold"] = 0.0f,
                ["CcdSweptSphereRadius"] = 0.0f,
                ["IsTrigger"] = false,
                ["AlwaysUpdateNaviMeshCache"] = false,
                ["ColliderShapes"] = colliderShapes
            };
        }

        /// <summary>
        /// Creates a complete RigidbodyComponent with one or more collider shapes.
        /// </summary>
        /// <param name="mass">Mass of the rigidbody</param>
        /// <param name="isKinematic">Whether the rigidbody is kinematic (not affected by physics)</param>
        /// <param name="shapes">One or more collider shapes (use Create*Shape methods)</param>
        public static Dictionary<string, object> CreateRigidbodyComponent(
            float mass = 1.0f,
            bool isKinematic = false,
            params Dictionary<string, object>[] shapes)
        {
            if (shapes == null || shapes.Length == 0)
                throw new ArgumentException("At least one collider shape is required", nameof(shapes));

            var colliderShapes = new Dictionary<string, object>();
            foreach (var shape in shapes)
            {
                var shapeGuid = GuidHelper.NewGuidNoDashes();
                colliderShapes[shapeGuid] = shape;
            }

            return new Dictionary<string, object>
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
                ["ColliderShapes"] = colliderShapes
            };
        }
    }
}
