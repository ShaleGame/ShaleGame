global using Xunit;
global using twodog.xunit;
global using Shouldly;

namespace CrossedDimensions.Tests;

[CollectionDefinition("GodotHeadless", DisableParallelization = true)]
public class GodotHeadlessCollection : ICollectionFixture<GodotHeadlessFixedFpsFixture>
{

}
