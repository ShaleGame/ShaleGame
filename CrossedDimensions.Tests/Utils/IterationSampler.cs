namespace CrossedDimensions.Tests.Utils;

public class IterationSampler<T>
{
    public Godot.GodotInstance Instance { get; }

    public IterationSampler(Godot.GodotInstance instance)
    {
        Instance = instance;
    }

    public IterationSample AtAnyPointBefore(System.Func<bool> condition, int maxIterations = 3600)
    {
        if (!condition())
        {
            return new IterationSample();
        }

        for (int i = 0; i < maxIterations; i++)
        {
            Instance.Iteration();

            if (!condition())
            {
                return new IterationSample();
            }
        }

        return new IterationSample();
    }
}
