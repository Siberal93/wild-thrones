using UnityEngine;
using UnityEngine.InputSystem; // <—

public class UnitInputRouter : MonoBehaviour
{
    [SerializeField] private float longPressSeconds = 0.4f;

    UnitRuntime pressed;
    float pressedTime;
    bool longFired;
    private UnitRuntime dragging;
    private Vector3 dragOffset;
    private string hoverSlot;
    private UnitRuntime hoverUnit;


    void Update()
    {
        // DOWN (mouse o touch)
        if (PointerDownThisFrame())
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

        // HOLD: se sto premendo su qualcosa e non ho ancora triggerato long
        if (pressed != null && !longFired && PointerIsPressed())
        {
            float t = Time.time - pressedTime;

            // debug ogni ~0.2s (non spam)

            if (t >= longPressSeconds)
            {
                longFired = true;
                Debug.Log($"[LONG] {pressed.displayName}#{pressed.instanceId} t={t:0.00}/{longPressSeconds:0.00}");
                BeginDrag(pressed);
            }
        }

        // DRAG: una volta iniziato, aggiorna OGNI frame finché è premuto
        if (dragging != null && PointerIsPressed())
        {
            UpdateDrag();
        }

        // UP
        if (PointerUpThisFrame())
        {
            if (pressed != null && !longFired)
            {
                Debug.Log($"[TAP] {pressed.displayName}#{pressed.instanceId}");
                pressed.OnTap();
            }

            pressed = null;

            if (dragging != null)
                EndDrag();
        }
    }

    UnitRuntime HitUnitUnderPointer()
    {
        Vector2 world = PointerWorld();
        var hit = Physics2D.OverlapPoint(world);
        if (hit == null) return null;

        return hit.GetComponentInParent<UnitRuntime>();
    }

    void BeginDrag(UnitRuntime u)
    {
        dragging = u;

        Vector2 world = PointerWorld();
        dragOffset = Vector3.zero;

        Debug.Log($"[DRAG START] {dragging.displayName}#{dragging.instanceId}");

        // porta davanti e centra sotto il puntatore
        dragging.transform.position = new Vector3(world.x, world.y, -2f);
    }

    void UpdateDrag()
    {
        if (dragging == null) return;

        Vector3 world = PointerWorld();
        dragging.transform.position = new Vector3(world.x, world.y, -2f) + dragOffset;

        // trova slot target (nearest)
        var bb = BoardBootstrap.Instance;
        if (bb == null) return;

        string newHoverSlot = bb.FindNearestSlotKey(
            new Vector2(dragging.transform.position.x, dragging.transform.position.y),
            maxDist: 0.9f
        );

        // highlight slot
        if (newHoverSlot != hoverSlot)
        {
            bb.HighlightSlot(newHoverSlot);
            hoverSlot = newHoverSlot;
        }

        // feedback su unità target (se occupato)
        var newHoverUnit = (hoverSlot != null) ? bb.GetUnitAt(hoverSlot) : null;
        if (newHoverUnit == dragging) newHoverUnit = null;

        if (newHoverUnit != hoverUnit)
        {
            if (hoverUnit != null) hoverUnit.SetHover(false);
            hoverUnit = newHoverUnit;
            if (hoverUnit != null) hoverUnit.SetHover(true);
        }
    }

    void EndDrag()
    {
        if (dragging == null) return;

        var bb = BoardBootstrap.Instance;

        // reset feedback
        if (hoverUnit != null) hoverUnit.SetHover(false);
        hoverUnit = null;

        if (bb != null)
            bb.ClearHighlightSlot();

        if (bb != null && !string.IsNullOrEmpty(hoverSlot))
        {
            bb.MoveOrSwap(dragging, hoverSlot);
            Debug.Log($"[DRAG DROP] -> {hoverSlot}");
        }
        else
        {
            // snap back
            if (bb != null) bb.RegisterUnit(dragging, dragging.slotKey);
            Debug.Log("[DRAG CANCEL] snap back");
        }

        dragging = null;
        hoverSlot = null;
    }

    bool PointerIsPressed()
    {
        // Mouse
        if (Mouse.current != null && Mouse.current.leftButton.isPressed) return true;

        // Touch (mobile / trackpad touch simulato)
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed) return true;

        return false;
    }

    Vector2 PointerWorld()
    {
        var cam = Camera.main;
        if (cam == null) return Vector2.zero;

        // Mouse position (default su editor)
        Vector2 screen = Vector2.zero;
        if (Mouse.current != null)
            screen = Mouse.current.position.ReadValue();
        else if (Touchscreen.current != null)
            screen = Touchscreen.current.primaryTouch.position.ReadValue();
        else
            screen = (Vector2)Input.mousePosition;

        var w = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, 0f));
        return new Vector2(w.x, w.y);
    }

    bool PointerDownThisFrame()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) return true;
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame) return true;
        return false;
    }

    bool PointerUpThisFrame()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame) return true;
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame) return true;
        return false;
    }

}
