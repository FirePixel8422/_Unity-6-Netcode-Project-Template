using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// Performance optimized DebugLogger Logger that only logs messages when conditional <see cref="GlobalGameData.SDS_DEBUG_ONLY"/> argument is provided in build settings. Also has logging based on condition support
/// </summary>
public static class DebugLogger
{
    [HideInCallstack, DebuggerHidden]
    [System.Diagnostics.Conditional(GlobalGameData.SDS_DEBUG_ONLY)]
    public static void Log(object message, bool logCondition = true)
    {
        if (!logCondition) return;

        Debug.Log(message);
    }

    [HideInCallstack, DebuggerHidden]
    [System.Diagnostics.Conditional(GlobalGameData.SDS_DEBUG_ONLY)]
    public static void LogWarning(object message, bool logCondition = true)
    {
        if (!logCondition) return;

        Debug.LogWarning(message);
    }

    [HideInCallstack, DebuggerHidden]
    [System.Diagnostics.Conditional(GlobalGameData.SDS_DEBUG_ONLY)]
    public static void LogError(object message, bool logCondition = true)
    {
        if (!logCondition) return;

        Debug.LogError(message);
    }

    [HideInCallstack, DebuggerHidden]
    [System.Diagnostics.Conditional(GlobalGameData.SDS_DEBUG_ONLY)]
    public static void LogAssertion(object message, bool errorCondition = true)
    {
        if (!errorCondition) return;

        Debug.LogAssertion(message);
    }

    [HideInCallstack, DebuggerHidden]
    [System.Diagnostics.Conditional(GlobalGameData.SDS_DEBUG_ONLY)]
    public static void LogException(System.Exception exc)
    {
        Debug.LogException(exc);
    }
}