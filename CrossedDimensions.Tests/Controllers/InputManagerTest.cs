using CrossedDimensions.Characters.Controllers;
using Godot;

namespace CrossedDimensions.Tests.Controllers;

[Collection("GodotHeadless")]
public class InputManagerTest(GodotHeadlessFixedFpsFixture godot)
{
    [Fact]
    public void DetectDevice_KeyEvent_ReturnsKeyboardMouse()
    {
        var @event = new InputEventKey { Keycode = Key.A, Pressed = true };

        InputManager.DetectDevice(@event).ShouldBe(InputDevice.KeyboardMouse);
    }

    [Fact]
    public void DetectDevice_MouseButton_ReturnsKeyboardMouse()
    {
        var @event = new InputEventMouseButton
        {
            ButtonIndex = MouseButton.Left,
            Pressed = true,
        };

        InputManager.DetectDevice(@event).ShouldBe(InputDevice.KeyboardMouse);
    }

    [Fact]
    public void DetectDevice_MouseMotionWithMovement_ReturnsKeyboardMouse()
    {
        var @event = new InputEventMouseMotion { Relative = new Vector2(1f, 0f) };

        InputManager.DetectDevice(@event).ShouldBe(InputDevice.KeyboardMouse);
    }

    [Fact]
    public void DetectDevice_MouseMotionWithoutMovement_ReturnsNull()
    {
        // Phantom zero-delta motion (focus changes, rumble) must not steal the device.
        var @event = new InputEventMouseMotion { Relative = Vector2.Zero };

        InputManager.DetectDevice(@event).ShouldBeNull();
    }

    [Fact]
    public void DetectDevice_JoypadButtonPressed_ReturnsController()
    {
        var @event = new InputEventJoypadButton
        {
            ButtonIndex = JoyButton.A,
            Pressed = true,
        };

        InputManager.DetectDevice(@event).ShouldBe(InputDevice.Controller);
    }

    [Fact]
    public void DetectDevice_JoypadButtonReleased_ReturnsNull()
    {
        var @event = new InputEventJoypadButton
        {
            ButtonIndex = JoyButton.A,
            Pressed = false,
        };

        InputManager.DetectDevice(@event).ShouldBeNull();
    }

    [Theory]
    [InlineData(0.6f, true)]   // clearly past threshold
    [InlineData(-0.6f, true)]  // magnitude matters, not sign
    [InlineData(0.5f, true)]   // boundary is inclusive (>=)
    [InlineData(0.3f, false)]  // sub-threshold drift is ignored
    [InlineData(0.0f, false)]  // resting stick is ignored
    public void DetectDevice_JoypadMotion_RespectsThreshold(
        float axisValue,
        bool expectController)
    {
        var @event = new InputEventJoypadMotion
        {
            Axis = JoyAxis.RightX,
            AxisValue = axisValue,
        };

        InputDevice? result = InputManager.DetectDevice(@event);

        if (expectController)
        {
            result.ShouldBe(InputDevice.Controller);
        }
        else
        {
            result.ShouldBeNull();
        }
    }
}
