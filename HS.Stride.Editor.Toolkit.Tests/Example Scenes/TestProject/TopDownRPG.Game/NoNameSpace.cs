
using Stride.Animations;
using Stride.Core;
using Stride.Core.Serialization;
using Stride.Engine;
using Stride.Rendering;
using System.Collections.Generic;


public class NoNameSpace : SyncScript
{
    // Primitives
    public int intValue;
    public float floatValue;
    public bool boolValue;
    public string stringValue;

    // Entity references
    public Entity singleEntity;

    [DataMember]
    public List<Entity> entityList { get; set; } = new List<Entity>();

    // Asset references (Prefab, Model, Material)
    public Prefab prefabRef;

    [DataMember]
    public List<Prefab> prefabList { get; set; } = new List<Prefab>();

    public Model modelRef;
    public Material materialRef;

    public UrlReference RawAsset { get; set; }

    // Arrays
    [DataMember]
    public int[] intArray { get; set; } = new int[0];

    [DataMember]
    public Entity[] entityArray { get; set; } = new Entity[0];

    [DataMember]
    public List<int> Stuff2 { get; set; } = new List<int>();

    [DataMember]
    public Dictionary<string, AnimationClip> AnimationClips = new Dictionary<string, AnimationClip>();

    [DataMember]
    public Dictionary<int, string> primClips = new Dictionary<int, string>();

    // Component reference
    public TransformComponent transformRef;

    public override void Start()
    {
        // Initialization of the script.
    }

    public override void Update()
    {
        // Do stuff every new frame
    }
}

