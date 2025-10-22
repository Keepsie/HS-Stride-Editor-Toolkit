# HS Stride Editor Toolkit

A library for creating custom editor tools for Stride. Batch task automation for scenes. Create UI and prefabs via code. Edit assets programmatically. Build CLI or GUI tools for repetitive editor work.

## 🔄 How This Works (Not Like Unity)

**This is NOT live editor scripting.** You're not connected to a running Stride editor or working with in-memory scenes.

**This is file-based batch automation.** Close the editor, run your script, open the editor to see the results.

### Batch Operation Workflow

**Manual workflow (the old way):**

```
1. Import large blockout from Blender (200+ objects)
2. Open scene in GameStudio
3. Click each entity one by one
4. Add StaticColliderComponent manually
5. Set collider properties
6. Realize you missed one
7. Repeat for 1+ hour 🙃
```

**Batch automation workflow (with this toolkit):**

```
1. Plan: "Add box colliders to all BlockOut_* entities"
2. Close Stride (avoid file conflicts)
3. Run your script - edits .sdscene files directly
4. Open Stride - see 200 colliders added in 10 seconds
```

## 🚀 Quick Start

### Installation

```bash
dotnet add package HS.Stride.Editor.Toolkit
```

**📖 For complete API documentation:** See [API.md](API.md) at the root of this repository.

**🤖 AI-Friendly API Reference:** Use `hs_stride_editor_api_for_llms.txt` to give AI assistants (ChatGPT/Claude) the complete API - they can generate tools for you!

**Below are practical examples** showing what you can do. Highlights include bulk scene operations and programmatic UI creation - especially useful if you've struggled with Stride's UI editor.

### 🚨 BACKUP FIRST!

**Before using this toolkit, ALWAYS backup your project:**

- Commit to version control (Git)
- Create a backup copy of your project folder
- Test on a copy first

See [full disclaimer below](#️-important-backup--disclaimers) for important legal terms.

### Basic Example

```csharp
using HS.Stride.Editor.Toolkit.Core;

var project = new StrideProject(@"C:\MyGame");
var scene = project.LoadScene("Level1");
var blockouts = scene.FindEntitiesByName("BlockOut_*");

foreach (var entity in blockouts) {
    var collider = entity.AddStaticCollider();
    collider.AddBoxShape(10f, 2f, 10f);
    collider.Friction = 0.5f;
}

scene.Save();
```

## 💡 Common Use Cases

### 1. Procedural Generation

Generate levels, dungeons, or cities programmatically and save them as permanent scenes:

```csharp
var project = new StrideProject(@"C:\MyGame");
var scene = project.LoadScene("ProceduralDungeon");

// Generate 10x10 dungeon grid
for (int x = 0; x < 10; x++) {
    for (int z = 0; z < 10; z++) {
        var room = scene.CreateEntity($"Room_{x}_{z}", "Dungeon/Rooms");
        var transform = room.GetTransform();
        transform.SetPosition(x * 20f, 0f, z * 20f);

        // Add floor collider
        room.AddStaticCollider().AddBoxShape(20f, 1f, 20f);

        // Instantiate room prefab
        var roomPrefab = project.FindAsset("DungeonRoom", AssetType.Prefab);
        scene.InstantiatePrefab(roomPrefab, room);
    }
}

scene.Save();
```

### 3. Create Prefabs Programmatically

Mass-generate enemy types or props:

```csharp
var project = new StrideProject(@"C:\MyGame");

var enemyTypes = new Dictionary<string, (float health, float speed)> {
    ["Goblin"] = (50f, 3.0f),
    ["Orc"] = (100f, 2.0f),
    ["Troll"] = (200f, 1.5f)
};

foreach (var enemy in enemyTypes) {
    var prefab = project.CreatePrefab(enemy.Key, $"Prefabs/Enemies");
    var root = prefab.GetRootEntity();

    root.AddModel();
    root.AddStaticCollider().AddCapsuleShape(0.5f, 2.0f);

    var ai = root.AddComponent("EnemyAI");
    ai.Set("MaxHealth", enemy.Value.health);
    ai.Set("MoveSpeed", enemy.Value.speed);

    prefab.Save();
}

project.Rescan();
Console.WriteLine($"Generated {enemyTypes.Count} enemy prefabs!");
```

### 4. Create UI with Code

Generate entire UI menus instead of clicking through Stride's UI editor:

```csharp
var project = new StrideProject(@"C:\MyGame");
var page = project.CreateUIPage("MainMenu", "UI/Menus");

var canvas = page.CreateCanvas("menu_canvas", width: 800f, height: 600f);

// Title
var title = page.CreateTextBlock("title", "MY AWESOME GAME", canvas, fontSize: 50f);
title.SetMargin(top: 100f);
title.SetAlignment(horizontal: "Center");

// Buttons
var startButton = page.CreateButton("start_btn", "Start Game", canvas, width: 300f, height: 60f);
startButton.SetMargin(left: 250f, top: 250f);

var settingsButton = page.CreateButton("settings_btn", "Settings", canvas, width: 300f, height: 60f);
settingsButton.SetMargin(left: 250f, top: 330f);

var quitButton = page.CreateButton("quit_btn", "Quit", canvas, width: 300f, height: 60f);
quitButton.SetMargin(left: 250f, top: 410f);

page.Save();
project.Rescan();
```

## ⚠️ IMPORTANT: Backup & Disclaimers

### 🚨 ALWAYS BACKUP YOUR SCENES AND PROJECT BEFORE USE

**This toolkit directly modifies Stride asset files. Changes are permanent and cannot be undone.**

**Before using this toolkit:**

1. ✅ **Commit your project to version control (Git)**
2. ✅ **Create a backup copy of your entire project folder**
3. ✅ **Test on a copy of your project first**
4. ✅ **Verify changes in Stride GameStudio after running scripts**

### ⚖️ Liability Disclaimer

**THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND.**

- ❌ We are **NOT responsible** for any data loss, project corruption, or issues caused by using this toolkit
- ❌ We provide **NO guarantees** that this toolkit will work correctly with your specific project
- ❌ You use this toolkit **entirely at your own risk**
- ✅ **Always maintain backups** - this is your responsibility

By using this toolkit, you acknowledge that:

- You understand the risks of automated file modification
- You have adequate backups of your work
- You accept full responsibility for any consequences

**See the [LICENSE](LICENSE) file for complete legal terms.**

**Close and reopen GameStudio after running scripts** - Changes won't show until you restart.

## � Development & Contributions

This tool was built to fulfill my own needs for Stride editor automation. I'll update it when it makes sense and do my best to keep the docs current.

If you encounter issues or have use cases that would benefit the community:

- **Report bugs:** Open an issue on GitHub
- **Request features:** Open an issue describing your use case
- **Contribute:** PRs are welcome - feel free to add features yourself

## 📄 License

Apache License 2.0 - see LICENSE for full text.

---

Copyright © 2025 Happenstance Games LLC
