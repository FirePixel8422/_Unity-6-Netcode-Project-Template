using UnityEditor;
using UnityEngine;

namespace Fire_Pixel.Utility
{
    [CustomEditor(typeof(DebugDataDisplay))]
    public class DebugDataDisplayEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            DebugDataDisplay debugDataDisplay = (DebugDataDisplay)target;

            if (GUILayout.Button("Reload Expensive Stats"))
            {
                debugDataDisplay.ReloadExpensiveStats();
            }
        }
    }
}