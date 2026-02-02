using FirePixel.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;


public static class ExtensionMethods
{
    #region Invoke

    /// <summary>
    /// Call function after a delay
    /// </summary>
    public static void Invoke(this MonoBehaviour mb, float delay, Action f)
    {
        mb.StartCoroutine(InvokeRoutine(delay, f));
    }

    public static void Invoke<T>(this MonoBehaviour mb, float delay, Action<T> f, T param)
    {
        mb.StartCoroutine(InvokeRoutine(delay, f, param));
    }

    private static IEnumerator InvokeRoutine(float delay, Action f)
    {
        yield return new WaitForSeconds(delay);
        f.Invoke();
    }

    private static IEnumerator InvokeRoutine<T>(float delay, Action<T> f, T param)
    {
        yield return new WaitForSeconds(delay);
        f.Invoke(param);
    }

    #endregion


    #region Transform Logic

    public static void SetParent(this Transform trans, Transform parent, bool keepLocalPos, bool keepLocalRot)
    {
        if (parent == null)
        {
#if UNITY_EDITOR
            DebugLogger.LogWarning("You are trying to set a transform to a parent that doesnt exist, this is not allowed");
#endif
            return;
        }

        trans.SetParent(parent);
        if (!keepLocalPos)
        {
            trans.localPosition = Vector3.zero;
        }
        if (!keepLocalRot)
        {
            trans.localRotation = Quaternion.identity;
        }
    }
    public static void SetParent(this Transform trans, Transform parent, bool keepLocalPos, bool keepLocalRot, bool keepLocalScale)
    {
        if (parent == null)
        {
#if UNITY_EDITOR
            DebugLogger.LogWarning("You are trying to set a transform to a parent that doesnt exist, this is not allowed");
#endif
            return;
        }

        trans.SetParent(parent);
        if (!keepLocalPos)
        {
            trans.localPosition = Vector3.zero;
        }
        if (!keepLocalRot)
        {
            trans.localRotation = Quaternion.identity;
        }
        if (!keepLocalScale)
        {
            trans.localScale = Vector3.one;
        }
    }

    #endregion


    #region (Try)GetComponent(s)

    public static bool TryGetComponents<T>(this Transform trans, out T[] components) where T : UnityEngine.Object
    {
        components = trans.GetComponents<T>();

        return components.Length > 0;
    }

    public static bool TryGetComponentInChildren<T>(this Transform trans, out T component, bool includeInactive = false) where T : UnityEngine.Object
    {
        component = trans.GetComponentInChildren<T>(includeInactive);
        return component != null;
    }
    public static bool TryGetComponentsInChildren<T>(this Transform trans, out T[] components, bool includeInactive) where T : UnityEngine.Object
    {
        components = trans.GetComponentsInChildren<T>(includeInactive);

        return components.Length > 0;
    }

    public static bool TryGetComponentInParent<T>(this Transform trans, out T component) where T : UnityEngine.Object
    {
        component = trans.GetComponentInParent<T>();
        return component != null;
    }
    public static bool TryGetComponentsInParent<T>(this Transform trans, out T[] component) where T : UnityEngine.Object
    {
        component = trans.GetComponentsInParent<T>();
        return component != null;
    }

    public static bool TryGetComponentInParentRecursively<T>(this Transform trans, bool checkStartTransform, out T component) where T : UnityEngine.Object
    {
        Transform current = checkStartTransform ? trans : trans.parent;

        while (current != null)
        {
            if (current.TryGetComponent(out component))
            {
                return true;
            }

            current = current.parent;
        }

        component = null;
        return false;
    }

    #endregion


    #region (Try)FindObjectOfType

    public static bool TryFindObjectOfType<T>(this UnityEngine.Object obj, out T component, bool includeInactive = false) where T : UnityEngine.Object
    {
        FindObjectsInactive findObjectsInactive = includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude;

        component = UnityEngine.Object.FindFirstObjectByType<T>(findObjectsInactive);
        return component != null;
    }
    public static bool TryFindObjectsOfType<T>(this UnityEngine.Object obj, out T[] component, bool includeInactive = false, bool sortByInstanceID = false) where T : UnityEngine.Object
    {
        FindObjectsInactive findObjectsInactive = includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude;
        FindObjectsSortMode sortMode = sortByInstanceID ? FindObjectsSortMode.InstanceID : FindObjectsSortMode.None;

        component = UnityEngine.Object.FindObjectsByType<T>(findObjectsInactive, sortMode);
        return component != null;
    }

