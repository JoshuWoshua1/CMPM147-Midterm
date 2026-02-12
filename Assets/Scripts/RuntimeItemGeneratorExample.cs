using System.Text;
using UnityEngine;

public class RuntimeItemGeneratorExample : MonoBehaviour
{
    [Tooltip("Reference to a GameObject with an ItemTool component.")]
    public ItemTool itemTool;

    [Tooltip("How many items to generate at runtime.")]
    public int count = 5;

    [Tooltip("Optional loot luck override (-1 uses ItemTool's lootLuck).")]
    public int lootLuckOverride = -1;

    private void Start()
    {
        if (itemTool == null)
        {
            Debug.LogError("RuntimeItemGeneratorExample: itemTool is not assigned.");
            return;
        }

        GenerateItems();
    }

    public void GenerateItems()
    {
        ItemTool.ItemGenerationOptions options = new ItemTool.ItemGenerationOptions
        {
            LootLuckOverride = lootLuckOverride
        };

        for (int i = 0; i < count; i++)
        {
            GeneratedItem item = itemTool.GenerateSingleItem(options);
            Debug.Log(FormatItem(item));
        }
    }

    private static string FormatItem(GeneratedItem item)
    {
        if (item == null)
        {
            return "<null item>";
        }

        StringBuilder builder = new StringBuilder();
        builder.Append(item.Name);
        builder.Append(" [");
        builder.Append(item.Rarity);
        builder.Append("]");

        if (item.PrefixModifiers != null && item.PrefixModifiers.Count > 0)
        {
            builder.Append(" | Prefixes: ");
            builder.Append(string.Join(", ", item.PrefixModifiers));
        }

        if (item.SuffixModifiers != null && item.SuffixModifiers.Count > 0)
        {
            builder.Append(" | Suffixes: ");
            builder.Append(string.Join(", ", item.SuffixModifiers));
        }

        return builder.ToString();
    }
}
