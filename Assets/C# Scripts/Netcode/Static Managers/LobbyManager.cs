using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;


namespace Fire_Pixel.Networking
{
    public static class LobbyManager
    {
        public static Lobby CurrentLobby { get; private set; }
        public static string LobbyId => CurrentLobby.Id;
        public static string LobbyCode => CurrentLobby.Data[LobbyMaker.JOINCODE_STR].Value;

#pragma warning disable UDR0001
        private static Coroutine heartBeatCo;
#pragma warning restore UDR0001

#if Enable_Debug_Systems
        private static bool LogDebugInfo => ClientManager.Instance.LogDebugInfo;
#endif


        /// <summary>
        /// Set the lobby reference for host and clients here and start heartbeat coroutine if called from the server (or host)
        /// </summary>
        public static async Task SetLobbyData(Lobby lobby, bool calledFromHost = false)
        {
            CurrentLobby = lobby;

            if (calledFromHost)
            {
                heartBeatCo = NetworkManager.Singleton.StartCoroutine(HeartbeatLobbyCycle(LobbyId, 25));

                // Host doesnt have to save rejoin data, if host disconnects, the lobby will be deleted
                return;
            }

            await FileManager.SaveInfoAsync(new ValueWrapper<string>(LobbyId), LobbyMaker.REJOINDATA_PATH);
        }

        /// <summary>
        /// MUST be called on server. Deletes Lobby instantly
        /// </summary>
        public static void DeleteLobbyInstant_OnServer()
        {
            if (heartBeatCo != null)
            {
                NetworkManager.Singleton.StopCoroutine(heartBeatCo);
                heartBeatCo = null;
            }

            _ = UpdateLobbyDataAsync(LobbyId, LobbyMaker.LOBBY_TERMINATED_STR, "true");
            _ = LobbyService.Instance.DeleteLobbyAsync(LobbyId);
        }

        /// <summary>
        /// MUST be called on server. Sets Lobby.IsLocked state
        /// </summary>
        public static async Task SetLobbyLockStateAsync_OnServer(bool isLocked)
        {
            UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions()
            {
                IsLocked = isLocked,
            };

            try
            {
                // Update the lobby with the new field value
                CurrentLobby = await LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, updateLobbyOptions);
            }
            catch (System.Exception e)
            {
                DebugLogger.LogError($"Error updating lobby: {e.Message}");
            }
        }

        public static async Task UpdateLobbyDataAsync(string lobbyId, string key, string value)
        {
            try
            {
                if (CurrentLobby.Data.TryGetValue(key, out DataObject existingData))
                {
                    UpdateLobbyOptions updateOptions = new UpdateLobbyOptions
                    {
                        Data = new Dictionary<string, DataObject>
                        {
                            [key] = new DataObject(
                                visibility: existingData.Visibility,
                                value: value
                            )
                        }
                    };

                    CurrentLobby = await LobbyService.Instance.UpdateLobbyAsync(lobbyId, updateOptions);

#if Enable_Debug_Systems
                    DebugLogger.Log($"Lobby updated: {key} = {value}", LogDebugInfo);
#endif
                }
                else
                {
                    DebugLogger.LogWarning($"Failed to update lobby data: Data '{key}' does not exist");
                }
            }
            catch (LobbyServiceException e)
            {
                DebugLogger.LogWarning($"Failed to update lobby data: {e}");
            }
        }


        /// <summary>
        /// Send ping to server every "pingDelayTicks" so it doesnt auto delete itself.
        /// </summary>
        private static IEnumerator HeartbeatLobbyCycle(string lobbyId, float pingDelaySeconds)
        {
            WaitForSeconds delay = new WaitForSeconds(pingDelaySeconds);

            while (true)
            {
                _ = LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
                _ = UpdateLobbyDataAsync(lobbyId, LobbyMaker.LOBBY_LAST_HEARTBEAT_STR, DateTime.UtcNow.Ticks.ToString());

                yield return delay;
            }
        }
    }
}