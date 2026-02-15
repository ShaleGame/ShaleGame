using CrossedDimensions.Environment.Cutscene;
using GdUnit4;
using static GdUnit4.Assertions;

namespace CrossedDimensions.Tests.Environment.Cutscene;

[TestSuite]
public partial class CutscenePlayerTest
{
    [TestCase]
    [RequireGodotRuntime]
    public void StartScene_ShouldSetSceneActiveTrue()
    {
        var scene_player = new CutscenePlayer
        {
            SceneActive = false
        };

        var scene_timeline = new ActionTimeline();

        scene_player.StartScene(scene_timeline);

        AssertThat(scene_player.SceneActive).IsTrue();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void EndScene_ShouldSetSceneActiveFalse()
    {
        var scene_player = new CutscenePlayer
        {
            SceneActive = true
        };

        scene_player.EndScene();
        AssertThat(scene_player.SceneActive).IsTrue();
    }
}