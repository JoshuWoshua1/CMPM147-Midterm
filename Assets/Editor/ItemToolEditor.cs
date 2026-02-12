using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemTool))]
public class ItemToolEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawPropertiesExcluding(serializedObject, "exclusiveGenerationMode", "useSpecificBaseName", "specificBaseName", "useSpecificBaseCategory", "specificBaseCategory");

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Item Type Filter", EditorStyles.boldLabel);

        SerializedProperty exclusiveGenerationModeProp = serializedObject.FindProperty("exclusiveGenerationMode");
        SerializedProperty useSpecificBaseNameProp = serializedObject.FindProperty("useSpecificBaseName");
        SerializedProperty specificBaseNameProp = serializedObject.FindProperty("specificBaseName");
        SerializedProperty useSpecificBaseCategoryProp = serializedObject.FindProperty("useSpecificBaseCategory");
        SerializedProperty specificBaseCategoryProp = serializedObject.FindProperty("specificBaseCategory");

        ItemTool tool = (ItemTool)target;
        List<string> baseNames = tool.GetAvailableBaseNames();
        List<string> categories = tool.GetAvailableBaseCategories();

        bool hasBaseNames = baseNames != null && baseNames.Count > 0;
        bool hasCategories = categories != null && categories.Count > 0;

        if (!hasBaseNames)
        {
            EditorGUILayout.HelpBox("No base names loaded. Check the JSON file path.", MessageType.Warning);
            exclusiveGenerationModeProp.enumValueIndex = (int)ItemTool.ExclusiveGenerationMode.Any;
            useSpecificBaseNameProp.boolValue = false;
            specificBaseNameProp.stringValue = string.Empty;
            useSpecificBaseCategoryProp.boolValue = false;
            specificBaseCategoryProp.stringValue = string.Empty;
        }
        else
        {
            string[] filterOptions = hasCategories
                ? new[] { "Any", "Base Name", "Category" }
                : new[] { "Any", "Base Name" };

            int filterIndex = exclusiveGenerationModeProp.enumValueIndex;
            if (!hasCategories && filterIndex == (int)ItemTool.ExclusiveGenerationMode.Category)
            {
                filterIndex = (int)ItemTool.ExclusiveGenerationMode.Any;
            }

            int newFilterIndex = EditorGUILayout.Popup("Exclusive Generation", filterIndex, filterOptions);
            if (!hasCategories && newFilterIndex == (int)ItemTool.ExclusiveGenerationMode.Category)
            {
                newFilterIndex = (int)ItemTool.ExclusiveGenerationMode.Any;
            }

            exclusiveGenerationModeProp.enumValueIndex = newFilterIndex;

            if (newFilterIndex == (int)ItemTool.ExclusiveGenerationMode.BaseName)
            {
                useSpecificBaseNameProp.boolValue = true;
                useSpecificBaseCategoryProp.boolValue = false;

                string[] options = new string[baseNames.Count + 1];
                options[0] = "Any";
                for (int i = 0; i < baseNames.Count; i++)
                {
                    options[i + 1] = baseNames[i];
                }

                int currentIndex = 0;
                if (!string.IsNullOrWhiteSpace(specificBaseNameProp.stringValue))
                {
                    int foundIndex = baseNames.IndexOf(specificBaseNameProp.stringValue);
                    if (foundIndex >= 0)
                    {
                        currentIndex = foundIndex + 1;
                    }
                }

                int newIndex = EditorGUILayout.Popup("Specific Base Name", currentIndex, options);
                if (newIndex <= 0)
                {
                    specificBaseNameProp.stringValue = string.Empty;
                }
                else
                {
                    specificBaseNameProp.stringValue = baseNames[newIndex - 1];
                }
            }
            else if (hasCategories && newFilterIndex == (int)ItemTool.ExclusiveGenerationMode.Category)
            {
                useSpecificBaseNameProp.boolValue = false;
                useSpecificBaseCategoryProp.boolValue = true;

                string[] options = new string[categories.Count + 1];
                options[0] = "Any";
                for (int i = 0; i < categories.Count; i++)
                {
                    options[i + 1] = categories[i];
                }

                int currentIndex = 0;
                if (!string.IsNullOrWhiteSpace(specificBaseCategoryProp.stringValue))
                {
                    int foundIndex = categories.IndexOf(specificBaseCategoryProp.stringValue);
                    if (foundIndex >= 0)
                    {
                        currentIndex = foundIndex + 1;
                    }
                }

                int newIndex = EditorGUILayout.Popup("Specific Category", currentIndex, options);
                if (newIndex <= 0)
                {
                    specificBaseCategoryProp.stringValue = string.Empty;
                }
                else
                {
                    specificBaseCategoryProp.stringValue = categories[newIndex - 1];
                }
            }
            else
            {
                exclusiveGenerationModeProp.enumValueIndex = (int)ItemTool.ExclusiveGenerationMode.Any;
                useSpecificBaseNameProp.boolValue = false;
                specificBaseNameProp.stringValue = string.Empty;
                useSpecificBaseCategoryProp.boolValue = false;
                specificBaseCategoryProp.stringValue = string.Empty;
            }
        }

        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

        if (GUILayout.Button("Generate Items", GUILayout.Height(40f)))
        {
            tool.Generate();
        }

        if (GUILayout.Button("Open Luck Visualizer", GUILayout.Height(35f)))
        {
            ItemToolLuckVisualizer window = EditorWindow.GetWindow<ItemToolLuckVisualizer>("Luck Visualizer");
            window.SetItemTool(target as ItemTool);
        }
    }
}
