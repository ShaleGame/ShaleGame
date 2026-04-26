using CrossedDimensions.Characters;
using CrossedDimensions.States;
using Godot;

public partial class Still : State
{
    
    private Character _drill;

    public override State Enter(State previousState)
    {
        _drill = Context as Character;

        return base.Enter(previousState);
    }

    public override State Process(double delta)
    {
        _drill.Velocity = Vector2.Zero;

        return base.Process(delta);
    }

}
