using TMPro;
using UnityEngine;


public class LoadingTextAnimator : UpdateMonoBehaviour
{
    [SerializeField] private TextMeshProUGUI loadingTextObj;
    [SerializeField] private float animationSpeed = 0.5f;

    private string loadingText;
    private float time;


    private void Start()
    {
        loadingText = loadingTextObj.text;
        time = animationSpeed * 2;
    }


    protected override void OnUpdate()
    {
        time += Time.deltaTime;

        float dotCount = time / animationSpeed % 4;

        loadingTextObj.text = loadingText + new string('.', (int)dotCount);
    }
}
