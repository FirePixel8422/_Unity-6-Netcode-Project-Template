using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class RebindManager : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private InputActionReference cancelRebindAction;

    private InputActionRebindingExtensions.RebindingOperation currentOperation;

    [Tooltip("Path to rebind save file)")]
    private const string REBINDS_PATH = "Input/Rebinds";

#if Enable_Debug_Logging
    [SerializeField] private bool logRebindOperations = true;
#endif


    private void OnEnable()
    {
        cancelRebindAction.action.performed += OnCancelRebind;
        cancelRebindAction.action.Enable();
    }
    private void OnDisable()
    {
        cancelRebindAction.action.performed -= OnCancelRebind;
        cancelRebindAction.action.Disable();
    }
    private void OnCancelRebind(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            CancelRebind();
        }
    }

    private async void Start()
    {
        await LoadRebindsAsync();
    }


    public void StartRebind(string actionName, int bindingIndex)
    {
        if (inputActions.TryFindAction(actionName, out InputAction action))
        {
            action.Disable();

            currentOperation = action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("Mouse")
            .OnCancel(op =>
            {
                DebugLogger.Log($"Rebind for {actionName} canceled.", logRebindOperations);

                action.Enable();
                op.Dispose();
                currentOperation = null;
            })
            .OnComplete(op =>
            {
                DebugLogger.Log($"Rebound {actionName} to {action.bindings[bindingIndex].effectivePath}", logRebindOperations);

                action.Enable();
                op.Dispose();
                currentOperation = null;
                _ = SaveRebindsAsync();
            })
            .Start();
        }
    }
    public void CancelRebind()
    {
        if (currentOperation != null)
        {
            currentOperation.Cancel();
            currentOperation.Dispose();
            currentOperation = null;
        }
    }


    #region Save, Load and Reset Rebinds

    private async Task LoadRebindsAsync()
    {
        (bool success, ValueWrapper<string> rebindJson) = await FileManager.LoadInfoAsync<ValueWrapper<string>>(REBINDS_PATH);
        if (success && !string.IsNullOrEmpty(rebindJson.Value))
        {
            inputActions.LoadBindingOverridesFromJson(rebindJson.Value);
        }
    }
    private async Task SaveRebindsAsync()
    {
        string json = inputActions.SaveBindingOverridesAsJson();
        await FileManager.SaveInfoAsync(new ValueWrapper<string>(json), REBINDS_PATH);
    }

    public void ResetRebinds()
    {
        inputActions.RemoveAllBindingOverrides();

        bool success = FileManager.TryDeleteFile(REBINDS_PATH);


#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (logRebindOperations)
        {
            if (success)
            {
                DebugLogger.Log("Rebinds reset and rebind file deleted.");
            }
            else
            {
                DebugLogger.Log("Rebinds reset but no rebind file found to delete.");
            }
        }
#endif
    }

    #endregion


    private void OnDestroy()
    {
        CancelRebind();
    }
}
