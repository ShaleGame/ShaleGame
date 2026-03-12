using Godot;

namespace CrossedDimensions.Extensions;

public static class AudioStreamPlayer2DExtensions
{
    /// <summary>
    /// Duplicates the AudioStreamPlayer2D, reparents the duplicate to the
    /// tree root, plays the audio, and frees the duplicate after the audio
    /// finishes playing.
    /// </summary>
    public static AudioStreamPlayer2D PlayOneShot(this AudioStreamPlayer2D player)
    {
        if (player.Stream == null)
        {
            return null;
        }

        var oneShot = player.Duplicate() as AudioStreamPlayer2D;
        if (oneShot == null)
        {
            return null;
        }

        player.GetTree().Root.AddChild(oneShot);
        oneShot.GlobalPosition = player.GlobalPosition;
        player.Finished += () => oneShot.QueueFree();
        oneShot.Play();
        return oneShot;
    }

    /// <summary>
    /// Chainable method that applies a random pitch variation to the
    /// AudioStreamPlayer2D.
    /// </summary>
    public static AudioStreamPlayer2D WithRandomPitch(
        this AudioStreamPlayer2D player,
        float variation)
    {
        player.PitchScale += (float)GD.RandRange(-variation, variation);

        return player;
    }
}
