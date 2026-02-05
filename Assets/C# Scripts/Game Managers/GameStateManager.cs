using System;
using Unity.Netcode;


namespace FirePixel.Networking
{
    public class GameStateManager : SmartNetworkBehaviour
    {
        public static GameStateManager Instance { get; private set; }
        private void Awake()
        {
            Instance = this;
        }


#pragma warning disable UDR0001
        public static Action OnStartMatch_OnServer;
        private int playerReadyCount;
#pragma warning restore UDR0001


        protected override void OnNetworkSystemsSetup()
        {
            NetworkManager.SceneManager.OnSynchronizeComplete += ClientLoadedNetworkScene_ServerCallback;
        }
        private void ClientLoadedNetworkScene_ServerCallback(ulong clientId)
        {
            playerReadyCount += 1;
            if (playerReadyCount == GlobalGameData.MaxPlayers)
            {
                OnStartMatch_OnServer?.Invoke();
                NetworkManager.Singleton.SceneManager.OnSynchronizeComplete -= ClientLoadedNetworkScene_ServerCallback;

                DebugLogger.Log("Game Ready");
            }
        }
    }
}