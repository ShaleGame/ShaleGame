using System.Threading.Tasks;
using CrossedDimensions.Characters;
using CrossedDimensions.Characters.Controllers;
using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

namespace CrossedDimensions.Tests.Integration;

[TestSuite]
[TestCategory("Integration")]
public partial class CharacterFlipDirectionIntegrationTest
{
    private const string ScenePath = "res://Characters/Character.tscn";

    private ISceneRunner _runner;
    private Character _character;
    private EnemyController _enemyController;

    [BeforeTest]
    public void SetupTest()
    {
        _runner = ISceneRunner.Load(ScenePath);
        var scene = _runner.Scene();
        _character = scene as Character ?? scene.GetNode<Character>("Character");

        // Replace the controller with an EnemyController to allow programmatic input
        var oldController = _character.GetNode<Node2D>("ControllerComponent");
        if (oldController != null)
        {
            oldController.QueueFree();
        }

        _enemyController = new EnemyController();
        _enemyController.Name = "ControllerComponent";
        _character.AddChild(_enemyController);
        _enemyController.Owner = _character.Owner;
        _character.Controller = _enemyController;
    }

    [AfterTest]
    public void TearDownTest()
    {
        _runner?.Scene()?.QueueFree();
        _runner = null;
        _character = null;
    }

    [TestCase]
    [RequireGodotRuntime]
    public void GivenScene_WhenLoaded_ShouldInitializeCorrectly()
    {
        AssertThat(_character).IsNotNull();
        AssertThat(_enemyController).IsNotNull();
        AssertThat(_character.Controller).IsEqual(_enemyController);
    }

    [TestCase]
    [RequireGodotRuntime]
    public async Task GivenCharacter_WhenMovingRight_ThenSpriteAnchorFacesRight()
    {
        // ensure sprite anchor exists
        AssertThat(_character.SpriteAnchor).IsNotNull();

        // set movement input to the right
        _enemyController.SetTargetInput(new Vector2(1, 0));

        // run a few physics frames to allow the movement state to process
        await _runner.SimulateFrames(2, 20);

        AssertThat(_character.SpriteAnchor.Scale.X).IsEqual(1f);
    }

    [TestCase]
    [RequireGodotRuntime]
    public async Task GivenCharacter_WhenMovingLeft_ThenSpriteAnchorFacesLeft()
    {
        AssertThat(_character.SpriteAnchor).IsNotNull();

        // set movement input to the left
        _enemyController.SetTargetInput(new Vector2(-1, 0));

        // run a few physics frames to allow the movement state to process
        await _runner.SimulateFrames(2, 20);

        AssertThat(_character.SpriteAnchor.Scale.X).IsEqual(-1f);
    }
}
