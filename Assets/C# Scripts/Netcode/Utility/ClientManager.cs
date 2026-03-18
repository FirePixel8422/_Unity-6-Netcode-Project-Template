using Fire_Pixel.Utility;
using System;
using Unity.Netcode;
using UnityEngine;


namespace Fire_Pixel.Networking
{
    public class ClientManager : NetworkBehaviour
    {
        public static ClientManager Instance { get; private set; }
        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        [Header("Scene to load when exiting a lobby for any reason")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        [Space(8)]
        [SerializeField] private NetworkStruct<PlayerIdDataArray> playerIdDataArray = new NetworkStruct<PlayerIdDataArray>();

#pragma warning disable UDR0001
        public static OneTimeAction PostInitialized = new OneTimeAction();
#pragma warning restore UDR0001

#if Enable_Debug_Systems
        public bool LogDebugInfo = true;
#endif


        #region PlayerIdDataArray var get, set and sync methods

        /// <summary>
        /// Get PlayerIdDataArray Copy (changes on copy wont sync back to clientManager and wont cause a networkSync unless sent back with  <see cref="SetPlayerIdDataArray_OnServer"/>")
        /// </summary>
        public static PlayerIdDataArray GetPlayerIdDataArray()
        {
            return Instance.playerIdDataArray.Value;
        }

        /// <summary>
        /// Set Value Of PlayerIdDataArray, Must be called from server (Will trigger networkSync)
        /// </summary>
        public static void SetPlayerIdDataArray_OnServer(PlayerIdDataArray newValue)
        {
#if Enable_Debug_Systems
            DebugLogger.LogError("SetPlayerIdDataArray_OnServer called on non server Client, this should only be called from the server!", Instance.IsServer == false);
#endif
            Instance.playerIdDataArray.Value = newValue;
        }


        private void SendPlayerIdDataArrayChange_OnServer(PlayerIdDataArray newValue)
        {
            ReceivePlayerIdDataArray_ClientRPC(newValue, RPCTargetFilters.SendToAllButHost());
        }

        [ClientRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
        private void ReceivePlayerIdDataArray_ClientRPC(PlayerIdDataArray newValue, ClientRpcParams rpcParams = default)
        {
            if (IsHost && RPCTargetFilters.ShouldHostSkip(rpcParams)) return;

            playerIdDataArray.Value = newValue;
        }


        [ServerRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
        private void RequestPlayerIdDataArray_ServerRPC(ServerRpcParams rpcParams = default)
        {
            ulong senderClientNetworkId = rpcParams.Receive.SenderClientId;

            ReceiveSilentPlayerIdDataArray_ClientRPC(playerIdDataArray.Value, RPCTargetFilters.SendToTargetClient(senderClientNetworkId));
        }

        [ClientRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
        private void ReceiveSilentPlayerIdDataArray_ClientRPC(PlayerIdDataArray newValue, ClientRpcParams rpcParams = default)
        {
            if (IsHost && RPCTargetFilters.ShouldHostSkip(rpcParams)) return;

            playerIdDataArray.SilentValue = newValue;
        }

        #endregion


        #region  OnConnect, OnDisconnect and OnKicked Callbacks

#pragma warning disable UDR0001
        /// <summary>
        /// Invoked after NetworkManager.OnClientConnected, before updating ClientManager gameId logic.
        /// </summary>
        public static event Action<ClientSessionContext> OnClientConnectedCallback;

        /// <summary>
        /// Invoked after <see cref="NetworkManager.OnClientDisconnectCallback"/>, before updating <see cref="ClientManager"/> gameId logic.
        /// </summary>
        public static event Action<ClientSessionContext> OnClientDisconnectedCallback;
#pragma warning restore UDR0001

        #endregion


        #region Usefull Data and LocalClient Data

        /// <summary>
        /// Local Client gameId, an int ranging from 0 to MaxPlayers-1
        /// </summary>
        public static int LocalClientGameId { get; private set; }

       /// <summary>
        /// Amount of Players in server that have been setup by ClientManager (game/team ID System)
        /// </summary>
        public static int PlayerCount => Instance.playerIdDataArray.Value.PlayerCount;

        /// <summary>
        /// Amount of Players in server that have been setup is 1 higher then the highestPlayerId
        /// </summary>
        public static ulong UnAsignedPlayerId => (ulong)Instance.playerIdDataArray.Value.PlayerCount;


        /// <summary>
        /// Local Client UserName, value is set by <see cref="PlayerNameHandler"/>
        /// </summary>
        public static string LocalUserName { get; private set; }
        public static void SetLocalUsername(string name)
        {
            LocalUserName = name;
        }

        /// <summary>
        /// Local Player GUID, value is set by loaded or generated through LobbyMaker
        /// </summary>
        public static string LocalPlayerGUID { get; private set; }
        public static void SetPlayerGUID(string guid)
        {
            LocalPlayerGUID = guid;
        }

        #endregion


        #region Send/Recieve Username and GUID and set that data in PlayerIdDataArray

        [ClientRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
        private void RequestUsernameAndGUID_ClientRPC(int fromPlayerGameId, ClientRpcParams rpcParams = default)
        {
            if (IsHost && RPCTargetFilters.ShouldHostSkip(rpcParams)) return;

            SendUsernameAndGUID_ServerRPC(fromPlayerGameId, LocalUserName, LocalPlayerGUID);
        }

        [ServerRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
        private void SendUsernameAndGUID_ServerRPC(int fromPlayerGameId, string username, string guid)
        {
            PlayerIdDataArray updatedDataArray = playerIdDataArray.Value;
            updatedDataArray.SetUserNameAndGUID(fromPlayerGameId, username, guid);
            playerIdDataArray.Value = updatedDataArray;
        }

        #endregion


        #region Get Player Data From PlayerIdDataArray Methods

        /// <summary>
        /// Turn GameId into NetworkId
        /// </summary>
        public static ulong GetClientNetworkId(int gameId) => Instance.playerIdDataArray.Value.GetPlayerNetworkId(gameId);

        /// <summary>
        /// Turn NetworkId into GameId
        /// </summary>
        public static int GetClientGameId(ulong networkId) => Instance.playerIdDataArray.Value.GetPlayerGameId(networkId);

        /// <summary>
        /// Turn GameId into Player Username
        /// </summary>
        public static string GetPlayerName(int gameId) => Instance.playerIdDataArray.Value.GetUserName(gameId);

        #endregion



        public override void OnNetworkSpawn()
        {
            playerIdDataArray = new NetworkStruct<PlayerIdDataArray>(new PlayerIdDataArray(GlobalGameData.MAX_PLAYERS));

            if (IsServer)
            {
                // On value changed event of playerIdDataArray
                playerIdDataArray.OnValueChanged += (PlayerIdDataArray newValue) =>
                {
                    SendPlayerIdDataArrayChange_OnServer(newValue);
                };

                // host (server) is always gameId 0
                LocalClientGameId = 0;

                // Setup server only events
                NetworkManager.OnClientConnectedCallback += OnClientConnected_OnServer;
                NetworkManager.OnClientDisconnectCallback += OnClientDisconnected_OnServer;

                PostInitialized?.Invoke();
            }
            else
            {
                // Catches up late joining clients with newest value
                RequestPlayerIdDataArray_ServerRPC();

                // On value changed event of playerIdDataArray
                playerIdDataArray.OnValueChanged += (PlayerIdDataArray newValue) =>
                {
                    LocalClientGameId = newValue.GetPlayerGameId(NetworkManager.LocalClientId);
                };
                playerIdDataArray.OnValueChanged += FinishSystemInitialization;
            }

            // Setup server and client event
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnected_OnClient;
        }

        private void FinishSystemInitialization(PlayerIdDataArray newValue)
        {
            playerIdDataArray.OnValueChanged -= FinishSystemInitialization;
            PostInitialized?.Invoke();
        }


        #region Client OnDisconnect/OnDisconnect Methods

        /// <summary>
        /// When a clients joins the lobby, called on the server only
        /// </summary>
        private void OnClientConnected_OnServer(ulong clientNetworkId)
        {
            PlayerIdDataArray updatedDataArray = playerIdDataArray.Value;
            updatedDataArray.AddPlayer(clientNetworkId);
            playerIdDataArray.Value = updatedDataArray;

            RequestUsernameAndGUID_ClientRPC(GetClientGameId(clientNetworkId), RPCTargetFilters.SendToTargetClient(clientNetworkId));

            OnClientConnectedCallback?.Invoke(new ClientSessionContext()
            {
                NetworkId = clientNetworkId,
                GameId = playerIdDataArray.Value.GetPlayerGameId(clientNetworkId),
                PlayerCount = NetworkManager.ConnectedClients.Count,
            });

#if Enable_Debug_Systems
            DebugLogger.Log("Player " + GetClientGameId(clientNetworkId) + ", (NetworkId: " + clientNetworkId + "), connected to server!", LogDebugInfo);
#endif
        }

        /// <summary>
        /// When a client leaves the lobby, called on the server only
        /// </summary>
        private void OnClientDisconnected_OnServer(ulong clientNetworkId)
        {
#if Enable_Debug_Systems
            DebugLogger.Log("Player " + GetClientGameId(clientNetworkId) + ", (NetworkId: " + clientNetworkId + "), disconnected from server", LogDebugInfo);
#endif

            // If the diconnecting client is the host dont update data, the server is shut down anyways.
            if (clientNetworkId == 0) return;

            PlayerIdDataArray updatedDataArray = GetPlayerIdDataArray();
            updatedDataArray.RemovePlayer(clientNetworkId);
            playerIdDataArray.Value = updatedDataArray;

            OnClientDisconnectedCallback?.Invoke(new ClientSessionContext()
            {
                NetworkId = clientNetworkId,
                GameId = playerIdDataArray.Value.GetPlayerGameId(clientNetworkId),
                PlayerCount = PlayerCount,
            });
        }

        /// <summary>
        /// When a client leaves the lobby, called only on disconnecting client
        /// </summary>
        private void OnClientDisconnected_OnClient(ulong clientNetworkId)
        {
            // Call function only on client who disconnected or on everyone if the host disconnected
            if (clientNetworkId != NetworkManager.LocalClientId && clientNetworkId != 0) return;

            Destroy(gameObject);

            // If the disconnecting client is the server (host), unsubscribe subscribed NetworkManager Callbacks
            if (IsServer)
            {
                NetworkManager.OnClientConnectedCallback -= OnClientConnected_OnServer;
                NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected_OnServer;
            }

            NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected_OnClient;

            // If The host disconnected
            if (clientNetworkId == 0)
            {
#if Enable_Debug_Systems
                // Destroy the rejoin reference on the kicked client
                bool deletionSucces = FileManager.TryDeleteFile(LobbyMaker.REJOINDATA_PATH);
                DebugLogger.Log($"{LobbyMaker.REJOINDATA_PATH} deleted: " + deletionSucces, LogDebugInfo);
#else
                // Destroy the rejoin reference on the kicked client
                FileManager.TryDeleteFile(LobbyMaker.REJOINDATA_PATH);
#endif

                if (MessageHandler.Instance != null)
                {
                    MessageHandler.Instance.SendTextLocal("You have been kicked from the server!");
                }
            }

            SceneManager.LoadScene(mainMenuSceneName);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        #endregion


        #region Kick and Shutdown Methods

        [ServerRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
        public void KickTargetClient_ServerRPC(ulong clientNetworkId)
        {
            NetworkManager.DisconnectClient(clientNetworkId);
        }

        [ServerRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
        public void ShutDownNetwork_ServerRPC()
        {
            if (IsServer == false) return;

            for (int i = 1; i < PlayerCount; i++)
            {
                NetworkManager.DisconnectClient(GetClientNetworkId(i));
            }

            // Terminate lobby and shutdown network.
            LobbyManager.DeleteLobbyInstant_OnServer();
            NetworkManager.Shutdown();
        }

        #endregion


        public override void OnDestroy()
        {
            base.OnDestroy();

            playerIdDataArray.ResetEventCallbacks();
            OnClientConnectedCallback = null;
            OnClientDisconnectedCallback = null;
            PostInitialized = new OneTimeAction();

            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.Shutdown();
            }
        }
    }
}