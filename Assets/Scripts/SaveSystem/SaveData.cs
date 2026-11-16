using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Provides global persistent storage for arbitrary serializable values.
/// </summary>
public static class SaveData
{
    private const bool ENABLE_DEBUG_LOGGING = false;
    private const string FILE_NAME = "SaveData.json";

    private static readonly Dictionary<string, string> _data = new();


    #region Load SaveFile

    public static bool Load()
    {
        return Load(false, false);
    }
    public static bool Load(bool deObfuscateFile, bool decryptFile)
    {
        string path = GetPath();

        if (!File.Exists(path))
            return true;

        try
        {
            string json = File.ReadAllText(path);

            if (decryptFile)
            {
                json = EncryptionUtility.DecryptAsync(json).GetAwaiter().GetResult();
            }

            if (deObfuscateFile)
            {
                (bool success, string deObfuscatedData) = ObfuscationUtility.DeObfuscate(json);

                if (!success)
                    return false;

                json = deObfuscatedData;
            }

            SaveFile saveFile = JsonUtility.FromJson<SaveFile>(json);

            _data.Clear();

            if (saveFile?.Entries != null)
            {
                for (int i = 0; i < saveFile.Entries.Count; i++)
                {
                    SaveEntry entry = saveFile.Entries[i];

                    if (!string.IsNullOrEmpty(entry.Key))
                        _data[entry.Key] = entry.Value;
                }
            }

            DebugLogger.Log($"Loaded {_data.Count} save entries.", ENABLE_DEBUG_LOGGING);
            return true;
        }
        catch (Exception ex)
        {
            DebugLogger.LogError($"Failed to load save data: {ex.Message}", ENABLE_DEBUG_LOGGING);
            return false;
        }
    }

    public static Task<bool> LoadAsync()
    {
        return LoadAsync(false, false);
    }
    public static async Task<bool> LoadAsync(bool deObfuscateFile, bool decryptFile)
    {
        string path = GetPath();

        if (!File.Exists(path))
            return true;

        try
        {
            string json = await File.ReadAllTextAsync(path);

            if (decryptFile)
            {
                json = await EncryptionUtility.DecryptAsync(json);
            }

            if (deObfuscateFile)
            {
                (bool success, string deObfuscatedData) = ObfuscationUtility.DeObfuscate(json);

                if (!success)
                    return false;

                json = deObfuscatedData;
            }

            SaveFile saveFile = JsonUtility.FromJson<SaveFile>(json);

            _data.Clear();

            if (saveFile?.Entries != null)
            {
                for (int i = 0; i < saveFile.Entries.Count; i++)
                {
                    SaveEntry entry = saveFile.Entries[i];

                    if (!string.IsNullOrEmpty(entry.Key))
                        _data[entry.Key] = entry.Value;
                }
            }

            DebugLogger.Log($"Loaded {_data.Count} save entries.", ENABLE_DEBUG_LOGGING);
            return true;
        }
        catch (Exception ex)
        {
            DebugLogger.LogError($"Failed to load save data: {ex.Message}", ENABLE_DEBUG_LOGGING);
            return false;
        }
    }

    #endregion


    #region Save SaveFile

    public static bool Save()
    {
        return Save(false, false);
    }
    public static bool Save(bool obfuscateFile, bool encryptFile)
    {
        try
        {
            string path = GetPath();
            string directory = Path.GetDirectoryName(path);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string json = JsonUtility.ToJson(CreateSaveFile(), true);

            if (obfuscateFile)
            {
                json = ObfuscationUtility.Obfuscate(json);
            }

            if (encryptFile)
            {
                json = EncryptionUtility.EncryptAsync(json).GetAwaiter().GetResult();
            }

            File.WriteAllText(path, json);

            DebugLogger.Log($"Saved {_data.Count} save entries.", ENABLE_DEBUG_LOGGING);
            return true;
        }
        catch (Exception ex)
        {
            DebugLogger.LogError($"Failed to save data: {ex.Message}", ENABLE_DEBUG_LOGGING);
            return false;
        }
    }

    public static Task<bool> SaveAsync()
    {
        return SaveAsync(false, false);
    }
    public static async Task<bool> SaveAsync(bool obfuscateFile, bool encryptFile)
    {
        try
        {
            string path = GetPath();
            string directory = Path.GetDirectoryName(path);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string json = JsonUtility.ToJson(CreateSaveFile(), true);

            if (obfuscateFile)
            {
                json = ObfuscationUtility.Obfuscate(json);
            }

            if (encryptFile)
            {
                json = await EncryptionUtility.EncryptAsync(json);
            }

            await File.WriteAllTextAsync(path, json);

            DebugLogger.Log($"Saved {_data.Count} save entries.", ENABLE_DEBUG_LOGGING);
            return true;
        }
        catch (Exception ex)
        {
            DebugLogger.LogError($"Failed to save data: {ex.Message}", ENABLE_DEBUG_LOGGING);
            return false;
        }
    }

    #endregion


    #region Get/Set SaveData

    public static void Set<T>(string key, T value)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Save data key cannot be null or empty.", nameof(key));

        try
        {
            _data[key] = JsonUtility.ToJson(new ValueWrapper<T>(value));
            DebugLogger.Log($"Set '{key}'.", ENABLE_DEBUG_LOGGING);
        }
        catch (Exception ex)
        {
            DebugLogger.LogError($"Failed to set '{key}': {ex.Message}", ENABLE_DEBUG_LOGGING);
            throw;
        }
    }

    public static T Get<T>(string key)
    {
        if (!TryGet(key, out T value))
            throw new KeyNotFoundException($"Save data key '{key}' was not found.");

        return value;
    }
    public static bool TryGet<T>(string key, out T value)
    {
        value = default;

        if (!_data.TryGetValue(key, out string serializedValue))
            return false;

        try
        {
            ValueWrapper<T> wrapper = JsonUtility.FromJson<ValueWrapper<T>>(serializedValue);
            value = wrapper.Value;
            DebugLogger.Log($"Got '{key}'.", ENABLE_DEBUG_LOGGING);
            return true;
        }
        catch (Exception ex)
        {
            DebugLogger.LogError($"Failed to get '{key}': {ex.Message}", ENABLE_DEBUG_LOGGING);
            return false;
        }
    }

    #endregion


    public static bool Delete(string key)
    {
        bool removed = _data.Remove(key);

        if (removed)
        {
            DebugLogger.Log($"Removed '{key}'.", ENABLE_DEBUG_LOGGING);
        }

        return removed;
    }


    public static bool Contains(string key)
    {
        return _data.ContainsKey(key);
    }
    public static void Clear()
    {
        _data.Clear();
        DebugLogger.Log("Cleared all save data.", ENABLE_DEBUG_LOGGING);
    }

    private static SaveFile CreateSaveFile()
    {
        SaveFile saveFile = new();

        foreach (KeyValuePair<string, string> pair in _data)
        {
            saveFile.Entries.Add(new SaveEntry
            {
                Key = pair.Key,
                Value = pair.Value
            });
        }

        return saveFile;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetPath()
    {
        return Path.Combine(Application.persistentDataPath, FILE_NAME);
    }

    [Serializable]
    private class SaveFile
    {
        public List<SaveEntry> Entries = new();
    }
    [Serializable]
    private struct SaveEntry
    {
        public string Key;
        public string Value;
    }
    [Serializable]
    private struct ValueWrapper<T>
    {
        public T Value;

        public ValueWrapper(T value)
        {
            Value = value;
        }
    }
}