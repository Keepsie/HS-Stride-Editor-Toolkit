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
        /// Sets the material for a specific slot. If the slot already has a material, replaces it.
        /// If the slot is empty, creates a new material entry.
        /// </summary>
        /// <param name="slotIndex">The material slot index (0, 1, 2, etc.)</param>
        /// <param name="material">The material asset to set</param>
        public void SetMaterial(int slotIndex, AssetReference material)
        {
            var materials = Materials;

            // Find the existing key that ends with ~{slotIndex}
            var slotKey = materials.Keys.FirstOrDefault(k => k.EndsWith($"~{slotIndex}"));

            if (slotKey != null)
            {
                // Slot exists - replace the material reference (keep the same key)
                materials[slotKey] = material.Reference;
            }
            else
            {
                // Slot doesn't exist - create new entry with hash~index format
                var slotKeyHash = Utilities.GuidHelper.NewGuidNoDashes();
                slotKey = $"{slotKeyHash}~{slotIndex}";
                materials[slotKey] = material.Reference;
            }

            Materials = materials;
        }
    }
}
