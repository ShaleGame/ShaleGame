namespace CrossedDimensions.Tests;

public static class GodotInstanceExtensions
{
    /// <summary>
    /// Performs a specified number of iterations on the given Godot instance.
    /// </summary>
    public static bool Iteration(this Godot.GodotInstance instance, int n)
    {
        for (int i = 0; i < n; i++)
        {
            if (instance.Iteration())
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Performs iterations on the given Godot instance while a specified
    /// condition is true, up to a maximum number of iterations.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the condition is still true after reaching the maximum
    /// number of iterations; <c>false</c> if the condition became false before
    /// reaching the maximum number of iterations.
    /// </returns>
    public static bool IterateWhile(
        this Godot.GodotInstance instance,
        System.Func<bool> condition,
        int maxIterations = 3600)
    {
        if (!condition())
        {
            return false;
        }

        for (int i = 0; i < maxIterations; i++)
        {
            instance.Iteration();

            if (!condition())
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Performs iterations on the given Godot instance until a specified
    /// condition becomes true, up to a maximum number of iterations.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the condition became true within the maximum number of
    /// iterations; <c>false</c> if the condition is still false after reaching the
    /// maximum number of iterations.
    /// </returns>
    public static bool IterateUntil(
        this Godot.GodotInstance instance,
        System.Func<bool> condition,
        int maxIterations = 3600)
    {
        if (condition())
        {
            return true;
        }

        for (int i = 0; i < maxIterations; i++)
        {
            instance.Iteration();

            if (condition())
            {
                return true;
            }
        }

        return false;
    }
}
