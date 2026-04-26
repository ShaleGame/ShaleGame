class_name SettingsKeybinds
extends Control

const LISTENING_TEXT := "Press a key..."

@onready var grid_container: GridContainer = $MarginContainer/Keybinds/Labels/ScrollContainer/GridContainer

var settings_manager = null
var action_buttons: Dictionary = {}
var listening_action := ""

func _ready() -> void:
	settings_manager = get_node_or_null("/root/SettingsManager")
	_build_action_rows()

func _input(event: InputEvent) -> void:
	if listening_action.is_empty():
		return

	if event is InputEventKey:
		var key_event: InputEventKey = event
		if not key_event.pressed or key_event.echo:
			return
		_apply_rebind(listening_action, key_event)
		get_viewport().set_input_as_handled()
		return

	if event is InputEventMouseButton:
		var mouse_event: InputEventMouseButton = event
		if not mouse_event.pressed:
			return
		_apply_rebind(listening_action, mouse_event)
		get_viewport().set_input_as_handled()

func _on_back_button_pressed() -> void:
	_cancel_rebind()
	hide()

func _build_action_rows() -> void:
	for child in grid_container.get_children():
		child.queue_free()

	action_buttons.clear()

	for action_name in InputMap.get_actions():
		if action_name.begins_with("debug_") or action_name.begins_with("ui_"):
			continue

		var label := Label.new()
		label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
		label.text = _format_action_name(action_name)
		grid_container.add_child(label)

		var button := Button.new()
		button.focus_mode = Control.FOCUS_CLICK
		button.mouse_default_cursor_shape = Control.CURSOR_POINTING_HAND
		button.text = _get_action_text(action_name)
		button.pressed.connect(_on_action_button_pressed.bind(action_name))
		grid_container.add_child(button)

		action_buttons[action_name] = button

func _on_action_button_pressed(action_name: String) -> void:
	if listening_action == action_name:
		_cancel_rebind()
		return

	_cancel_rebind()
	call_deferred("_begin_rebind", action_name)

func _begin_rebind(action_name: String) -> void:
	listening_action = action_name
	var button: Button = action_buttons.get(action_name)
	if button != null:
		button.text = LISTENING_TEXT

func _apply_rebind(action_name: String, event: InputEvent) -> void:
	InputMap.action_erase_events(action_name)
	InputMap.action_add_event(action_name, event)

	var button: Button = action_buttons.get(action_name)
	if button != null:
		button.text = _get_action_text(action_name)

	listening_action = ""

	if settings_manager != null and settings_manager.has_method("SaveInputBindings"):
		settings_manager.SaveInputBindings()

func _cancel_rebind() -> void:
	if listening_action.is_empty():
		return

	var button: Button = action_buttons.get(listening_action)
	if button != null:
		button.text = _get_action_text(listening_action)

	listening_action = ""

func _get_action_text(action_name: String) -> String:
	var events: Array[InputEvent] = InputMap.action_get_events(action_name)
	if events.is_empty():
		return "Unbound"
	return _format_input_event(events[0])

func _format_action_name(action_name: String) -> String:
	var words := PackedStringArray()
	for word in action_name.split("_"):
		words.append(word.capitalize())
	return " ".join(words)

func _format_input_event(event: InputEvent) -> String:
	if event == null:
		return "Unbound"
	return event.as_text()
