using CrossedDimensions.Saves;
using CrossedDimensions.UI;
using Godot;

namespace CrossedDimensions.Entities.Bosses.FileCypher;

public partial class FileCypherDeathHandler : Node
{
    private const string EndingScreenScenePath = "res://UI/UIEndingScreen/EndingScreen.tscn";

    private bool _transitionScheduled;
    private double _transitionStartTime;
    private double _transitionDelay;

    [Export]
    public float DeathFreezeDuration { get; set; } = 1.0f;

    public override void _Process(double delta)
    {
        if (!_transitionScheduled)
        {
            return;
        }

        double elapsed = (Time.GetTicksMsec() / 1000.0) - _transitionStartTime;
        if (elapsed < _transitionDelay)
        {
            return;
        }

        _transitionScheduled = false;
        SceneManager.Instance?.LoadSceneSync(EndingScreenScenePath, true);
    }

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
        _transitionStartTime = Time.GetTicksMsec() / 1000.0;
        _transitionDelay = Mathf.Max(0.05f, DeathFreezeDuration);

        var camera = GetViewport()?.GetCamera2D();
        ScreenOverlayManager.Instance?.TriggerDeathFeedback(camera);
    }
}
