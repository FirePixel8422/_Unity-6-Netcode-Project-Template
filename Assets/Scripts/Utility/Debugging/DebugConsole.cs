using Fire_Pixel.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DebugConsole : UpdateMonoBehaviour
{
    public static DebugConsole Instance { get; private set; }
    private static readonly Queue<LogEntry> _pendingLogs = new();
    private static LogEntry _lastLog;
    private static bool _hasWarnedOfInexistance;
    private static bool _isSceneLoaded;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _textBoxPrefab;
    [SerializeField] private Transform _chatContentHolder;
    [SerializeField] private Scrollbar _scrollBar;
    [SerializeField] private GameObject uiParent;

    [Header("Limits")]
    [Min(1)][SerializeField] private int _maxLogEntries = 100;
    [Min(1)][SerializeField] private int _maxLogsPerFrame = 10;
    [SerializeField] private bool _filterDuplicateEntries = true;

    private readonly Queue<TextMeshProUGUI> _visibleEntries = new();
    private bool _isScrolling;


    private void Awake() => Instance = this;
#if UNITY_EDITOR
    private void OnApplicationQuit()
    {
        _hasWarnedOfInexistance = false;
        _isSceneLoaded = false;
    }
#endif
    public static void SetActive(bool state)
    {
        if (Instance == null) return;
        
        Instance.uiParent.SetActive(state);
    }


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;

        if (!_isSceneLoaded)
        {
            OnSceneLoaded(default, default);
        }
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _hasWarnedOfInexistance = false;
        _isSceneLoaded = true;
    }
    private static void OnSceneUnloaded(Scene scene)
    {
        _pendingLogs.Clear();
        _lastLog = null;
    }

    public static void Log(object message, bool alsoLogToUnityConsole = false)
    {
        LogInternal(message, false, Color.white, alsoLogToUnityConsole);
    }
    public static void Log(object message, Color color, bool alsoLogToUnityConsole = false)
    {
        LogInternal(message, true, color, alsoLogToUnityConsole);
    }
    private static void LogInternal(object message, bool useColor, Color color, bool alsoLogToUnityConsole)
    {
        string text = message?.ToString() ?? "null";
        _pendingLogs.Enqueue(new LogEntry(text, useColor, color));

        if (_isSceneLoaded && !_hasWarnedOfInexistance && (Instance == null || !Instance.isActiveAndEnabled) && _pendingLogs.Count > 0)
        {
            DebugLogger.LogWarning("[DebugConsole]: Console instance does not exist or is inactive.");
            _hasWarnedOfInexistance = true;
        }

        if (alsoLogToUnityConsole)
        {
            string formattedText = useColor
                ? $"#{ColorUtility.ToHtmlStringRGB(color)}>{message}</color>"
                : message.ToString();

            DebugLogger.Log(formattedText);
        }
    }

    protected override void OnUpdate()
    {
        int logsProcessed = 0;

        while (logsProcessed < _maxLogsPerFrame && _pendingLogs.Count > 0)
        {
            LogEntry entry = _pendingLogs.Dequeue();
            entry.FormattedText = entry.UseColor
                ? $"[{DateTime.Now:HH:mm:ss}] <#{ColorUtility.ToHtmlStringRGB(entry.Color)}>{entry.Message}</color>"
                : $"[{DateTime.Now:HH:mm:ss}] {entry.Message}";

            if (_filterDuplicateEntries && _lastLog != null && _lastLog.Message == entry.Message)
            {
                _lastLog.FormattedText = entry.FormattedText;
                if (_lastLog.TextObject != null) _lastLog.TextObject.text = entry.FormattedText;
            }
            else
            {
                PrintToConsole(entry);
                _lastLog = entry;
            }

            logsProcessed++;
        }

        if (!_isScrolling && logsProcessed > 0)
        {
            StartCoroutine(ScrollToBottom());
        }
    }

    private void PrintToConsole(LogEntry entry)
    {
        TextMeshProUGUI textObj;

        if (_visibleEntries.Count < _maxLogEntries)
        {
            textObj = Instantiate(_textBoxPrefab, _chatContentHolder, false);
        }
        else
        {
            textObj = _visibleEntries.Dequeue();
            textObj.transform.SetAsLastSibling();
        }

        textObj.text = entry.FormattedText;
        entry.TextObject = textObj;
        _visibleEntries.Enqueue(textObj);
    }

    private IEnumerator ScrollToBottom()
    {
        _isScrolling = true;

        yield return null;
        yield return new WaitForEndOfFrame();

        _scrollBar.value = 0f;
        _isScrolling = false;
    }

    private sealed class LogEntry
    {
        public readonly string Message;
        public readonly bool UseColor;
        public readonly Color Color;
        public string FormattedText;
        public TextMeshProUGUI TextObject;

        public LogEntry(string message, bool useColor, Color color)
        {
            Message = message;
            UseColor = useColor;
            Color = color;
        }
    }
}