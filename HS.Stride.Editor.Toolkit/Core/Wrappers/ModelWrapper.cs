// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using HS.Stride.Editor.Toolkit.Core.AssetEditing;


namespace HS.Stride.Editor.Toolkit.Core.Wrappers
{
    /// <summary>
    /// Wrapper for ModelComponent providing easy access to model and material properties
    /// </summary>
    public class ModelWrapper
    {
        public Component Component { get; private set; }

        public ModelWrapper(Component component)
        {
            Component = component;
        }

        /// <summary>
        /// Creates a new ModelComponent with default values
        /// </summary>
        public static Component CreateComponent()
        {
            return new Component
            {
                Type = "ModelComponent",
                Id = Utilities.GuidHelper.NewGuid(),
                Properties = new Dictionary<string, object>
                {
                    ["Enabled"] = true,
                    ["Model"] = null,
                    ["Materials"] = new Dictionary<string, object>()
                }
            };
        }

        public bool Enabled
        {
            get => Component.Get<bool?>("Enabled") ?? true;
            set => Component.Set("Enabled", value);
        }

        public string Model
        {
            get => Component.Get<string>("Model") ?? string.Empty;
            set => Component.Set("Model", value);
        }

        public Dictionary<string, object> Materials
        {
            get => Component.GetMultiValueProperty("Materials") ?? new Dictionary<string, object>();
            set => Component.Set("Materials", value);
        }

        /// <summary>
        /// Sets the model using GUID and path
        /// </summary>
        public void SetModel(string guid, string path)
        {
            Model = $"{guid}:{path}";
        }

        /// <summary>
        /// Sets the model using an AssetReference
        /// </summary>
        public void SetModel(AssetReference modelAsset)
        {
            Model = modelAsset.Reference;
        }

        /// <summary>
        /// Adds a material to a specific slot using GUID and name
        /// </summary>
        public void AddMaterial(string slotKey, string materialGuid, string materialName)
        {
            var materials = Materials;
            materials[slotKey] = new Dictionary<string, object>
            {
                ["Name"] = materialName,
                ["MaterialInstance"] = new Dictionary<string, object>
                {
                    ["Material"] = $"{materialGuid}:{materialName}"
                }
            };
            Materials = materials;
        }

        /// <summary>
        /// Adds a material to a specific slot using an AssetReference
        /// </summary>
        public void AddMaterial(string slotKey, AssetReference materialAsset)
        {
            var materials = Materials;
            materials[slotKey] = new Dictionary<string, object>
            {
                ["Name"] = materialAsset.Name,
                ["MaterialInstance"] = new Dictionary<string, object>
                {
                    ["Material"] = materialAsset.Reference
                }
            };
            Materials = materials;
        }
    }
}
