using System;
using CrossedDimensions.Audio;
using CrossedDimensions.Saves;
using Godot;

namespace CrossedDimensions.UI.UIEndingScreen;

public partial class EndingScreen : Control
{
    private const string MainMenuScenePath = "res://UI/UIMainMenu/MainMenu.tscn";

    private AnimationPlayer _animationPlayer;
    private SceneManager _sceneManager;
    private VBoxContainer[] _stanzaContainers;
    private Button[] _continueButtons;
    private int _currentStanzaIndex;

    public override void _Ready()
    {
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _sceneManager = GetNode<SceneManager>("/root/SceneManager");
        _stanzaContainers =
        [
            GetNode<VBoxContainer>("MarginContainer/CenterContainer/EndingMenu/Stanza1"),
            GetNode<VBoxContainer>("MarginContainer/CenterContainer/EndingMenu/Stanza2"),
            GetNode<VBoxContainer>("MarginContainer/CenterContainer/EndingMenu/Stanza3"),
            GetNode<VBoxContainer>("MarginContainer/CenterContainer/EndingMenu/Stanza4")
        ];
        _continueButtons =
        [
            GetNode<Button>("MarginContainer/CenterContainer/EndingMenu/Stanza1/ContinueButton"),
            GetNode<Button>("MarginContainer/CenterContainer/EndingMenu/Stanza2/ContinueButton"),
            GetNode<Button>("MarginContainer/CenterContainer/EndingMenu/Stanza3/ContinueButton"),
            GetNode<Button>("MarginContainer/CenterContainer/EndingMenu/Stanza4/ContinueButton")
        ];

        StopAllMusic();

        for (int index = 0; index < _continueButtons.Length; index++)
        {
            int stanzaIndex = index;
            _continueButtons[index].Pressed += () => OnContinuePressed(stanzaIndex);
        }

        _animationPlayer.AnimationFinished += OnAnimationFinished;

        ShowStanza(0);
    }

    private void ShowStanza(int index)
    {
        _currentStanzaIndex = index;

        for (int stanzaIndex = 0; stanzaIndex < _stanzaContainers.Length; stanzaIndex++)
        {
            ResetStanza(_stanzaContainers[stanzaIndex], stanzaIndex == index);
        }

        _animationPlayer.Play(AnimationName(index));
    }

    private static void ResetStanza(VBoxContainer stanza, bool isVisible)
    {
        stanza.Visible = isVisible;

        foreach (Node child in stanza.GetChildren())
        {
            if (child is CanvasItem canvasItem)
            {
                canvasItem.Modulate = new Color(1, 1, 1, 0);
            }

            if (child is Button button)
            {
                button.Disabled = true;
            }
        }
    }

    private static string AnimationName(int index)
    {
        return $"stanza_{index}";
    }

    private void OnAnimationFinished(StringName animationName)
    {
        if (animationName != AnimationName(_currentStanzaIndex))
        {
            return;
        }

        _continueButtons[_currentStanzaIndex].GrabFocus();
    }

    private void OnContinuePressed(int index)
    {
        if (index < _stanzaContainers.Length - 1)
        {
            ShowStanza(index + 1);
            return;
        }

        _sceneManager.LoadSceneSync(MainMenuScenePath, true);
    }

    private static void StopAllMusic()
    {
        foreach (MusicPriority priority in Enum.GetValues<MusicPriority>())
        {
            MusicManager.Instance?.StopTrack(priority);
        }
    }
}
