using Unity.Netcode;
using UnityEngine;


namespace FirePixel.Networking
{
    /// <summary>
    /// Extended version of <see cref="NetworkBehaviour"/> with easy access to local client info and network systems setup callback. (Warning, MUST Call base.OnNetworkSpawn() if overriden)
    /// </summary>
    public abstract class SmartNetworkBehaviour : NetworkBehaviour
    {
        /// <summary>
        /// True 
        /// </summary>
        [HideInInspector] public bool IsNetworkSystemInitilized;


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
            ClientManager.GetOnInitializedCallback(() => 
            {
                IsNetworkSystemInitilized = true;
                OnNetworkSystemsSetup(); 
            });

            NetworkManager.NetworkTickSystem.Tick += OnNetworkTick;
        }

        /// <summary>
        /// Called After all custom build NetworkSystems have been setup through <see cref="ClientManager.OnInitialized"/>
        /// </summary>
        protected virtual void OnNetworkSystemsSetup() { }

        /// <summary>
        /// Called before every network tick (before all scheduled RPCs are executed)
        /// </summary>
        protected virtual void OnNetworkTick() { }


        public override void OnDestroy()
        {
            if (IsSpawned && NetworkManager != null && NetworkManager.NetworkTickSystem != null)
            {
                NetworkManager.NetworkTickSystem.Tick -= OnNetworkTick;
            }
        }
    }
}