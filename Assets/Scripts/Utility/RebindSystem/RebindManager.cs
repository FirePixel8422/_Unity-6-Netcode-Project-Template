using Fire_Pixel.Utility;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;


public class RebindManager : MonoBehaviour
{
    public static RebindManager Instance { get; private set; }

#pragma warning disable UDR0001
    public static OneTimeAction PostRebindsLoaded = new OneTimeAction();
#pragma warning restore UDR0001


    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private InputActionReference cancelRebindAction;

    [SerializeField] private GameObject rebindPopupObj;
    [SerializeField] private TextMeshProUGUI toRebindControlText;

    [SerializeField] private bool logRebindOperations = true;

    private InputActionRebindingExtensions.RebindingOperation currentOperation;

    private const string REBINDS_SAVEDATA = "Input/Rebinds";



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

    private void Awake()
    {
        Instance = this;
        LoadRebinds();
    }
    private void OnDestroy()
    {
        PostRebindsLoaded = new OneTimeAction();
        CancelRebind();
    }

    private void OnCancelRebind(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        CancelRebind();
    }


    public void StartRebind(string actionName, string bindingId, TextMeshProUGUI rebindKeyText)
    {
        if (currentOperation != null)
        {
            return;
        }

        if (!inputActions.TryFindAction(actionName, out InputAction action))
        {
            DebugLogger.Log($"Action {actionName} not found.", logRebindOperations);
            return;
        }

        int bindingIndex = action.bindings
            .Select((b, i) => new { binding = b, index = i })
            .FirstOrDefault(x => x.binding.id.ToString() == bindingId)?.index ?? -1;

        if (bindingIndex == -1)
        {
            DebugLogger.Log($"Binding ID {bindingId} not found on action {actionName}.", logRebindOperations);
            return;
        }

        // Prevent rebinding composite headers
        if (action.bindings[bindingIndex].isComposite)
        {
            DebugLogger.Log("Cannot rebind a composite root binding.", logRebindOperations);
            return;
        }

        action.Disable();
        rebindPopupObj.SetActive(true);
        toRebindControlText.text = $"Rebinding '{actionName}' ({action.GetBindingDisplayString(bindingIndex)})";

        currentOperation = action
            .PerformInteractiveRebinding(bindingIndex)
            .WithCancelingThrough("<Keyboard>/escape")
            .OnMatchWaitForAnother(0.1f)
            .OnCancel(operation =>
            {
                Cleanup(action, operation);
                DebugLogger.Log($"Rebind for {actionName} canceled.", logRebindOperations);
            })
            .OnComplete(operation =>
            {
                DebugLogger.Log($"Rebound {actionName} to {action.bindings[bindingIndex].effectivePath}", logRebindOperations);

                rebindKeyText.text = action.GetBindingDisplayString(bindingIndex);
                rebindPopupObj.SetActive(false);

                Cleanup(action, operation);
                SaveRebinds();
            });

        currentOperation.Start();
    }

    public void CancelRebind()
    {
        currentOperation?.Cancel();
        rebindPopupObj.SetActive(false);
    }
    private void Cleanup(InputAction action, InputActionRebindingExtensions.RebindingOperation operation)
    {
        action.Enable();

        operation.Dispose();
        currentOperation = null;
    }

    private void LoadRebinds()
    {
        if (SaveData.TryGet(REBINDS_SAVEDATA, out string rebindJson) && !string.IsNullOrEmpty(rebindJson))
        {
            inputActions.LoadBindingOverridesFromJson(rebindJson);

            DebugLogger.Log("Rebinds loaded.", logRebindOperations);
        }
        PostRebindsLoaded?.Invoke();
    }
    private void SaveRebinds()
    {
        string json = inputActions.SaveBindingOverridesAsJson();
        SaveData.Set(REBINDS_SAVEDATA, json);

        DebugLogger.Log("Rebinds saved.", logRebindOperations);
    }

    public void ResetRebinds()
    {
        inputActions.RemoveAllBindingOverrides();

#if Enable_Debug_Systems
        bool success = SaveData.Delete(REBINDS_SAVEDATA);

        DebugLogger.Log("Rebinds reset " + (success ?
            "and file deleted." :
            "but no file found."),
            logRebindOperations);
#else
        SaveData.Delete(REBINDS_SAVEDATA);
#endif
    }
}
