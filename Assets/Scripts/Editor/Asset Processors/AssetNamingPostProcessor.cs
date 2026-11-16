using System;
using System.IO;
using UnityEditor;

public class AssetNamingPostprocessor : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string path in importedAssets)
        {
            string prefix = GetPrefix(path);

            if (prefix == null)
            {
                continue;
            }

            string fileName = Path.GetFileNameWithoutExtension(path).Replace(" ", "_").TrimStart('_');

            if (fileName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                fileName = prefix + fileName.Substring(prefix.Length);
            }
            else
            {
                fileName = prefix + fileName;
            }

            AssetDatabase.RenameAsset(path, fileName);
        }
    }

    private static string GetPrefix(string path)
    {
        string extension = Path.GetExtension(path).ToLowerInvariant();

        return extension switch
        {
            ".fbx" => "Mdl_",
            ".obj" => "Mdl_",
            ".blend" => "Mdl_",
            ".dae" => "Mdl_",
            ".3ds" => "Mdl_",
            ".dxf" => "Mdl_",
            ".max" => "Mdl_",
            ".ma" => "Mdl_",
            ".mb" => "Mdl_",

            ".mat" => "Mat_",

            ".png" => "Tex_",
            ".jpg" => "Tex_",
            ".jpeg" => "Tex_",
            ".tga" => "Tex_",
            ".psd" => "Tex_",
            ".tif" => "Tex_",
            ".tiff" => "Tex_",
            ".exr" => "Tex_",
            ".hdr" => "Tex_",
            ".bmp" => "Tex_",

            ".shader" => "Sha_",
            ".shadergraph" => "Sha_",
            ".shadersubgraph" => "Sha_",

            _ => null
        };
    }
}