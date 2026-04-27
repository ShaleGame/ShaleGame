using Godot;

namespace CrossedDimensions.UI.UISettings;

[GlobalClass]
public partial class SettingsManager : Node
{
    private const string SettingsPath = "user://settings.tres";
    private const string InputBindingsPath = "user://input_bindings.json";
    private float _feedbackTimeScaleMultiplier = 1.0f;

    public SettingsResource Current { get; private set; }

    public override void _Ready()
    {
        LoadOrCreateSettings();
        LoadInputBindings();
        ApplyWindowMode(Current.WindowMode);
        ApplyHdr(Current.HdrEnabled);
        ApplyTimeScale();
    }

    public Error Save()
    {
        if (Current is null)
        {
            return Error.Failed;
        }

        return ResourceSaver.Save(Current, SettingsPath);
    }

    public Error SaveInputBindings()
    {
        var serializedBindings = new Godot.Collections.Dictionary();

        foreach (string actionName in InputMap.GetActions())
        {
            if (!ShouldPersistAction(actionName))
                continue;

            var serializedEvents = new Godot.Collections.Array();
            foreach (InputEvent inputEvent in InputMap.ActionGetEvents(actionName))
            {
                var serializedEvent = SerializeInputEvent(inputEvent);
                if (serializedEvent.Count > 0)
                {
                    serializedEvents.Add(serializedEvent);
                }
            }

            serializedBindings[actionName] = serializedEvents;
        }

        using FileAccess file = FileAccess.Open(InputBindingsPath, FileAccess.ModeFlags.Write);
        if (file is null)
        {
            return FileAccess.GetOpenError();
        }

        file.StoreString(Json.Stringify(serializedBindings));
        return Error.Ok;
    }

    public void ApplyWindowMode(int modeIndex)
    {
        int clampedMode = Mathf.Clamp(modeIndex, 0, 2);
        DisplayServer.WindowSetMode(WindowModeToDisplayServerMode(clampedMode));
    }

    public float GetAssistGameSpeed()
    {
        return Current?.AssistGameSpeed ?? 1.0f;
    }

    public void SetAssistGameSpeed(float assistGameSpeed)
    {
        if (Current is null)
        {
            return;
        }

        Current.AssistGameSpeed = assistGameSpeed;
        ApplyTimeScale();
        Save();
    }

    public void SetFeedbackTimeScaleMultiplier(float multiplier)
    {
        _feedbackTimeScaleMultiplier = Mathf.Clamp(multiplier, 0.0f, 1.0f);
        ApplyTimeScale();
    }

    public void ResetFeedbackTimeScaleMultiplier()
    {
        _feedbackTimeScaleMultiplier = 1.0f;
        ApplyTimeScale();
    }

    public bool IsScreenShakeEnabled()
    {
        return Current?.ScreenShakeEnabled ?? true;
    }

    public bool IsVisualEffectsEnabled()
    {
        return Current?.VisualEffectsEnabled ?? true;
    }

    public void ApplyHdr(bool enabled)
    {
        ProjectSettings.SetSetting("rendering/viewport/hdr_2d", enabled);
        if (GetTree() is not null)
        {
            GetTree().Root.UseHdr2D = enabled;
        }
    }

    public void ApplyTimeScale()
    {
        Engine.TimeScale = GetAssistGameSpeed() * _feedbackTimeScaleMultiplier;
    }

    private void LoadOrCreateSettings()
    {
        var loadedSettings = ResourceLoader.Load<SettingsResource>(SettingsPath);
        if (loadedSettings is not null)
        {
            Current = loadedSettings;
            return;
        }

        Current = new SettingsResource();
        Save();
    }

