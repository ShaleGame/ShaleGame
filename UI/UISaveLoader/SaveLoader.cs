using Godot;
using CrossedDimensions.Saves;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CrossedDimensions.UI.UISaveLoader;

[GlobalClass]
public partial class SaveLoader : Control
{
    private List<LoadSaveFileButton> _loadButtons;
    private SaveManager _saveManager;
    [Export]
    private PackedScene _loadButtonScene;
    [Export]
    private LoadSaveFileButton _newSaveButton;
    [Export]
    private Button _backButton;
    public override void _Ready()
    {
        _saveManager = SaveManager.Instance;
        _backButton.Pressed += CloseSaveLoader;
    }

    private void CreateLoadSaveButtons()
    {
        _loadButtons = _saveManager.ListAllSaves()
            .OrderByDescending(x => x.Timestamp)
            .Select((save) =>
            {
                var loadButton = _loadButtonScene.Instantiate<LoadSaveFileButton>();
                loadButton.AttachedSaveFile = save;
                loadButton.Text = save.SaveName;
                return loadButton;
            })
            .ToList();
    }
    private void SetUpNewSaveButton()
    {
        var newSave = SaveManager.Instance.CreateNewSave();
        _newSaveButton.AttachedSaveFile = newSave;
    }
    public void OpenSaveLoader()
    {
        CreateLoadSaveButtons();
        SetUpNewSaveButton();
        var saveList = GetNode<VBoxContainer>("%SaveList");
        foreach (LoadSaveFileButton loadButton in _loadButtons)
        {
            saveList.AddChild(loadButton);
        }
        //toggle the visablity of this node
        Show();
    }
    public void CloseSaveLoader()
    {
        Hide();
        var saveList = GetNode<VBoxContainer>("%SaveList");
        foreach (LoadSaveFileButton loadButton in _loadButtons)
        {
            saveList.RemoveChild(loadButton);
        }
    }
}
