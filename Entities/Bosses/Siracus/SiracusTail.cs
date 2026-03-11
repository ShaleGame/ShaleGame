using Godot;
using System;

namespace CrossedDimensions.Entities.Bosses.Siracus;

public partial class SiracusTail : Node2D
{
    [Signal]
    public delegate void TailDespawnedEventHandler();

    public AnimationPlayer _animPlayer;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _animPlayer = FindChild("AnimationPlayer") as AnimationPlayer;
        _animPlayer.Play("PopUp");

        _animPlayer.AnimationFinished += AnimationFinished;
    }

    public void AnimationFinished(StringName anim_name)
    {
        _animPlayer.AnimationFinished -= AnimationFinished;

        EmitSignal(SignalName.TailDespawned);

        QueueFree();
    }
}
