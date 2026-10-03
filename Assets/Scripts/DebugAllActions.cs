using UnityEngine;
using UnityEngine.InputSystem;

public class DebugAllActions : MonoBehaviour
{
    public InputActionAsset inputActionAsset;

    void OnEnable()
    {
        if (inputActionAsset == null) return;
        foreach (var map in inputActionAsset.actionMaps)
        {
            foreach (var action in map.actions)
            {
                action.performed += OnActivate;
            }
        }
        inputActionAsset.Enable();
    }

    void OnDisable()
    {
        if (inputActionAsset == null) return;
        foreach (var map in inputActionAsset.actionMaps)
        {
            foreach (var action in map.actions)
            {
                action.performed -= OnActivate;
            }
        }
        inputActionAsset.Disable();
    }

    void OnActivate(InputAction.CallbackContext ctx)
    {
        Debug.Log($"Action performed: {ctx.action.name} (map: {ctx.action.actionMap.name}) | control: {ctx.control.displayName}");
    }
}