    private void LoadInputBindings()
    {
        if (!FileAccess.FileExists(InputBindingsPath))
        {
            return;
        }

        using FileAccess file = FileAccess.Open(InputBindingsPath, FileAccess.ModeFlags.Read);
        if (file is null)
        {
            return;
        }

        Variant parsed = Json.ParseString(file.GetAsText());
        if (parsed.VariantType != Variant.Type.Dictionary)
        {
            return;
        }

        var bindings = parsed.AsGodotDictionary();
        foreach (Variant actionVariant in bindings.Keys)
        {
            string actionName = actionVariant.AsString();
            if (!InputMap.HasAction(actionName) || !ShouldPersistAction(actionName))
            {
                continue;
            }

            if (bindings[actionName].Obj is not Godot.Collections.Array serializedEvents)
            {
                continue;
            }

            InputMap.ActionEraseEvents(actionName);

            foreach (Variant serializedEventVariant in serializedEvents)
            {
                if (serializedEventVariant.Obj is not Godot.Collections.Dictionary serializedEvent)
                {
                    continue;
                }

                InputEvent inputEvent = DeserializeInputEvent(serializedEvent);
                if (inputEvent is not null)
                {
                    InputMap.ActionAddEvent(actionName, inputEvent);
                }
            }
        }
    }

    private static bool ShouldPersistAction(string actionName)
    {
        return !actionName.StartsWith("debug_") && !actionName.StartsWith("ui_");
    }

    private static Godot.Collections.Dictionary SerializeInputEvent(InputEvent inputEvent)
    {
        var serialized = new Godot.Collections.Dictionary();

        if (inputEvent is InputEventKey keyEvent)
        {
            serialized["type"] = "key";
            serialized["keycode"] = (int)keyEvent.Keycode;
            serialized["physical_keycode"] = (int)keyEvent.PhysicalKeycode;
            serialized["unicode"] = keyEvent.Unicode;
            serialized["location"] = (int)keyEvent.Location;
            serialized["shift_pressed"] = keyEvent.ShiftPressed;
            serialized["alt_pressed"] = keyEvent.AltPressed;
            serialized["ctrl_pressed"] = keyEvent.CtrlPressed;
            serialized["meta_pressed"] = keyEvent.MetaPressed;
            return serialized;
        }

        if (inputEvent is InputEventMouseButton mouseButtonEvent)
        {
            serialized["type"] = "mouse_button";
            serialized["button_index"] = (int)mouseButtonEvent.ButtonIndex;
            serialized["shift_pressed"] = mouseButtonEvent.ShiftPressed;
            serialized["alt_pressed"] = mouseButtonEvent.AltPressed;
            serialized["ctrl_pressed"] = mouseButtonEvent.CtrlPressed;
            serialized["meta_pressed"] = mouseButtonEvent.MetaPressed;
            return serialized;
        }

        return serialized;
    }

    private static InputEvent DeserializeInputEvent(Godot.Collections.Dictionary serializedEvent)
    {
        if (!serializedEvent.ContainsKey("type"))
        {
            return null;
        }

        string eventType = serializedEvent["type"].AsString();
        if (eventType == "key")
        {
            return new InputEventKey
            {
                Keycode = (Key)(int)serializedEvent["keycode"],
                PhysicalKeycode = (Key)(int)serializedEvent["physical_keycode"],
                Unicode = (long)serializedEvent["unicode"],
                Location = (KeyLocation)(long)serializedEvent["location"],
                ShiftPressed = (bool)serializedEvent["shift_pressed"],
                AltPressed = (bool)serializedEvent["alt_pressed"],
                CtrlPressed = (bool)serializedEvent["ctrl_pressed"],
                MetaPressed = (bool)serializedEvent["meta_pressed"]
            };
        }

        if (eventType == "mouse_button")
        {
            return new InputEventMouseButton
            {
                ButtonIndex = (MouseButton)(long)serializedEvent["button_index"],
                ShiftPressed = (bool)serializedEvent["shift_pressed"],
                AltPressed = (bool)serializedEvent["alt_pressed"],
                CtrlPressed = (bool)serializedEvent["ctrl_pressed"],
                MetaPressed = (bool)serializedEvent["meta_pressed"]
            };
        }

        return null;
    }

    private static DisplayServer.WindowMode WindowModeToDisplayServerMode(int modeIndex)
    {
        return modeIndex switch
        {
            0 => DisplayServer.WindowMode.ExclusiveFullscreen,
            1 => DisplayServer.WindowMode.Fullscreen,
            2 => DisplayServer.WindowMode.Windowed,
            _ => DisplayServer.WindowMode.ExclusiveFullscreen
        };
    }
}
