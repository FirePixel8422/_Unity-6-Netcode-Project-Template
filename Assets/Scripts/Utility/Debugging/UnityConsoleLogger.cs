using System.Collections.Generic;
using UnityEngine;
using Fire_Pixel.Utility;

public class UnityConsoleLogger : UpdateMonoBehaviour
{
    private readonly Queue<(string message, LogType type)> _pendingLogs = new();
    private readonly object _logLock = new();

    protected override void OnEnable()
    {
        base.OnEnable();
        Application.logMessageReceivedThreaded += OnUnityLog;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        Application.logMessageReceivedThreaded -= OnUnityLog;
    }

    private void OnUnityLog(string condition, string stackTrace, LogType type)
    {
        string message = type == LogType.Exception ? $"{condition}\n{stackTrace}" : condition;

        lock (_logLock)
        {
            _pendingLogs.Enqueue((message, type));
        }
    }

    protected override void OnUpdate()
    {
        lock (_logLock)
        {
            while (_pendingLogs.Count > 0)
            {
                var (message, type) = _pendingLogs.Dequeue();
                Color color = type switch
                {
                    LogType.Log => Color.white,
                    LogType.Warning => Color.yellow,

                    LogType.Error or
                    LogType.Assert or
                    LogType.Exception
                        => Color.red,

                    _ => Color.white,
                };

                DebugConsole.Log(message, color);
            }
        }
    }
}