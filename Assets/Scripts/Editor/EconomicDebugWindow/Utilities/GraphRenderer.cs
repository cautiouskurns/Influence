using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Editor.DebugWindow.Utilities
{
    /// <summary>
    /// Utility class for rendering economic graphs
    /// </summary>
    public class GraphRenderer
    {
        /// <summary>
        /// Draw a line graph with integer data
        /// </summary>
        public static void DrawLineGraph(List<int> data, Rect rect, Color color, string label, int currentTurn)
        {
            if (data == null || data.Count < 2) return;
            
            // Find max value for scaling
            int maxValue = 10; // Minimum to ensure visibility
            foreach (int value in data)
            {
                maxValue = Mathf.Max(maxValue, value);
            }
            
            // Add headroom to the top
            maxValue = Mathf.CeilToInt(maxValue * 1.1f);
            
            DrawGraph(data, rect, color, label, maxValue, currentTurn);
        }
        
        /// <summary>
        /// Draw a line graph with float data
        /// </summary>
        public static void DrawLineGraph(List<float> data, Rect rect, Color color, string label, int currentTurn)
        {
            if (data == null || data.Count < 2) return;
            
            // Find max value for scaling
            float maxValue = 10f; // Minimum to ensure visibility
            foreach (float value in data)
            {
                maxValue = Mathf.Max(maxValue, value);
            }
            
            // Add headroom to the top
            maxValue = Mathf.Ceil(maxValue * 1.1f);
            
            DrawGraph(data, rect, color, label, maxValue, currentTurn);
        }
        
        /// <summary>
        /// Draw a legend for the graph
        /// </summary>
        public static void DrawLegend(Rect graphRect, string title, Dictionary<string, Color> legendEntries)
        {
            float legendWidth = 120;
            float legendHeight = 30 + (legendEntries.Count * 20); // Height based on number of entries
            
            Rect legendRect = new Rect(
                graphRect.x + graphRect.width - legendWidth - 10,
                graphRect.y + 10,
                legendWidth,
                legendHeight
            );
            
            // Draw semi-transparent background
            EditorGUI.DrawRect(legendRect, new Color(0.1f, 0.1f, 0.1f, 0.7f));
            
            GUIStyle legendStyle = new GUIStyle(EditorStyles.label);
            legendStyle.normal.textColor = Color.white;
            
            // Draw title at the bottom
            GUI.Label(new Rect(legendRect.x + 5, legendRect.y + legendHeight - 20, legendWidth - 10, 20), 
                       title, EditorStyles.boldLabel);
            
            // Draw each legend entry
            int index = 0;
            foreach (var entry in legendEntries)
            {
                Rect colorRect = new Rect(legendRect.x + 10, legendRect.y + 10 + (index * 20), 10, 10);
                EditorGUI.DrawRect(colorRect, entry.Value);
                GUI.Label(new Rect(colorRect.x + 15, colorRect.y - 2, legendWidth - 35, 20), entry.Key, legendStyle);
                index++;
            }
        }
        
        // Private helper methods
        
        /// <summary>
        /// Generic graph drawing helper for both int and float data
        /// </summary>
        private static void DrawGraph<T>(List<T> data, Rect rect, Color color, string label, float maxValue, int currentTurn)
        {
            // Adjust the rect to leave more space for axis labels
            Rect adjustedRect = new Rect(
                rect.x + 100, // Move graph right to make room for y-axis labels
                rect.y + 20, // Move graph down to make room for top labels
                rect.width - 80, // Reduce width to keep right margin
                rect.height - 40  // Reduce height to keep bottom margin for x-axis labels
            );
            
            // Add a dark background to the graph area to improve contrast
            EditorGUI.DrawRect(adjustedRect, new Color(0.15f, 0.15f, 0.15f, 0.4f));
            
            // Calculate points
            Vector3[] points = new Vector3[data.Count];
            
            for (int i = 0; i < data.Count; i++)
            {
                float x = adjustedRect.x + (i * adjustedRect.width) / (data.Count - 1);
                float dataValue = System.Convert.ToSingle(data[i]); // Convert T to float
                float y = adjustedRect.y + adjustedRect.height - (dataValue * adjustedRect.height) / maxValue;
                points[i] = new Vector3(x, y, 0);
            }
            
            // Draw lines
            Handles.color = color;
            Handles.DrawAAPolyLine(4f, points);
            
            // Create better styles for labels
            GUIStyle axisLabelStyle = new GUIStyle(EditorStyles.boldLabel);
            axisLabelStyle.normal.textColor = Color.white;
            axisLabelStyle.fontSize = 12;
            
            GUIStyle tickLabelStyle = new GUIStyle(EditorStyles.miniLabel);
            tickLabelStyle.normal.textColor = Color.white;
            tickLabelStyle.fontSize = 10;
            tickLabelStyle.alignment = TextAnchor.MiddleRight;
            
            // Draw outer box around the entire graph area
            Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            DrawGraphBorder(adjustedRect);
            
            // Draw y-axis labels
            DrawYAxisLabels(adjustedRect, maxValue, tickLabelStyle);
            
            // Draw axes
            DrawAxes(adjustedRect);
            
            // Draw grid lines
            DrawGridLines(adjustedRect);
            
            // Draw x-axis ticks and labels
            DrawXAxisLabels(adjustedRect, data.Count, currentTurn);
            
            // Draw axis labels
            GUI.Label(new Rect(adjustedRect.x - 70, adjustedRect.y + (adjustedRect.height / 2) - 30, 20, 60), 
                       "Value", axisLabelStyle);
            
            GUI.Label(new Rect(adjustedRect.x + adjustedRect.width/2 - 20, adjustedRect.y + adjustedRect.height + 25, 40, 20), 
                       "Turn", axisLabelStyle);
        }
        
        /// <summary>
        /// Draw the border around the graph
        /// </summary>
        private static void DrawGraphBorder(Rect rect)
        {
            Handles.DrawLine(
                new Vector3(rect.x - 5, rect.y - 5, 0),
                new Vector3(rect.x + rect.width + 5, rect.y - 5, 0)
            );
            Handles.DrawLine(
                new Vector3(rect.x + rect.width + 5, rect.y - 5, 0),
                new Vector3(rect.x + rect.width + 5, rect.y + rect.height + 5, 0)
            );
            Handles.DrawLine(
                new Vector3(rect.x + rect.width + 5, rect.y + rect.height + 5, 0),
                new Vector3(rect.x - 5, rect.y + rect.height + 5, 0)
            );
            Handles.DrawLine(
                new Vector3(rect.x - 5, rect.y + rect.height + 5, 0),
                new Vector3(rect.x - 5, rect.y - 5, 0)
            );
        }
        
        /// <summary>
        /// Draw the Y-axis labels
        /// </summary>
        private static void DrawYAxisLabels(Rect rect, float maxValue, GUIStyle style)
        {
            // Max value at top
            GUI.Label(new Rect(rect.x - 50, rect.y - 10, 45, 20), maxValue.ToString(), style);
            
            // Mid-values
            for (int i = 1; i < 4; i++)
            {
                float yPos = rect.y + (rect.height / 4) * i;
                float value = maxValue * (1 - (i / 4.0f));
                string valueText = value.ToString("F1");
                if (value == Mathf.Floor(value))
                    valueText = Mathf.FloorToInt(value).ToString();
                    
                GUI.Label(new Rect(rect.x - 50, yPos - 10, 45, 20), valueText, style);
            }
            
            // Zero at bottom
            GUI.Label(new Rect(rect.x - 50, rect.y + rect.height - 10, 45, 20), "0", style);
        }
        
        /// <summary>
        /// Draw the X and Y axes
        /// </summary>
        private static void DrawAxes(Rect rect)
        {
            Handles.color = Color.gray;
            
            // Draw x-axis (time)
            Handles.DrawLine(
                new Vector3(rect.x, rect.y + rect.height, 0),
                new Vector3(rect.x + rect.width, rect.y + rect.height, 0)
            );
            
            // Draw y-axis
            Handles.DrawLine(
                new Vector3(rect.x, rect.y, 0),
                new Vector3(rect.x, rect.y + rect.height, 0)
            );
        }
        
        /// <summary>
        /// Draw the grid lines
        /// </summary>
        private static void DrawGridLines(Rect rect)
        {
            Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
            
            // Draw interval lines for y-axis
            for (int i = 1; i < 5; i++)
            {
                float y = rect.y + (rect.height / 5) * i;
                Handles.DrawDottedLine(
                    new Vector3(rect.x, y, 0),
                    new Vector3(rect.x + rect.width, y, 0),
                    2f
                );
            }
        }
        
        /// <summary>
        /// Draw the X-axis labels
        /// </summary>
        private static void DrawXAxisLabels(Rect rect, int dataCount, int currentTurn)
        {
            int tickCount = Mathf.Min(10, dataCount); // Show at most 10 ticks to avoid overcrowding
            if (dataCount <= 0) return;
            
            GUIStyle turnLabelStyle = new GUIStyle(EditorStyles.miniLabel);
            turnLabelStyle.normal.textColor = Color.white;
            turnLabelStyle.fontSize = 10;
            turnLabelStyle.alignment = TextAnchor.UpperCenter;
            
            for (int i = 0; i < tickCount; i++)
            {
                // Calculate position evenly spaced along x-axis
                int dataIndex = i * (dataCount - 1) / (tickCount - 1);
                if (tickCount == 1) dataIndex = 0;
                
                float x = rect.x + (dataIndex * rect.width) / (dataCount - 1);
                
                // Draw tick mark
                Handles.DrawLine(
                    new Vector3(x, rect.y + rect.height, 0),
                    new Vector3(x, rect.y + rect.height + 5, 0)
                );
                
                // Calculate turn number based on current turn and history length
                int turnNumber = currentTurn - (dataCount - 1 - dataIndex);
                
                // Only show turn number if it's positive (actual turns that have happened)
                if (turnNumber > 0)
                {
                    // Draw turn number with better styling and alignment
                    GUI.Label(new Rect(x - 15, rect.y + rect.height + 5, 30, 20), 
                               turnNumber.ToString(), turnLabelStyle);
                }
            }
        }
    }
}