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
        GD.Print("Attacking");

        _siracus = Context as Character;
        if (_siracus == null) { GD.PrintErr("[Critters] Context is not a Character!"); return base.Enter(previousState); }

        _animSprite = _siracus.FindChild("AnimatedSprite2D") as AnimatedSprite2D;
        if (_animSprite == null) GD.PrintErr("[Critters] AnimatedSprite2D not found!");

        _animSprite.Play("Shotgun");

        if (critter == null) GD.PrintErr("[Critters] critter PackedScene is not assigned!");
        if (critterSpawnPoint == null) GD.PrintErr("[Critters] critterSpawnPoint is not assigned!");

        curCritters = 0;
        curTime = 0.0f;

        rng = new RandomNumberGenerator();

        GD.Print("[Critters] Ready. Will spawn ", maxCritters, " critters every ", waitTimeMax, "s from ", critterSpawnPoint?.GlobalPosition);

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        curTime += delta;

        if (curTime >= waitTimeMax)
        {
            curTime = 0;

            var critterInstance = critter.Instantiate() as Character;
            if (critterInstance == null) { GD.PrintErr("[Critters] Instantiated critter is not a Character!"); return base.Process(delta); }

            var randX = rng.RandiRange(150, 300);
            var arcHeight = -400f;
            critterInstance.Velocity = new Vector2(-randX, arcHeight);

            GetTree().CurrentScene.AddChild(critterInstance);

            critterInstance.GlobalPosition = critterSpawnPoint.GlobalPosition;

            GD.Print(_siracus.GlobalPosition);
            GD.Print(critterInstance.GlobalPosition);

            curCritters++;

            GD.Print("[Critters] Spawned critter ", curCritters, "/", maxCritters, " at ", critterInstance.GlobalPosition, " with velocity (", critterInstance.Velocity, ")");

            if (curCritters >= maxCritters)
            {
                GD.Print("[Critters] All critters spawned — transitioning to AttackIdle.");
                var stateMachine = GetParent() as StateMachine;
                if (stateMachine == null) { GD.PrintErr("[Critters] Parent is not a StateMachine!"); return base.Process(delta); }
                State attackIdle = stateMachine.FindChild("AttackIdle") as State;
                if (attackIdle == null) { GD.PrintErr("[Critters] AttackIdle state not found!"); return base.Process(delta); }

                stateMachine.ChangeState(attackIdle);
            }
        }

        return base.Process(delta);
    }
}