    // Unity 6s new FindobjectOfType is stupid
    public static T FindObjectOfType<T>(this UnityEngine.Object obj) where T : UnityEngine.Object
    {
        return UnityEngine.Object.FindFirstObjectByType<T>();
    }

    public static T[] FindObjectsOfType<T>(this UnityEngine.Object obj, bool includeInactive = false, bool sortByInstanceID = false) where T : UnityEngine.Object
    {
        FindObjectsInactive findObjectsInactive = includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude;
        FindObjectsSortMode sortMode = sortByInstanceID ? FindObjectsSortMode.InstanceID : FindObjectsSortMode.None;

        return UnityEngine.Object.FindObjectsByType<T>(findObjectsInactive, sortMode);
    }

    #endregion


    #region HasComponent

    public static bool HasComponent<T>(this Transform trans) where T : Component
    {
        return trans.GetComponent<T>() != null;
    }

    public static bool HasComponentInChildren<T>(this Transform trans, bool includeInactive = false) where T : Component
    {
        return trans.GetComponentInChildren<T>(includeInactive) != null;
    }

    public static bool HasComponentInParent<T>(this Transform trans, bool includeInactive = false) where T : Component
    {
        return trans.GetComponentInParent<T>(includeInactive) != null;
    }

    #endregion


    #region PlayClip with Pitch and Clip overloads for AudioSource

    /// <summary>
    /// Lets AudioSource play selected clip with selected pitch
    /// </summary>
    public static void PlayClipWithPitch(this AudioSource source, AudioClip clip, float pitch)
    {
        source.clip = clip;
        source.pitch = pitch;
        source.Play();
    }
    /// <summary>
    /// Lets AudioSource play with selected pitch
    /// </summary>
    public static void PlayWithPitch(this AudioSource source, float pitch)
    {
        source.pitch = pitch;
        source.Play();
    }
    /// <summary>
    /// Lets AudioSource play with selected pitch
    /// </summary>
    public static void PlayOneShotWithPitch(this AudioSource source, float pitch)
    {
        source.pitch = pitch;
        source.PlayOneShot(source.clip);
    }
    /// <summary>
    /// Lets AudioSource play selected clip with selected pitch
    /// </summary>
    public static void PlayOneShotClipWithPitch(this AudioSource source, AudioClip clip, float pitch)
    {
        source.pitch = pitch;
        source.PlayOneShot(clip);
    }

    #endregion


    #region SetActiveSmart for GameObjects and Components

    /// <summary>
    /// Set the active state of a Behaviour component only if the state is different from the current state.
    /// </summary>
    public static void SetActiveStateSmart(this Behaviour comp, bool state)
    {
        if (comp.enabled != state)
        {
            comp.enabled = state;
        }
    }

    /// <summary>
    /// Set the active state of a GameObject only if the state is different from the current state.
    /// </summary>
    public static void SetActiveSmart(this GameObject obj, bool state)
    {
        if (obj.activeInHierarchy != state)
        {
            obj.SetActive(state);
        }
    }

    #endregion


    #region DisposeIfCreated for Native Collections

    /// <summary>
    /// Check if the NativeArray is created, and if so, dispose of it
    /// </summary>
    public static void DisposeIfCreated<T>(this NativeArray<T> array) where T : unmanaged
    {
        if (array.IsCreated)
            array.Dispose();
    }
    /// <summary>
    /// Check if the NativeArray is created, and if so, dispose of it
    /// </summary>
    public static void DisposeIfCreated<T>(this NativeList<T> array) where T : unmanaged
    {
        if (array.IsCreated)
            array.Dispose();
    }
    /// <summary>
    /// Check if the NativeArray is created, and if so, dispose of it
    /// </summary>
    public static void DisposeIfCreated<T>(this NativeReference<T> array) where T : unmanaged
    {
        if (array.IsCreated)
            array.Dispose();
    }
    /// <summary>
    /// Check if the NativeArray is created, and if so, dispose of it
    /// </summary>
    public static void DisposeIfCreated<T>(this NativeHashSet<T> array) where T : unmanaged, IEquatable<T>
    {
        if (array.IsCreated)
            array.Dispose();
    }
    /// <summary>
    /// Check if the NativeArray is created, and if so, dispose of it
    /// </summary>
    public static void DisposeIfCreated<Tkey, TValue>(this NativeHashMap<Tkey, TValue> array) where Tkey : unmanaged, IEquatable<Tkey> where TValue : unmanaged
    {
        if (array.IsCreated)
            array.Dispose();
    }

