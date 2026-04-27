class_name SettingsAssist
extends Control

const GAME_SPEED_OPTIONS := [1.0, 0.9, 0.8, 0.7, 0.6, 0.5]

@onready var game_speed_dropdown: OptionButton = %GameSpeedDropdown
var settings_manager = null

func _ready() -> void:
	settings_manager = get_node_or_null("/root/SettingsManager")

	if not _has_current_settings():
		return

	var current_speed: float = settings_manager.GetAssistGameSpeed()
	var selected_index := GAME_SPEED_OPTIONS.find(current_speed)
	if selected_index == -1:
		selected_index = 0
	game_speed_dropdown.select(selected_index)

func _on_game_speed_dropdown_item_selected(index: int) -> void:
	if not _has_current_settings():
		return
	if index < 0 or index >= GAME_SPEED_OPTIONS.size():
		return
	settings_manager.SetAssistGameSpeed(GAME_SPEED_OPTIONS[index])

func _on_back_button_pressed() -> void:
	hide()

func _has_current_settings() -> bool:
	if settings_manager == null:
		settings_manager = get_node_or_null("/root/SettingsManager")
	if settings_manager == null:
		return false
	return settings_manager.Current != null
