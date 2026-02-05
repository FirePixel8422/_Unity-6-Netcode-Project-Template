using Unity.Netcode;
using System;


namespace FirePixel.Networking
{
    /// <summary>
    /// MB manager class that tracks player on turn GameId through <see cref="ClientManager"/> GameId System. Also has callback event for OnTurnChanged and OnTurn -Started and -Ended
    /// </summary>
    public class TurnManager : SmartNetworkBehaviour
    {
        public static TurnManager Instance { get; private set; }


        private int clientOnTurnId;
        public static int ClientOnTurnId => Instance.clientOnTurnId;

        public static bool IsMyTurn => Instance.clientOnTurnId == LocalClientGameId;

#pragma warning disable UDR0001
        public static Action<int> OnTurnChanged;
        public static Action OnTurnStarted;
        public static Action OnTurnEnded;
#pragma warning restore UDR0001


        [ServerRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
        private void StartGame_ServerRPC()
        {

        }


        [ServerRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
        public void EndTurn_ServerRPC()
        {
            clientOnTurnId.IncrementSmart(GlobalGameData.MaxPlayers);

            OnTurnSwapped_ClientRPC(clientOnTurnId);
        }

        [ClientRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
        private void OnTurnSwapped_ClientRPC(int newClientTurnId)
        {
            int prevClientOnTurnId = clientOnTurnId;
            clientOnTurnId = newClientTurnId;

            // If it becomes or stays local clients turn, Invoke OnMyTurnStarted.
            if (IsMyTurn)
            {
                OnTurnStarted?.Invoke();
            }
            // If its not local clients turn, check if they lost the turn and Invoke OnTurnEnded if so.
            else if (prevClientOnTurnId == LocalClientGameId)
            {
                OnTurnEnded?.Invoke();
            }
            // Invoke OnTurnChanged with new clientId.
            OnTurnChanged?.Invoke(clientOnTurnId);
        }
    }
}