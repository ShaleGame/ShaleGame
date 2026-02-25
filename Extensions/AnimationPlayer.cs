using Godot;

namespace CrossedDimensions.Extensions;

public static class AnimationPlayerExtensions
{
    /// <summary>
    /// Safely queues an animation if it exists. Does nothing if the animation
    /// is not found.
    /// </summary>
    /// <param name="player">The AnimationPlayer to queue the animation on.</param>
    /// <param name="animationName">The name of the animation to queue.</param>
    public static void SafeQueueAnimation(
        this AnimationPlayer player,
        StringName animationName
    ) {
        if (player.HasAnimation(animationName))
        {
            player.Queue(animationName);
        }
    }
}
