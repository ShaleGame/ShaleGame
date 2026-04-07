using CrossedDimensions.Characters;
using CrossedDimensions.Characters.Controllers;
using Godot;

namespace CrossedDimensions.Tests.Integration;

[Collection("GodotHeadless")]
public sealed partial class CharacterCoyoteJumpIntegrationTest : System.IDisposable
{
    private const string CharacterScenePath = "res://Characters/Character.tscn";

    private readonly GodotHeadlessFixedFpsFixture _godot;
    private readonly Node _sceneRoot;
    private readonly Character _player;
    private readonly TestController _controller;

    public CharacterCoyoteJumpIntegrationTest(GodotHeadlessFixedFpsFixture godot)
    {
        _godot = godot;
        _sceneRoot = new Node { Name = "character_coyote_jump_test_root" };
        _godot.Tree.Root.AddChild(_sceneRoot);

        _player = ResourceLoader
            .Load<PackedScene>(CharacterScenePath)
            .Instantiate<Character>();
        _sceneRoot.AddChild(_player);

        _controller = new TestController();
        _player.AddChild(_controller);
        _player.Controller = _controller;
        _controller.OwnerCharacter = _player;

        _player.CoyoteTimeMs = 100.0f;

        _godot.GodotInstance.Iteration(2);
    }

    public void Dispose()
    {
        _sceneRoot?.QueueFree();
    }

    [Fact]
    public void Character_WhenJumpPressedWithinCoyoteTime_PerformsJump()
    {
        var floor = AddFloorAndSettlePlayer();

        floor.QueueFree();
        _godot.GodotInstance.Iteration(1);
        _godot.GodotInstance.IterateUntil(() => !_player.IsOnFloor(), maxIterations: 120)
            .ShouldBeTrue();

        _controller.JumpPressed = true;
        _godot.GodotInstance.Iteration(1);
        _controller.JumpPressed = false;

        _player.VelocityFromExternalForces.Y.ShouldBeLessThan(0f);
    }

    [Fact]
    public void Character_WhenJumpPressedAfterCoyoteTime_DoesNotPerformJump()
    {
        var floor = AddFloorAndSettlePlayer();

        floor.QueueFree();
        _godot.GodotInstance.Iteration(1);
        _godot.GodotInstance.IterateUntil(() => !_player.IsOnFloor(), maxIterations: 120)
            .ShouldBeTrue();

        _godot.GodotInstance.Iteration(8);

        _controller.JumpPressed = true;
        _godot.GodotInstance.Iteration(1);
        _controller.JumpPressed = false;

        _player.VelocityFromExternalForces.Y.ShouldBeGreaterThanOrEqualTo(0f);
    }

    private StaticBody2D AddFloorAndSettlePlayer()
    {
        var floor = new StaticBody2D
        {
            Position = new Vector2(0, 24),
            CollisionLayer = 1,
            CollisionMask = 0
        };

        var shape = new CollisionShape2D
        {
            Shape = new RectangleShape2D { Size = new Vector2(400, 20) }
        };

        floor.AddChild(shape);
        _sceneRoot.AddChild(floor);

        _godot.GodotInstance.IterateUntil(() => _player.IsOnFloor(), maxIterations: 120)
            .ShouldBeTrue();

        return floor;
    }

    private sealed partial class TestController : CharacterController
    {
        public bool JumpPressed { get; set; }

        public override Vector2 MovementInput => Vector2.Zero;

        public override Vector2 Target => Vector2.Zero;

        public override bool IsJumping => JumpPressed;

        public override bool IsJumpHeld => JumpPressed;

        public override bool IsJumpReleased => false;

        public override bool IsMouse1Held => false;

        public override bool IsMouse2Held => false;

        public override bool IsSplitting => false;

        public override bool IsSplitReleased => false;

        public override bool IsSplitHeld => false;

        public override bool IsInteractHeld => false;
    }
}
