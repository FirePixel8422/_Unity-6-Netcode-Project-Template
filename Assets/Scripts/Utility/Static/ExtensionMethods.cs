using Fire_Pixel.Networking;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public static class ExtensionMethods
{
    #region Transform Logic

    public static Transform Transform(this Object obj)
    {
        return obj switch
        {
            GameObject go => go.transform,
            Component component => component.transform,
            _ => null
        };
    }

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

    public static bool TryGetComponents<T>(this Object obj, out T[] components) where T : Component
    {
        components = obj.Transform().GetComponents<T>();

        return components.Length > 0;
    }

    public static bool TryGetComponentInChildren<T>(this Object obj, out T component, bool includeInactive = false) where T : Component
    {
        component = obj.Transform().GetComponentInChildren<T>(includeInactive);
        return component != null;
    }
    public static bool TryGetComponentsInChildren<T>(this Object obj, out T[] components, bool includeInactive) where T : Component
    {
        components = obj.Transform().GetComponentsInChildren<T>(includeInactive);

        return components.Length > 0;
    }

    public static bool TryGetComponentInParent<T>(this Object obj, out T component) where T : Component
    {
        component = obj.Transform().GetComponentInParent<T>();
        return component != null;
    }
    public static bool TryGetComponentsInParent<T>(this Object obj, out T[] component) where T : Component
    {
        component = obj.Transform().GetComponentsInParent<T>();
        return component != null;
    }

    public static bool TryGetComponentInParentRecursively<T>(this Object obj, bool checkStartTransform, out T component) where T : Component
    {
        Transform current = checkStartTransform ? obj.Transform() : obj.Transform().parent;

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

    public static bool TryFindObjectOfType<T>(this Object _, out T component, bool includeInactive = false) where T : Object
    {
        FindObjectsInactive findObjectsInactive = includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude;

        component = Object.FindFirstObjectByType<T>(findObjectsInactive);
        return component != null;
    }
    public static bool TryFindObjectsOfType<T>(this Object _, out T[] component, bool includeInactive = false, bool sortByInstanceID = false) where T : Object
    {
        FindObjectsInactive findObjectsInactive = includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude;
        FindObjectsSortMode sortMode = sortByInstanceID ? FindObjectsSortMode.InstanceID : FindObjectsSortMode.None;

        component = Object.FindObjectsByType<T>(findObjectsInactive, sortMode);
        return component != null;
    }

    public static T FindObjectOfType<T>(this Object _) where T : Object
    {
        return Object.FindFirstObjectByType<T>();
    }

    public static T[] FindObjectsOfType<T>(this Object _, bool includeInactive = false, bool sortByInstanceID = false) where T : Object
    {
        FindObjectsInactive findObjectsInactive = includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude;
        FindObjectsSortMode sortMode = sortByInstanceID ? FindObjectsSortMode.InstanceID : FindObjectsSortMode.None;

        return UnityEngine.Object.FindObjectsByType<T>(findObjectsInactive, sortMode);
    }

    #endregion


    #region GetComponents Ordered

    public static T[] GetComponentsOrdered<T>(this Object obj) where T : Component
    {
        T[] components = obj.Transform().GetComponents<T>();

        System.Array.Sort(components, (a, b) =>
        {
            return a.GetInstanceID().CompareTo(b.GetInstanceID());
        });

        return components;
    }

    public static T[] GetComponentsInChildrenOrdered<T>(this Object obj, bool includeInactive = false) where T : Component
    {
        T[] components = obj.Transform().GetComponentsInChildren<T>(includeInactive);

        System.Array.Sort(components, (a, b) =>
        {
            string pathA = GetHierarchyPath(a.transform);
            string pathB = GetHierarchyPath(b.transform);
            return string.CompareOrdinal(pathA, pathB);
        });

        return components;
    }

    public static T[] GetComponentsInParentOrdered<T>(this Object obj, bool includeInactive = false) where T : Component
    {
        T[] components = obj.Transform().GetComponentsInParent<T>(includeInactive);

        System.Array.Sort(components, (a, b) =>
        {
            string pathA = GetHierarchyPath(a.transform);
            string pathB = GetHierarchyPath(b.transform);
            return string.CompareOrdinal(pathA, pathB);
        });

        return components;
    }

    public static T[] FindObjectsOfTypeOrdered<T>() where T : Component
    {
        T[] objects = Object.FindObjectsByType<T>(FindObjectsSortMode.None);
        
        Array.Sort(objects, (a, b) =>
        {
            string pathA = GetHierarchyPath(a.transform);
            string pathB = GetHierarchyPath(b.transform);
            return string.CompareOrdinal(pathA, pathB);
        });

        return objects;
    }

    private static string GetHierarchyPath(Transform t)
    {
        System.Text.StringBuilder builder = new System.Text.StringBuilder(64);

        while (t != null)
        {
            builder.Insert(0, '/');
            builder.Insert(1, t.name);
            t = t.parent;
        }

        return builder.ToString();
    }

    #endregion


    #region HasComponent

    public static bool HasComponent<T>(this Object obj) where T : Component
    {
        return obj.Transform().GetComponent<T>() != null;
    }

    public static bool HasComponentInChildren<T>(this Object obj, bool includeInactive = false) where T : Component
    {
        return obj.Transform().GetComponentInChildren<T>(includeInactive) != null;
    }

    public static bool HasComponentInParent<T>(this Object obj, bool includeInactive = false) where T : Component
    {
        return obj.Transform().GetComponentInParent<T>(includeInactive) != null;
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


    #region List/Array Shuffle Shuffle

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
        DebugLogger.LogAssertion("AddSmart called with length 0", length <= 0);

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


    #region List/Array SelectRandom

    /// <summary>Selects and returns a random entry of Array.</summary>
    public static T SelectRandom<T>(this T[] targetArray)
    {
        return targetArray[Random.Range(0, targetArray.Length)];
    }
    /// <summary>Selects and returns a random entry of List.</summary>
    public static T SelectRandom<T>(this List<T> targetList)
    {
        return targetList[Random.Range(0, targetList.Count)];
    }
    /// <summary>Selects and returns a random entry of NativeArray.</summary>
    public static T SelectRandom<T>(this NativeArray<T> targetArray) where T : struct
    {
        return targetArray[Random.Range(0, targetArray.Length)];
    }
    /// <summary>Selects and returns a random entry of NativeList.</summary>
    public static T SelectRandom<T>(this NativeList<T> targetList) where T : unmanaged
    {
        return targetList[Random.Range(0, targetList.Length)];
    }

    /// <summary>Selects and returns a random list filled with entries of <paramref name="targetArray"/></summary>
    public static T[] SelectRandomRange<T>(this T[] targetArray, int entryCount, bool forceUniqueEntries = true)
    {
        T[] output = new T[entryCount];

        if (!forceUniqueEntries)
        {
            for (int i = 0; i < entryCount; i++)
            {
                output[i] = targetArray[Random.Range(0, targetArray.Length)];
            }
            return output;
        }

        List<int> numberPot = new List<int>(targetArray.Length);
        for (int i = 0; i < targetArray.Length; i++)
        {
            numberPot.Add(i);
        }
        for (int i = 0; i < entryCount; i++)
        {
            int r = Random.Range(0, numberPot.Count);
            output[i] = targetArray[numberPot[r]];
            numberPot.RemoveAt(r);
        }
        return output;
    }
    /// <summary>Selects and returns a random list filled with entries of <paramref name="targetList"/></summary>
    public static T[] SelectRandomRange<T>(this List<T> targetList, int entryCount, bool forceUniqueEntries = true)
    {
        T[] output = new T[entryCount];

        if (!forceUniqueEntries)
        {
            for (int i = 0; i < entryCount; i++)
            {
                output[i] = targetList[Random.Range(0, targetList.Count)];
            }
            return output;
        }

        List<int> numberPot = new List<int>(targetList.Count);
        for (int i = 0; i < targetList.Count; i++)
        {
            numberPot.Add(i);
        }
        for (int i = 0; i < entryCount; i++)
        {
            int r = Random.Range(0, numberPot.Count);
            output[i] = targetList[numberPot[r]];
            numberPot.RemoveAt(r);
        }
        return output;
    }
    /// <summary>Selects and returns a random list filled with entries of <paramref name="targetArray"/></summary>
    public static T[] SelectRandomRange<T>(this NativeArray<T> targetArray, int entryCount, bool forceUniqueEntries = true) where T : struct
    {
        T[] output = new T[entryCount];

        if (!forceUniqueEntries)
        {
            for (int i = 0; i < entryCount; i++)
            {
                output[i] = targetArray[Random.Range(0, targetArray.Length)];
            }
            return output;
        }

        List<int> numberPot = new List<int>(targetArray.Length);
        for (int i = 0; i < targetArray.Length; i++)
        {
            numberPot.Add(i);
        }
        for (int i = 0; i < entryCount; i++)
        {
            int r = Random.Range(0, numberPot.Count);
            output[i] = targetArray[numberPot[r]];
            numberPot.RemoveAt(r);
        }
        return output;
    }
    /// <summary>Selects and returns a random list filled with entries of <paramref name="targetList"/></summary>
    public static T[] SelectRandomRange<T>(this NativeList<T> targetList, int entryCount, bool forceUniqueEntries = true) where T : unmanaged
    {
        T[] output = new T[entryCount];

        if (!forceUniqueEntries)
        {
            for (int i = 0; i < entryCount; i++)
            {
                output[i] = targetList[Random.Range(0, targetList.Length)];
            }
            return output;
        }

        List<int> numberPot = new List<int>(targetList.Length);
        for (int i = 0; i < targetList.Length; i++)
        {
            numberPot.Add(i);
        }
        for (int i = 0; i < entryCount; i++)
        {
            int r = Random.Range(0, numberPot.Count);
            output[i] = targetList[numberPot[r]];
            numberPot.RemoveAt(r);
        }
        return output;
    }

    #endregion


    #region List/Array Has(Valid)Data

    /// <returns>Whether array is valid and has at least 1 entry.</returns>
    public static bool HasData<T>(this T[] array)
    {
        return array != null && array.Length > 0;
    }
    /// <returns>Whether array is invalid or it has 0 entries.</returns>
    public static bool IsNullOrEmpty<T>(this T[] array)
    {
        return array == null || array.Length == 0;
    }
    /// <returns>Whether array is invalid or it has 0 entries.</returns>
    public static bool IsNotNullOrEmpty<T>(this T[] array)
    {
        return array != null && array.Length != 0;
    }
    /// <returns>Whether array is invalid  or it has 0 entries or it has one or more null entries</returns>
    public static bool HasInvalidData<T>(this T[] array)
    {
        bool arrayValid = array != null && array.Length > 0;
        if (arrayValid)
        {
            int count = array.Length;
            for (int i = 0; i < count; i++)
            {
                if (array[i] == null)
                {
                    return true;
                }
            }
        }
        return false;
    }
    /// <returns>Whether array is invalid  or it has 0 entries or it has one or more null entries</returns>
    public static bool HasNoInvalidData<T>(this T[] array)
    {
        bool arrayValid = array != null && array.Length > 0;
        if (arrayValid)
        {
            int count = array.Length;
            for (int i = 0; i < count; i++)
            {
                if (array[i] == null)
                {
                    return false;
                }
            }
        }
        return true;
    }

    #endregion


    #region Netcode Utility

    /// <summary>
    /// Get PlayerGameId through ClientManager using OwnerClientId.
    /// </summary>
    public static int GetOwnerClientGameId(this NetworkObject networkObj)
    {
        return ClientManager.GetClientGameId(networkObj.OwnerClientId);
    }
    /// <summary>
    /// Gets <see cref="ServerRpcParams.Receive"/>.SenderClientId and converts it to gameId using <see cref="ClientManager"/> gamdeId system
    /// </summary>
    public static int GetSenderClientGameId(this RpcParams receive)
    {
        return ClientManager.GetClientGameId(receive.Receive.SenderClientId);
    }

    #endregion


    /// <summary>
    /// Try finding an action by name, returns true if found, false if not. Outputs the found action
    /// </summary>
    public static bool TryFindAction(this InputActionAsset actionAsset, string actionName, out InputAction action)
    {
        action = actionAsset.FindAction(actionName);
        return action != null;
    }
}