using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using UnityEngine;


namespace FirePixel.Networking
{
    /// <summary>
    /// Utility class to Save/Load data from or to User unique cloud or global cloud, supporting local fallback through <see cref="FileManager"></see>
    /// </summary>
    public static class CloudSaveManager
    {
        #region User Cloud System

        /// <summary>
        /// Save a value to the User's Unity Cloud Save service.
        /// Also Save Local Fallback through <see cref="FileManager"></see> if <paramref name="localFallbackPath"/> is provided
        /// </summary>
        public static async Task<bool> SaveInfoToUserCloudAsync<T>(T data, string key, string localFallbackPath = "")
        {
            try
            {
                // Serialize and save
                await CloudSaveService.Instance.Data.Player.SaveAsync(new Dictionary<string, object> { { key, data } });
                
                if (localFallbackPath != "")
                {
                    await FileManager.SaveInfoAsync(new ValueWrapper<T>(data), localFallbackPath);
                }

                return true;
            }
            catch (Exception ex)
            {
                DebugLogger.LogError($"[CloudSave] Failed to save key '{key}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Load a value from the User's Unity Cloud Save service.
        /// Load Local Fallback through <see cref="FileManager"/> if cloud load fails. (Only if <paramref name="localFallbackPath"/> is provided)
        /// </summary>
        public static async Task<(bool Succes, T Value)> LoadInfoFromUserCloudAsync<T>(string key, string localFallbackPath = "")
        {
            try
            {
                var result = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { key });

                if (result.TryGetValue(key, out Item item))
                {
                    return (true, item.Value.GetAs<T>());
                }
                else if (string.IsNullOrEmpty(localFallbackPath) == false)
                {
                    (bool succes, ValueWrapper<T> data) = await FileManager.LoadInfoAsync<ValueWrapper<T>>(localFallbackPath);
                    return (succes, data.Value);
                }

                DebugLogger.LogWarning($"[CloudSave] Key '{key}' not found, and potential fallback failed");
                return (false, default);
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"[CloudSave] Failed to load key '{key}': {ex.Message}" + (localFallbackPath != "" ? "" : ", Using local fallback"));

                if (localFallbackPath != "")
                {
                    return await FileManager.LoadInfoAsync<T>(localFallbackPath);
                }

                return (false, default);
            }
        }

        /// <summary>
        /// Deletes a value from the User's Unity Cloud Save service if it exists.
        /// Delete Local Fallback through <see cref="FileManager"></see> if <paramref name="localFallbackPath"/> is provided
        /// </summary>
        public static async Task<bool> TryDeleteFileFromUserCloudAsync(string key, string localFallbackPath = "")
        {
            try
            {
                var result = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { key });

                if (!result.ContainsKey(key))
                {
                    DebugLogger.LogWarning($"[CloudSave] Key '{key}' not found. Skipping delete.");
                    return false;
                }

                await CloudSaveService.Instance.Data.Player.DeleteAsync(
                    key,
                    new Unity.Services.CloudSave.Models.Data.Player.DeleteOptions()
                );

                // Delete local fallback if path provided
                if (localFallbackPath != "")
                {
                    FileManager.TryDeleteFile(localFallbackPath);
                }

                return true;
            }
            catch (Exception ex)
            {
                DebugLogger.LogError($"[CloudSave] Failed to delete key '{key}': {ex.Message}");
                return false;
            }
        }


        #endregion




        ///Global Cloud is a work in progress
        ///Global Cloud is a work in progress
        ///Global Cloud is a work in progress
        ///Global Cloud is a work in progress
        ///Global Cloud is a work in progress
        ///Global Cloud is a work in progress
        ///Global Cloud is a work in progress
        ///Global Cloud is a work in progress
        ///Global Cloud is a work in progress
        ///Global Cloud is a work in progress
        ///Global Cloud is a work in progress
        ///Global Cloud is a work in progress
    }
}
