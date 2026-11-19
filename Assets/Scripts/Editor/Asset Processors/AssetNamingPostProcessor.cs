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

            if (prefix == null || IsGeneratedLightingAsset(path)) continue;

            string fileName = Path.GetFileNameWithoutExtension(path);

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

    // Lightmaps and reflection probes are written by the lightmapper into a folder named after the scene
    // (e.g. Scenes/MyScene/Lightmap-0_comp_light.exr). Renaming them mid-bake breaks the LightingData references and crashes the editor.
    private static bool IsGeneratedLightingAsset(string path)
    {
        if (Lightmapping.isRunning) return true;

        string folder = Path.GetDirectoryName(path)?.Replace('\\', '/');

        return !string.IsNullOrEmpty(folder) && File.Exists(folder + ".unity");
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

            ".anim" => "Anim_",
            ".controller" => "AnimCon_",

            ".wav" => "Aud_",
            ".mp3" => "Aud_",
            ".ogg" => "Aud_",
            ".aiff" => "Aud_",
            ".aif" => "Aud_",
            ".flac" => "Aud_",
            ".mod" => "Aud_",
            ".it" => "Aud_",
            ".s3m" => "Aud_",
            ".xm" => "Aud_",

            ".shader" => "Sha_",
            ".shadergraph" => "Sha_",
            ".shadersubgraph" => "Sha_",

            _ => null
        };
    }
}