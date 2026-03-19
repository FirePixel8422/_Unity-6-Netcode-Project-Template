using Fire_Pixel.Utility;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class NetworkCursorHandler : MonoBehaviour
{
    [SerializeField] private float updateInterval = 0.02f;
    [SerializeField] private float lerpSpeed = 25;
    [SerializeField] private float spritePixelSize = 32f;
    [SerializeField] private Vector2 hotspot = new Vector2(6.25f, -11.5f);

    private const int SM_CXCURSOR = 13;
    private const int SM_CYCURSOR = 14;

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    private RectTransform rectTransform;
    private float elapsed;
    private Vector2 lastNormalizedPos;
    private Vector2 targetRecievedPosition;


    public void Init(Color cursorColor, bool owner)
    {
        Image img = GetComponent<Image>();
        img.color = cursorColor;

        rectTransform = (RectTransform)transform;
        ApplyWindowsCursorScale();

        if (owner)
        {
            img.enabled = false;
            CallbackScheduler.RegisterUpdate(OnUpdateOwner);
        }
        else
        {
            img.enabled = true;
            CallbackScheduler.RegisterUpdate(OnUpdateNonOwner);
        }
    }

    private void ApplyWindowsCursorScale()
    {
        int width = GetSystemMetrics(SM_CXCURSOR);
        int height = GetSystemMetrics(SM_CYCURSOR);

        float scaleX = width / spritePixelSize;
        float scaleY = height / spritePixelSize;

        rectTransform.localScale = new Vector3(scaleX, scaleY, 1f);
    }

    private void OnUpdateOwner()
    {
        elapsed += Time.deltaTime;
        if (elapsed < updateInterval) return;
        elapsed = 0;

        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        Vector2 normalizedPos = Mouse.current.position.value / screenSize;

        if (Vector2.Distance(normalizedPos, lastNormalizedPos) > 0.001f)
        {
            lastNormalizedPos = normalizedPos;
            NetworkCursorManager.Instance.SendMousePosition_RPC(normalizedPos);
        }
    }
    private void OnUpdateNonOwner()
    {
        rectTransform.position = Vector2.Lerp(rectTransform.position, targetRecievedPosition, lerpSpeed * Time.deltaTime);
    }
    public void RecieveMousePosition_Local(Vector2 finalMousePos)
    {
        targetRecievedPosition = finalMousePos + hotspot;
    }

    private void OnDestroy()
    {
        CallbackScheduler.UnRegisterUpdate(OnUpdateOwner);
        CallbackScheduler.UnRegisterUpdate(OnUpdateNonOwner);
    }
}