using System;
using System.Linq;
using UnityEngine;

namespace Fire_Pixel.Utility
{
    public static class ScheduleHandler
    {
        private static IQueue[] _queues;
        private static int _queueCount;

        private static ScheduleConfig _scheduleConfig;

        private const string POOL_CONFIGS_KEY = "ScheduleConfig";
        public const bool ENABLE_DEBUG_LOGGING = true;


        private static void Init()
        {
            if (!SaveData.TryGet(POOL_CONFIGS_KEY, out _scheduleConfig))
                _scheduleConfig = new ScheduleConfig { TypeAllocations = new ExposedList<TypeAllocation>() };

            int typeCount = _scheduleConfig.TypeAllocations.Count;
            _queues = new IQueue[typeCount];
            _queueCount = 0;

            for (int i = 0; i < typeCount; i++)
            {
                TypeAllocation allocation = _scheduleConfig.TypeAllocations[i];
                Type type = Type.GetType(allocation.TypeName);

                if (string.IsNullOrEmpty(allocation.TypeName))
                {
                    CreatePool(allocation.Capacity);
                    continue;
                }

                if (type == null) continue;

                typeof(ScheduleHandler)
                    .GetMethod(nameof(CreatePool), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static, null, new[] { typeof(int) }, null)
                    .MakeGenericMethod(type)
                    .Invoke(null, new object[] { allocation.Capacity });
            }
        }

        public static void Update()
        {
            float time = Time.time;
            for (int i = 0; i < _queues.Length; i++)
            {
                _queues[i].Update(time);
            }
        }

        private static ScheduleQueue CreatePool(int capacity)
        {
            ScheduleQueue queue = new ScheduleQueue(capacity);
            _queues[_queueCount++] = queue;
            return queue;
        }
        private static ScheduleQueue<T> CreatePool<T>(int capacity)
        {
            ScheduleQueue<T> queue = new ScheduleQueue<T>(capacity);
            _queues[_queueCount++] = queue;
            return queue;
        }

        private static void ReSizePool(int capacity)
        {
            ScheduleQueue.Instance.Resize(capacity);
        }
        private static void ReSizePool<T>(int capacity)
        {
            ScheduleQueue<T>.Instance.Resize(capacity);
        }

        public static void Schedule(Action callback, float delay, int groupId = 0)
        {
            if (!ScheduleQueue.TryGetInstance(out ScheduleQueue queue))
            {
                queue = CreatePool(1);

                _scheduleConfig.AddTypeConfig(null);
                SaveData.Set(POOL_CONFIGS_KEY, _scheduleConfig);
            }
            queue.Add(callback, Time.time + delay, groupId);
        }
        public static void Schedule<T>(Action<T> callback, T param, float delay, int groupId = 0)
        {
            if (!ScheduleQueue<T>.TryGetInstance(out ScheduleQueue<T> queue))
            {
                queue = CreatePool<T>(1);

                _scheduleConfig.AddTypeConfig(typeof(T));
                SaveData.Set(POOL_CONFIGS_KEY, _scheduleConfig);
            }
            queue.Add(callback, param, Time.time + delay, groupId);
        }


        public interface IQueue
        {
            public void Update(float time);
        }
        public class ScheduleQueue : IQueue
        {
            public static ScheduleQueue Instance { get; private set; }
            public static bool TryGetInstance(out ScheduleQueue queue)
            {
                queue = Instance;
                return Instance != null;
            }

            private CallbackEntry[] _entryList;
            private int _entryCount;


            public ScheduleQueue(int capacity)
            {
                Instance = this;
                _entryList = new CallbackEntry[capacity];
            }
            public void Resize(int newCapacity)
            {
                Array.Resize(ref _entryList, newCapacity);

                _scheduleConfig.UpdateTypeConfig(null, newCapacity);
                SaveData.Set(POOL_CONFIGS_KEY, _scheduleConfig);

                DebugLogger.LogWarning($"Resized SchedulePool for: 'void (parameterless)' to '{newCapacity}'", ENABLE_DEBUG_LOGGING);
            }

