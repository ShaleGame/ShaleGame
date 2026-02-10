global using Xunit;
global using twodog.xunit;
global using Shouldly;

namespace CrossedDimensions.Tests;

[CollectionDefinition("Godot", DisableParallelization = true)]
public class GodotCollection : ICollectionFixture<GodotFixture>
{

}
