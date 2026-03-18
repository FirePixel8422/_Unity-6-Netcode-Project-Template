using System.Collections.Generic;
using Unity.Netcode;


namespace Fire_Pixel.Networking
{

    /// <summary>
    /// Custom Utility class to handle creating ClientRpcParams based on NetworkIds and <see cref="ClientManager"/> GameIds.
    /// </summary>
    public static class RPCTargetFilters
    {
        /// <summary>
        /// Returns true if the host was not designated as target for this RPC (Since the host always recieves any RPC, even when not targetted).
        /// </summary>
        public static bool ShouldHostSkip(ClientRpcParams rpcParams)
        {
            IReadOnlyList<ulong> targets = rpcParams.Send.TargetClientIds;

            if (targets == null) return false;

            int rpcTargetCount = targets.Count;
            for (int i = 0; i < rpcTargetCount; i++)
            {
                if (targets[i] == 0)
                {
                    return false;
                }
            }
            return true;
        }

        public static ClientRpcParams BroadCast => default;
        
        public static ClientRpcParams SendToTargetClient(int clientGameId)
        {
            ulong clientNetworkId = ClientManager.GetClientNetworkId(clientGameId);

            return CreateRpcParams(clientNetworkId);
        }
        public static ClientRpcParams SendToTargetClient(ulong clientNetworkId)
        {
            return CreateRpcParams(clientNetworkId);
        }

        /// <summary>
        /// ONLY functional for 2 player games
        /// </summary>
        public static ClientRpcParams SendToOppositeClient(int clientGameId)
        {
            int opponentGameId = (clientGameId == 0) ? 1 : 0;
            ulong opponentNetworkId = ClientManager.GetClientNetworkId(opponentGameId);

            return CreateRpcParams(opponentNetworkId);
        }
        /// <summary>
        /// ONLY functional for 2 player games
        /// </summary>
        public static ClientRpcParams SendToOppositeClient(ulong clientNetworkId)
        {
            int clientGameId = ClientManager.GetClientGameId(clientNetworkId);
            return SendToOppositeClient(clientGameId);
        }

        public static ClientRpcParams SendToAllButClient(int clientGameId)
        {
            int playerCount = ClientManager.PlayerCount;
            ulong[] clientNetworkIds = new ulong[playerCount - 1];

            int arrayIndex = 0;
            for (int i = 0; i < playerCount; i++)
            {
                if (i == clientGameId) continue;

                clientNetworkIds[arrayIndex++] = ClientManager.GetClientNetworkId(i);
            }

            return CreateRpcParams(clientNetworkIds);
        }
        public static ClientRpcParams SendToAllButClient(ulong clientNetworkId)
        {
            int clientGameId = ClientManager.GetClientGameId(clientNetworkId);
            return SendToAllButClient(clientGameId);
        }

        public static ClientRpcParams SendToAllButHost()
        {
            int playerCount = ClientManager.PlayerCount;
            ulong[] clientNetworkIds = new ulong[playerCount - 1];

            int arrayIndex = 0;
            for (int i = 0; i < playerCount; i++)
            {
                if (i == 0) continue;

                clientNetworkIds[arrayIndex++] = ClientManager.GetClientNetworkId(i);
            }

            return CreateRpcParams(clientNetworkIds);
        }

        private static ClientRpcParams CreateRpcParams(ulong clientNetworkId)
        {
            return new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { clientNetworkId },
                }
            };
        }
        private static ClientRpcParams CreateRpcParams(ulong[] clientNetworkIds)
        {
            return new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = clientNetworkIds,
                }
            };
        }
    }
}