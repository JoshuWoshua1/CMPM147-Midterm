using UnityEngine;
using UnityEditor;

public class ItemToolLuckVisualizer : EditorWindow
{
    private ItemTool itemTool;
    private Vector2 scrollPosition;
    private bool showGraph = true;
    private bool showMultiplierControls = true;
    
    private const float GraphWidth = 500f;
    private const float GraphHeight = 300f;
    private const float LabelWidth = 100f;

    private static readonly Color CommonColor = new Color(0.7f, 0.7f, 0.7f);      // Gray
    private static readonly Color RareColor = new Color(0.2f, 0.5f, 1f);          // Blue
    private static readonly Color EpicColor = new Color(0.8f, 0.2f, 1f);          // Purple
    private static readonly Color LegendaryColor = new Color(1f, 0.8f, 0f);       // Gold

    [MenuItem("Window/ItemTool Luck Visualizer")]
    public static void ShowWindow()
    {
        GetWindow<ItemToolLuckVisualizer>("Luck Visualizer");
    }

    public void SetItemTool(ItemTool tool)
    {
        itemTool = tool;
    }

    private void OnGUI()
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        GUILayout.Label("ItemTool Luck System Visualizer", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Visualize how item rarity changes with loot luck values (0-2500).", MessageType.Info);

        EditorGUILayout.Space();

        // ItemTool Selection
        EditorGUILayout.LabelField("Setup", EditorStyles.boldLabel);
        itemTool = EditorGUILayout.ObjectField("ItemTool Reference", itemTool, typeof(ItemTool), true) as ItemTool;

        if (itemTool == null)
        {
            EditorGUILayout.HelpBox("Select an ItemTool instance to visualize its luck configuration.", MessageType.Warning);
            GUILayout.EndScrollView();
            return;
        }

        EditorGUILayout.Space();

        // Multiplier Controls
        showMultiplierControls = EditorGUILayout.Foldout(showMultiplierControls, "Luck Multipliers (Edit in ItemTool Inspector)", EditorStyles.foldout);
        if (showMultiplierControls)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Common", itemTool.luckMultipliers.common.ToString("F1"));
            EditorGUILayout.LabelField("Rare", itemTool.luckMultipliers.rare.ToString("F1"));
            EditorGUILayout.LabelField("Epic", itemTool.luckMultipliers.epic.ToString("F1"));
            EditorGUILayout.LabelField("Legendary", itemTool.luckMultipliers.legendary.ToString("F1"));
            EditorGUILayout.HelpBox("Edit these values directly in the ItemTool Inspector component.", MessageType.Info);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        // Graph
        showGraph = EditorGUILayout.Foldout(showGraph, "Rarity Distribution Graph", EditorStyles.foldout);
        if (showGraph)
        {
            EditorGUILayout.Space();
            DrawGraph();
            EditorGUILayout.Space();
        }

        // Statistics Table
        EditorGUILayout.LabelField("Rarity Percentages at Key Luck Values", EditorStyles.boldLabel);
        DrawStatisticsTable();

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "The graph shows how item rarity percentages shift as Loot Luck increases from 0 to 2500.\n\n" +
            "• At Luck 0: Base weights only (Common heavy)\n" +
            "• At Luck 2500: Full multiplier applied (Legendary favored)\n\n" +
            "Edit the multipliers in ItemTool to adjust the curve shape.",
            MessageType.Info
        );

