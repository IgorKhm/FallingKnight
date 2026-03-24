using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Keyboard / gamepad menu for the boot menu: moves a finger image to the selected TMP label and handles confirm.
/// Label positions and rotations are owned by each TMP object's Rect Transform — this script only reads them.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [System.Serializable]
    public class MainMenuRow
    {
        [Tooltip("TMP label text component.")]
        public TMP_Text label;

        [Tooltip("Usually the same GameObject as Label. Assign to tweak Anchored Position X/Y and rotation on this row straight from the Main Menu Controller component.")]
        public RectTransform layoutRect;

        [Tooltip("Extra offset in the label's local space (typical: nudge finger left/right of the text).")]
        public Vector3 pointerLocalOffset;

        [Tooltip("World-space Euler angles for the finger when this row is selected.")]
        public Vector3 pointerWorldEulerAngles;
    }

    [Header("Refs")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private RectTransform pointingFinger;

    [Header("Rows (Play = first / typically above, Quit = second / typically below)")]
    [SerializeField] private MainMenuRow playRow;
    [SerializeField] private MainMenuRow quitRow;

    [Header("Pointer (global tweak)")]
    [Tooltip("Added in world space after the per-row offset is applied.")]
    [SerializeField] private Vector3 pointerWorldOffset;

    [Header("Pointer Animation")]
    [Tooltip("How far the finger bobs toward the text (pixels).")]
    [SerializeField] private float bobAmplitude = 15f;
    [Tooltip("Full back-and-forth cycles per second.")]
    [SerializeField] private float bobFrequency = 2f;

    [Header("Navigation")]
    [SerializeField] private float stickNavigateThreshold = 0.55f;

    private int _index;
    private float _lastStickY;

    private void OnEnable()
    {
        _index = 0;
        _lastStickY = ReadStickY();
        RefreshPointerTransform();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying)
            return;
        RefreshPointerTransform();
    }
#endif

    private void Update()
    {
        if (gameManager == null || pointingFinger == null)
            return;

        if (gameManager.GameState != GameState.BootMenu)
            return;

        TryNavigate();
        RefreshPointerTransform();

        if (TrySubmit())
            ActivateSelection();

        _lastStickY = ReadStickY();
    }

    /// <summary>
    /// Play is index 0 (upper), Quit is index 1 (lower). Up moves toward Play; Down moves toward Quit; wraps at ends.
    /// </summary>
    private bool TryNavigate()
    {
        bool up = IsUpEdge();
        bool down = IsDownEdge();

        if (up == down)
            return false;

        if (up)
            _index = (_index - 1 + 2) % 2;
        else
            _index = (_index + 1) % 2;

        return true;
    }

    private bool IsUpEdge()
    {
        var kb = Keyboard.current;
        if (kb != null && (kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame))
            return true;

        var pad = Gamepad.current;
        if (pad != null)
        {
            if (pad.dpad.up.wasPressedThisFrame)
                return true;
            float y = pad.leftStick.ReadValue().y;
            if (y > stickNavigateThreshold && _lastStickY <= stickNavigateThreshold)
                return true;
        }

        return false;
    }

    private bool IsDownEdge()
    {
        var kb = Keyboard.current;
        if (kb != null && (kb.sKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame))
            return true;

        var pad = Gamepad.current;
        if (pad != null)
        {
            if (pad.dpad.down.wasPressedThisFrame)
                return true;
            float y = pad.leftStick.ReadValue().y;
            if (y < -stickNavigateThreshold && _lastStickY >= -stickNavigateThreshold)
                return true;
        }

        return false;
    }

    private static float ReadStickY()
    {
        var pad = Gamepad.current;
        return pad != null ? pad.leftStick.ReadValue().y : 0f;
    }

    private bool TrySubmit()
    {
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame)
                return true;
        }

        var pad = Gamepad.current;
        if (pad != null && pad.buttonSouth.wasPressedThisFrame)
            return true;

        return false;
    }

    private void ActivateSelection()
    {
        if (_index == 0)
            gameManager.StartGame();
        else
            gameManager.QuitGame();
    }

    private void RefreshPointerTransform()
    {
        MainMenuRow row = _index == 0 ? playRow : quitRow;
        RectTransform labelRt = RowRect(row);
        if (labelRt == null)
            return;

        float bob = Mathf.Sin(Time.unscaledTime * bobFrequency * Mathf.PI * 2f) * bobAmplitude;
        Vector3 bobOffset = labelRt.TransformDirection(Vector3.right) * bob;
        Vector3 world = labelRt.TransformPoint(row.pointerLocalOffset) + pointerWorldOffset + bobOffset;
        pointingFinger.SetPositionAndRotation(world, Quaternion.Euler(row.pointerWorldEulerAngles));
    }

    private static RectTransform RowRect(MainMenuRow row)
    {
        if (row.layoutRect != null)
            return row.layoutRect;
        return row.label != null ? row.label.rectTransform : null;
    }
}
