using CrossedDimensions.Characters.Controllers;
using Godot;

namespace CrossedDimensions.Tests.Controllers;

[Collection("GodotHeadless")]
public class VirtualCursorTest(GodotHeadlessFixedFpsFixture godot)
{
    [Fact]
    public void Combat_ProducesAbsoluteOffset_IndependentOfDelta()
    {
        var cursor = new VirtualCursor { CombatRadius = 100f };

        cursor.Process(new Vector2(0.5f, 0f), 0.016f);
        cursor.Offset.IsEqualApprox(new Vector2(50f, 0f)).ShouldBeTrue();

        // Absolute mapping must not depend on frame time.
        cursor.Process(new Vector2(0.5f, 0f), 999f);
        cursor.Offset.IsEqualApprox(new Vector2(50f, 0f)).ShouldBeTrue();
    }

    [Fact]
    public void Combat_IsStateless_ReflectsLatestStick()
    {
        var cursor = new VirtualCursor { CombatRadius = 100f };

        cursor.Process(new Vector2(1f, 0f), 0.1f);
        cursor.Process(new Vector2(0f, 1f), 0.1f);

        cursor.Offset.IsEqualApprox(new Vector2(0f, 100f)).ShouldBeTrue();
    }

    [Fact]
    public void Combat_NeutralStick_ReturnsToAnchor()
    {
        var cursor = new VirtualCursor { CombatRadius = 100f };

        cursor.Process(new Vector2(1f, 0f), 0.1f);
        cursor.Process(Vector2.Zero, 0.1f);

        cursor.Offset.IsZeroApprox().ShouldBeTrue();
    }

    [Fact]
    public void Precision_IntegratesVelocityOverTime()
    {
        var cursor = new VirtualCursor { PrecisionSpeed = 200f, MaxRange = 1000f };
        cursor.ToggleMode();

        cursor.Process(new Vector2(1f, 0f), 0.5f); // +100
        cursor.Offset.IsEqualApprox(new Vector2(100f, 0f)).ShouldBeTrue();

        cursor.Process(new Vector2(1f, 0f), 0.5f); // +100 again, accumulates
        cursor.Offset.IsEqualApprox(new Vector2(200f, 0f)).ShouldBeTrue();
    }

    [Fact]
    public void Precision_NeutralStick_HoldsPosition()
    {
        var cursor = new VirtualCursor { PrecisionSpeed = 200f, MaxRange = 1000f };
        cursor.ToggleMode();

        cursor.Process(new Vector2(1f, 0f), 0.5f);
        Vector2 held = cursor.Offset;

        cursor.Process(Vector2.Zero, 5f); // long frame, still neutral
        cursor.Offset.IsEqualApprox(held).ShouldBeTrue();
    }

    [Fact]
    public void Precision_ClampsToMaxRange_PreservingDirection()
    {
        var cursor = new VirtualCursor { PrecisionSpeed = 1000f, MaxRange = 50f };
        cursor.ToggleMode();

        cursor.Process(new Vector2(1f, 0f), 1f); // wants 1000, clamped to 50

        cursor.Offset.IsEqualApprox(new Vector2(50f, 0f)).ShouldBeTrue();
    }

    [Fact]
    public void EnterPrecision_SeedsFromCurrentOffset_NoJump()
    {
        var cursor = new VirtualCursor { CombatRadius = 100f, PrecisionSpeed = 200f };

        cursor.Process(new Vector2(0.5f, 0f), 0.1f); // combat -> (50, 0)
        cursor.ToggleMode();                        // enter precision

        // Seeded, not reset: a neutral first precision frame holds the seed.
        cursor.Process(Vector2.Zero, 0.1f);
        cursor.Offset.IsEqualApprox(new Vector2(50f, 0f)).ShouldBeTrue();
    }

    [Fact]
    public void ExitPrecision_ReclaimsAbsoluteControl()
    {
        var cursor = new VirtualCursor { CombatRadius = 100f, PrecisionSpeed = 200f };
        cursor.ToggleMode(); // precision
        cursor.Process(new Vector2(1f, 0f), 0.25f); // (50, 0)

        cursor.ToggleMode(); // back to combat
        cursor.Process(new Vector2(0f, 1f), 0.1f);

        cursor.Offset.IsEqualApprox(new Vector2(0f, 100f)).ShouldBeTrue();
    }

    [Theory]
    [InlineData(3f, 4f, 10f, 3f, 4f)]   // length 5, within range -> unchanged
    [InlineData(3f, 4f, 5f, 3f, 4f)]    // length 5 == max -> unchanged (boundary)
    [InlineData(6f, 8f, 5f, 3f, 4f)]    // length 10 -> scaled to 5, same direction
    public void ClampToRange_LimitsMagnitude(
        float x, float y, float maxRange, float expectedX, float expectedY)
    {
        Vector2 result = VirtualCursor.ClampToRange(new Vector2(x, y), maxRange);

        result.IsEqualApprox(new Vector2(expectedX, expectedY)).ShouldBeTrue();
    }

    [Fact]
    public void AimVector_PlayerAndClone_ConvergeOnSameWorldPoint()
    {
        // The load-bearing constraint: bodies at different positions must aim at
        // the SAME world point, so split fire converges like it does at the mouse.
        var cursorWorld = new Vector2(120f, -30f);
        var player = new Vector2(100f, 100f);
        var clone = new Vector2(300f, 50f);

        Vector2 playerAim = VirtualCursor.AimVector(cursorWorld, player);
        Vector2 cloneAim = VirtualCursor.AimVector(cursorWorld, clone);

        (player + playerAim).IsEqualApprox(cursorWorld).ShouldBeTrue();
        (clone + cloneAim).IsEqualApprox(cursorWorld).ShouldBeTrue();
        (player + playerAim).IsEqualApprox(clone + cloneAim).ShouldBeTrue();
    }

    [Fact]
    public void CursorWorld_IsAnchorPlusOffset()
    {
        var cursor = new VirtualCursor { CombatRadius = 100f };
        cursor.Process(new Vector2(0.5f, 0f), 0.1f); // offset (50, 0)

        cursor.CursorWorld(new Vector2(10f, 10f))
            .IsEqualApprox(new Vector2(60f, 10f)).ShouldBeTrue();
    }
}
