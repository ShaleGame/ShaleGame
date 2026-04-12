using Godot;
using Characters = CrossedDimensions.Characters;

namespace CrossedDimensions.UI;

[GlobalClass]
public partial class CloneOffscreenIndicator : Control
{
    [Export]
    public float EdgePadding { get; set; } = 12f;

    [Export]
    public Color IndicatorColor { get; set; } = new(0.753f, 0.502f, 1f);

    [Export]
    public float PulseFrequency { get; set; } = 2f;

    [Export]
    public bool SnapToPixel { get; set; } = true;

    private Characters.Character _clone;
    private float _pulseTime;

    public void SetClone(Characters.Character clone)
    {
        _clone = clone;
        Visible = true;
    }

    public void ClearClone()
    {
        _clone = null;
        Visible = false;
    }

    public override void _Process(double delta)
    {
        if (_clone is null || !IsInstanceValid(_clone))
        {
            Visible = false;
            _pulseTime = 0f;
            return;
        }

        Transform2D canvasXform = GetViewport().GetCanvasTransform();
        Vector2 screenPos = canvasXform * _clone.GlobalPosition;
        Vector2 screenSize = GetViewportRect().Size;

        var screenRect = new Rect2(Vector2.Zero, screenSize);
        if (screenRect.HasPoint(screenPos))
        {
            Visible = false;
            _pulseTime = 0f;
            return;
        }

        Vector2 center = screenSize * 0.5f;
        Vector2 dir = (screenPos - center).Normalized();

        float tMin = float.PositiveInfinity;

        if (!Mathf.IsZeroApprox(dir.X))
        {
            float tLeft = (0f - center.X) / dir.X;
            if (tLeft > 0f)
            {
                tMin = Mathf.Min(tMin, tLeft);
            }

            float tRight = (screenSize.X - center.X) / dir.X;
            if (tRight > 0f)
            {
                tMin = Mathf.Min(tMin, tRight);
            }
        }

        if (!Mathf.IsZeroApprox(dir.Y))
        {
            float tTop = (0f - center.Y) / dir.Y;
            if (tTop > 0f)
            {
                tMin = Mathf.Min(tMin, tTop);
            }

            float tBottom = (screenSize.Y - center.Y) / dir.Y;
            if (tBottom > 0f)
            {
                tMin = Mathf.Min(tMin, tBottom);
            }
        }

        Vector2 edgePoint = float.IsPositiveInfinity(tMin)
            ? screenPos
            : center + dir * tMin;

        float clampedPadding = Mathf.Max(0f, EdgePadding);
        Vector2 paddedSize = new(
            Mathf.Max(0f, screenSize.X - clampedPadding * 2f),
            Mathf.Max(0f, screenSize.Y - clampedPadding * 2f));
        var paddedRect = new Rect2(new Vector2(clampedPadding, clampedPadding), paddedSize);

        Position = paddedRect.HasArea()
            ? edgePoint.Clamp(paddedRect.Position, paddedRect.End)
            : edgePoint;

        if (SnapToPixel)
        {
            Position = Position.Round();
        }

        Rotation = (screenPos - Position).Angle();
        _pulseTime += (float)delta;
        Visible = true;
        QueueRedraw();
    }

    public override void _Draw()
    {
        float blend = 0.5f + 0.5f * Mathf.Sin(_pulseTime * Mathf.Tau * PulseFrequency);
        Color animatedColor = IndicatorColor.Lerp(new Color(1f, 1f, 1f, IndicatorColor.A), blend);

        Color backColor = animatedColor;
        backColor.A = 0.5f;

        DrawPolygon(
            new Vector2[] { new(-6f, -5f), new(0f, 0f), new(-6f, 5f) },
            new Color[] { backColor, backColor, backColor });

        DrawPolygon(
            new Vector2[] { new(-1f, -5f), new(5f, 0f), new(-1f, 5f) },
            new Color[] { animatedColor, animatedColor, animatedColor });

        DrawCircle(new Vector2(-13f, 0f), 3f, animatedColor);
    }
}
