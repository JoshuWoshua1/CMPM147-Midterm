using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(GeneratedItem))]
public class GeneratedItemDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Calculate rects
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        float totalHeight = 0;

        // Get rarity for coloring
        SerializedProperty rarityProp = property.FindPropertyRelative("Rarity");
        Color rarityColor = GetRarityColor(rarityProp.stringValue);

        // Foldout with rarity color
        Rect foldoutRect = new Rect(position.x, position.y, position.width, lineHeight);
        GUI.color = rarityColor;
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);
        GUI.color = Color.white;
        totalHeight += lineHeight + spacing;

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            // Name with color based on rarity
            SerializedProperty nameProp = property.FindPropertyRelative("Name");
            
            GUI.color = rarityColor;
            Rect nameRect = new Rect(position.x, position.y + totalHeight, position.width, lineHeight);
            EditorGUI.PropertyField(nameRect, nameProp);
            GUI.color = Color.white;
            totalHeight += lineHeight + spacing;

            // Rarity
            Rect rarityRect = new Rect(position.x, position.y + totalHeight, position.width, lineHeight);
            EditorGUI.PropertyField(rarityRect, rarityProp);
            totalHeight += lineHeight + spacing;

            // Prefix Modifiers
            SerializedProperty prefixProp = property.FindPropertyRelative("PrefixModifiers");
            float prefixHeight = EditorGUI.GetPropertyHeight(prefixProp, true);
            Rect prefixRect = new Rect(position.x, position.y + totalHeight, position.width, prefixHeight);
            EditorGUI.PropertyField(prefixRect, prefixProp, true);
            totalHeight += prefixHeight + spacing;

            // Suffix Modifiers
            SerializedProperty suffixProp = property.FindPropertyRelative("SuffixModifiers");
            float suffixHeight = EditorGUI.GetPropertyHeight(suffixProp, true);
            Rect suffixRect = new Rect(position.x, position.y + totalHeight, position.width, suffixHeight);
            EditorGUI.PropertyField(suffixRect, suffixProp, true);
            totalHeight += suffixHeight + spacing;

            // Export Button
            Rect buttonRect = new Rect(position.x, position.y + totalHeight, position.width, lineHeight + 4);
            if (GUI.Button(buttonRect, "Export to JSON"))
            {
                // Get the ItemTool component
                ItemTool itemTool = (property.serializedObject.targetObject as ItemTool);
                if (itemTool != null)
                {
                    // Create a GeneratedItem from the serialized data
                    GeneratedItem item = new GeneratedItem
                    {
                        Name = nameProp.stringValue,
                        Rarity = rarityProp.stringValue
                    };

                    // Copy prefix modifiers
                    for (int i = 0; i < prefixProp.arraySize; i++)
                    {
                        item.PrefixModifiers.Add(prefixProp.GetArrayElementAtIndex(i).stringValue);
                    }

                    // Copy suffix modifiers
                    for (int i = 0; i < suffixProp.arraySize; i++)
                    {
                        item.SuffixModifiers.Add(suffixProp.GetArrayElementAtIndex(i).stringValue);
                    }

                    // Copy modifier rarities (if present)
                    SerializedProperty prefixRarityProp = property.FindPropertyRelative("PrefixModifierRarities");
                    if (prefixRarityProp != null)
                    {
                        for (int i = 0; i < prefixRarityProp.arraySize; i++)
                        {
                            item.PrefixModifierRarities.Add(prefixRarityProp.GetArrayElementAtIndex(i).stringValue);
                        }
                    }

                    SerializedProperty suffixRarityProp = property.FindPropertyRelative("SuffixModifierRarities");
                    if (suffixRarityProp != null)
                    {
                        for (int i = 0; i < suffixRarityProp.arraySize; i++)
                        {
                            item.SuffixModifierRarities.Add(suffixRarityProp.GetArrayElementAtIndex(i).stringValue);
                        }
                    }

                    itemTool.ExportItemToJson(item);
                }
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    private Color GetRarityColor(string rarity)
    {
        switch (rarity)
        {
            case "Common":
                return new Color(0.7f, 0.7f, 0.7f);      // Gray
            case "Rare":
                return new Color(0.2f, 0.5f, 1f);        // Blue
            case "Epic":
                return new Color(0.8f, 0.2f, 1f);        // Purple
            case "Legendary":
                return new Color(1f, 0.8f, 0f);          // Gold
            default:
                return Color.white;
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        float height = lineHeight + spacing; // Foldout

        if (property.isExpanded)
        {
            height += lineHeight + spacing; // Name
            height += lineHeight + spacing; // Rarity

            SerializedProperty prefixProp = property.FindPropertyRelative("PrefixModifiers");
            height += EditorGUI.GetPropertyHeight(prefixProp, true) + spacing;

            SerializedProperty suffixProp = property.FindPropertyRelative("SuffixModifiers");
            height += EditorGUI.GetPropertyHeight(suffixProp, true) + spacing;

            height += lineHeight + 4 + spacing; // Export button
        }

        return height;
    }
}
