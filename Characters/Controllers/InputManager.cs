using Godot;
using CrossedDimensions.UI.UISettings;

namespace CrossedDimensions.Characters.Controllers;

/// <summary>
/// The physical input device the player is currently using. Consumed by aiming
/// (mouse vs joypad), the reticle/cursor, and UI glyph selection.
/// </summary>
public enum InputDevice
{
    KeyboardMouse,
    Controller,
}

/// <summary>
/// How <see cref="InputManager.Device"/> is resolved. Persisted in settings.
/// </summary>
public enum InputDeviceMode
{
    Auto,
    ForceKeyboardMouse,
    ForceController,
}

/// <summary>
/// Autoload that tracks which input device is currently active. In
/// <see cref="InputDeviceMode.Auto"/> the device follows the most recent
/// physical input; the force modes pin it. Emits <see cref="DeviceChanged"/> so
/// aiming, the cursor/reticle, and UI glyphs can react to a swap.
/// </summary>
/// <remarks>
/// This owns only volatile per-frame device state. Binding storage and
/// preferences live in <see cref="SettingsManager"/>; the virtual aim cursor
/// lives in the aim layer, which only reads <see cref="Device"/> from here.
/// </remarks>
[GlobalClass]
public partial class InputManager : Node
{
    /// <summary>
    /// Emitted when the resolved active device changes. The argument is an
    /// <see cref="InputDevice"/> value, passed as <see cref="int"/> for GDScript
    /// interop.
    /// </summary>
    [Signal]
    public delegate void DeviceChangedEventHandler(int device);

    /// <summary>
    /// Minimum absolute joypad axis value that counts as intentional input.
    /// Higher than the in-game move deadzone (0.2) so a resting or drifting
    /// stick cannot steal the active device away from keyboard/mouse.
    /// </summary>
    private const float JoypadActivationThreshold = 0.5f;

    private InputDevice _device = InputDevice.KeyboardMouse;
    private InputDeviceMode _mode = InputDeviceMode.Auto;

    /// <summary>The currently active input device.</summary>
    public InputDevice Device => _device;

    /// <summary>
    /// How the active device is resolved. Assigning re-resolves immediately, so
    /// switching to a force mode takes effect on the next frame.
    /// </summary>
    public InputDeviceMode Mode
    {
        get => _mode;
        set
        {
            _mode = value;
            if (ForcedDevice() is InputDevice device)
            {
                SetDevice(device);
            }
        }
    }

    public override void _Ready()
    {
        // Keep detecting while the tree is paused (e.g. in menus).
        ProcessMode = ProcessModeEnum.Always;
        _mode = LoadPersistedMode();
        _device = ForcedDevice() ?? _device;
        ApplyCursorVisibility();
    }

    public override void _Input(InputEvent @event)
    {
        // Force modes ignore auto-detection entirely.
        if (_mode != InputDeviceMode.Auto)
        {
            return;
        }

        if (DetectDevice(@event) is InputDevice device)
        {
            SetDevice(device);
        }
    }

    /// <summary>
    /// Classifies which device an event came from, or <c>null</c> when the event
    /// is not a meaningful device signal (e.g. a sub-threshold stick drift or a
    /// zero-delta mouse motion).
    /// </summary>
    public static InputDevice? DetectDevice(InputEvent @event)
    {
        switch (@event)
        {
            case InputEventKey:
            case InputEventMouseButton:
                return InputDevice.KeyboardMouse;
            case InputEventMouseMotion motion when !motion.Relative.IsZeroApprox():
                return InputDevice.KeyboardMouse;
            case InputEventJoypadButton { Pressed: true }:
                return InputDevice.Controller;
            case InputEventJoypadMotion motion
                when Mathf.Abs(motion.AxisValue) >= JoypadActivationThreshold:
                return InputDevice.Controller;
            default:
                return null;
        }
    }

    private InputDevice? ForcedDevice() => _mode switch
    {
        InputDeviceMode.ForceKeyboardMouse => InputDevice.KeyboardMouse,
        InputDeviceMode.ForceController => InputDevice.Controller,
        _ => null,
    };

    private void SetDevice(InputDevice device)
    {
        if (device == _device)
        {
            return;
        }

        _device = device;
        ApplyCursorVisibility();
        EmitSignal(SignalName.DeviceChanged, (int)_device);
    }

    private void ApplyCursorVisibility()
    {
        Input.MouseMode = _device == InputDevice.Controller
            ? Input.MouseModeEnum.Hidden
            : Input.MouseModeEnum.Visible;
    }

    private InputDeviceMode LoadPersistedMode()
    {
        var settings = GetNodeOrNull<SettingsManager>("/root/SettingsManager");
        if (settings?.Current is null)
        {
            return InputDeviceMode.Auto;
        }

        return (InputDeviceMode)settings.Current.DeviceMode;
    }
}
