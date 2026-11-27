using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Unity.Netcode;

public class NetworkPrefabAutoNamer : AssetPostprocessor
{
    private const string Prefix = "Net_";

    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        for (int i = 0; i < importedAssets.Length; i++)
        {
            string path = importedAssets[i];

            if (!Path.GetExtension(path).Equals(".prefab", StringComparison.OrdinalIgnoreCase)) continue;

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null || !prefab.transform.HasComponent<NetworkObject>()) continue;

            string fileName = Path.GetFileNameWithoutExtension(path);

            if (fileName.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
            {
                fileName = Prefix + fileName.Substring(Prefix.Length);
            }
            else
            {
                fileName = Prefix + fileName;
            }

            AssetDatabase.RenameAsset(path, fileName);
        }
    }
}