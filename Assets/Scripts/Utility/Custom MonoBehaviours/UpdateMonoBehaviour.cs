using UnityEngine;
using Fire_Pixel.Utility;


public class UpdateMonoBehaviour : MonoBehaviour
{
    protected virtual void OnEnable() => CallbackScheduler.RegisterCallback(OnUpdate, CallbackType.Update);
    protected virtual void OnDisable() => CallbackScheduler.RegisterCallback(OnUpdate, CallbackType.Update);

    protected virtual void OnUpdate() { }
}