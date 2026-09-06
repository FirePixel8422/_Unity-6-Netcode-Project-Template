#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Unity.Netcode;

public class NetworkPrefabAutoNamer : AssetPostprocessor
{
    private const string Prefix = "NET_";

    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        int assetCount = importedAssets.Length;
        for (int i = 0; i < assetCount; i++)
        {
            string path = importedAssets[i];

            if (!path.EndsWith(".prefab"))
            {
                continue;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null || 
                prefab.GetComponent<NetworkObject>() == null ||
                prefab.name.StartsWith(Prefix))
            {
                continue;
            }

            string newName = Prefix + prefab.name;
            AssetDatabase.RenameAsset(path, newName);
        }
    }
}
#endif