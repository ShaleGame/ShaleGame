using Godot;
using CrossedDimensions.Characters;
using CrossedDimensions.Characters.Controllers;
using CrossedDimensions.Entities.Enemies;

namespace CrossedDimensions.States.Enemies;

public partial class EnemyState : State
{
    public Character CharacterContext { get; private set; }

    public EnemyComponent EnemyComponent { get; private set; }

    public EnemyController EnemyController { get; private set; }

    public override Node Context
    {
        get => base.Context;
        set
        {
            base.Context = value;
            CharacterContext = value as Character;

            if (CharacterContext is not null)
            {
                EnemyController = CharacterContext.Controller as EnemyController;
                EnemyComponent = CharacterContext.GetNodeOrNull<EnemyComponent>("EnemyComponent");
            }
        }
    }
}
