extends Control
@onready var settings_menu : SettingsSelector = $SettingSelectMenu
@onready var save_loader : SaveLoader = $SaveLoader
func _on_start_buitton_pressed() -> void:
	save_loader.OpenSaveLoader()
	
	
func _on_settings_button_pressed() -> void:
	settings_menu.open_settings()


func _on_exit_button_pressed() -> void:
	get_tree().quit(0)
