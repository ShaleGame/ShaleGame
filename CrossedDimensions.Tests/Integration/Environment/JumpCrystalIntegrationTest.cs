using CrossedDimensions.Characters;
using CrossedDimensions.Environment.Triggers;
using Godot;

namespace CrossedDimensions.Tests.Integration.Environment;

[Collection("GodotHeadless")]
public sealed class JumpCrystalIntegrationTest : System.IDisposable
{
    private const string CharacterScenePath = "res://Characters/Character.tscn";

    private readonly GodotHeadlessFixedFpsFixture _godot;
    private readonly Node _sceneRoot;
    private readonly Character _player;
    private readonly JumpCrystal _crystal;

    public JumpCrystalIntegrationTest(GodotHeadlessFixedFpsFixture godot)
    {
        _godot = godot;
        _sceneRoot = new Node { Name = "jump_crystal_test_root" };
        _godot.Tree.Root.AddChild(_sceneRoot);

        _player = ResourceLoader
            .Load<PackedScene>(CharacterScenePath)
            .Instantiate<Character>();
        _sceneRoot.AddChild(_player);
        _player.Controller.IsActive = false;

        _crystal = new JumpCrystal
        {
            Name = "jump_crystal"
        };
        _sceneRoot.AddChild(_crystal);

        _godot.GodotInstance.Iteration(2);
    }

    public void Dispose()
    {
        _sceneRoot?.QueueFree();
    }

    [Fact]
    public void JumpCrystal_WhenOriginalPlayerEnters_SetsAllowJumpInputTrue()
    {
        _player.AllowJumpInput = false;
        _player.AllowMidAirJump = false;

        _crystal.EmitSignal(Area2D.SignalName.BodyEntered, _player);
        _godot.GodotInstance.Iteration(1);

        _player.AllowJumpInput.ShouldBeTrue();
        _player.AllowMidAirJump.ShouldBeTrue();
    }

    [Fact]
    public void JumpCrystal_WhenOriginalPlayerExitsAirborneUnused_RevokesAllowJumpInput()
    {
        _player.AllowJumpInput = false;
        _player.AllowMidAirJump = false;
        _player.IsOnFloor().ShouldBeFalse();

        _crystal.EmitSignal(Area2D.SignalName.BodyEntered, _player);
        _godot.GodotInstance.Iteration(1);
        _player.AllowJumpInput.ShouldBeTrue();
        _player.AllowMidAirJump.ShouldBeTrue();

        _crystal.EmitSignal(Area2D.SignalName.BodyExited, _player);

        _player.AllowJumpInput.ShouldBeFalse();
        _player.AllowMidAirJump.ShouldBeFalse();
    }

    [Fact]
    public void JumpCrystal_WhenOriginalPlayerExitsGrounded_LeavesAllowJumpInputTrue()
    {
        AddFloorAndSettlePlayer();
        _player.AllowJumpInput = false;
        _player.AllowMidAirJump = false;

        _crystal.EmitSignal(Area2D.SignalName.BodyEntered, _player);
        _godot.GodotInstance.Iteration(1);
        _player.AllowJumpInput.ShouldBeTrue();
        _player.AllowMidAirJump.ShouldBeTrue();

        _crystal.EmitSignal(Area2D.SignalName.BodyExited, _player);

        _player.AllowJumpInput.ShouldBeTrue();
        _player.AllowMidAirJump.ShouldBeTrue();
    }

    [Fact]
    public void JumpCrystal_WhenCloneEntersAndExits_DoesNotAffectOriginalAllowJumpInput()
    {
        _player.AllowJumpInput = false;
        _player.AllowMidAirJump = false;
        var clone = _player.Cloneable.Split();

        _crystal.EmitSignal(Area2D.SignalName.BodyEntered, clone);
        _crystal.EmitSignal(Area2D.SignalName.BodyExited, clone);

        _player.AllowJumpInput.ShouldBeFalse();
        _player.AllowMidAirJump.ShouldBeFalse();
    }

    [Fact]
    public void JumpCrystal_WhenOriginalPlayerReenters_RegrantsAllowJumpInput()
    {
        _player.AllowJumpInput = false;
        _player.AllowMidAirJump = false;

        _crystal.EmitSignal(Area2D.SignalName.BodyEntered, _player);
        _godot.GodotInstance.Iteration(1);
        _player.AllowJumpInput.ShouldBeTrue();
        _player.AllowMidAirJump.ShouldBeTrue();

        _crystal.EmitSignal(Area2D.SignalName.BodyExited, _player);
        _player.AllowJumpInput.ShouldBeFalse();
        _player.AllowMidAirJump.ShouldBeFalse();

        _crystal.EmitSignal(Area2D.SignalName.BodyEntered, _player);

        _player.AllowJumpInput.ShouldBeTrue();
        _player.AllowMidAirJump.ShouldBeTrue();
    }

    private void AddFloorAndSettlePlayer()
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
    }
}
