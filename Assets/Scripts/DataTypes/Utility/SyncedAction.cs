using Fire_Pixel.Utility;
using System;
using Unity.Netcode;


namespace Fire_Pixel.Networking
{
    /// <summary>
    /// Container that stores an <see cref="Action"/> which can be subscribed and auto invoked through a globalServerTimeStamp paramater.
    /// Can be used to create an event that executes on all client at the same time, with 0ms lag.
    /// </summary>
    public class SyncedAction
    {
        public int Id = -1;

        private event Action Synchronise;
        private double executeAtServerTime;
        private bool isScheduled;


        public void Create()
        {
            SyncedActionScheduler.RegisterSyncedActionLocal(this);
        }
        public void Destroy_OnServer()
        {
#if Enable_Debug_Systems
            if (!NetworkManager.Singleton.IsServer)
            {
                DebugLogger.LogError("Destroy SyncedAction may ONLY be called on the server");
                return;
            }
#endif
            SyncedActionScheduler.Instance.UnRegisterSyncedAction_ServerRPC(Id);
        }

        public void Schedule_ServerRPC(float delaySeconds)
        {
            SyncedActionScheduler.Instance.ScheduleSyncedAction_ServerRPC(Id, NetworkManager.Singleton.ServerTime.Time, delaySeconds);
        }

        public void ScheduleLocal(double timeStamp)
        {
#if Enable_Debug_Systems
            if (isScheduled)
            {
                DebugLogger.LogError("SyncedAction is already scheduled! You must not call Schedule() twice before it executes.");
                return;
            }
#endif
            executeAtServerTime = timeStamp;
            isScheduled = true;

            CallbackScheduler.RegisterNetworkTick(OnNetworkTick);
        }
        private void OnNetworkTick()
        {
            if (NetworkManager.Singleton.LocalTime.Time >= executeAtServerTime)
            {
                Synchronise?.Invoke();
                isScheduled = false;

                CallbackScheduler.UnRegisterNetworkTick(OnNetworkTick);
            }
        }
        public static SyncedAction operator +(SyncedAction e, Action action)
        {
            e.Synchronise += action;
            return e;
        }
        public static SyncedAction operator -(SyncedAction e, Action action)
        {
            e.Synchronise -= action;
            return e;
        }
    }
}