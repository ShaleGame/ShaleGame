using CrossedDimensions.Environment;
using Godot;

namespace CrossedDimensions.UI.UIPlayerHud;

[GlobalClass]
public partial class AreaTitleCard : Control
{
    [Export]
    public Label TitleLabel { get; set; }

    [Export]
    public Label SubtitleLabel { get; set; }

    [Export]
    public AnimationPlayer AnimationPlayer { get; set; }

    public int AnimationPlayCount { get; private set; }

    private AreaData _lastShownAreaData;

    public override void _Ready()
    {
        Visible = false;

        if (AnimationPlayer is not null)
        {
            AnimationPlayer.AnimationFinished += OnAnimationFinished;
        }

        if (AreaManager.Instance is not null)
        {
            AreaManager.Instance.AreaTriggerEntered += OnAreaTriggerEntered;
        }
        else
        {
            GD.PushWarning("AreaTitleCard: AreaManager.Instance is null in _Ready.");
        }
    }

    public override void _Notification(int what)
    {
        if (what == NotificationPredelete && AreaManager.Instance is not null)
        {
            AreaManager.Instance.AreaTriggerEntered -= OnAreaTriggerEntered;
        }

        if (what == NotificationPredelete && AnimationPlayer is not null)
        {
            AnimationPlayer.AnimationFinished -= OnAnimationFinished;
        }
    }

    private void OnAreaTriggerEntered(AreaData data, bool updateLastShownArea)
    {
        if (data is null || (updateLastShownArea && IsSameArea(data, _lastShownAreaData)))
        {
            return;
        }

        if (updateLastShownArea)
        {
            _lastShownAreaData = data;
        }

        if (TitleLabel is not null)
        {
            TitleLabel.Text = data.Title ?? "";
        }

        if (SubtitleLabel is not null)
        {
            SubtitleLabel.Text = data.Subtitle ?? "";
            SubtitleLabel.Visible = !string.IsNullOrEmpty(SubtitleLabel.Text);
        }

        Show();
        if (AnimationPlayer is not null)
        {
            AnimationPlayCount++;
            if (AnimationPlayer.IsPlaying())
            {
                AnimationPlayer.Stop();
            }
            AnimationPlayer.Play("display");
        }
    }
    private void OnAnimationFinished(StringName animationName)
    {
        if (animationName == "display")
        {
            Hide();
        }
    }

    private static bool IsSameArea(AreaData left, AreaData right)
    {
        if (left is null || right is null)
        {
            return false;
        }

        if (ReferenceEquals(left, right))
        {
            return true;
        }

        return left.Title == right.Title && left.Subtitle == right.Subtitle;
    }
}
