using CrossedDimensions.Saves;
using Godot;

namespace CrossedDimensions.Entities.Bosses.FileCypher;

public partial class FileCypherDeathHandler : Node
{
    private const string EndingScreenScenePath = "res://UI/UIEndingScreen/EndingScreen.tscn";

    private bool _transitionScheduled;

    private void _on_health_component_health_changed(int oldHealth)
    {
        if (_transitionScheduled)
        {
            return;
        }

        var healthComponent = GetParent<Components.HealthComponent>();
        if (healthComponent.IsAlive)
        {
            return;
        }

        _transitionScheduled = true;
        SceneManager.Instance?.LoadSceneSync(EndingScreenScenePath, true);
    }
}
