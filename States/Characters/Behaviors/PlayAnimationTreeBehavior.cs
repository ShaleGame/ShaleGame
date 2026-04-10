using Godot;

namespace CrossedDimensions.States.Characters.Behaviors;

/// <summary>
/// A state behavior that drives an AnimationTree state-machine when the
/// parent state is entered. Uses a single StringName for the target state.
/// </summary>
[GlobalClass]
public partial class PlayAnimationTreeBehavior : State
{
    [Export]
    public AnimationTree AnimationTree { get; set; }

    // Single target animation (state-machine node) to travel to.
    [Export]
    public StringName Animation { get; set; } = "";

    public override State Enter(State previousState)
    {
        if (AnimationTree is null || string.IsNullOrEmpty(Animation.ToString()))
        {
            return base.Enter(previousState);
        }

        // The playback object is available at parameters/playback. Attempt a
        // typed cast first; if that fails, try a dynamic call on the returned
        // object.
        // Directly cast to AnimationNodeStateMachinePlayback. If this cast
        // fails it's appropriate to let the exception bubble up so the
        // developer can notice a misconfiguration (per the project style).
        var stateMachine = (AnimationNodeStateMachinePlayback)AnimationTree.Get("parameters/playback");
        stateMachine.Travel(Animation.ToString());

        return base.Enter(previousState);
    }
}
