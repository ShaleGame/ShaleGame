using Godot;
using CrossedDimensions.Saves;

namespace CrossedDimensions.UI.UISaveLoader;

[GlobalClass]
public partial class LoadSaveFileButton : Button
{
    public SaveFile AttachedSaveFile { get; set; }

    private SaveManager _saveManager;

    private SceneManager _sceneManager;

    public override void _Ready()
    {
        _saveManager = SaveManager.Instance;
        _sceneManager = SceneManager.Instance;

        Pressed += OnPressed;
    }

    private void OnPressed()
    {
        LoadSave();
    }

    public void LoadSave()
    {
        _saveManager.CurrentSave = AttachedSaveFile;
        _sceneManager.LoadSceneFromSave(AttachedSaveFile);
    }
}
