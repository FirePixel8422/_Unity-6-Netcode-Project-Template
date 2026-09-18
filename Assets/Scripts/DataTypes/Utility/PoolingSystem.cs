using Fire_Pixel.Utility;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class PoolingSystem<T> where T : Component
{
    private readonly T _type;
    private readonly bool _autoRefill;
    private readonly int _startSize;

    private readonly Queue<T> _pool;

#if UNITY_EDITOR
    private readonly Transform _parent;
    private int _capacity;
#endif


    public PoolingSystem(T type, bool autoRefill, int startSize)
    {
        _type = type;
        _autoRefill = autoRefill;
        _startSize = Mathf.Max(0, startSize);

        _pool = new Queue<T>(_startSize);

#if UNITY_EDITOR
        _parent = new GameObject($"PoolingSystem '{type.name}' (Parent)").transform;
#endif

        for (int i = 0; i < _startSize; i++)
        {
            T obj = Object.Instantiate(_type);
            obj.gameObject.SetActive(false);

            _pool.Enqueue(obj);

#if UNITY_EDITOR
            obj.transform.SetParent(_parent);
            _capacity = _startSize;
#endif
        }
    }


    public T GetPoolObject(Transform parent)
    {
        return GetPoolObject(parent, parent.position, parent.rotation);
    }
    public T GetPoolObject(Transform parent, Vector3 position)
    {
        return GetPoolObject(parent, position, parent.rotation);
    }
    public T GetPoolObject(Transform parent, Vector3 position, Quaternion rotation)
    {
        T obj = GetPoolObject(position, rotation);
        if (obj == null) { return null; }

        obj.transform.SetParent(parent);
        return obj;
    }

    public T GetPoolObject(Vector3 position, Quaternion rotation)
    {
        if (_pool.TryDequeue(out T obj))
        {
            obj.transform.SetPositionAndRotation(position, rotation);
            obj.gameObject.SetActive(true);
            return obj;
        }

        if (!_autoRefill) { return null; }

#if UNITY_EDITOR
        DebugLogger.LogWarning($"{_parent.name} max entry capacity resized to {_capacity}");
#endif

        T newObject = Object.Instantiate(_type, position, rotation);

#if UNITY_EDITOR
        newObject.transform.SetParent(_parent);
        _capacity += 1;
#endif

        return newObject;
    }

    public T GetPoolObject()
    {
        return GetPoolObject(Vector3.zero, Quaternion.identity);
    }


    public void ReturnPooledObject(T obj)
    {
        if (obj == null) { return; }

        _pool.Enqueue(obj);
        obj.gameObject.SetActive(false);

#if UNITY_EDITOR
        obj.transform.SetParent(_parent);
#endif
    }
}
