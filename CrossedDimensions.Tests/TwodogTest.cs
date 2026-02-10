using twodog.xunit;
using Xunit;
using Godot;

namespace CrossedDimensions.Tests;

[Collection("Godot")]
public class GodotSceneTests(GodotFixture godot) : IClassFixture<GodotFixture>
{
    [Fact]
    public void LoadScene_ValidPath_Succeeds()
    {
        var scene = GD.Load<PackedScene>("res://Characters/Character.tscn");
        var instance = scene.Instantiate<Characters.Character>();

        godot.Tree.Root.AddChild(instance);

        Assert.NotNull(instance.GetParent());
    }
}
