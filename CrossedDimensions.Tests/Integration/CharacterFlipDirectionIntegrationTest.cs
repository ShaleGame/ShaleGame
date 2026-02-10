using System;
using CrossedDimensions.Characters;
using CrossedDimensions.Characters.Controllers;
using Godot;
using Shouldly;

namespace CrossedDimensions.Tests.Integration;

[Collection("GodotHeadless")]
public class CharacterFlipDirectionIntegrationTest : IDisposable
{
    private const string ScenePath = "res://Characters/Character.tscn";
    private readonly GodotHeadlessFixture _godot;
    private Node _scene;
    private Character _character;
    private EnemyController _enemyController;

    public CharacterFlipDirectionIntegrationTest(GodotHeadlessFixture godot)
    {
        _godot = godot;

        var packed = ResourceLoader.Load<PackedScene>(ScenePath);
        _scene = packed.Instantiate() as Node;
        _godot.Tree.Root.AddChild(_scene);

        _character = _scene as Character ?? _scene.GetNode<Character>("Character");

        var oldController = _character.GetNode<Node2D>("ControllerComponent");
        if (oldController is { })
        {
            oldController.QueueFree();
        }

        _enemyController = new EnemyController();
        _enemyController.Name = "ControllerComponent";
        _character.AddChild(_enemyController);
        _enemyController.Owner = _character.Owner;
        _character.Controller = _enemyController;
    }

    public void Dispose()
    {
        _scene?.QueueFree();
        _scene = null;
    }

    [Fact]
    public void GivenScene_WhenLoaded_ShouldInitializeCorrectly()
    {
        _character.ShouldNotBeNull();
        _enemyController.ShouldNotBeNull();
        _character.Controller.ShouldBe(_enemyController);
    }

    [Fact]
    public void GivenCharacter_WhenMovingRight_ThenSpriteAnchorFacesRight()
    {
        _character.SpriteAnchor.ShouldNotBeNull();

        _enemyController.SetTargetInput(new Vector2(1, 0));

        _godot.GodotInstance.Iteration(5);

        _character.SpriteAnchor.Scale.X.ShouldBe(1f);
    }

    [Fact]
    public void GivenCharacter_WhenMovingLeft_ThenSpriteAnchorFacesLeft()
    {
        _character.SpriteAnchor.ShouldNotBeNull();

        _enemyController.SetTargetInput(new Vector2(-1, 0));

        _godot.GodotInstance.Iteration(5);

        _character.SpriteAnchor.Scale.X.ShouldBe(-1f);
    }
}
