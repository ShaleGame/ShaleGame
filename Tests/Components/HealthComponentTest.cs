using System.Threading.Tasks;
using CrossedDimensions.Components;
using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

namespace CrossedDimensions.Tests.Components;

[TestSuite]
public partial class HealthComponentTest
{
    [TestCase]
    [RequireGodotRuntime]
    public void HealthComponent_WhenCreated_ShouldInitializeWithDefaultValues()
    {
        var healthComponent = new HealthComponent();
        AddNode(healthComponent);

        AssertThat(healthComponent.MaxHealth).IsEqual(0);
        AssertThat(healthComponent.CurrentHealth).IsEqual(0);
        AssertThat(healthComponent.IsAlive).IsFalse();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void HealthComponent_WhenMaxHealthSet_ShouldInitializeWithMaxHealth()
    {
        var healthComponent = new HealthComponent();
        healthComponent.MaxHealth = 100;
        AddNode(healthComponent);

        AssertThat(healthComponent.MaxHealth).IsEqual(100);
        AssertThat(healthComponent.CurrentHealth).IsEqual(0);
        AssertThat(healthComponent.IsAlive).IsFalse();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void CurrentHealth_WhenSetAboveMaxHealth_ShouldClampToMaxHealth()
    {
        var healthComponent = new HealthComponent();
        healthComponent.MaxHealth = 50;
        AddNode(healthComponent);

        healthComponent.CurrentHealth = 75;
        AssertThat(healthComponent.CurrentHealth).IsEqual(50);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void CurrentHealth_WhenSetBelowZero_ShouldClampToZero()
    {
        var healthComponent = new HealthComponent();
        healthComponent.MaxHealth = 50;
        healthComponent.CurrentHealth = 25;
        AddNode(healthComponent);

        healthComponent.CurrentHealth = -10;
        AssertThat(healthComponent.CurrentHealth).IsEqual(0);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void CurrentHealth_WhenSetToValidValue_ShouldSetCorrectly()
    {
        var healthComponent = new HealthComponent();
        healthComponent.MaxHealth = 50;
        AddNode(healthComponent);

        healthComponent.CurrentHealth = 30;
        AssertThat(healthComponent.CurrentHealth).IsEqual(30);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void IsAlive_WhenHealthPositive_ShouldReturnTrue()
    {
        var healthComponent = new HealthComponent();
        healthComponent.MaxHealth = 100;
        healthComponent.CurrentHealth = 50;
        AddNode(healthComponent);

        AssertThat(healthComponent.IsAlive).IsTrue();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void IsAlive_WhenHealthZero_ShouldReturnFalse()
    {
        var healthComponent = new HealthComponent();
        healthComponent.MaxHealth = 100;
        healthComponent.CurrentHealth = 0;
        AddNode(healthComponent);

        AssertThat(healthComponent.IsAlive).IsFalse();
    }

    [TestCase]
    [RequireGodotRuntime]
    public async Task HealthChanged_WhenHealthModified_ShouldEmitWithOldHealthValue()
    {
        var healthComponent = new HealthComponent();
        healthComponent.MaxHealth = 100;
        healthComponent.CurrentHealth = 50;
        AddNode(healthComponent);

        AssertSignal(healthComponent).StartMonitoring();

        healthComponent.CurrentHealth = 25;

        await AssertSignal(healthComponent)
            .IsEmitted(HealthComponent.SignalName.HealthChanged, 50)
            .WithTimeout(200);
    }

    [TestCase]
    [RequireGodotRuntime]
    public async Task HealthChanged_WhenHealthClampedToZero_ShouldEmitWithOldHealthValue()
    {
        var healthComponent = new HealthComponent();
        healthComponent.MaxHealth = 50;
        healthComponent.CurrentHealth = 25;
        AddNode(healthComponent);

        AssertSignal(healthComponent).StartMonitoring();

        healthComponent.CurrentHealth = -10;

        await AssertSignal(healthComponent)
            .IsEmitted(HealthComponent.SignalName.HealthChanged, 25)
            .WithTimeout(200);
    }

    [TestCase]
    [RequireGodotRuntime]
    public async Task HealthChanged_WhenValueUnchanged_ShouldNotEmit()
    {
        var healthComponent = new HealthComponent();
        healthComponent.MaxHealth = 100;
        healthComponent.CurrentHealth = 50;
        AddNode(healthComponent);

        AssertSignal(healthComponent).StartMonitoring();

        healthComponent.CurrentHealth = 50;

        await AssertSignal(healthComponent)
            .IsNotEmitted(HealthComponent.SignalName.HealthChanged)
            .WithTimeout(200);
    }
}
