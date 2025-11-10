using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace _Workspace._Scripts.Selections
{
    public class ScSharkSelectionInput : MonoBehaviour
    { 
        
        [SerializeField] private ScSharkSelectionController controller;
    [Header("Swipe")]
    [SerializeField] private float minSwipeDistance = 80f;
    [SerializeField] private float maxSwipeTime = 0.7f;
    [SerializeField] private float maxVerticalDrift = 60f;
    [SerializeField] private float cooldown = 0.2f;

    private InputSystem_Actions _actions;
    private InputAction _press;
    private InputAction _pos;

    private bool _pressed, _startedOverUI;
    private Vector2 _startPos;
    private float _startTime, _cdTimer;

    private void OnEnable()
    {
        _actions ??= new InputSystem_Actions();
        _press = _actions.SkinSelect.Press;
        _pos   = _actions.SkinSelect.Position;

        _press.started  += OnPressStarted;
        _press.canceled += OnPressCanceled;

        _actions.SkinSelect.Enable();
    }

    private void OnDisable()
    {
        if (_actions == null) return;
        _press.started  -= OnPressStarted;
        _press.canceled -= OnPressCanceled;
        _actions.SkinSelect.Disable();
    }

    private void Update() { _cdTimer -= Time.unscaledDeltaTime; }

    private void OnPressStarted(InputAction.CallbackContext _)
    {
        _pressed = true;
        _startPos = _pos.ReadValue<Vector2>();
        _startTime = Time.unscaledTime;
        _startedOverUI = IsPointerOverUI();
    }

    private void OnPressCanceled(InputAction.CallbackContext _)
    {
        if (!_pressed) return;
        _pressed = false;
        if (_cdTimer > 0f) return;
        if (_startedOverUI) return;

        var endPos = _pos.ReadValue<Vector2>();
        var dt = Time.unscaledTime - _startTime;
        var delta = endPos - _startPos;

        if (dt <= maxSwipeTime &&
            Mathf.Abs(delta.x) >= minSwipeDistance &&
            Mathf.Abs(delta.y) <= maxVerticalDrift)
        {
            if (delta.x > 0) controller.Prev();
            else             controller.Next();
            _cdTimer = cooldown;
        }
    }

    private static bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
#if UNITY_EDITOR || UNITY_STANDALONE
        return EventSystem.current.IsPointerOverGameObject();
#else
        if (Touchscreen.current != null)
        {
            foreach (var t in Touchscreen.current.touches)
            {
                int id = t.touchId.ReadValue();
                if (EventSystem.current.IsPointerOverGameObject(id)) return true;
            }
        }
        return false;
#endif
    }
    }
}
