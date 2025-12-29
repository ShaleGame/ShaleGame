namespace CrossedDimensions.Environment.Cutscene;

/// <summary>
/// A class for individual 'frames' of dialogue
/// </summary>

public class DialogueFrame
{
    string speaker { get; set; }
    string text { get; set; }
    Sprite2D[] portrait { get; set; }
    Vector2[] portraitPosition { get; set; }
    Sprite2D background { get; set; }
}