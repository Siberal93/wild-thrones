using UnityEngine;
using UnityEngine.InputSystem; // <—

public class UnitInputRouter : MonoBehaviour
{
    [SerializeField] private float dragDelaySeconds = 0.4f;

    UnitRuntime pressed;
    float pressedTime;
    bool longFired;
    private UnitRuntime dragging;
    private Vector3 dragOffset;
    private string hoverSlot;
    private UnitRuntime hoverUnit;


    void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        var bb = BoardBootstrap.Instance;
        if (bb == null) return;

        // DOWN
        if (mouse.leftButton.wasPressedThisFrame)
        {
            var rt = HitUnitUnderPointer();
            if (rt != null)
            {
                pressed = rt;
                pressedTime = Time.time;
                longFired = false;

                // mostra card subito
                Vector2 sp = bb.GetOppositeGridCenterScreen(pressed);
                bb.CardView?.Show(rt, sp);

                Debug.Log($"[DOWN] {pressed.displayName}#{pressed.instanceId} slot={pressed.slotKey} (CARD SHOW)");
            }
        }

        // HOLD
        if (pressed != null && mouse.leftButton.isPressed)
        {
            // se non stiamo già dragando, controlla se “armare” il drag
            if (!longFired)
            {
                float held = Time.time - pressedTime;

                // drag solo se: held >= 0.4s E pointer fuori dallo slot
                if (held >= dragDelaySeconds)
                {
                    var w = PointerWorld();
                    bool inside = bb.IsWorldInsideSlot(pressed.slotKey, w);

                    if (!inside)
                    {
                        longFired = true;

                        // nascondi card quando parte il drag (scelta UX: così non copre)
                        bb.CardView?.Hide();

                        BeginDrag(pressed);
                        Debug.Log($"[DRAG ARMED] {pressed.displayName}#{pressed.instanceId} (left slot after {held:0.00}s)");
                    }
                    else
                    {
                        // debug utile per capire “perché non draga”
                        // (commentalo se spam)
                        // Debug.Log($"[HOLD] {pressed.displayName} inside slot -> no drag yet ({held:0.00}s)");
                    }
                }
            }

            // se stiamo dragando, muovi
            if (dragging != null && PointerIsPressed())
            {
                UpdateDrag();
            }
        }

        // UP
        if (mouse.leftButton.wasReleasedThisFrame)
        {
            // se NON abbiamo dragato -> chiudi card
            if (pressed != null && !longFired)
            {
                bb.CardView?.Hide();
                Debug.Log($"[UP] (CARD HIDE) {pressed.displayName}#{pressed.instanceId}");
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
        UpdateDrag(); // così snap immediato

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
