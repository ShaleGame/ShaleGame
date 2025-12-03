using Godot;
using CrossDimensions.Characters;

namespace CrossDimensions.States.Characters;

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
        if (CharacterContext.JumpGravBoostTime > 0) {
            float t_left = CharacterContext.JumpGravBoostTime 
                - (Time.GetTicksMsec() - CharacterContext.JumpReleasedAtTime);
            if (t_left < 0 || CharacterContext.IsOnFloor())
            {
                CharacterContext.JumpGravBoostTime = 0;
                CharacterContext.JumpHeldAtTime = 0;
                GD.Print($"Gravity boost expired");
            } else
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
        float jumpTime = Mathf.Sqrt(2 * CharacterContext.JumpHeight
            / gravity_base.Length());
        float t_left = (jumpTime * 1000) 
            - (Time.GetTicksMsec() - CharacterContext.JumpHeldAtTime);
    
        // if jump released or timer exceeded, prevent jump until on ground
        if ( ( CharacterContext.Controller.IsJumpReleased || t_left < 0.0 )
                && !CharacterContext.IsOnFloor()
                && CharacterContext.AllowJumpInput )
        {
            CharacterContext.AllowJumpInput = false;
            CharacterContext.JumpReleasedAtTime = Time.GetTicksMsec();
            CharacterContext.JumpGravBoostTime = t_left / gravity_k;
            if (CharacterContext.JumpGravBoostTime > 0)
            {
                GD.Print($"Gravity boost time: {CharacterContext.JumpGravBoostTime}");
            } else
            {
                CharacterContext.JumpGravBoostTime = 0;
            }
        }

        if (CharacterContext.AllowJumpInput) 
        { 
            if (CharacterContext.Controller.IsJumping )
            {
                //on the first frame that the jump is held for, set the timer
                CharacterContext.JumpHeldAtTime = Time.GetTicksMsec();

                //set initial velocity
                float initialVelocity = Mathf.Sqrt(2 * gravity_base.Length()
                    * CharacterContext.JumpHeight);

                Vector2 velocity = CharacterContext.VelocityFromExternalForces;
                velocity.Y = -initialVelocity;
                CharacterContext.VelocityFromExternalForces = velocity;
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
