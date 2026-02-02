using UnityEngine;
using Fire_Pixel.Utility;


public class UpdateMonoBehaviour : MonoBehaviour
{
    private void OnEnable() => UpdateScheduler.RegisterUpdate(OnUpdate);
    private void OnDisable() => UpdateScheduler.UnRegisterUpdate(OnUpdate);

    protected virtual void OnUpdate()
    {

    }
}