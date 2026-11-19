using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;

namespace Fire_Pixel.Networking
{
    /// <summary>Provides access to Unity Cloud Save with optional local SaveData fallback.</summary>
    public static class CloudSaveManager
    {
        /// <summary>Saves a value to the user's cloud data and optionally to local SaveData.</summary>
        public static async Task<bool> SaveToUserCloudAsync<T>(T data, string key, string localFallbackKey = "")
        {
            try
            {
                await CloudSaveService.Instance.Data.Player.SaveAsync(new Dictionary<string, object>
                {
                    { key, data }
                });

                if (localFallbackKey != "")
                    SaveData.Set(localFallbackKey, data);

                return true;
            }
            catch (Exception ex)
            {
                DebugLogger.LogError($"[CloudSave] Failed to save key '{key}': {ex.Message}");
                return false;
            }
        }

        /// <summary>Loads a value from the user's cloud data and optionally falls back to local SaveData.</summary>
        public static async Task<(bool Success, T Value)> LoadFromUserCloudAsync<T>(string key, string localFallbackKey = "")
        {
            try
            {
                Dictionary<string, Item> result = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { key });

                if (result.TryGetValue(key, out Item item))
                    return (true, item.Value.GetAs<T>());

                if (localFallbackKey != "" && SaveData.TryGet(localFallbackKey, out T value))
                    return (true, value);

                DebugLogger.LogWarning($"[CloudSave] Key '{key}' not found, and local fallback failed.");
                return (false, default);
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"[CloudSave] Failed to load key '{key}': {ex.Message}");

                if (localFallbackKey != "" && SaveData.TryGet(localFallbackKey, out T value))
                    return (true, value);

                return (false, default);
            }
        }

        /// <summary>Deletes a value from the user's cloud data and optionally removes its local SaveData fallback.</summary>
        public static async Task<bool> TryDeleteFromUserCloudAsync(string key, string localFallbackKey = "")
        {
            try
            {
                Dictionary<string, Item> result = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { key });

                if (!result.ContainsKey(key))
                {
                    DebugLogger.LogWarning($"[CloudSave] Key '{key}' not found. Skipping delete.");
                    return false;
                }

                await CloudSaveService.Instance.Data.Player.DeleteAsync(
                    key,
                    new Unity.Services.CloudSave.Models.Data.Player.DeleteOptions()
                );

                if (localFallbackKey != "")
                    SaveData.Delete(localFallbackKey);

                return true;
            }
            catch (Exception ex)
            {
                DebugLogger.LogError($"[CloudSave] Failed to delete key '{key}': {ex.Message}");
                return false;
            }
        }
    }
}