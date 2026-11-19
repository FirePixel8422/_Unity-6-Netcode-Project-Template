using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Fire_Pixel.Utility
{
    public static class InspectorButtonDrawer
    {
        private const float ButtonHeight = 22f;
        private const float BoxPadding = 4f;
        private const float TabWidth = 15f;

        private class MethodCacheEntry
        {
            public MethodInfo method;
            public ParameterInfo[] parameters;
            public object[] args;
        }

        private static readonly Dictionary<string, MethodCacheEntry> cache = new();
        private static readonly HashSet<Type> compatibilityWarnings = new();

        public static float GetPropertyHeight(SerializedObject serializedObject, SerializedProperty property)
        {
            Type type = GetPropertyType(property);

            if (type == null || !IsMarked(type))
                return EditorGUI.GetPropertyHeight(property, true);

            float height = EditorGUI.GetPropertyHeight(property, true);

            if (!property.isExpanded)
                return height;

            object value = GetValue(serializedObject.targetObject, property.propertyPath);

            if (value == null)
                return height;

            height += EditorGUIUtility.standardVerticalSpacing;
            height += GetMethodsHeight(value, serializedObject.targetObject, property.propertyPath);

            return height;
        }

        public static void DrawProperty(Rect position, SerializedObject serializedObject, SerializedProperty property)
        {
            Type type = GetPropertyType(property);

            if (type == null)
            {
                EditorGUI.PropertyField(position, property, true);
                return;
            }

            WarnIfMissingCompatibility(type, serializedObject.targetObject);

            if (!IsMarked(type))
            {
                EditorGUI.PropertyField(position, property, true);
                return;
            }

            DrawMarkedProperty(position, serializedObject, property);
        }

        public static void DrawProperty(SerializedObject serializedObject, SerializedProperty property)
        {
            float height = GetPropertyHeight(serializedObject, property);
            Rect position = EditorGUILayout.GetControlRect(false, height);
            DrawProperty(position, serializedObject, property);
        }

        public static void DrawObjectMethods(object obj)
        {
            if (obj == null)
                return;

            UnityEngine.Object rootObject = obj as UnityEngine.Object;
            string path = obj.GetType().Name;
            float height = GetMethodsHeight(obj, rootObject, path);

            if (height <= 0f)
                return;

            Rect position = EditorGUILayout.GetControlRect(false, height);
            DrawMethods(obj, rootObject, path, position);
        }

        private static void DrawMarkedProperty(Rect position, SerializedObject serializedObject, SerializedProperty property)
        {
            float propertyHeight = EditorGUI.GetPropertyHeight(property, true);
            Rect propertyRect = new Rect(position.x, position.y, position.width, propertyHeight);

            EditorGUI.PropertyField(propertyRect, property, true);

            if (!property.isExpanded)
                return;

            object value = GetValue(serializedObject.targetObject, property.propertyPath);

            if (value == null)
                return;

            float spacing = EditorGUIUtility.standardVerticalSpacing;
            Rect methodsRect = new Rect(position.x, propertyRect.yMax + spacing, position.width, position.height - propertyHeight - spacing);

            DrawMethods(value, serializedObject.targetObject, property.propertyPath, methodsRect);
        }

        private static float GetMethodsHeight(object obj, UnityEngine.Object rootObject, string path)
        {
            MethodInfo[] methods = obj.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            float height = 0f;
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            foreach (MethodInfo method in methods)
            {
                InspectorButtonAttribute button = method.GetCustomAttribute<InspectorButtonAttribute>();

                if (button == null)
                    continue;

                height += GetMethodHeight(rootObject, method, button, path);
                height += spacing;
            }

            if (height > 0f)
                height -= spacing;

            return height;
        }

        private static float GetMethodHeight(UnityEngine.Object rootObject, MethodInfo method, InspectorButtonAttribute button, string path)
        {
            MethodCacheEntry entry = GetCacheEntry(rootObject, method, path);

            if (entry.parameters.Length == 0)
                return ButtonHeight;

            float spacing = EditorGUIUtility.standardVerticalSpacing;
            float height = BoxPadding + ButtonHeight + spacing + EditorGUIUtility.singleLineHeight;

            string foldoutKey = GetParameterFoldoutKey(rootObject, method, path);

            if (GetParameterFoldout(foldoutKey))
            {
                height += spacing;
                height += entry.parameters.Length * EditorGUIUtility.singleLineHeight;
                height += (entry.parameters.Length - 1) * spacing;
            }

            height += BoxPadding;

            return height;
        }

        private static void DrawMethods(object obj, UnityEngine.Object rootObject, string path, Rect position)
        {
            MethodInfo[] methods = obj.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            float y = position.y;
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            foreach (MethodInfo method in methods)
            {
                InspectorButtonAttribute button = method.GetCustomAttribute<InspectorButtonAttribute>();

                if (button == null)
                    continue;

                float height = GetMethodHeight(rootObject, method, button, path);
                Rect methodRect = new Rect(position.x, y, position.width, height);

                DrawMethod(methodRect, obj, rootObject, method, button, path);

                y += height + spacing;
            }
        }

        private static MethodCacheEntry GetCacheEntry(UnityEngine.Object rootObject, MethodInfo method, string path)
        {
            string rootId = rootObject != null ? rootObject.GetEntityId().ToString() : "null";
            string key = $"{rootId}_{path}_{method.MetadataToken}";

            if (cache.TryGetValue(key, out MethodCacheEntry entry))
                return entry;

            ParameterInfo[] parameters = method.GetParameters();

            entry = new MethodCacheEntry
            {
                method = method,
                parameters = parameters,
                args = new object[parameters.Length]
            };

            cache[key] = entry;

            return entry;
        }

        private static void DrawMethod(Rect position, object obj, UnityEngine.Object rootObject, MethodInfo method, InspectorButtonAttribute button, string path)
        {
            MethodCacheEntry entry = GetCacheEntry(rootObject, method, path);
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            bool allowed = button.AllowUsageOutsidePlayMode || EditorApplication.isPlaying;
            string label = string.IsNullOrEmpty(button.Label) ? ObjectNames.NicifyVariableName(method.Name) : button.Label;

            if (!allowed)
                label += " (Play Mode Only)";

            if (entry.parameters.Length == 0)
            {
                Rect buttonRect = new Rect(position.x, position.y, position.width, ButtonHeight);

                using (new EditorGUI.DisabledScope(!allowed))
                {
                    if (GUI.Button(buttonRect, label))
                        InvokeMethod(obj, rootObject, method, entry.args, label);
                }

                return;
            }

            GUI.Box(position, GUIContent.none, EditorStyles.helpBox);

            float y = position.y + BoxPadding;

            Rect buttonRectWithParameters = new Rect(position.x + BoxPadding, y, position.width - BoxPadding * 2f, ButtonHeight);

            using (new EditorGUI.DisabledScope(!allowed))
            {
                if (GUI.Button(buttonRectWithParameters, label))
                    InvokeMethod(obj, rootObject, method, entry.args, label);
            }

            y += ButtonHeight + spacing;

            string foldoutKey = GetParameterFoldoutKey(rootObject, method, path);
            bool expanded = GetParameterFoldout(foldoutKey);

            Rect foldoutRect = new Rect(position.x + BoxPadding + TabWidth, y, position.width - BoxPadding * 2f - TabWidth, EditorGUIUtility.singleLineHeight);
            bool newExpanded = EditorGUI.Foldout(foldoutRect, expanded, "Parameters", true);

            if (newExpanded != expanded)
                SetParameterFoldout(foldoutKey, newExpanded);

            if (!newExpanded)
                return;

            y += EditorGUIUtility.singleLineHeight + spacing;

            using (new EditorGUI.DisabledScope(!allowed))
            {
                for (int i = 0; i < entry.parameters.Length; i++)
                {
                    Rect parameterRect = new Rect(position.x + BoxPadding, y, position.width - BoxPadding * 2f, EditorGUIUtility.singleLineHeight);
                    entry.args[i] = DrawParameter(parameterRect, entry.parameters[i], entry.args[i]);
                    y += EditorGUIUtility.singleLineHeight + spacing;
                }
            }
        }

        private static void InvokeMethod(object obj, UnityEngine.Object rootObject, MethodInfo method, object[] args, string label)
        {
            if (rootObject != null)
                Undo.RecordObject(rootObject, label);

            try
            {
                method.Invoke(obj, args.Length == 0 ? null : args);

                if (rootObject != null)
                {
                    EditorUtility.SetDirty(rootObject);
                    SerializedObjectUpdate(rootObject);
                }
            }
            catch (TargetInvocationException exception)
            {
                Debug.LogException(exception.InnerException ?? exception);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private static object DrawParameter(Rect position, ParameterInfo parameter, object current)
        {
            Type type = parameter.ParameterType;

            if (type == typeof(int))
                return EditorGUI.IntField(position, parameter.Name, current != null ? (int)current : 0);

            if (type == typeof(float))
                return EditorGUI.FloatField(position, parameter.Name, current != null ? (float)current : 0f);

            if (type == typeof(bool))
                return EditorGUI.Toggle(position, parameter.Name, current != null && (bool)current);

            if (type == typeof(string))
                return EditorGUI.TextField(position, parameter.Name, current as string ?? "");

            if (type == typeof(Vector3))
                return EditorGUI.Vector3Field(position, parameter.Name, current != null ? (Vector3)current : Vector3.zero);

            if (type.IsEnum)
            {
                Enum value = current as Enum ?? (Enum)Activator.CreateInstance(type);

                return Attribute.IsDefined(type, typeof(FlagsAttribute))
                    ? EditorGUI.EnumFlagsField(position, parameter.Name, value)
                    : EditorGUI.EnumPopup(position, parameter.Name, value);
            }

            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
                return EditorGUI.ObjectField(position, parameter.Name, current as UnityEngine.Object, type, true);

            EditorGUI.LabelField(position, $"{parameter.Name} (unsupported: {type.Name})");
            return current;
        }

        private static bool GetParameterFoldout(string key) => SessionState.GetBool(key, false);

        private static void SetParameterFoldout(string key, bool value) => SessionState.SetBool(key, value);

        private static string GetParameterFoldoutKey(UnityEngine.Object rootObject, MethodInfo method, string path) => $"InspectorButton.Parameters.{rootObject?.GetInstanceID()}.{path}.{method.MetadataToken}";

        private static void WarnIfMissingCompatibility(Type type, UnityEngine.Object rootObject)
        {
            if (type.GetCustomAttribute<InspectorButtonCompatibilityAttribute>() != null)
                return;

            MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (MethodInfo method in methods)
            {
                if (method.GetCustomAttribute<InspectorButtonAttribute>() == null)
                    continue;

                if (compatibilityWarnings.Add(type))
                    Debug.LogError($"{ObjectNames.NicifyVariableName(type.Name)} is missing [InspectorButtonCompatibility]. [InspectorButton] only works on non-MonoBehaviour targets when they have the [InspectorButtonCompatibility] attribute.", rootObject);

                return;
            }
        }

        private static Type GetPropertyType(SerializedProperty property)
        {
            Type type = property.serializedObject.targetObject.GetType();
            string[] parts = property.propertyPath.Split('.');

            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];

                if (part == "Array" && i + 1 < parts.Length && parts[i + 1].StartsWith("data["))
                    return null;

                FieldInfo field = GetField(type, part);

                if (field == null)
                    return null;

                type = field.FieldType;
            }

            return type;
        }

        private static FieldInfo GetField(Type type, string fieldName)
        {
            while (type != null)
            {
                FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

                if (field != null)
                    return field;

                type = type.BaseType;
            }

            return null;
        }

        private static bool IsMarked(Type type) => type.GetCustomAttribute<InspectorButtonCompatibilityAttribute>() != null;

        private static object GetValue(object obj, string path)
        {
            if (obj == null)
                return null;

            string[] parts = path.Split('.');

            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];

                if (part == "Array" && i + 1 < parts.Length && parts[i + 1].StartsWith("data["))
                    return null;

                FieldInfo field = GetField(obj.GetType(), part);

                if (field == null)
                    return null;

                obj = field.GetValue(obj);

                if (obj == null)
                    return null;
            }

            return obj;
        }

        private static void SerializedObjectUpdate(UnityEngine.Object obj)
        {
            EditorUtility.SetDirty(obj);
        }
    }
}