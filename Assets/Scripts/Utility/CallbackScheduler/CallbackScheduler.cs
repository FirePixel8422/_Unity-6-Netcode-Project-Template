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
    /// Utility class to have optimized easy access to varying callbacks by using an Action based callback system.
    /// Handles callbacks and batches them for every script by an event based register system.
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

        private static List<DelayedCallback> _delayedCallbacks = new();
        private static Stack<DelayedCallback> _callbackPool = new();

        private static bool _networkTickActive;
        private static bool _quitting;

        private const string POOLSIZE_KEY = "InvokePoolSize";
        private static int _peakCallbackCount;


        #region Initalize

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            Reset();

            SaveData.Load();

            bool success = SaveData.TryGet(POOLSIZE_KEY, out int poolSize);
            InitPool(success ? poolSize : 0);

            CallbackRunnerInstance gameManager = new GameObject(">>CallbackScheduler<<").AddComponent<CallbackRunnerInstance>();
            gameManager.Init();

            GameObject.DontDestroyOnLoad(gameManager.gameObject);
        }

        private static void InitPool(int initialPoolSize)
        {
            _delayedCallbacks = new List<DelayedCallback>(initialPoolSize);
            _callbackPool = new Stack<DelayedCallback>(initialPoolSize);

            for (int i = 0; i < initialPoolSize; i++)
            {
                _callbackPool.Push(new DelayedCallback());
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

            for (int i = 0; i < _delayedCallbacks.Count; i++)
            {
                DelayedCallback callback = _delayedCallbacks[i];

                callback.Id = -1;
                callback.Reset();

                _callbackPool.Push(callback);
            }

            _delayedCallbacks.Clear();

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


        #region Delayed Invoke Callbacks

        /// <summary>
        /// Schedules a callback and returns its cancellation token.
        /// </summary>
        public static DelayedCallback Invoke(Action callback, float delay, int groupId = 0)
        {
            DelayedCallback delayedCallback = _callbackPool.Count > 0 ? _callbackPool.Pop() : new DelayedCallback();

            delayedCallback.Init(callback, Time.time + delay, groupId, _delayedCallbacks.Count);
            _delayedCallbacks.Add(delayedCallback);

            if (_delayedCallbacks.Count > _peakCallbackCount)
            {
                _peakCallbackCount = _delayedCallbacks.Count;
            }

            return delayedCallback;
        }

        /// <summary>
        /// Schedules a callback without creating a cancellation token.
        /// </summary>
        public static void InvokeAndForget(Action callback, float delay, int groupId = 0)
        {
            DelayedCallback delayedCallback = _callbackPool.Count > 0 ? _callbackPool.Pop() : new DelayedCallback();

            delayedCallback.Init(callback, Time.time + delay, groupId, _delayedCallbacks.Count);
            _delayedCallbacks.Add(delayedCallback);

            if (_delayedCallbacks.Count > _peakCallbackCount)
            {
                _peakCallbackCount = _delayedCallbacks.Count;
            }
        }

        /// <summary>
        /// Cancels all invokes with the same group id.
        /// </summary>
        public static void CancelAllInvokesInGroup(int groupId)
        {
            for (int i = _delayedCallbacks.Count - 1; i >= 0; i--)
            {
                if (_delayedCallbacks[i].GroupId == groupId)
                {
                    RemoveDelayedCallback(_delayedCallbacks[i]);
                }
            }
        }

        /// <summary>
        /// Removes a delayed callback using its current list index.
        /// Swap-back keeps removal O(1).
        /// </summary>
        internal static void RemoveDelayedCallback(DelayedCallback callback)
        {
            if (!callback.IsScheduled) return;

            int id = callback.Id;
            int lastId = _delayedCallbacks.Count - 1;

            callback.Id = -1;

            if (id != lastId)
            {
                DelayedCallback movedCallback = _delayedCallbacks[lastId];
                movedCallback.Id = id;
                _delayedCallbacks[id] = movedCallback;
            }

            _delayedCallbacks.RemoveAt(lastId);

            callback.Reset();
            _callbackPool.Push(callback);
        }

        #endregion


        #region Delayed Invoke Extension Methods

        /// <summary>
        /// Invoke function <paramref name="callback"/> after <paramref name="delay"/> seconds. Schedules a Delayed Callback on <see cref="CallbackScheduler"/> with groupId: <see cref="MonoBehaviour".EntityId/>
        /// </summary>
        /// <returns>The scheduled callback ref</returns>
        public static DelayedCallback Invoke(this MonoBehaviour mb, Action callback, float delay)
        {
            return Invoke(callback, delay, mb.GetInstanceID());
        }

        /// <summary>
        /// Invoke function <paramref name="callback"/> after <paramref name="delay"/> seconds. Schedules a Delayed Callback on <see cref="CallbackScheduler"/> with groupId: <see cref="MonoBehaviour"/>.EntityId.
        /// No callback reference is created. The invoke is only cancelable with the groupId (<see cref="MonoBehaviour"/>.EntityId).
        /// </summary>
        public static void InvokeAndForget(this MonoBehaviour mb, Action callback, float delay)
        {
            InvokeAndForget(callback, delay, mb.GetInstanceID());
        }

        /// <summary>
        /// Stops all scheduled Delayed Callbacks on <see cref="CallbackScheduler"/> with groupId: <see cref="MonoBehaviour"/>.EntityId.
        /// No callback reference is created. The invokes are only cancelable with the groupId and their references are cleared.
        /// </summary>
        public static void CancelAllInvokes(this MonoBehaviour mb)
        {
            CancelAllInvokesInGroup(mb.GetInstanceID());
        }

        #endregion


        /// <summary>
        /// Callback runner instance to invoke the registered callbacks.
        /// </summary>
        private class CallbackRunnerInstance : MonoBehaviour
        {
            public void Init()
            {
                StartCoroutine(UpdateLoop());
            }

            private IEnumerator UpdateLoop()
            {
                float fixedAccumulator = 0f;
                float fixedDelta = Time.fixedDeltaTime;

                while (true)
                {
                    _onUpdateEvent?.Invoke();

                    float time = Time.time;

                    for (int i = _delayedCallbacks.Count - 1; i >= 0; i--)
                    {
                        DelayedCallback callback = _delayedCallbacks[i];

                        if (time < callback.InvokeGlobalTime) continue;

                        callback.Callback?.Invoke();

                        // The callback may have cancelled itself or been cancelled by another callback.
                        if (callback.IsScheduled)
                        {
                            RemoveDelayedCallback(callback);
                        }
                    }

                    fixedAccumulator += Time.deltaTime;

                    while (fixedAccumulator >= fixedDelta)
                    {
                        _onFixedUpdateEvent?.Invoke();
                        fixedAccumulator -= fixedDelta;
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

            private void OnApplicationQuit()
            {
                SaveData.Set(POOLSIZE_KEY, _peakCallbackCount);
                SaveData.Save();

                _quitting = true;
                _onApplicationQuitEvent?.Invoke();
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
#pragma warning restore UDR0002
#pragma warning restore UDR0004


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

    public sealed class DelayedCallback
    {
        public Action Callback;
        public float InvokeGlobalTime;
        public int GroupId;

        internal int Id;
        internal bool IsScheduled => Id != -1;

        internal DelayedCallback()
        {
            Id = -1;
        }

        internal void Init(Action callback, float invokeGlobalTime, int groupId, int id)
        {
            Callback = callback;
            InvokeGlobalTime = invokeGlobalTime;
            GroupId = groupId;
            Id = id;
        }

        internal void Reset()
        {
            Callback = null;
            InvokeGlobalTime = -1f;
            GroupId = -1;
        }

        public void Cancel()
        {
            CallbackScheduler.RemoveDelayedCallback(this);
        }
    }
}