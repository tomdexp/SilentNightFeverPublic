using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseDisabler : MonoBehaviour
{
    [SerializeField, LabelText("Using keyboard shows cursor")] private bool _keyboardShows;
    [SerializeField, LabelText("Put cursor on the bottom left of screen when hidden")] private bool _resetCursorPos;

    private Vector2 _oldMousePosition;
    private bool _warping;

    [Title("    Events")]
    public static Action OnMouseDisabled;
    public static Action OnMouseEnabled;

    private void Start()
    {
        _oldMousePosition = Mouse.current.position.value;
        _warping = false;
    }

    private void OnEnable()
    {
        InputSystem.onActionChange += EnableOrDisableCursor;
    }

    private void OnDisable()
    {
        InputSystem.onActionChange -= EnableOrDisableCursor;
    }

    /// <summary>
    /// Meant to be called with the event : " InputSystem.onActionChange". 
    /// If the last Input was performed using a mouse (IE : moving the mouse, clicking), the cursor appears and is enabled.
    /// If the last Input was performed using a controller, the cursor disappears and is disabled.
    /// </summary>
    private void EnableOrDisableCursor(object arg1, InputActionChange arg2)
    {
        try
        {
            InputAction InpAct = (InputAction)arg1;
            if (!InpAct.IsPressed()) return;

            if (DoesDeviceShowsCursor(InpAct))
                EnableCursor(true);
            else
                EnableCursor(false);
        }
        catch (System.Exception)
        { }
    }

    private bool DoesDeviceShowsCursor(InputAction InpAct)
    {
        return (InpAct.activeControl.device.displayName == "Mouse" || (InpAct.activeControl.device.displayName == "Keyboard" && _keyboardShows));
    }

    private void EnableCursor(bool enable)
    {
        // if cursor is already visible, ignore enable
        if (UnityEngine.Cursor.visible == false && enable)
        {
            if (_warping) return;

            if (_resetCursorPos)
                Mouse.current.WarpCursorPosition(_oldMousePosition);
            UnityEngine.Cursor.visible = true;

            OnMouseEnabled?.Invoke();
            return;
        }

        // if cursor is already hiden, ignore disable
        if (UnityEngine.Cursor.visible == true && !enable)
        {
            if (_resetCursorPos)
            {
                // save mouse position and put it on the bottom left of the screen
                _oldMousePosition = Mouse.current.position.value;
                Mouse.current.WarpCursorPosition(new Vector2(0, 0));
                StartCoroutine(DelayForWarping());
            }

            UnityEngine.Cursor.visible = false;

            OnMouseDisabled?.Invoke();
            return;
        }
    }

    IEnumerator DelayForWarping()
    {
        _warping = true;
        for (int i = 0; i < 1; i++)
            yield return null;

        _warping = false;
    }
}
