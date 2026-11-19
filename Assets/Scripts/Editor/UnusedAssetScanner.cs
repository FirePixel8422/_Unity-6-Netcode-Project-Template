using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Object = UnityEngine.Object;

namespace Fire_Pixel.Utility
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Editor Tools/Unused Asset Scanner", order = 10000)]
    public class UnusedAssetScanner : ScriptableObject
    {
        [SerializeField] private string[] _ignoredFolders;
        [SerializeField] private string[] _ignoredPaths;
        [SerializeField] private string[] _ignoredExtensions;
        [SerializeField] private List<Object> _potentiallyUnusedAssets = new();

        public void ScanAssets()
        {
            Clear();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            string[] allAssets = AssetDatabase.GetAllAssetPaths();
            HashSet<string> referencedAssets = new();

            // Collect references from ALL assets.
            for (int i = 0; i < allAssets.Length; i++)
            {
                string path = allAssets[i];
                if (!path.StartsWith("Assets/")) continue;
                if (AssetDatabase.IsValidFolder(path)) continue;

                string[] dependencies = AssetDatabase.GetDependencies(path, false);

                for (int j = 0; j < dependencies.Length; j++)
                {
                    if (dependencies[j] != path)
                        referencedAssets.Add(dependencies[j]);
                }
            }

            // Collect references from compiled scripts.
            CollectScriptReferences(referencedAssets);

            // Apply ignore filters only when collecting unused assets.
            for (int i = 0; i < allAssets.Length; i++)
            {
                string path = allAssets[i];
                if (!path.StartsWith("Assets/")) continue;
                if (AssetDatabase.IsValidFolder(path)) continue;
                if (IsIgnoredFolder(path) || IsIgnoredPath(path) || IsIgnoredExtension(path)) continue;
                if (referencedAssets.Contains(path)) continue;

                Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (asset == null) continue;

                _potentiallyUnusedAssets.Add(asset);
            }

            _potentiallyUnusedAssets.Sort((a, b) =>
            {
                int typeComparison = string.Compare(a.GetType().Name, b.GetType().Name, StringComparison.Ordinal);
                return typeComparison != 0 ? typeComparison : string.Compare(a.name, b.name, StringComparison.Ordinal);
            });

            EditorUtility.SetDirty(this);
            Debug.Log($"Unused asset scan complete. Found {_potentiallyUnusedAssets.Count} potentially unused assets.");
        }

        private void CollectScriptReferences(HashSet<string> referencedAssets)
        {
            Dictionary<string, string> typeToAsset = new();

            string[] scripts = AssetDatabase.FindAssets("t:MonoScript");
            for (int i = 0; i < scripts.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(scripts[i]);
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (script == null) continue;

                Type type = script.GetClass();
                if (type == null) continue;

                typeToAsset[NormalizeTypeName(type.FullName)] = path;
            }

            string assemblyPath = Path.GetFullPath("Library/ScriptAssemblies");
            if (!Directory.Exists(assemblyPath)) return;

            string[] assemblies = Directory.GetFiles(assemblyPath, "*.dll");

            for (int i = 0; i < assemblies.Length; i++)
            {
                AssemblyDefinition assembly = null;

                try
                {
                    assembly = AssemblyDefinition.ReadAssembly(assemblies[i]);

                    foreach (ModuleDefinition module in assembly.Modules)
                    {
                        foreach (TypeDefinition type in module.Types)
                            CollectTypeReferences(type, typeToAsset, referencedAssets);
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogWarning($"Failed to scan assembly '{assemblies[i]}': {exception.Message}");
                }
                finally
                {
                    assembly?.Dispose();
                }
            }
        }
        private static void CollectTypeReferences(TypeDefinition type, Dictionary<string, string> typeToAsset, HashSet<string> referencedAssets)
        {
            string typeName = NormalizeTypeName(type.FullName);
            typeToAsset.TryGetValue(typeName, out string currentAssetPath);

            if (type.BaseType != null)
                CheckTypeReference(type.BaseType, currentAssetPath, typeToAsset, referencedAssets);

            foreach (InterfaceImplementation implementation in type.Interfaces)
                CheckTypeReference(implementation.InterfaceType, currentAssetPath, typeToAsset, referencedAssets);

            foreach (GenericParameter parameter in type.GenericParameters)
                CheckGenericConstraints(parameter, currentAssetPath, typeToAsset, referencedAssets);

            foreach (FieldDefinition field in type.Fields)
                CheckTypeReference(field.FieldType, currentAssetPath, typeToAsset, referencedAssets);

            foreach (PropertyDefinition property in type.Properties)
            {
                CheckTypeReference(property.PropertyType, currentAssetPath, typeToAsset, referencedAssets);

                foreach (ParameterDefinition parameter in property.Parameters)
                    CheckTypeReference(parameter.ParameterType, currentAssetPath, typeToAsset, referencedAssets);
            }

            foreach (EventDefinition eventDefinition in type.Events)
                CheckTypeReference(eventDefinition.EventType, currentAssetPath, typeToAsset, referencedAssets);

            foreach (MethodDefinition method in type.Methods)
            {
                CheckTypeReference(method.ReturnType, currentAssetPath, typeToAsset, referencedAssets);

                foreach (ParameterDefinition parameter in method.Parameters)
                    CheckTypeReference(parameter.ParameterType, currentAssetPath, typeToAsset, referencedAssets);

                foreach (GenericParameter parameter in method.GenericParameters)
                    CheckGenericConstraints(parameter, currentAssetPath, typeToAsset, referencedAssets);

                foreach (MethodReference methodOverride in method.Overrides)
                    CheckMethodReference(methodOverride, currentAssetPath, typeToAsset, referencedAssets);

                if (!method.HasBody) continue;

                foreach (Instruction instruction in method.Body.Instructions)
                {
                    if (instruction.Operand is TypeReference typeReference)
                    {
                        CheckTypeReference(typeReference, currentAssetPath, typeToAsset, referencedAssets);
                    }
                    else if (instruction.Operand is MethodReference methodReference)
                    {
                        CheckMethodReference(methodReference, currentAssetPath, typeToAsset, referencedAssets);
                    }
                    else if (instruction.Operand is FieldReference fieldReference)
                    {
                        CheckTypeReference(fieldReference.DeclaringType, currentAssetPath, typeToAsset, referencedAssets);
                        CheckTypeReference(fieldReference.FieldType, currentAssetPath, typeToAsset, referencedAssets);
                    }
                    else if (instruction.Operand is CallSite callSite)
                    {
                        CheckTypeReference(callSite.ReturnType, currentAssetPath, typeToAsset, referencedAssets);

                        foreach (ParameterDefinition parameter in callSite.Parameters)
                            CheckTypeReference(parameter.ParameterType, currentAssetPath, typeToAsset, referencedAssets);
                    }
                }
            }

            foreach (TypeDefinition nestedType in type.NestedTypes)
                CollectTypeReferences(nestedType, typeToAsset, referencedAssets);
        }
        private static void CheckMethodReference(MethodReference method, string currentAssetPath, Dictionary<string, string> typeToAsset, HashSet<string> referencedAssets)
        {
            CheckTypeReference(method.DeclaringType, currentAssetPath, typeToAsset, referencedAssets);
            CheckTypeReference(method.ReturnType, currentAssetPath, typeToAsset, referencedAssets);

            foreach (ParameterDefinition parameter in method.Parameters)
                CheckTypeReference(parameter.ParameterType, currentAssetPath, typeToAsset, referencedAssets);

            if (method is GenericInstanceMethod genericMethod)
            {
                foreach (TypeReference argument in genericMethod.GenericArguments)
                    CheckTypeReference(argument, currentAssetPath, typeToAsset, referencedAssets);
            }
        }
        private static void CheckGenericConstraints(GenericParameter parameter, string currentAssetPath, Dictionary<string, string> typeToAsset, HashSet<string> referencedAssets)
        {
            foreach (GenericParameterConstraint constraint in parameter.Constraints)
                CheckTypeReference(constraint.ConstraintType, currentAssetPath, typeToAsset, referencedAssets);
        }
        private static void CheckTypeReference(TypeReference type, string currentAssetPath, Dictionary<string, string> typeToAsset, HashSet<string> referencedAssets)
        {
            if (type == null) return;

            if (type is GenericInstanceType generic)
            {
                CheckTypeReference(generic.ElementType, currentAssetPath, typeToAsset, referencedAssets);

                foreach (TypeReference argument in generic.GenericArguments)
                    CheckTypeReference(argument, currentAssetPath, typeToAsset, referencedAssets);

                return;
            }

            if (type is OptionalModifierType optional)
            {
                CheckTypeReference(optional.ModifierType, currentAssetPath, typeToAsset, referencedAssets);
                CheckTypeReference(optional.ElementType, currentAssetPath, typeToAsset, referencedAssets);
                return;
            }

            if (type is RequiredModifierType required)
            {
                CheckTypeReference(required.ModifierType, currentAssetPath, typeToAsset, referencedAssets);
                CheckTypeReference(required.ElementType, currentAssetPath, typeToAsset, referencedAssets);
                return;
            }

            if (type is TypeSpecification specification)
            {
                CheckTypeReference(specification.ElementType, currentAssetPath, typeToAsset, referencedAssets);
                return;
            }

            string fullName = NormalizeTypeName(type.FullName);

            if (typeToAsset.TryGetValue(fullName, out string path) &&
                !string.Equals(path, currentAssetPath, StringComparison.OrdinalIgnoreCase))
            {
                referencedAssets.Add(path);
            }
        }

        private static string NormalizeTypeName(string typeName)
        {
            return typeName?.Replace('/', '+');
        }

        private bool IsIgnoredFolder(string path)
        {
            if (_ignoredFolders == null) return false;

            path = path.Replace('\\', '/');
            string[] pathParts = path.Split('/');

            for (int i = 0; i < _ignoredFolders.Length; i++)
            {
                string folder = _ignoredFolders[i];
                if (string.IsNullOrWhiteSpace(folder)) continue;

                folder = folder.Trim().Replace('\\', '/').TrimEnd('/');
                folder = Path.GetFileName(folder);

                if (string.IsNullOrEmpty(folder)) continue;

                for (int j = 1; j < pathParts.Length - 1; j++)
                {
                    if (string.Equals(pathParts[j], folder, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }

            return false;
        }
        private bool IsIgnoredPath(string path)
        {
            if (_ignoredPaths == null) return false;

            path = path.Replace('\\', '/');

            for (int i = 0; i < _ignoredPaths.Length; i++)
            {
                string ignoredPath = _ignoredPaths[i];
                if (string.IsNullOrWhiteSpace(ignoredPath)) continue;

                ignoredPath = ignoredPath.Trim().Replace('\\', '/').TrimEnd('/');

                if (!ignoredPath.StartsWith("Assets/", StringComparison.Ordinal))
                    ignoredPath = $"Assets/{ignoredPath}";

                if (path.Equals(ignoredPath, StringComparison.OrdinalIgnoreCase) ||
                    path.StartsWith($"{ignoredPath}/", StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
        private bool IsIgnoredExtension(string path)
        {
            if (_ignoredExtensions == null) return false;

            string extension = Path.GetExtension(path);
            string fileName = Path.GetFileNameWithoutExtension(path);

            for (int i = 0; i < _ignoredExtensions.Length; i++)
            {
                string ignored = _ignoredExtensions[i];
                if (string.IsNullOrWhiteSpace(ignored)) continue;

                ignored = ignored.Trim().TrimStart('.');

                if (string.Equals(extension, $".{ignored}", StringComparison.OrdinalIgnoreCase) ||
                    fileName.EndsWith($".{ignored}", StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        public void Clear()
        {
            _potentiallyUnusedAssets.Clear();
            EditorUtility.SetDirty(this);
        }
    }

    [CustomEditor(typeof(UnusedAssetScanner))]
    public class UnusedAssetScannerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Scan Assets"))
                ((UnusedAssetScanner)target).ScanAssets();

            if (GUILayout.Button("Clear"))
                ((UnusedAssetScanner)target).Clear();

            EditorGUILayout.HelpBox(
                "WARNING: Scanning all project assets can take a long time, especially in large projects.",
                UnityEditor.MessageType.Warning);

            base.OnInspectorGUI();
        }
    }
}