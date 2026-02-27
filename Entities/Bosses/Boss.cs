using Godot;
using CrossedDimensions.Characters;

namespace CrossedDimensions.Entities.Bosses;

[GlobalClass]
public partial class Boss : Character
{
    // Boss-specific properties and methods can be added here

    [Export] public string bossName = "Boss";

}
