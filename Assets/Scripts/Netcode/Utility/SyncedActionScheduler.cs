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

        [Rpc(SendTo.ClientsAndHost)]
        public void UnRegisterSyncedActionRpc(int actionId)
        {
            int lastIndex = syncedActions.Count - 1;
            if (actionId < lastIndex)
            {
                syncedActions[actionId] = syncedActions[lastIndex];
                syncedActions[actionId].Id = actionId;
            }
            syncedActions.RemoveAt(lastIndex);
        }


        [Rpc(SendTo.Server)]
        public void ScheduleSyncedActionRpc(int actionId, double serverTimeOnSent, float delaySeconds)
        {
            double serverTime = NetworkManager.Singleton.ServerTime.Time;
            double lagTime = serverTime - serverTimeOnSent;
            double timeStamp = serverTime + delaySeconds - lagTime;

            ScheduleSyncedActionRpc(actionId, timeStamp);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void ScheduleSyncedActionRpc(int actionId, double timeStamp)
        {
            syncedActions[actionId].ScheduleLocal(timeStamp);
        }
    }
}