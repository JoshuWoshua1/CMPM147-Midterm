# ItemTool - Procedural Item Generator

A tool for generating randomized items with modifiable properties, rarity levels, and modifier systems. Perfect for loot systems, inventory systems, or any game that needs dynamic item creation.

## Table of Contents

- [Quick Start](#quick-start)
- [Features](#features)
- [Installation](#installation)
- [Configuration](#configuration)
- [Usage](#usage)
- [Understanding Rarity](#understanding-rarity)
- [Loot Luck System](#loot-luck-system)
- [API Reference](#api-reference)
- [Examples](#examples)

## Quick Start

1. Add the `ItemTool` script to any GameObject in your scene
2. Create or provide a JSON file with item names and modifiers (see [Configuration](#configuration))
3. In the Inspector, set the `jsonFilePath` to point to your JSON file
4. Click **"Generate"** in the Inspector context menu to generate items
5. View the results in the `generatedHistory` list

## Features

- **Procedural Item Generation** - Create unique items on-demand or in bulk
- **Rarity System** - Items generate as Common, Rare, Epic, or Legendary with different properties
- **Modifier System** - Attach prefix and suffix modifiers to items (e.g., "+5 Damage", "of Fire")
- **Category Filters** - Group base names into categories (Weapons, Armor, Tools, etc.)
- **Modifier Restrictions** - Limit modifiers to specific base names or categories
- **Loot Luck** - Bias item rarity toward better drops based on a configurable luck value
- **Weight-Based Rarity** - Fine-tune the drop rate of each rarity level
- **Flexible Configuration** - All parameters are adjustable in the Unity Inspector
- **Unique Modifiers** - Prevent duplicate modifiers on a single item

## Installation

1. Copy `ItemTool.cs` to your `Assets/Scripts/` folder (contains all core classes)
2. Ensure Editor scripts are in `Assets/Editor/` folder:
   - `ItemToolEditor.cs`
   - `ItemToolLuckVisualizer.cs`
   - `GeneratedItemDrawer.cs`
3. Attach ItemTool to any GameObject in your scene
4. Add your item data JSON file to the project

## Configuration

### JSON Data Format

ItemTool expects a JSON file with the following structure:

```json
{
  "baseNames": ["Sword", "Axe", "Mace", "Dagger"],
  "categories": [
    { "name": "Weapons", "baseNames": ["Sword", "Axe", "Mace", "Dagger"] }
  ],
  "prefix": [
    {
      "template": "+{0} Damage",
      "namePrefix": "Sharp",
      "rare": { "min": 2, "max": 5 },
      "epic": { "min": 5, "max": 10 },
      "legendary": { "min": 10, "max": 20 },
      "allowedBaseNames": ["Weapons"]
    },
    {
      "template": "+{0} Attack Speed",
      "namePrefix": "Swift",
      "rare": { "min": 1, "max": 3 },
      "epic": { "min": 2, "max": 5 },
      "legendary": { "min": 5, "max": 10 }
    }
  ],
  "suffix": [
    {
      "template": "+{0} Fire Damage",
      "nameSuffix": "of Fire",
      "rare": { "min": 2, "max": 8 },
      "epic": { "min": 5, "max": 15 },
      "legendary": { "min": 10, "max": 30 }
    },
    {
      "template": "+{0}% Crit Chance",
      "nameSuffix": "of Precision",
      "rare": { "min": 5, "max": 15 },
      "epic": { "min": 10, "max": 25 },
      "legendary": { "min": 20, "max": 50 }
    }
  ]
}
```

**JSON Fields Explained:**
- `baseNames` - List of base weapon/item names
- `categories` - Optional grouping of base names (used by filters and modifier restrictions)
- `prefix` - Array of prefix modifiers (applied to the start of item names)
- `suffix` - Array of suffix modifiers (applied to the end of item names)
- `template` - The modifier text with `{0}` as the placeholder for the generated value
- `namePrefix/nameSuffix` - The descriptive text used in the item's display name
- `rare/epic/legendary` - Min/max value ranges for each rarity level
- `allowedBaseNames` - Optional list of base names or categories that can roll this modifier

### Inspector Parameters

#### Generation
- **Items To Generate** - How many items to create when clicking "Generate"

#### Data Source
- **JSON File Path** - Path to your JSON file relative to the Assets folder (e.g., `Scripts/ItemNames.json`)

#### Rarity Weights
- **Common Weight** - Base weight/probability for Common items (default: 60)
- **Rare Weight** - Base weight/probability for Rare items (default: 25)
- **Epic Weight** - Base weight/probability for Epic items (default: 10)
- **Legendary Weight** - Base weight/probability for Legendary items (default: 5)

#### Luck Bias
- **Loot Luck** - Your character's luck value (0-2500, default: 0)
  - 0 = No luck bonus, uses base weights
  - 2500 = Maximum luck, applies full multiplier boost
- **Luck Multipliers** (foldable menu) - Configures how much better each rarity becomes at max luck
  - **Common** - Multiplier at max luck (default: 1x)
  - **Rare** - Multiplier at max luck (default: 10x)
  - **Epic** - Multiplier at max luck (default: 70x)
  - **Legendary** - Multiplier at max luck (default: 250x)

#### Item Type Filter
- **Exclusive Generation** - Choose Any, Base Name, or Category
- **Specific Base Name** - Base name to force when Base Name is selected
- **Specific Category** - Category to force when Category is selected

## Usage

### During Development (Editor Only)

Right-click the ItemTool component in the Inspector to access quick commands:

1. **Generate** - Generates the number of items specified in `Items To Generate` and stores them in `generatedHistory`
2. **Show Rarity Percentages** - Logs the current rarity drop chances based on your Loot Luck setting (the luck visualizer is usually more informative)
3. **Open Luck Visualizer** - Button at bottom of Inspector opens a graph showing how luck affects drop rates

**Note:** These are testing tools only. They don't work in builds or during gameplay.

### During Runtime (In-Game)

ItemTool is designed to be called from other scripts during gameplay. Other systems (enemies, chests, quests) reference the ItemTool and call its methods when items need to be generated.

**Setup:**
1. Add ItemTool component to a GameObject in your scene (or make it a singleton)
2. Ensure your JSON file is accessible at runtime (place in `Resources` folder if needed)
3. Reference the ItemTool from your gameplay scripts

**Basic Integration:**

```csharp
// Get a reference to the ItemTool
public class LootManager : MonoBehaviour
{
    public ItemTool itemTool; // Assign in Inspector

    void DropLoot()
    {
        // Generate a single item with default settings
        GeneratedItem item = itemTool.GenerateSingleItem();
        Debug.Log($"Generated: {item.Name} ({item.Rarity})");
        
        // Use the item in your game
        DisplayItemToPlayer(item);
    }
}
```

**Advanced Integration:**

```csharp
// Generate a single item with custom options
var options = new ItemTool.ItemGenerationOptions
{
    BaseNameOverride = "Sword",           // Force a specific base name
  BaseNameCategoryOverride = "Weapons", // Or force a category
    LootLuckOverride = 2500,              // Use max luck for this item
    RarityOverride = "Legendary"          // Force a specific rarity
};
GeneratedItem legendary = itemTool.GenerateSingleItem(options);

// Get all available base names from JSON
List<string> baseNames = itemTool.GetAvailableBaseNames();
```

**What Works at Runtime:**
- `GenerateSingleItem()` - Generate items on demand
- `GenerateSingleItem(options)` - Generate with custom parameters
- `GetAvailableBaseNames()` - Get available item types
- `GetAvailableBaseCategories()` - Get available item categories
- All luck/weight/multiplier systems
- JSON loading and parsing

## Understanding Rarity

Items are generated with one of four rarity levels:

### Common
- **Modifiers:** None (just the base name)
- **Example:** "Sword"
- **Typical Drop Rate:** ~40-60% (depends on loot luck)

### Rare
- **Modifiers:** 1 prefix + 1 suffix
- **Example Name:** "Sharp Sword of Fire"
- **Example Modifiers:** "+3 Damage", "+5 Fire Damage"
- **Typical Drop Rate:** ~15-30% (depends on loot luck)
- **Modifier Ranges:** Smaller values (e.g., +2 to +5 damage)

### Epic
- **Modifiers:** 2 prefix + 2 suffix
- **Example Name:** "Swift Sword of Precision" (uses best prefix/suffix)
- **Example Modifiers:** "+7 Damage", "+4 Attack Speed", "+12 Fire Damage", "+18% Crit Chance"
- **Typical Drop Rate:** ~5-15% (depends on loot luck)
- **Modifier Ranges:** Medium values (e.g., +5 to +10 damage)

### Legendary
- **Modifiers:** 3 prefix + 3 suffix
- **Example Name:** "Mighty Sword of Devastation" (uses best prefix/suffix)
- **Example Modifiers:** "+15 Damage", "+8 Attack Speed", "+12 Strength", "+25 Fire Damage", "+35% Crit Chance", "+40% Crit Damage"
- **Typical Drop Rate:** ~1-5% (depends on loot luck)
- **Modifier Ranges:** Large values (e.g., +10 to +20 damage)

**Note:** For rarities Epic and Legendary the name only shows the highest-value prefix and suffix, but all modifiers still apply to the item

**Note:** Modifiers are unique per item (no duplicate prefix/suffix definitions)

## Loot Luck System

Loot Luck is a value (0-2500) that biases item rarity toward better drops:

### How It Works

1. **Base Rarity Weights** - Each rarity has a base weight (Common: 60, Rare: 25, Epic: 10, Legendary: 5)
2. **Luck Multiplier** - Based on your Loot Luck value, a multiplier is applied:
   - Luck 0 → 1x multiplier (no boost)
   - Luck 1250 → 0.5x multiplier
   - Luck 2500 → 1.0x multiplier (full boost)
3. **Final Weights** - Each rarity's weight is multiplied by its configured multiplier

### Example

With default settings:
- **Loot Luck 0:** Common ~60%, Rare ~25%, Epic ~10%, Legendary ~5%
- **Loot Luck 2500:** Common ~2.7%, Rare ~11.1%, Epic ~31.0%, Legendary ~55.3%

You can see exact percentages by right-clicking ItemTool and selecting **"Show Rarity Percentages"** or by opening the luck visualizer (recommended for most cases)

### Adjusting the Multipliers

Higher multipliers = more aggressive rarity scaling at max luck:
- **Conservative:** Set Rare/Epic/Legendary to 5/20/50 for gradual scaling
- **Aggressive:** Set to 10/70/250 (default) for steep scaling
- **Extreme:** Set to 50/300/1000 for dramatic scaling

## API Reference

### Public Methods

```csharp
// Generate a single item
GeneratedItem GenerateSingleItem()

// Generate a single item with custom options
GeneratedItem GenerateSingleItem(ItemGenerationOptions options)

// Get all available base names from the JSON file
List<string> GetAvailableBaseNames()

// Get all available base categories from the JSON file
List<string> GetAvailableBaseCategories()
```

### GeneratedItem Class

```csharp
public class GeneratedItem
{
    public string Name;                          // Full generated name
    public string Rarity;                        // "Common", "Rare", "Epic", or "Legendary"
    public List<string> PrefixModifiers;        // List of prefix modifiers applied
    public List<string> SuffixModifiers;        // List of suffix modifiers applied
}
```

### ItemGenerationOptions Struct

```csharp
public struct ItemGenerationOptions
{
    public string BaseNameOverride;    // Override base name (empty = random)
  public string BaseNameCategoryOverride; // Override base category (empty = random)
    public int LootLuckOverride;       // Override luck value (negative = use tool's setting)
    public string RarityOverride;      // Override rarity (empty = roll randomly)
}
```

### Extending Item Data (Stats, Armor, Damage)

If you want armor, damage, or other stats, you can keep the generator as-is and attach extra data in your own system (e.g., a wrapper class or dictionary keyed by the returned `GeneratedItem`). That approach avoids modifying the generator. I kept base stats out on purpose so users can implement them in whatever way fits their game. If you need those stats to be generated from JSON, then you’d extend `GeneratedItem`, update the JSON schema, and adjust the generator to populate those new fields.

### Data Classes (used internally)

These classes are automatically defined in ItemTool.cs and used for JSON parsing:

**ModifierRange** - Min/max value range for a modifier
**ModifierDefinition** - A single modifier with template and name parts
**ItemNameData** - Root data structure loaded from JSON

## Examples

### Example 1: Enemy Loot Drop (Runtime)

```csharp
public class Enemy : MonoBehaviour
{
    public ItemTool itemGenerator;
    public int enemyLootLuck = 100;
    
    void OnDeath()
    {
        var options = new ItemTool.ItemGenerationOptions
        {
            LootLuckOverride = enemyLootLuck
        };
        
        GeneratedItem drop = itemGenerator.GenerateSingleItem(options);
        
        // Display to player
        Debug.Log($"Enemy dropped: {drop.Name}");
        lootDisplay.ShowItem(drop);
    }
}
```

### Example 2: Chest Opening (Runtime)

```csharp
public class Chest : MonoBehaviour
{
    public ItemTool itemGenerator;
    public int itemCount = 3;
    
    void OnOpen()
    {
        for (int i = 0; i < itemCount; i++)
        {
            GeneratedItem item = itemGenerator.GenerateSingleItem();
            inventory.AddItem(item);
        }
    }
}
```

### Example 3: Boss Drop with Guaranteed Legendary (Runtime)

```csharp
public class Boss : MonoBehaviour
{
    public ItemTool itemGenerator;
    
    void OnDefeat()
    {
        var options = new ItemTool.ItemGenerationOptions
        {
            LootLuckOverride = 2500,     // Maximum luck
            RarityOverride = "Legendary" // Force legendary
        };
        
        GeneratedItem bossLoot = itemGenerator.GenerateSingleItem(options);
        player.GiveReward(bossLoot);
    }
}
```

### Example 4: Player Luck Stat Integration (Runtime)

```csharp
public class Player : MonoBehaviour
{
    public ItemTool itemGenerator;
    public int playerLuck = 0; // Increases as player levels up
    
    public void OpenLootBox()
    {
        // Use player's current luck stat
        var options = new ItemTool.ItemGenerationOptions
        {
            LootLuckOverride = playerLuck
        };
        
        GeneratedItem loot = itemGenerator.GenerateSingleItem(options);
        inventory.AddItem(loot);
    }
}
```

### Example 5: Check Probabilities (Editor Only - Testing)

```csharp
// This only works in the Editor during development
ItemTool itemTool = GetComponent<ItemTool>();

itemTool.lootLuck = 0;
itemTool.ShowRarityPercentages();      // Shows base percentages (luck visualizer is usually more informative)

itemTool.lootLuck = 2500;
itemTool.ShowRarityPercentages();      // Shows max luck percentages (luck visualizer is usually more informative)
```

### Example 6: Generate Specific Item Types Only (Editor/Runtime)

In the Inspector:
1. Set **"Exclusive Generation"** to **Base Name** or **Category**
2. Choose a **Specific Base Name** or **Specific Category**
3. Click **"Generate"**

Now all generated items will be limited to that base name or category.

At runtime, you can do the same thing via the API using `ItemGenerationOptions` with `BaseNameOverride` or `BaseNameCategoryOverride`.

## Troubleshooting

### "Could not find JSON file" Error

**Problem:** ItemTool can't locate your JSON file

**Solution:**
1. Check the **JSON File Path** in the Inspector
2. Path should be relative to the Assets folder (e.g., `Scripts/ItemNames.json`)
3. Verify the file exists in that location
4. Try placing the JSON in `Assets/Resources/` if loading fails

### "ItemNameData.baseNames is empty" Error

**Problem:** The JSON file loaded but has no base names

**Solution:**
1. Open your JSON file and verify it has a `baseNames` array
2. Check that the array is not empty
3. Ensure JSON formatting is valid (use a JSON validator if unsure)

### Generated Items Have "Unnamed" or "Invalid" Names

**Problem:** Modifier definitions are missing or malformed

**Solution:**
1. Verify all modifiers have `namePrefix` (for prefix modifiers) and `nameSuffix` (for suffix modifiers)
2. Check that all modifiers have `rare`, `epic`, and `legendary` range definitions
3. Ensure ranges are valid (min ≤ max)

### Legendary Items Missing Modifiers

**Problem:** Legendary items show fewer than 3 prefix/suffix modifiers

**Cause:** The item’s base name/category doesn’t have at least 3 valid unique modifiers in the JSON.

**Solution:** Ensure each base name/category has at least 3 valid prefix modifiers and 3 valid suffix modifiers (after `allowedBaseNames` filtering). If fewer are available, the generator will use as many as it can.

### Rarity Percentages Are Unexpected

**Problem:** Drop rates don't match expectations

**Solution:**
1. Use the luck visualizer (recommended) or click **"Show Rarity Percentages"** to see actual probabilities
2. Adjust the **Rarity Weights** to match desired base probabilities
3. Adjust the **Luck Multipliers** to control how much luck affects drop rates
4. Remember: luck is only a multiplier; base weights still matter significantly

### JSON Won't Load with Newtonsoft

**Problem:** Using alternating JSON field names breaks parsing

**Solution:**
- Use consistent field names:
  - `prefix` and `suffix` (not `prefixModifiers`/`suffixModifiers`)
  - `baseNames` (not `itemNames`)

ItemTool supports both old and new formats, but mixing them may cause issues.

---

**Questions or issues?** Check the code comments in ItemTool.cs or review the examples above.
