using UnityEngine;
using Unity.Netcode;
using TMPro;


namespace Fire_Pixel.Networking
{
    public class PlayerNameDisplayer : NetworkBehaviour
    {
        [SerializeField] private TextMeshProUGUI localNameText, oponnentNameText;



        private void Awake()
        {
            MatchManager.PostMatchStarted += OnGameStarted;
        }
        private void OnGameStarted()
        {
            string userName = ClientManager.LocalUserName;

            UpdateUserName(userName, NetworkManager.LocalClientId);
            SendPlayerNameRpc(userName);
        }


        [Rpc(SendTo.Server)]
        private void SendPlayerNameRpc(string userName, RpcParams rpcParams = default)
        {
            ulong clientNetworkId = rpcParams.Receive.SenderClientId;
            SendPlayerNameRpc(userName, clientNetworkId, RpcTarget.Single(clientNetworkId, RpcTargetUse.Temp));
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void SendPlayerNameRpc(string userName, ulong playerNetworkId, RpcParams rpcParams = default)
        {
            UpdateUserName(userName, playerNetworkId);
        }

        private void UpdateUserName(string userName, ulong nameTargetNetworkId)
        {
            if (nameTargetNetworkId == NetworkManager.LocalClientId)
            {
                localNameText.text = userName;
            }
            else
            {
                oponnentNameText.text = userName;
            }
        }
    }
}