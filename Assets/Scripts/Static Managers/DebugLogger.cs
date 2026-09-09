using UnityEngine;


/// <summary>
/// Performance optimized DebugLogger Logger that only logs messages when conditional <see cref="SDS"/> argument is provided in build settings. Also has logging based on condition support
/// </summary>
public static class DebugLogger
{
    public const string SDS = "ENABLE_DEBUG_SYSTEMS";


    [HideInCallstack]
    [System.Diagnostics.Conditional(SDS)]
    public static void Log(object message, bool logCondition = true)
    {
        if (!logCondition) return;

        Debug.Log(message);
    }

    [HideInCallstack]
    [System.Diagnostics.Conditional(SDS)]
    public static void LogWarning(object message, bool logCondition = true)
    {
        if (!logCondition) return;

        Debug.LogWarning(message);
    }

    [HideInCallstack]
    [System.Diagnostics.Conditional(SDS)]
    public static void LogError(object message, bool logCondition = true)
    {
        if (!logCondition) return;

        Debug.LogError(message);
    }

    [HideInCallstack]
    [System.Diagnostics.Conditional(SDS)]
    public static void LogAssertion(object message, bool errorCondition = true)
    {
        if (!errorCondition) return;

        Debug.LogAssertion(message);
    }
}