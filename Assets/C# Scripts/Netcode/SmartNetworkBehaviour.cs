using Unity.Netcode;
using UnityEngine;



namespace FirePixel.Networking
{
    /// <summary>
    /// Extended version of <see cref="NetworkBehaviour"/> with easy access to local client info and network systems setup callback. (Warning, MUST Call base.OnNetworkSpawn() if overriden)
    /// </summary>
    public class SmartNetworkBehaviour : NetworkBehaviour
    {
        public bool isNetworkSystemInitilized;

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




        public override void OnNetworkSpawn()
        {
            ClientManager.GetOnInitializedCallback(() => 
            {
                isNetworkSystemInitilized = true;
                OnNetworkSystemsSetup(); 
            });
        }

        /// <summary>
        /// Called After all custom build NetworkSystems have been setup through <see cref="ClientManager.OnInitialized"/>
        /// </summary>
        public virtual void OnNetworkSystemsSetup() { }
    }
}