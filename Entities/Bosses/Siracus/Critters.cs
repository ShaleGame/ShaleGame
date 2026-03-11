using Godot;
using System;
using CrossedDimensions.States;
using CrossedDimensions.Characters;

namespace CrossedDimensions.Entities.Bosses.Siracus;

// Spits out miniature critters to run to the player and then bury

public partial class Critters : State
{
    [Export]
    public PackedScene critter;

    [Export]
    public int maxCritters = 6;

    [Export]
    public Node2D critterSpawnPoint;

    private int curCritters = 0;

    private double waitTimeMax = 0.5f;
    private double curTime = 0.0f;

    private AnimatedSprite2D _animSprite;

    private Character _siracus;

    private RandomNumberGenerator rng;

    public override State Enter(State previousState)
    {
        _siracus = Context as Character;

        _animSprite = _siracus.FindChild("AnimatedSprite2D") as AnimatedSprite2D;
        
        _animSprite.Play("Shotgun");

        curCritters = 0;
        curTime = 0.0f;

        rng = new RandomNumberGenerator();

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        curTime += delta;

        if (curTime >= waitTimeMax)
        {
            var critterInstance = critter.Instantiate() as Character;

            critterInstance.GlobalPosition = critterSpawnPoint.GlobalPosition;

            var randY = rng.RandiRange(-10, 10);

            critterInstance.Velocity = new Vector2(50, randY);
            critterInstance.MoveAndSlide();

            curCritters++;

            if (curCritters >= maxCritters)
            {
                var stateMachine = GetParent() as StateMachine;

                State attackIdle = stateMachine.FindChild("Idle") as State;

                stateMachine.ChangeState(attackIdle);
            }
        }

        return base.Process(delta);
    }
}
