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

            UpdateUserName_Local(userName, NetworkManager.LocalClientId);
            SendPlayerName_ServerRPC(userName);
        }


        [ServerRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
        private void SendPlayerName_ServerRPC(string userName, ServerRpcParams rpcParams = default)
        {
            ulong clientNetworkId = rpcParams.Receive.SenderClientId;
            SendPlayerName_ClientRPC(userName, clientNetworkId, RPCTargetFilters.SendToOppositeClient(clientNetworkId));
        }

        [ClientRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
        private void SendPlayerName_ClientRPC(string userName, ulong playerNetworkId, ClientRpcParams rpcParams = default)
        {
            if (IsHost && RPCTargetFilters.ShouldHostSkip(rpcParams)) return;

            UpdateUserName_Local(userName, playerNetworkId);
        }

        private void UpdateUserName_Local(string userName, ulong nameTargetNetworkId)
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