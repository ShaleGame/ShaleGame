class_name SettingsVideo
extends Control

@onready var display_mode_dropdown: OptionButton = %DisplayModeDropdown
@onready var screen_shake_toggle: CheckBox = %ScreenShakeToggle
@onready var visual_effects_toggle: CheckBox = %VisualEffectsToggle
@onready var hdr_toggle: CheckBox = %HDRToggle
var settings_manager = null

func _ready() -> void:
	settings_manager = get_node_or_null("/root/SettingsManager")

	if not _has_current_settings():
		return

	var current_settings = settings_manager.Current
	display_mode_dropdown.select(settings_manager.Current.WindowMode)
	screen_shake_toggle.button_pressed = current_settings.ScreenShakeEnabled
	visual_effects_toggle.button_pressed = current_settings.VisualEffectsEnabled
	hdr_toggle.button_pressed = current_settings.HdrEnabled

func _on_display_mode_dropdown_item_selected(index: int) -> void:
	if not _has_current_settings():
		return
	settings_manager.ApplyWindowMode(index)
	settings_manager.Current.WindowMode = index
	settings_manager.Save()

func _on_screen_shake_toggle_toggled(button_pressed: bool) -> void:
	if not _has_current_settings():
		return
	settings_manager.Current.ScreenShakeEnabled = button_pressed
	settings_manager.Save()

func _on_visual_effects_toggle_toggled(button_pressed: bool) -> void:
	if not _has_current_settings():
		return
	settings_manager.Current.VisualEffectsEnabled = button_pressed
	settings_manager.Save()

func _on_hdr_toggle_toggled(button_pressed: bool) -> void:
	if not _has_current_settings():
		return
	settings_manager.ApplyHdr(button_pressed)
	settings_manager.Current.HdrEnabled = button_pressed
	settings_manager.Save()

func _on_back_button_pressed() -> void:
	hide()

func _has_current_settings() -> bool:
	if settings_manager == null:
		settings_manager = get_node_or_null("/root/SettingsManager")
	if settings_manager == null:
		return false
	return settings_manager.Current != null
