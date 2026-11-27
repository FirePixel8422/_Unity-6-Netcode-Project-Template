using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Fire_Pixel.Utility
{
#pragma warning disable UDR0002
#pragma warning disable UDR0004

    /// <summary>
    /// Utility class to have optimized easy access to varying callbacks by using a pooled callback system.
    /// Handles callbacks and batches them for every script by an event based register system.
    /// Does not guarantee order!!
    /// </summary>
    public static class CallbackScheduler
    {
        private static event Action _onUpdateEvent;
        private static event Action _onLateUpdateEvent;
        private static event Action _onFixedUpdateEvent;
        private static event Action _onNetworkTickEvent;

        private static event Action _onLateDestroyEvent;
        private static event Action _onApplicationQuitEvent;
        private static event Action _onLateApplicationQuitEvent;

        private static List<ScheduledCallback> _scheduledCallbacks = new();
        private static Stack<ScheduledCallback> _callbackPool = new();

        private static bool _networkTickActive;
        private static bool _quitting;

        private const string POOL_SIZE_KEY = "PoolConfig";
        private static int _peakCallbackCount;


        #region Initialization

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            Reset();

            bool success = SaveData.TryGet(POOL_SIZE_KEY, out int poolSize);
            InitPool(success ? poolSize : 0);

            CallbackRunnerInstance runnerInstance = new GameObject(">>CallbackScheduler<<").AddComponent<CallbackRunnerInstance>();
            runnerInstance.Init();

            GameObject.DontDestroyOnLoad(runnerInstance.gameObject);
        }

        private static void InitPool(int initialPoolSize)
        {
            _scheduledCallbacks = new List<ScheduledCallback>(initialPoolSize);
            _callbackPool = new Stack<ScheduledCallback>(initialPoolSize);

            for (int i = 0; i < initialPoolSize; i++)
            {
                _callbackPool.Push(new ScheduledCallback());
            }

            _peakCallbackCount = initialPoolSize;
        }

        public static void Reset()
        {
            _onUpdateEvent = null;
            _onLateUpdateEvent = null;
            _onFixedUpdateEvent = null;
            _onNetworkTickEvent = null;

            _onLateDestroyEvent = null;
            _onApplicationQuitEvent = null;
            _onLateApplicationQuitEvent = null;

            for (int i = 0; i < _scheduledCallbacks.Count; i++)
            {
                ScheduledCallback callback = _scheduledCallbacks[i];

                callback.Id = -1;
                callback.Reset();

                _callbackPool.Push(callback);
            }

            _scheduledCallbacks.Clear();
            _networkTickActive = false;
            _quitting = false;
        }

        #endregion


        #region Network Tick

        /// <summary>
        /// Enables the NGO NetworkTick callback.
        /// </summary>
        public static void EnableNetworkTickEvent()
        {
            if (_networkTickActive || NetworkManager.Singleton == null) return;

            NetworkManager.Singleton.NetworkTickSystem.Tick += NetworkTick;
            _networkTickActive = true;
        }

        /// <summary>
        /// Disables the NGO NetworkTick callback.
        /// </summary>
        public static void DisableNetworkTickEvent()
        {
            if (!_networkTickActive || NetworkManager.Singleton == null) return;

            NetworkManager.Singleton.NetworkTickSystem.Tick -= NetworkTick;
            _networkTickActive = false;
        }

        private static void NetworkTick()
        {
            _onNetworkTickEvent?.Invoke();
        }

        #endregion


        #region Register/UnRegister/Manage Callbacks

        /// <summary>
        /// Register a method to call every frame like Update().
        /// </summary>
        public static void RegisterCallback(Action action, CallbackType type)
        {
            switch (type)
            {
                case CallbackType.Update:
                    _onUpdateEvent += action;
                    return;

                case CallbackType.LateUpdate:
                    _onLateUpdateEvent += action;
                    return;

                case CallbackType.FixedUpdate:
                    _onFixedUpdateEvent += action;
                    return;

                case CallbackType.NetworkTick:
                    _onNetworkTickEvent += action;
                    return;

                case CallbackType.LateDestroy:
                    _onLateDestroyEvent += action;
                    return;

                case CallbackType.ApplicationQuit:
                    _onApplicationQuitEvent += action;
                    return;

                case CallbackType.LateApplicationQuit:
                    _onLateApplicationQuitEvent += action;
                    return;

                default:
                    DebugLogger.LogError($"CallbackType {type} doesnt exist. Register failed.");
                    return;
            }
        }

        /// <summary>
        /// Unregister a registered method for callback <paramref name="type"/>.
        /// </summary>
        public static void UnRegisterCallback(Action action, CallbackType type)
        {
            switch (type)
            {
                case CallbackType.Update:
                    _onUpdateEvent -= action;
                    return;

                case CallbackType.LateUpdate:
                    _onLateUpdateEvent -= action;
                    return;

                case CallbackType.FixedUpdate:
                    _onFixedUpdateEvent -= action;
                    return;

                case CallbackType.NetworkTick:
                    _onNetworkTickEvent -= action;
                    return;

                case CallbackType.LateDestroy:
                    _onLateDestroyEvent -= action;
                    return;

                case CallbackType.ApplicationQuit:
                    _onApplicationQuitEvent -= action;
                    return;

                case CallbackType.LateApplicationQuit:
                    _onLateApplicationQuitEvent -= action;
                    return;

                default:
                    DebugLogger.LogError($"CallbackType {type} doesnt exist. Unregister failed.");
                    return;
            }
        }

        /// <summary>
        /// Register or unregister a method for callback <paramref name="type"/> based on <paramref name="doRegister"/>.
        /// </summary>
        public static void ManageCallback(Action action, CallbackType type, bool doRegister)
        {
            if (doRegister)
            {
                RegisterCallback(action, type);
            }
            else
            {
                UnRegisterCallback(action, type);
            }
        }

        #endregion


        #region Scheduled Callback System

        /// <summary>
        /// Schedules a callback stored with <paramref name="groupId"/>.
        /// </summary>
        /// <returns>The created and scheduled <see cref="ScheduledCallback"/></returns>
        public static ScheduledCallback Schedule(Action callback, float delay, int groupId = 0)
        {
            ScheduledCallback scheduledCallback = _callbackPool.Count > 0 ? _callbackPool.Pop() : new ScheduledCallback();

            scheduledCallback.Init(callback, Time.time + delay, groupId, _scheduledCallbacks.Count);
            _scheduledCallbacks.Add(scheduledCallback);

            if (_scheduledCallbacks.Count > _peakCallbackCount)
            {
                _peakCallbackCount = _scheduledCallbacks.Count;
            }

            return scheduledCallback;
        }

        /// <summary>
        /// Cancels all invokes with the same group id.
        /// </summary>
        public static void CancelAllSchedulesInGroup(int groupId)
        {
            for (int i = _scheduledCallbacks.Count - 1; i >= 0; i--)
            {
                if (_scheduledCallbacks[i].GroupId == groupId)
                {
                    RemoveScheduledCallback(_scheduledCallbacks[i]);
                }
            }
        }

        /// <summary>
        /// Remove (cancel) a delayed callback using its current list index.
        /// Swap-back keeps removal O(1).
        /// </summary>
        internal static void RemoveScheduledCallback(ScheduledCallback callback)
        {
            if (!callback.IsScheduled) return;

            int id = callback.Id;
            int lastId = _scheduledCallbacks.Count - 1;

            callback.Id = -1;

            if (id != lastId)
            {
                ScheduledCallback movedCallback = _scheduledCallbacks[lastId];
                movedCallback.Id = id;
                _scheduledCallbacks[id] = movedCallback;
            }

            _scheduledCallbacks.RemoveAt(lastId);

            callback.Reset();
            _callbackPool.Push(callback);
        }

        #endregion


        #region Scheduled Callbacks Extension Methods (MB)

        /// <summary>
        /// Schedule function <paramref name="callback"/> to execute after <paramref name="delay"/> seconds. Schedules a Delayed Callback on <see cref="CallbackScheduler"/> with groupId: <see cref="MonoBehaviour"/>.EntityId.
        /// </summary>
        /// <returns>The created and scheduled <see cref="ScheduledCallback"/></returns>
        public static ScheduledCallback Schedule(this MonoBehaviour mb, Action callback, float delay)
        {
            return Schedule(callback, delay, mb.GetInstanceID());
        }

        /// <summary>
        /// Stop (cancel) all scheduled <see cref="ScheduledCallback"/>'s on <see cref="CallbackScheduler"/> with groupId: <see cref="MonoBehaviour"/>.EntityId.
        /// </summary>
        public static void CancelAllSchedules(this MonoBehaviour mb)
        {
            CancelAllSchedulesInGroup(mb.GetInstanceID());
        }

        #endregion



        /// <summary>
        /// Callback runner instance to invoke the registered callbacks.
        /// </summary>
        private class CallbackRunnerInstance : MonoBehaviour
        {
            public void Init()
            {
#if ENABLE_DEBUG_SYSTEMS
                StartCoroutine(UpdateLoopDebug());
#else
                StartCoroutine(UpdateLoop());
#endif
            }

            private IEnumerator UpdateLoop()
            {
                float fixedAccumulator = 0f;
                float fixedDelta = Time.fixedDeltaTime;

                while (true)
                {
                    _onUpdateEvent?.Invoke();

                    fixedAccumulator += Time.deltaTime;
                    while (fixedAccumulator >= fixedDelta)
                    {
                        _onFixedUpdateEvent?.Invoke();

                        fixedAccumulator -= fixedDelta;
                    }

                    float time = Time.time;
                    for (int i = _scheduledCallbacks.Count - 1; i >= 0; i--)
                    {
                        ScheduledCallback callback = _scheduledCallbacks[i];

                        if (time < callback.ExecuteAtTime) continue;

                        callback.Callback?.Invoke();

                        // The callback may have cancelled itself or been cancelled by another callback.
                        if (callback.IsScheduled)
                        {
                            RemoveScheduledCallback(callback);
                        }
                    }

                    _onLateUpdateEvent?.Invoke();
                    _onLateDestroyEvent?.Invoke();

                    _onLateDestroyEvent = null;

                    if (_quitting)
                    {
                        _onLateApplicationQuitEvent?.Invoke();

                        _onLateApplicationQuitEvent = null;
                        StopAllCoroutines();
                        yield break;
                    }

                    yield return null;
                }
            }

#if ENABLE_DEBUG_SYSTEMS
            private IEnumerator UpdateLoopDebug()
            {
                float fixedAccumulator = 0f;
                float fixedDelta = Time.fixedDeltaTime;

                while (true)
                {
                    InvokeSafely(_onUpdateEvent);

                    float time = Time.time;

                    for (int i = _scheduledCallbacks.Count - 1; i >= 0; i--)
                    {
                        ScheduledCallback callback = _scheduledCallbacks[i];

                        if (time < callback.ExecuteAtTime) continue;

                        InvokeDelayedSafely(callback.Callback);

                        // The callback may have cancelled itself or been cancelled by another callback.
                        if (callback.IsScheduled)
                        {
                            RemoveScheduledCallback(callback);
                        }
                    }

                    fixedAccumulator += Time.deltaTime;

                    while (fixedAccumulator >= fixedDelta)
                    {
                        InvokeSafely(_onFixedUpdateEvent);

                        fixedAccumulator -= fixedDelta;
                    }

                    InvokeSafely(_onLateUpdateEvent);
                    InvokeSafely(_onLateDestroyEvent);

                    _onLateDestroyEvent = null;

                    if (_quitting)
                    {
                        InvokeSafely(_onLateApplicationQuitEvent);

                        _onLateApplicationQuitEvent = null;
                        StopAllCoroutines();
                        yield break;
                    }

                    yield return null;
                }
            }

            private static void InvokeSafely(Action callback)
            {
                if (callback == null) return;

                Delegate[] subscribers = callback.GetInvocationList();

                for (int i = 0; i < subscribers.Length; i++)
                {
                    try
                    {
                        ((Action)subscribers[i]).Invoke();
                    }
                    catch (Exception exception)
                    {
                        DebugLogger.LogException(exception);
                    }
                }
            }
            private static void InvokeDelayedSafely(Action callback)
            {
                try
                {
                    callback?.Invoke();
                }
                catch (Exception exception)
                {
                    DebugLogger.LogException(exception);
                }
            }
#endif

            private void OnApplicationQuit()
            {
                SaveData.Set(POOL_SIZE_KEY, _peakCallbackCount);
                SaveData.Save();

#if ENABLE_DEBUG_SYSTEMS
                InvokeSafely(_onApplicationQuitEvent);
#else
                _onApplicationQuitEvent?.Invoke();
#endif

                _quitting = true;
            }

            private void OnDestroy()
            {
                DisableNetworkTickEvent();

                _onUpdateEvent = null;
                _onLateUpdateEvent = null;
                _onFixedUpdateEvent = null;
                _onNetworkTickEvent = null;

                _onLateDestroyEvent = null;
                _onLateApplicationQuitEvent = null;
            }
        }
    }

    public enum CallbackType : byte
    {
        Update,
        LateUpdate,
        FixedUpdate,
        NetworkTick,

        LateDestroy,
        ApplicationQuit,
        LateApplicationQuit,
    }

    public sealed class ScheduledCallback
    {
        public Action Callback;
        public float ExecuteAtTime;
        public int GroupId;

        internal int Id;
        internal bool IsScheduled => Id != -1;

        internal ScheduledCallback()
        {
            Id = -1;
        }

        internal void Init(Action callback, float invokeGlobalTime, int groupId, int id)
        {
            Callback = callback;
            ExecuteAtTime = invokeGlobalTime;
            GroupId = groupId;
            Id = id;
        }

        internal void Reset()
        {
            Callback = null;
            ExecuteAtTime = -1f;
            GroupId = -1;
        }

        public void Cancel()
        {
            CallbackScheduler.RemoveScheduledCallback(this);
        }
    }

#pragma warning restore UDR0002
#pragma warning restore UDR0004
}