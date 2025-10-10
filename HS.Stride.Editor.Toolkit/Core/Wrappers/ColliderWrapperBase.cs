// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using HS.Stride.Editor.Toolkit.Core.AssetEditing;
using HS.Stride.Editor.Toolkit.Utilities;

namespace HS.Stride.Editor.Toolkit.Core.Wrappers
{
    /// <summary>
    /// Base class for collider wrappers (StaticCollider and Rigidbody) that share shape methods.
    /// </summary>
    public abstract class ColliderWrapperBase
    {
        public Component Component { get; protected set; }

        protected ColliderWrapperBase(Component component)
        {
            Component = component;
        }

        public Dictionary<string, object> ColliderShapes
        {
            get => Component.GetMultiValueProperty("ColliderShapes") ?? new Dictionary<string, object>();
            set => Component.Set("ColliderShapes", value);
        }

        // Add shape methods
        public void AddBoxShape(float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, bool is2D = false, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f)
        {
            var shapes = ColliderShapes;
            var shapeGuid = GuidHelper.NewGuidNoDashes();
            shapes[shapeGuid] = ColliderShapeHelper.CreateBoxShape(sizeX, sizeY, sizeZ, is2D, offsetX, offsetY, offsetZ);
            ColliderShapes = shapes;
        }

        public void AddSphereShape(float radius = 0.5f, bool is2D = false, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f)
        {
            var shapes = ColliderShapes;
            var shapeGuid = GuidHelper.NewGuidNoDashes();
            shapes[shapeGuid] = ColliderShapeHelper.CreateSphereShape(radius, is2D, offsetX, offsetY, offsetZ);
            ColliderShapes = shapes;
        }

        public void AddCapsuleShape(float length = 1.0f, float radius = 0.5f, string orientation = "UpY", float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f)
        {
            var shapes = ColliderShapes;
            var shapeGuid = GuidHelper.NewGuidNoDashes();
            shapes[shapeGuid] = ColliderShapeHelper.CreateCapsuleShape(length, radius, orientation, offsetX, offsetY, offsetZ);
            ColliderShapes = shapes;
        }

        public void AddCylinderShape(float height = 1.0f, float radius = 0.5f, string orientation = "UpY", float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f)
        {
            var shapes = ColliderShapes;
            var shapeGuid = GuidHelper.NewGuidNoDashes();
            shapes[shapeGuid] = ColliderShapeHelper.CreateCylinderShape(height, radius, orientation, offsetX, offsetY, offsetZ);
            ColliderShapes = shapes;
        }

        public void AddConeShape(float height = 1.0f, float radius = 0.5f, string orientation = "UpY", float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f)
        {
            var shapes = ColliderShapes;
            var shapeGuid = GuidHelper.NewGuidNoDashes();
            shapes[shapeGuid] = ColliderShapeHelper.CreateConeShape(height, radius, orientation, offsetX, offsetY, offsetZ);
            ColliderShapes = shapes;
        }

        public void AddMeshShape(AssetReference modelAsset)
        {
            var shapes = ColliderShapes;
            var shapeGuid = GuidHelper.NewGuidNoDashes();
            shapes[shapeGuid] = ColliderShapeHelper.CreateStaticMeshShape(modelAsset);
            ColliderShapes = shapes;
        }

        public void AddConvexHullShape(AssetReference modelAsset)
        {
            var shapes = ColliderShapes;
            var shapeGuid = GuidHelper.NewGuidNoDashes();
            shapes[shapeGuid] = ColliderShapeHelper.CreateConvexHullShape(modelAsset);
            ColliderShapes = shapes;
        }

        public void AddPlaneShape(float normalX = 0.0f, float normalY = 1.0f, float normalZ = 0.0f, float offset = 0.0f)
        {
            var shapes = ColliderShapes;
            var shapeGuid = GuidHelper.NewGuidNoDashes();
            shapes[shapeGuid] = ColliderShapeHelper.CreateStaticPlaneShape(normalX, normalY, normalZ, offset);
            ColliderShapes = shapes;
        }
    }
}
