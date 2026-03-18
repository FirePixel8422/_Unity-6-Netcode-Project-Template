using System.Collections.Generic;
using Unity.Netcode;



namespace Fire_Pixel.Networking
{
    public class SyncedActionScheduler : NetworkBehaviour
    {
        public static SyncedActionScheduler Instance {get; private set;}
        private void Awake() => Instance = this;


        private List<SyncedAction> syncedActions = new List<SyncedAction>();


        public static void RegisterSyncedActionLocal(SyncedAction action)
        {
            action.Id = Instance.syncedActions.Count;
            Instance.syncedActions.Add(action);
        }

        [ServerRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
        public void UnRegisterSyncedAction_ServerRPC(int actionId)
        {
            UnRegisterSyncedAction_ClientRPC(actionId);
        }
        [ClientRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
        private void UnRegisterSyncedAction_ClientRPC(int actionId)
        {
            int lastIndex = syncedActions.Count - 1;
            if (actionId < lastIndex)
            {
                syncedActions[actionId] = syncedActions[lastIndex];
                syncedActions[actionId].Id = actionId;
            }
            syncedActions.RemoveAt(lastIndex);
        }


        [ServerRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
        public void ScheduleSyncedAction_ServerRPC(int actionId, double serverTimeOnSent, float delaySeconds)
        {
            double serverTime = NetworkManager.Singleton.ServerTime.Time;
            double lagTime = serverTime - serverTimeOnSent;
            double timeStamp = serverTime + delaySeconds - lagTime;

            ScheduleSyncedAction_ClientRPC(actionId, timeStamp);
        }
        [ClientRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
        private void ScheduleSyncedAction_ClientRPC(int actionId, double timeStamp)
        {
            syncedActions[actionId].ScheduleLocal(timeStamp);
        }
    }
}