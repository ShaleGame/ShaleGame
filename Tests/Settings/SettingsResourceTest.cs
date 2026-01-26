using CrossedDimensions.Settings;
using GdUnit4;
using static GdUnit4.Assertions;

namespace CrossedDimensions.Tests.Settings;

[TestSuite]
public partial class SettingsResourceTest
{


    [TestCase]
    public void MasterVolume_WhenSetBelowZero_ShouldClampToZero()
    {
        // Arrange
        var settings = new SettingsResource();
        
        // Act
        settings.MasterVolume = -0.5f;
        
        // Assert
        AssertThat(settings.MasterVolume).IsEqual(0.0f);
    }

    [TestCase]
    public void MasterVolume_WhenSetAboveOne_ShouldClampToOne()
    {
        // Arrange
        var settings = new SettingsResource();
        
        // Act
        settings.MasterVolume = 1.5f;
        
        // Assert
        AssertThat(settings.MasterVolume).IsEqual(1.0f);
    }

    [TestCase]
    public void MasterVolume_WhenSetToValidValue_ShouldSetCorrectly()
    {
        // Arrange
        var settings = new SettingsResource();
        
        // Act
        settings.MasterVolume = 0.75f;
        
        // Assert
        AssertThat(settings.MasterVolume).IsEqual(0.75f);
    }

    [TestCase]
    public void MusicVolume_WhenSetBelowZero_ShouldClampToZero()
    {
        // Arrange
        var settings = new SettingsResource();
        
        // Act
        settings.MusicVolume = -0.5f;
        
        // Assert
        AssertThat(settings.MusicVolume).IsEqual(0.0f);
    }

    [TestCase]
    public void MusicVolume_WhenSetAboveOne_ShouldClampToOne()
    {
        // Arrange
        var settings = new SettingsResource();
        
        // Act
        settings.MusicVolume = 1.5f;
        
        // Assert
        AssertThat(settings.MusicVolume).IsEqual(1.0f);
    }

    [TestCase]
    public void MusicVolume_WhenSetToValidValue_ShouldSetCorrectly()
    {
        // Arrange
        var settings = new SettingsResource();
        
        // Act
        settings.MusicVolume = 0.5f;
        
        // Assert
        AssertThat(settings.MusicVolume).IsEqual(0.5f);
    }

    [TestCase]
    public void SfxVolume_WhenSetBelowZero_ShouldClampToZero()
    {
        // Arrange
        var settings = new SettingsResource();
        
        // Act
        settings.SfxVolume = -0.5f;
        
        // Assert
        AssertThat(settings.SfxVolume).IsEqual(0.0f);
    }

    [TestCase]
    public void SfxVolume_WhenSetAboveOne_ShouldClampToOne()
    {
        // Arrange
        var settings = new SettingsResource();
        
        // Act
        settings.SfxVolume = 1.5f;
        
        // Assert
        AssertThat(settings.SfxVolume).IsEqual(1.0f);
    }

    [TestCase]
    public void SfxVolume_WhenSetToValidValue_ShouldSetCorrectly()
    {
        // Arrange
        var settings = new SettingsResource();
        
        // Act
        settings.SfxVolume = 0.25f;
        
        // Assert
        AssertThat(settings.SfxVolume).IsEqual(0.25f);
    }



    [TestCase]
    public void AllVolumeProperties_WhenSetToBoundaryValues_ShouldClampCorrectly()
    {
        // Arrange
        var settings = new SettingsResource();
        
        // Act & Assert - Zero boundary
        settings.MasterVolume = 0.0f;
        settings.MusicVolume = 0.0f;
        settings.SfxVolume = 0.0f;
        
        AssertThat(settings.MasterVolume).IsEqual(0.0f);
        AssertThat(settings.MusicVolume).IsEqual(0.0f);
        AssertThat(settings.SfxVolume).IsEqual(0.0f);
        
        // Act & Assert - One boundary
        settings.MasterVolume = 1.0f;
        settings.MusicVolume = 1.0f;
        settings.SfxVolume = 1.0f;
        
        AssertThat(settings.MasterVolume).IsEqual(1.0f);
        AssertThat(settings.MusicVolume).IsEqual(1.0f);
        AssertThat(settings.SfxVolume).IsEqual(1.0f);
    }
}