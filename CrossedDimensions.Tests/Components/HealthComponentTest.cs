using Godot;
using twodog.xunit;
using Xunit;
using CrossedDimensions.Components;
using Shouldly;

namespace CrossedDimensions.Tests.Components;

[Collection("GodotHeadless")]
public class HealthComponentTest(GodotHeadlessFixedFpsFixture godot)
{
    [Fact]
    public void HealthComponent_WhenCreated_ShouldInitializeWithDefaultValues()
    {
        var healthComponent = new HealthComponent();
        godot.Tree.Root.AddChild(healthComponent);

        healthComponent.MaxHealth.ShouldBe(0);
        healthComponent.CurrentHealth.ShouldBe(0);
        healthComponent.IsAlive.ShouldBeFalse();
    }

    [Fact]
    public void HealthComponent_WhenMaxHealthSet_ShouldInitializeWithMaxHealth()
    {
        var healthComponent = new HealthComponent();
        healthComponent.MaxHealth = 100;
        godot.Tree.Root.AddChild(healthComponent);

        healthComponent.MaxHealth.ShouldBe(100);
        healthComponent.CurrentHealth.ShouldBe(0);
        healthComponent.IsAlive.ShouldBeFalse();
    }

    [Fact]
    public void CurrentHealth_WhenSetAboveMaxHealth_ShouldClampToMaxHealth()
    {
        var healthComponent = new HealthComponent();
        healthComponent.MaxHealth = 50;
        godot.Tree.Root.AddChild(healthComponent);

        healthComponent.CurrentHealth = 75;
        healthComponent.CurrentHealth.ShouldBe(50);
    }

    [Fact]
    public void CurrentHealth_WhenSetBelowZero_ShouldClampToZero()
    {
        var healthComponent = new HealthComponent();
        healthComponent.MaxHealth = 50;
        healthComponent.CurrentHealth = 25;
        godot.Tree.Root.AddChild(healthComponent);

        healthComponent.CurrentHealth = -10;
        healthComponent.CurrentHealth.ShouldBe(0);
    }

    [Fact]
    public void IsAlive_WhenCurrentHealthAboveZero_ShouldReturnTrue()
    {
        var healthComponent = new HealthComponent();
        healthComponent.MaxHealth = 50;
        healthComponent.CurrentHealth = 25;
        godot.Tree.Root.AddChild(healthComponent);

        healthComponent.IsAlive.ShouldBeTrue();
    }

    [Fact]
    public void IsAlive_WhenCurrentHealthZero_ShouldReturnFalse()
    {
        var healthComponent = new HealthComponent();
        healthComponent.MaxHealth = 50;
        healthComponent.CurrentHealth = 0;
        godot.Tree.Root.AddChild(healthComponent);

        healthComponent.IsAlive.ShouldBeFalse();
    }

    [Fact]
    public void HealthChanged_WhenHealthModified_ShouldEmitWithOldHealthValue()
    {
        var healthComponent = new HealthComponent();
        healthComponent.MaxHealth = 100;
        healthComponent.CurrentHealth = 50;
        godot.Tree.Root.AddChild(healthComponent);

        bool signalEmitted = false;

        healthComponent.HealthChanged += (oldHealth) =>
        {
            signalEmitted = true;
        };

        healthComponent.CurrentHealth = 25;

        signalEmitted.ShouldBeTrue();
    }

    public void HealthChanged_WhenValueUnchanged_ShouldNotEmit()
    {
        var healthComponent = new HealthComponent();
        healthComponent.MaxHealth = 100;
        healthComponent.CurrentHealth = 50;
        godot.Tree.Root.AddChild(healthComponent);

        bool signalEmitted = false;

        healthComponent.HealthChanged += (oldHealth) =>
        {
            signalEmitted = true;
        };

        healthComponent.CurrentHealth = 50;

        signalEmitted.ShouldBeFalse();
    }
}