            public void Add(Action callback, float invokeTime, int groupId)
            {
                if (_entryCount == _entryList.Length)
                {
                    Resize(_entryCount * 2);
                }
                _entryList[_entryCount++].Init(callback, invokeTime, groupId);
            }
            public void Update(float time)
            {
                for (int i = _entryCount; i >= 0; i--)
                {
                    if (!_entryList[i].TryInvoke(time)) continue;

                    _entryList[i] = _entryList[_entryCount--];
                }
            }
        }
        public class ScheduleQueue<T> : IQueue
        {
            public static ScheduleQueue<T> Instance { get; private set; }
            public static bool TryGetInstance(out ScheduleQueue<T> queue)
            {
                queue = Instance;
                return Instance != null;
            }

            private CallbackEntry<T>[] _entryList;
            private int _entryCount;


            public ScheduleQueue(int capacity)
            {
                Instance = this;
                _entryList = new CallbackEntry<T>[capacity];
            }
            public void Resize(int newCapacity)
            {
                Array.Resize(ref _entryList, newCapacity);

                _scheduleConfig.UpdateTypeConfig(typeof(T), newCapacity);
                SaveData.Set(POOL_CONFIGS_KEY, _scheduleConfig);

                DebugLogger.LogWarning($"Resized SchedulePool for: '{nameof(T)}' to '{newCapacity}'", ENABLE_DEBUG_LOGGING);
            }

            public void Add(Action<T> callback, T param, float invokeTime, int groupId)
            {
                if (_entryCount == _entryList.Length)
                {
                    Resize(_entryCount * 2);
                }
                _entryList[_entryCount++].Init(callback, param, invokeTime, groupId);
            }
            public void Update(float time)
            {
                for (int i = _entryCount; i >= 0; i--)
                {
                    if (!_entryList[i].TryInvoke(time)) continue;

                    _entryList[i] = _entryList[_entryCount--];
                }
            }
        }

        //Maybe move Resize logic
        //Maybe move Resize logic
        //Maybe move Resize logic
        //Maybe move Resize logic
        //Maybe move Resize logic
        //Maybe move Resize logic
        //Maybe move Resize logic

        public struct CallbackEntry
        {
            public Action Callback { get; private set; }
            public float InvokeTime { get; private set; }
            public int GroupId { get; private set; }


            public void Init(Action callback, float invokeTime, int groupId)
            {
                Callback = callback;
                InvokeTime = invokeTime;
                GroupId = groupId;
            }
            public readonly bool TryInvoke(float time)
            {
                if (time < InvokeTime) return false;

                Callback.Invoke();
                return true;
            }
        }
        public struct CallbackEntry<T>
        {
            public Action<T> Callback { get; private set; }
            public T Param { get; private set; }
            public float InvokeTime { get; private set; }
            public int GroupId { get; private set; }


            public void Init(Action<T> callback, T param, float invokeTime, int groupId)
            {
                Callback = callback;
                Param = param;
                InvokeTime = invokeTime;
                GroupId = groupId;
            }
            public readonly bool TryInvoke(float time)
            {
                if (time < InvokeTime) return false;

                Callback.Invoke(Param);
                return true;
            }
        }

        [System.Serializable]
        public struct TypeAllocation
        {
            public string TypeName;
            public int Capacity;

            public TypeAllocation(Type type, int capacity)
            {
                TypeName = type.AssemblyQualifiedName;
                Capacity = capacity;
            }
        }
        [System.Serializable]
        public struct ScheduleConfig
        {
            public ExposedList<TypeAllocation> TypeAllocations;


            public readonly void AddTypeConfig(Type type)
            {
                TypeAllocations.Add(new TypeAllocation(type, 1));
            }
            public readonly void UpdateTypeConfig(Type type, int capacity)
            {
                string typeName = type?.AssemblyQualifiedName;

                for (int i = 0; i < TypeAllocations.Count; i++)
                {
                    if (TypeAllocations[i].TypeName != typeName) continue;

                    TypeAllocations[i] = new TypeAllocation(type, capacity);
                    return;
                }
            }
        }
    }
}