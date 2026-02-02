using System;
using Unity.Netcode;
using UnityEngine;


namespace FirePixel.Networking
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

        [Header("Log Debug information")]
        [SerializeField] private bool logDebugInfo = true;

        [SerializeField] private NetworkStruct<PlayerIdDataArray> playerIdDataArray = new NetworkStruct<PlayerIdDataArray>();


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
#if UNITY_EDITOR
            DebugLogger.LogError("SetPlayerIdDataArray_OnServer called on non server Client, this should only be called from the server!", Instance.IsServer == false);
#endif
            Instance.playerIdDataArray.Value = newValue;
        }


        private void SendPlayerIdDataArrayChange_OnServer(PlayerIdDataArray newValue)
        {
            ReceivePlayerIdDataArray_ClientRPC(newValue, NetworkIdRPCTargets.SendToAllButServer());
        }

        [ClientRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
        private void ReceivePlayerIdDataArray_ClientRPC(PlayerIdDataArray newValue, NetworkIdRPCTargets rpcTargets)
        {
            if (rpcTargets.IsTarget == false) return;

            playerIdDataArray.Value = newValue;
        }


        [ServerRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
        private void RequestPlayerIdDataArray_ServerRPC(ulong clientNetworkId)
        {
            ReceiveSilentPlayerIdDataArray_ClientRPC(playerIdDataArray.Value, NetworkIdRPCTargets.SendToTargetClient(clientNetworkId));
        }

        [ClientRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
        private void ReceiveSilentPlayerIdDataArray_ClientRPC(PlayerIdDataArray newValue, NetworkIdRPCTargets rpcTargets)
        {
            if (rpcTargets.IsTarget == false) return;

            playerIdDataArray.SilentValue = newValue;
        }

        #endregion


        #region OnInitialized Callback System

#pragma warning disable UDR0001
#pragma warning disable UDR0004
        /// <summary>
        /// Invoked after <see cref="ClientManager.OnNetworkSpawn"/> is called.
        /// </summary>
        private static Action OnInitialized;
        private static bool initialized;

        /// <summary>
        /// Invoke action after <see cref="ClientManager.OnNetworkSpawn"/> is called or instantly if that already happened
        /// </summary>
        public static void GetOnInitializedCallback(Action toExecute)
        {
            if (initialized == false)
            {
                OnInitialized += toExecute;
            }
            else
            {
                toExecute.Invoke();
            }
        }
#pragma warning restore UDR0001
#pragma warning restore UDR0004

        #endregion


        #region  OnConnect, OnDisconnect and OnKicked Callbacks

#pragma warning disable UDR0001
        /// <summary>
        /// Invoked after NetworkManager.OnClientConnected, before updating ClientManager gameId logic. returns: ulong clientId, int clientGamId, int clientInLobbyCount
        /// </summary>
        public static Action<ulong, int, int> OnClientConnectedCallback;

        /// <summary>
        /// Invoked after <see cref="NetworkManager.OnClientDisconnectCallback"/>, before updating <see cref="ClientManager"/> gameId logic. returns: ulong clientId, int clientGamId, int clientInLobbyCount
        /// </summary>
        public static Action<ulong, int, int> OnClientDisconnectedCallback;

        /// <summary>
        /// Invoked when a client is kicked from the server, before destroying the <see cref="ClientManager"/> gameObject.
        /// </summary>
        public static Action OnKicked;
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
        private void RequestUsernameAndGUID_ClientRPC(int fromPlayerGameId, NetworkIdRPCTargets rpcTargets)
        {
            if (rpcTargets.IsTarget == false) return;

            SendUsernameAndGUID_ServerRPC(fromPlayerGameId, LocalUserName, LocalPlayerGUID);
        }

        [ServerRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
        private void SendUsernameAndGUID_ServerRPC(int fromPlayerGameId, string username, string guid)
        {
            playerIdDataArray.SilentValue.SetUserNameAndGUID(fromPlayerGameId, username, guid);
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
            playerIdDataArray = new NetworkStruct<PlayerIdDataArray>(new PlayerIdDataArray(GlobalGameData.MaxPlayers));

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
            }
            else
            {
                // Catches up late joining clients with newest value
                RequestPlayerIdDataArray_ServerRPC(NetworkManager.LocalClientId);

                // On value changed event of playerIdDataArray
                playerIdDataArray.OnValueChanged += (PlayerIdDataArray newValue) =>
                {
                    LocalClientGameId = newValue.GetPlayerGameId(NetworkManager.LocalClientId);
                };
            }

            // Setup server and client event
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnected_OnClient;

            OnInitialized?.Invoke();
            OnInitialized = null;
            initialized = true;
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

            RequestUsernameAndGUID_ClientRPC(GetClientGameId(clientNetworkId), NetworkIdRPCTargets.SendToTargetClient(clientNetworkId));

            OnClientConnectedCallback?.Invoke(clientNetworkId, playerIdDataArray.Value.GetPlayerGameId(clientNetworkId), NetworkManager.ConnectedClients.Count);

            DebugLogger.Log("Player " + GetClientGameId(clientNetworkId) + ", (NetworkId: " + clientNetworkId + "), connected to server!", logDebugInfo);
        }

        /// <summary>
        /// When a client leaves the lobby, called on the server only
        /// </summary>
        private void OnClientDisconnected_OnServer(ulong clientNetworkId)
        {
            DebugLogger.Log("Player " + GetClientGameId(clientNetworkId) + ", (NetworkId: " + clientNetworkId + "), disconnected from server", logDebugInfo);

            // If the diconnecting client is the host dont update data, the server is shut down anyways.
            if (clientNetworkId == 0) return;

            PlayerIdDataArray updatedDataArray = GetPlayerIdDataArray();

            updatedDataArray.RemovePlayer(clientNetworkId);

            playerIdDataArray.Value = updatedDataArray;

            OnClientDisconnectedCallback?.Invoke(clientNetworkId, playerIdDataArray.Value.GetPlayerGameId(clientNetworkId), PlayerCount);
        }

        /// <summary>
        /// When a client leaves the lobby, called only on disconnecting client
        /// </summary>
        private void OnClientDisconnected_OnClient(ulong clientNetworkId)
        {
            // Call function only on client who disconnected
            if (clientNetworkId != NetworkManager.LocalClientId) return;

            Destroy(gameObject);

            // If the disconnecting client is the server (host), unsubscribe subscribed NetworkManager Callbacks
            if (IsServer)
            {
                NetworkManager.OnClientConnectedCallback -= OnClientConnected_OnServer;
                NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected_OnServer;
            }

            NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected_OnClient;

            // When kicked from the server, load this scene
            SceneManager.LoadScene(mainMenuSceneName);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        #endregion


        #region Kick Client and kill Server Code

        [ServerRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
        public void DisconnectClient_ServerRPC(int clientGameId)
        {
            ulong clientNetworkId = GetClientNetworkId(clientGameId);

            GetKicked_ClientRPC(GameIdRPCTargets.SendToTargetClient(clientGameId));

            NetworkManager.DisconnectClient(clientNetworkId);
        }

        [ServerRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
        public void DisconnectAllClients_ServerRPC()
        {
            GetKicked_ClientRPC(GameIdRPCTargets.SendToAll());
        }

        [ClientRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
        private void GetKicked_ClientRPC(GameIdRPCTargets rpcTargets)
        {
            if (rpcTargets.IsTarget == false) return;

            OnKicked?.Invoke();

            // Destroy the rejoin reference on the kicked client
            bool deletionSucces = FileManager.TryDeleteFile("RejoinData.json");

            DebugLogger.Log("RejoinData.json deleted: " + deletionSucces, logDebugInfo);

            SceneManager.LoadScene(mainMenuSceneName);

            if (MessageHandler.Instance != null)
            {
                MessageHandler.Instance.SendTextLocal("You have been kicked from the server!");
            }
        }

        #endregion


        public override void OnDestroy()
        {
            base.OnDestroy();

            if (IsServer)
            {

                // Kick all clients, terminate lobby and shutdown network.
                DisconnectAllClients_ServerRPC();

                LobbyManager.DeleteLobbyInstant_OnServer();

                NetworkManager.Shutdown();
            }

            playerIdDataArray.OnValueChanged = null;
            OnClientConnectedCallback = null;
            OnClientDisconnectedCallback = null;
            OnInitialized = null;
            OnKicked = null;
        }
    }
}