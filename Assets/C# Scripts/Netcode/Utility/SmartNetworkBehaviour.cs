using Fire_Pixel.Utility;
using Unity.Netcode;


namespace Fire_Pixel.Networking
{
    /// <summary>
    /// Extended version of <see cref="NetworkBehaviour"/> with easy access to local client info and network systems setup callback. (Warning, MUST Call base.OnNetworkSpawn() if overriden)
    /// </summary>
    public abstract class SmartNetworkBehaviour : NetworkBehaviour
    {
        public bool IsNetworkSystemInitilized { get; private set; }
        private bool isPostSpawnReady;


        #region Usefull quick acces to data

        /// <summary>
        /// Pointer to <see cref="NetworkManager.LocalClientId"/>
        /// </summary>
        public static ulong LocalClientNetworkId => NetworkManager.Singleton.LocalClientId;

        /// <summary>
        /// Pointer to <see cref="ClientManager.LocalClientGameId"/>
        /// </summary>
        public static int LocalClientGameId => ClientManager.LocalClientGameId;

        /// <summary>
        /// Pointer to <see cref="ClientManager.LocalUserName"/>
        /// </summary>
        public static string LocalUserName => ClientManager.LocalUserName;

        /// <summary>
        /// Pointer to <see cref="ClientManager.LocalClientGameId"/>
        /// </summary>
        public static string LocalClientGUID => ClientManager.LocalPlayerGUID;

        #endregion


        public override void OnNetworkSpawn()
        {
            ClientManager.PostInitialized += () => 
            {
                IsNetworkSystemInitilized = true;
                OnNetworkSystemsSetup();

                if (isPostSpawnReady)
                {
                    OnNetworkSystemsSetupPostStart();
                }
                isPostSpawnReady = true;
            };

            CallbackScheduler.RegisterNetworkTick(OnNetworkTick);
        }
        private void Start()
        {
            if (isPostSpawnReady)
            {
                OnNetworkSystemsSetupPostStart();
            }
            isPostSpawnReady = true;
        }

        /// <summary>
        /// Called After all custom NetworkSystems have been setup through <see cref="ClientManager.PostInitialized"/>
        /// </summary>
        protected virtual void OnNetworkSystemsSetup() { }

        /// <summary>
        /// Called after OnNetworkSystemsSetup() and Start().
        /// </summary>
        protected virtual void OnNetworkSystemsSetupPostStart() { }

        /// <summary>
        /// Called before every network tick (before all scheduled RPCs are executed)
        /// </summary>
        protected virtual void OnNetworkTick() { }


        public override void OnDestroy()
        {
            if (IsSpawned)
            {
                CallbackScheduler.UnRegisterNetworkTick(OnNetworkTick);
            }
        }
    }
}