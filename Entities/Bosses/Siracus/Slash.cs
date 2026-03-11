using Godot;
using System;
using CrossedDimensions.States;

namespace CrossedDimensions.Entities.Bosses.Siracus;

// if player gets too close to boss, it does a slash attack with its claws

public partial class Slash : State
{
    [Export]
    public AnimationPlayer animPlay;

    public override State Enter(State previousState)
    {
        GD.Print("Attacking");

        animPlay.AnimationFinished += AnimationFinished;

        if (animPlay.HasAnimation("Slash"))
        {
            animPlay.Play("Slash");
        }

        return base.Enter(previousState);
    }

    private void AnimationFinished(StringName anim_name)
    {
        var _stateMachine = GetParent() as StateMachine;

        animPlay.AnimationFinished -= AnimationFinished;

        _stateMachine.ChangeState("AttackIdle");
    }
}
