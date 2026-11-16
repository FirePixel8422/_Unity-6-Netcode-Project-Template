using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(IntRange))]
[CustomPropertyDrawer(typeof(UIntRange))]
[CustomPropertyDrawer(typeof(ShortRange))]
[CustomPropertyDrawer(typeof(UShortRange))]
[CustomPropertyDrawer(typeof(ByteRange))]
[CustomPropertyDrawer(typeof(SByteRange))]
[CustomPropertyDrawer(typeof(LongRange))]
[CustomPropertyDrawer(typeof(ULongRange))]
[CustomPropertyDrawer(typeof(HalfRange))]
[CustomPropertyDrawer(typeof(FloatRange))]
[CustomPropertyDrawer(typeof(DoubleRange))]
public class InlineRangeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        Rect fieldRect = EditorGUI.PrefixLabel(position, label);
        SerializedProperty min = property.FindPropertyRelative("Min");
        SerializedProperty max = property.FindPropertyRelative("Max");

        float width = (fieldRect.width - 2f) * 0.5f;

        Rect minRect = new Rect(fieldRect.x, fieldRect.y, width, fieldRect.height);
        Rect maxRect = new Rect(fieldRect.x + width + 2f, fieldRect.y, width, fieldRect.height);

        EditorGUI.PropertyField(minRect, min, GUIContent.none);
        EditorGUI.PropertyField(maxRect, max, GUIContent.none);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}