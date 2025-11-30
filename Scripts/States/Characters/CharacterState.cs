using Godot;
using CrossDimensions.Characters;
using System.ComponentModel.DataAnnotations;

namespace CrossDimensions.States.Characters;

public partial class CharacterState : State
{
    public Character CharacterContext { get; private set; }

    public override Node Context
    {
        get => base.Context;
        set
        {
            base.Context = value;
            CharacterContext = value as Character;
        }
    }

    /// <summary>
    /// Handles common movement calculations to set character velocity from
    /// movement inputs.
    /// </summary>
    protected void ApplyGravity(double delta)
    {
        Vector2 gravity = ProjectSettings
            .GetSetting("physics/2d/default_gravity_vector")
            .AsVector2();
        gravity *= ProjectSettings
            .GetSetting("physics/2d/default_gravity")
            .AsSingle();

        // apply gravity to the character
        CharacterContext.VelocityFromExternalForces += gravity * (float)delta;
    }

    protected bool PerformJump()
    {
        float jumpHolder = Time.GetTicksMsec() - CharacterContext.JumpHeldTime ;
        if ( ( CharacterContext.Controller.IsJumpReleased || 
                ( jumpHolder > (CharacterContext.JumpHoldTime * 1000) ) ) &&
                ( !CharacterContext.IsOnFloor() ) )
        {
            // if jump released or timer exceeded, prevent jump until on ground
            CharacterContext.AllowJumpInput = false;
            GD.Print($"Jump time exceeded or key released, preventing jump input");
        }

        if (CharacterContext.AllowJumpInput) 
        {
            if (CharacterContext.Controller.IsJumping )
            {
                //on the first frame that the jump is held for, set the timer
                CharacterContext.JumpHeldTime = Time.GetTicksMsec();
                GD.Print($"Jump time set to {CharacterContext.JumpHeldTime}");
            }

            if (CharacterContext.Controller.IsJumpHeld) {
                // perform jump
                Vector2 velocity = CharacterContext.VelocityFromExternalForces;
                velocity.Y = -CharacterContext.JumpForce;
                CharacterContext.VelocityFromExternalForces = velocity;
                GD.Print($"Jump time: {jumpHolder} ");
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Applies friction to the character's horizontal velocity.
    /// </summary>
    /// <param name="delta">The time delta since the last frame.</param>
    /// <param name="friction">The friction coefficient to apply.</param>
    protected virtual void ApplyFriction(double delta, float friction)
    {
        Vector2 oldVelocity = CharacterContext.VelocityFromExternalForces;
        float newX = Mathf.MoveToward(oldVelocity.X, 0f, friction * (float)delta);
        CharacterContext.VelocityFromExternalForces = new Vector2(newX, oldVelocity.Y);
    }

    /// <summary>
    /// Common movement application logic that sets the character's velocity
    /// based on movement input and external forces.
    /// </summary>
    protected void ApplyMovement(double delta)
    {
        Vector2 wishDir = CharacterContext.Controller.MovementInput;

        // get target velocity based on input
        float wishSpeed = Mathf.Sign(wishDir.X) * CharacterContext.Speed;

        float externalX = CharacterContext.VelocityFromExternalForces.X;

        // get remaining needed velocity that is not covered by external forces
        float speedToAdd = wishSpeed - externalX;

        // if external forces already cover or exceed target velocity, zero out remaining
        if (Mathf.Sign(speedToAdd) != Mathf.Sign(wishSpeed) ||
            Mathf.Abs(externalX) >= Mathf.Abs(wishSpeed))
        {
            speedToAdd = 0f;
        }

        // Apply the computed velocity from input
        CharacterContext.VelocityFromInput = new Vector2(speedToAdd, 0);

        // Combine velocities
        CharacterContext.Velocity = CharacterContext.VelocityFromInput
            + CharacterContext.VelocityFromExternalForces;
    }

    /// <summary>
    /// Recalculates the external velocity by getting the change in total velocity,
    /// which is often modified by collision responses, and distributing that change
    /// proportionally between input and external forces. Note that this only adjusts
    /// the X component of the external velocity; the Y component is taken directly
    /// from the total velocity. This is because the input velocity only affects horizontal
    /// movement.
    /// </summary>
    protected void RecalculateExternalVelocity()
    {
        Vector2 inputVelocity = CharacterContext.VelocityFromInput;
        Vector2 externalVelocity = CharacterContext.VelocityFromExternalForces;
        Vector2 initialVelocity = inputVelocity + externalVelocity;
        Vector2 deltaVelocity = CharacterContext.Velocity - initialVelocity;

        float absSum = Mathf.Abs(externalVelocity.X) +
            Mathf.Abs(inputVelocity.X);

        // split delta velocity proportionally between input and external
        // v_input + v_external = v_total = v_init
        // delta v_external = v_external / v_init * delta v

        if (!Mathf.IsZeroApprox(absSum))
        {
            float weightInput;

            // if direction of delta matches external, give full weight to input
            // this prevents external forces being increased when input is
            // opposing them
            if (Mathf.Sign(deltaVelocity.X) == Mathf.Sign(externalVelocity.X))
            {
                weightInput = 1;
            }
            else
            {
                weightInput = Mathf.Abs(inputVelocity.X) / absSum;
            }
            float weightExternal = 1 - weightInput;

            externalVelocity.X += deltaVelocity.X * weightExternal;
        }

        externalVelocity.Y = CharacterContext.Velocity.Y;

        CharacterContext.VelocityFromExternalForces = externalVelocity;
    }
}