    #endregion


    #region Array/List/NativeCollection Shuffle

    /// <summary>
    /// Randomly shuffles the content of the array in place using Fisher–Yates.
    /// </summary>
    public static void Shuffle<T>(this T[] targetArray)
    {
        int n = targetArray.Length;
        for (int i = 0; i < n - 1; i++)
        {
            int j = UnityEngine.Random.Range(i, n);
            (targetArray[i], targetArray[j]) = (targetArray[j], targetArray[i]);
        }
    }
    /// <summary>
    /// Randomly shuffles the content of the array in place using Fisher–Yates.
    /// </summary>
    public static void Shuffle<T>(this List<T> targetArray)
    {
        int n = targetArray.Count;
        for (int i = 0; i < n - 1; i++)
        {
            int j = UnityEngine.Random.Range(i, n);
            (targetArray[i], targetArray[j]) = (targetArray[j], targetArray[i]);
        }
    }
    /// <summary>
    /// Randomly shuffles the content of the array in place using Fisher–Yates.
    /// </summary>
    public static void Shuffle<T>(this NativeArray<T> targetArray) where T : unmanaged
    {
        int n = targetArray.Length;
        for (int i = 0; i < n - 1; i++)
        {
            int j = UnityEngine.Random.Range(i, n);
            (targetArray[i], targetArray[j]) = (targetArray[j], targetArray[i]);
        }
    }
    /// <summary>
    /// Randomly shuffles the content of the array in place using Fisher–Yates.
    /// </summary>
    public static void Shuffle<T>(this NativeList<T> targetArray) where T : unmanaged
    {
        int n = targetArray.Length;
        for (int i = 0; i < n - 1; i++)
        {
            int j = UnityEngine.Random.Range(i, n);
            (targetArray[i], targetArray[j]) = (targetArray[j], targetArray[i]);
        }
    }

    #endregion


    #region Index Modify Utility

    /// <summary>
    /// Increments index by 1 and wraps to 0 if it reaches length
    /// </summary>
    public static int IncrementSmart(this ref int value, int length)
    {
        value += 1;
        if (value >= length)
        {
            value = 0;
        }
        return value;
    }
    /// <summary>
    /// Decrements index by 1 and wraps to length if it reaches 0
    /// </summary>
    public static int DecrementSmart(this ref int value, int length)
    {
        value -= 1;
        if (value < 0)
        {
            value = length - 1;
        }
        return value;
    }

    /// <summary>
    /// Updates index by adding <paramref name="toAdd"/> and wraps to 0 if it reaches length or length if it reaches 0
    /// </summary>
    public static int AddSmart(this ref int value, int toAdd, int length)
    {
        DebugLogger.Throw("AddSmart called with length 0", length <= 0);

        value += toAdd;
        while (value >= length)
        {
            value -= length;
        }
        while (value < 0)
        {
            value += length;
        }
        return value;
    }

    #endregion


    /// <summary>
    /// Get PlayerGameId through ClientManager using OwnerClientId.
    /// </summary>
    public static int GetOwnerClientGameId(this NetworkObject networkObj)
    {
        return ClientManager.GetClientGameId(networkObj.OwnerClientId);
    }

    /// <summary>
    /// Try finding an action by name, returns true if found, false if not. Outputs the found action
    /// </summary>
    public static bool TryFindAction(this InputActionAsset actionAsset, string actionName, out InputAction action)
    {
        action = actionAsset.FindAction(actionName);
        return action != null;
    }

    /// <summary>
    /// Randomly shuffles the content of the array in place using Fisher–Yates.
    /// </summary>
    public static T SelectRandom<T>(this T[] targetArray)
    {
        return targetArray[UnityEngine.Random.Range(0, targetArray.Length)];
    }
}