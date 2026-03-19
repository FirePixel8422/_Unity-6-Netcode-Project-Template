using Fire_Pixel.Utility;
using UnityEngine;
using Unity.Netcode;


namespace Fire_Pixel.Networking
{
    public class MatchManager : SmartNetworkBehaviour
    {
#pragma warning disable UDR0001
        public static OneTimeAction PostMatchStarted_OnServer = new OneTimeAction();
        public static OneTimeAction PostMatchStarted = new OneTimeAction();
#pragma warning restore UDR0001

        private int playerReadyCount;


        protected override void OnNetworkSystemsSetupPostStart()
        {
            TurnManager.TurnChanged += OnTurnChanged;
            MarkPlayerReady_ServerRPC();
        }

        [ContextMenu("Ready")]
        [ServerRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
        public void MarkPlayerReady_ServerRPC()
        {
            playerReadyCount += 1;
            if (playerReadyCount == GlobalGameData.MAX_PLAYERS)
            {
                PostMatchStarted_OnServer?.Invoke();
            }
        }
        private void OnTurnChanged(int clientGameId)
        {
            TurnManager.TurnChanged -= OnTurnChanged;
            PostMatchStarted.Invoke();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            TurnManager.TurnChanged -= OnTurnChanged;
            PostMatchStarted_OnServer = new OneTimeAction();
            PostMatchStarted = new OneTimeAction();
        }
    }
}