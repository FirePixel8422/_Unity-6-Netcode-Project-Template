using UnityEditor;
using UnityEngine;

namespace Fire_Pixel.Utility
{
    [CustomPropertyDrawer(typeof(EditorReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(
            SerializedProperty property,
            GUIContent label)
        {
            return InspectorButtonDrawer.GetPropertyHeight(
                property.serializedObject,
                property);
        }

        public override void OnGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label)
        {
            EditorGUI.BeginDisabledGroup(true);

            InspectorButtonDrawer.DrawProperty(
                position,
                property.serializedObject,
                property);

            EditorGUI.EndDisabledGroup();
        }
    }
}