using UnityEngine;
using UnityEngine.InputSystem; // <—

public class UnitInputRouter : MonoBehaviour
{
    [SerializeField] private float longPressSeconds = 1f;

    UnitRuntime pressed;
    float pressedTime;
    bool longFired;

    void Update()
    {
        // Se il mouse non esiste (es. build mobile), esci
        var mouse = Mouse.current;
        if (mouse == null) return;

        // DOWN
        if (mouse.leftButton.wasPressedThisFrame)
        {
            var rt = HitUnitUnderPointer();
            if (rt != null)
            {
                pressed = rt;
                pressedTime = Time.time;
                longFired = false;

                Debug.Log($"[DOWN] {pressed.displayName}#{pressed.instanceId} slot={pressed.slotKey}");
            }
        }

        // HOLD -> LONG
        if (pressed != null && !longFired && mouse.leftButton.isPressed)
        {
            if (Time.time - pressedTime >= longPressSeconds)
            {
                longFired = true;
                pressed.OnLongPress();
            }
        }

        // UP -> TAP (solo se non long)
        if (mouse.leftButton.wasReleasedThisFrame)
        {
            if (pressed != null && !longFired)
                pressed.OnTap();

            pressed = null;
        }
    }

    UnitRuntime HitUnitUnderPointer()
    {
        var cam = Camera.main;
        if (cam == null) return null;

        Vector3 m = Mouse.current.position.ReadValue();
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(m.x, m.y, -cam.transform.position.z));
        var hit = Physics2D.OverlapPoint(new Vector2(world.x, world.y));
        if (hit == null) return null;

        return hit.GetComponentInParent<UnitRuntime>();
    }
}