        GUILayout.EndScrollView();
    }

    private void DrawGraph()
    {
        Rect graphRect = GUILayoutUtility.GetRect(GraphWidth, GraphHeight);
        GUI.Box(graphRect, "");

        if (itemTool == null)
            return;

        const float maxLuck = 2500f;
        const int steps = 100;
        const float padding = 40f;

        float graphLeft = graphRect.x + padding;
        float graphRight = graphRect.x + graphRect.width - 20f;
        float graphTop = graphRect.y + 20f;
        float graphBottom = graphRect.y + graphRect.height - 30f;

        float graphW = graphRight - graphLeft;
        float graphH = graphBottom - graphTop;

        // Draw grid and axes
        GUI.color = Color.gray;
        Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);

        // Horizontal grid lines
        for (int i = 0; i <= 5; i++)
        {
            float y = graphTop + (graphH / 5f) * i;
            Handles.DrawLine(new Vector3(graphLeft, y), new Vector3(graphRight, y));
            GUI.Label(new Rect(graphLeft - 35f, y - 8f, 30f, 16f), ((5 - i) * 20f).ToString("F0") + "%", EditorStyles.miniLabel);
        }

        // Vertical grid lines
        for (int i = 0; i <= 5; i++)
        {
            float x = graphLeft + (graphW / 5f) * i;
            Handles.DrawLine(new Vector3(x, graphTop), new Vector3(x, graphBottom));
            GUI.Label(new Rect(x - 15f, graphBottom + 2f, 30f, 16f), ((int)(i * maxLuck / 5f)).ToString(), EditorStyles.miniLabel);
        }

        // Draw axis labels
        GUI.color = Color.white;
        GUI.Label(new Rect(graphLeft - 35f, graphTop - 20f, 30f, 16f), "100%", EditorStyles.miniLabel);
        GUI.Label(new Rect(graphLeft - 30f, graphBottom + 15f, 100f, 16f), "Loot Luck", EditorStyles.miniLabel);

        // Draw current loot luck indicator
        float clampedLuck = Mathf.Clamp(itemTool.lootLuck, 0, maxLuck);
        float luckX = graphLeft + (clampedLuck / maxLuck) * graphW;
        Handles.color = new Color(1f, 1f, 1f, 0.75f);
        Handles.DrawLine(new Vector3(luckX, graphTop), new Vector3(luckX, graphBottom));
        GUI.color = Color.white;
        GUI.Label(new Rect(luckX - 30f, graphTop - 18f, 60f, 16f), clampedLuck.ToString("F0"), EditorStyles.miniLabel);

        // Draw lines for each rarity
        Vector3[] commonPoints = new Vector3[steps];
        Vector3[] rarePoints = new Vector3[steps];
        Vector3[] epicPoints = new Vector3[steps];
        Vector3[] legendaryPoints = new Vector3[steps];

        for (int i = 0; i < steps; i++)
        {
            float luck = (i / (float)(steps - 1)) * maxLuck;
            GetAdjustedRarityWeights((int)luck, out float common, out float rare, out float epic, out float legendary);

            float total = common + rare + epic + legendary;
            common = (common / total) * 100f;
            rare = (rare / total) * 100f;
            epic = (epic / total) * 100f;
            legendary = (legendary / total) * 100f;

            float x = graphLeft + (i / (float)(steps - 1)) * graphW;
            float commonY = graphBottom - (common / 100f) * graphH;
            float rareY = commonY - (rare / 100f) * graphH;
            float epicY = rareY - (epic / 100f) * graphH;
            float legendaryY = epicY - (legendary / 100f) * graphH;

            commonPoints[i] = new Vector3(x, commonY);
            rarePoints[i] = new Vector3(x, rareY);
            epicPoints[i] = new Vector3(x, epicY);
            legendaryPoints[i] = new Vector3(x, legendaryY);
        }

        // Draw stacked area chart
        Handles.color = CommonColor;
        DrawLineChart(commonPoints);

        Handles.color = RareColor;
        DrawLineChart(rarePoints);

        Handles.color = EpicColor;
        DrawLineChart(epicPoints);

        Handles.color = LegendaryColor;
        DrawLineChart(legendaryPoints);

        // Draw legend
        GUI.color = Color.white;
        float legendX = graphRight - 150f;
        float legendY = graphTop + 10f;

        DrawColoredLabel(legendX, legendY, CommonColor, "■ Common");
        DrawColoredLabel(legendX, legendY + 18f, RareColor, "■ Rare");
        DrawColoredLabel(legendX, legendY + 36f, EpicColor, "■ Epic");
        DrawColoredLabel(legendX, legendY + 54f, LegendaryColor, "■ Legendary");
    }

    private void DrawLineChart(Vector3[] points)
    {
        for (int i = 0; i < points.Length - 1; i++)
        {
            Handles.DrawLine(points[i], points[i + 1]);
        }
    }

    private void DrawColoredLabel(float x, float y, Color color, string text)
    {
        GUI.color = color;
        GUI.Label(new Rect(x, y, 150f, 16f), text, EditorStyles.miniLabel);
    }

    private void DrawStatisticsTable()
    {
        int[] luckValues = { 0, 500, 1000, 1500, 2000, 2500 };

        // Header
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Loot Luck", EditorStyles.miniLabel, GUILayout.Width(80f));
        GUILayout.Label("Common", EditorStyles.miniLabel, GUILayout.Width(80f));
        GUILayout.Label("Rare", EditorStyles.miniLabel, GUILayout.Width(80f));
        GUILayout.Label("Epic", EditorStyles.miniLabel, GUILayout.Width(80f));
        GUILayout.Label("Legendary", EditorStyles.miniLabel, GUILayout.Width(80f));
        EditorGUILayout.EndHorizontal();

        // Rows
        foreach (int luck in luckValues)
        {
            GetAdjustedRarityWeights(luck, out float common, out float rare, out float epic, out float legendary);
            float total = common + rare + epic + legendary;
            
            float commonPct = (common / total) * 100f;
            float rarePct = (rare / total) * 100f;
            float epicPct = (epic / total) * 100f;
            float legendaryPct = (legendary / total) * 100f;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(luck.ToString(), EditorStyles.miniLabel, GUILayout.Width(80f));
            DrawPercentageLabel(commonPct, CommonColor, 80f);
            DrawPercentageLabel(rarePct, RareColor, 80f);
            DrawPercentageLabel(epicPct, EpicColor, 80f);
            DrawPercentageLabel(legendaryPct, LegendaryColor, 80f);
            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawPercentageLabel(float percentage, Color color, float width)
    {
        GUI.color = color;
        GUILayout.Label(percentage.ToString("F1") + "%", EditorStyles.miniLabel, GUILayout.Width(width));
        GUI.color = Color.white;
    }

    private void GetAdjustedRarityWeights(int lootLuck, out float adjustedCommon, out float adjustedRare, out float adjustedEpic, out float adjustedLegendary)
    {
        if (itemTool == null)
        {
            adjustedCommon = adjustedRare = adjustedEpic = adjustedLegendary = 0;
            return;
        }

        float baseCommon = Mathf.Max(0f, itemTool.commonWeight);
        float baseRare = Mathf.Max(0f, itemTool.rareWeight);
        float baseEpic = Mathf.Max(0f, itemTool.epicWeight);
        float baseLegendary = Mathf.Max(0f, itemTool.legendaryWeight);

        const float maxLuck = 2500f;
        float luckT = Mathf.Clamp01(lootLuck / maxLuck);
        float biasT = luckT;

        float commonMultiplier = Mathf.Lerp(1f, itemTool.luckMultipliers.common, biasT);
        float rareMultiplier = Mathf.Lerp(1f, itemTool.luckMultipliers.rare, biasT);
        float epicMultiplier = Mathf.Lerp(1f, itemTool.luckMultipliers.epic, biasT);
        float legendaryMultiplier = Mathf.Lerp(1f, itemTool.luckMultipliers.legendary, biasT);

        adjustedCommon = baseCommon * commonMultiplier;
        adjustedRare = baseRare * rareMultiplier;
        adjustedEpic = baseEpic * epicMultiplier;
        adjustedLegendary = baseLegendary * legendaryMultiplier;
    }
}
