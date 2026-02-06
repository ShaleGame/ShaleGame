using Godot;

namespace CrossedDimensions.Characters.Controllers;

[GlobalClass]
public sealed partial class EnemyController : CharacterController
{
    [Export]
    public Entities.Enemies.EnemyComponent EnemyComponent { get; set; }

    private Vector2 _movementInput = Vector2.Zero;
    private Vector2 _targetInput = Vector2.Zero;
    private bool _isJumping;
    private bool _isJumpHeld;
    private bool _isMouse1Held;

    public override Vector2 MovementInput => _movementInput * XScale;

    public override Vector2 Target => _targetInput;

    public override bool IsJumping => _isJumping;

    public override bool IsJumpHeld => _isJumpHeld;

    public override bool IsJumpReleased => false;

    public override bool IsMouse1Held => _isMouse1Held;

    public override bool IsMouse2Held => false;

    public override bool IsSplitting => false;

    public void SetMovementInput(Vector2 movement)
    {
        _movementInput = movement;
    }

    public void SetTargetInput(Vector2 target)
    {
        _targetInput = target;
    }

    public void SetJumping(bool isJumping, bool isHeld)
    {
        _isJumping = isJumping;
        _isJumpHeld = isHeld;
    }

    public void SetPrimaryAttack(bool isHeld)
    {
        _isMouse1Held = isHeld;
    }
}
