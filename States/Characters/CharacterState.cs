using Godot;
using CrossedDimensions.Characters;

namespace CrossedDimensions.States.Characters;

public partial class CharacterState : State
{
    public Character CharacterContext { get; private set; }

    private float gravity_k = 4.0f;

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

        //apply gravity boost if the player recently released a jump
        if (CharacterContext.JumpGravBoostTime > 0)
        {
            float t_left = CharacterContext.JumpGravBoostTime
                - (Time.GetTicksMsec() - CharacterContext.JumpReleasedAtTime);
            if (t_left < 0 || CharacterContext.IsOnFloor())
            {
                CharacterContext.JumpGravBoostTime = 0;
                CharacterContext.JumpHeldAtTime = 0;
            }
            else
            {
                gravity *= gravity_k;
            }

        }

        // apply gravity to the character
        CharacterContext.VelocityFromExternalForces += gravity * (float)delta;
    }

    protected bool PerformJump()
    {
        Vector2 gravity_base = ProjectSettings
            .GetSetting("physics/2d/default_gravity_vector")
            .AsVector2();
        gravity_base *= ProjectSettings
            .GetSetting("physics/2d/default_gravity")
            .AsSingle();

        float now = Time.GetTicksMsec();
        bool onFloor = CharacterContext.IsOnFloor();
        float jumpTimeMs = Mathf.Sqrt(2 * CharacterContext.JumpHeight
            / gravity_base.Length()) * 1000.0f;

        if (onFloor)
        {
            CharacterContext.AllowJumpInput = true;
            CharacterContext.JumpSustainActive = false;
            CharacterContext.JumpGravBoostTime = 0;
            CharacterContext.CoyoteJumpExpiresAtTime = now + CharacterContext.CoyoteTimeMs;
        }

        if (CharacterContext.JumpSustainActive && !onFloor)
        {
            float heldMs = now - CharacterContext.JumpHeldAtTime;
            bool timedOut = heldMs >= jumpTimeMs;
            if (CharacterContext.Controller.IsJumpReleased || timedOut)
            {
                CharacterContext.AllowJumpInput = false;
                CharacterContext.JumpSustainActive = false;
                CharacterContext.JumpReleasedAtTime = now;
                float remainingMs = Mathf.Max(0, jumpTimeMs - heldMs);
                CharacterContext.JumpGravBoostTime = remainingMs / gravity_k;
            }
        }

        if (!CharacterContext.Controller.IsJumping)
        {
            return false;
        }

        bool canGroundJump = onFloor || now <= CharacterContext.CoyoteJumpExpiresAtTime;
        bool canCrystalJump = !onFloor && CharacterContext.AllowMidAirJump;

        if (!canGroundJump && !canCrystalJump)
        {
            return false;
        }

        if (canCrystalJump)
        {
            CharacterContext.AllowMidAirJump = false;
        }

        CharacterContext.AllowJumpInput = true;
        CharacterContext.JumpSustainActive = true;
        CharacterContext.JumpHeldAtTime = now;
        CharacterContext.CoyoteJumpExpiresAtTime = 0;

        // set initial velocity
        float initialVelocity = Mathf.Sqrt(2 * gravity_base.Length()
            * CharacterContext.JumpHeight);

        Vector2 velocity = CharacterContext.VelocityFromExternalForces;
        velocity.Y = -initialVelocity;
        CharacterContext.VelocityFromExternalForces = velocity;
        return true;
    }

    protected bool ApplyUpwardCornerCorrection()
    {
        if (!CharacterContext.EnableCornerCorrection)
        {
            return false;
        }

        if (CharacterContext.IsOnFloor())
        {
            return false;
        }

        if (CharacterContext.VelocityFromExternalForces.Y >= 0)
        {
            return false;
        }

        if (!CharacterContext.IsOnCeiling())
        {
            return false;
        }

        int maxPixels = Mathf.Max(0, CharacterContext.CornerCorrectionMaxPixels);
        int step = Mathf.Max(1, CharacterContext.CornerCorrectionStepPixels);

        bool TryOffset(int offset)
        {
            Vector2 correction = new Vector2(offset, 0);
            Transform2D shifted = CharacterContext.GlobalTransform.Translated(correction);
            if (CharacterContext.TestMove(shifted, Vector2.Zero))
            {
                return false;
            }

            Vector2 upwardProbe = new Vector2(0, -Mathf.Max(1, step));
            if (CharacterContext.TestMove(shifted, upwardProbe))
            {
                return false;
            }

            CharacterContext.GlobalPosition += correction;
            return true;
        }

        int inputDirection = (int)Mathf.Sign(CharacterContext.Controller.MovementInput.X);
        if (inputDirection != 0)
        {
            for (int offset = step; offset <= maxPixels; offset += step)
            {
                if (TryOffset(inputDirection * offset))
                {
                    return true;
                }
            }

            return false;
        }

        int velocityDirection = (int)Mathf.Sign(CharacterContext.Velocity.X);
        if (velocityDirection != 0)
        {
            for (int offset = step; offset <= maxPixels; offset += step)
            {
                if (TryOffset(velocityDirection * offset))
                {
                    return true;
                }
            }

            return false;
        }

        int targetDirection = (int)Mathf.Sign(CharacterContext.Controller.Target.X);
        if (targetDirection == 0)
        {
            targetDirection = 1;
        }

        for (int offset = step; offset <= maxPixels; offset += step)
        {
            if (TryOffset(targetDirection * offset))
            {
                return true;
            }

            if (TryOffset(-targetDirection * offset))
            {
                return true;
            }
        }

        return false;
    }

    protected void RestoreUpwardVelocityAfterCornerCorrection(
        bool corrected,
        float preCollisionVerticalVelocity)
    {
        if (!corrected)
        {
            return;
        }

        if (preCollisionVerticalVelocity >= 0)
        {
            return;
        }

        Vector2 velocity = CharacterContext.Velocity;
        if (velocity.Y >= 0)
        {
            velocity.Y = preCollisionVerticalVelocity;
            CharacterContext.Velocity = velocity;
        }
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
