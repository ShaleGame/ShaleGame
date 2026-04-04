class_name SettingsVideo
extends Control

@onready var display_mode_dropdown: OptionButton = %DisplayModeDropdown
@onready var screen_shake_toggle: CheckBox = %ScreenShakeToggle
@onready var visual_effects_toggle: CheckBox = %VisualEffectsToggle
@onready var hdr_toggle: CheckBox = %HDRToggle
@onready var settings_manager: SettingsManager = $/root/SettingsManager

func _ready() -> void:
	settings_manager = $/root/SettingsManager

	if settings_manager.Current == null:
		return

	display_mode_dropdown.select(settings_manager.Current.WindowMode)
	screen_shake_toggle.button_pressed = settings_manager.Current.ScreenShakeEnabled
	visual_effects_toggle.button_pressed = settings_manager.Current.VisualEffectsEnabled
	hdr_toggle.button_pressed = settings_manager.Current.HdrEnabled

func _on_display_mode_dropdown_item_selected(index: int) -> void:
	if settings_manager.Current == null:
		return
	settings_manager.ApplyWindowMode(index)
	settings_manager.Current.WindowMode = index
	settings_manager.Save()

func _on_screen_shake_toggle_toggled(button_pressed: bool) -> void:
	if settings_manager.Current == null:
		return
	settings_manager.Current.ScreenShakeEnabled = button_pressed
	settings_manager.Save()

func _on_visual_effects_toggle_toggled(button_pressed: bool) -> void:
	if settings_manager.Current == null:
		return
	settings_manager.Current.VisualEffectsEnabled = button_pressed
	settings_manager.Save()

func _on_hdr_toggle_toggled(button_pressed: bool) -> void:
	if settings_manager.Current == null:
		return
	settings_manager.ApplyHdr(button_pressed)
	settings_manager.Current.HdrEnabled = button_pressed
	settings_manager.Save()

func _on_back_button_pressed() -> void:
	hide()
