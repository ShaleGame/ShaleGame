extends Control

func _on_start_buitton_pressed() -> void:
	get_tree().change_scene_to_file("res://Scenes/CaveLevel.tscn")
	
	
func _on_settings_button_pressed() -> void:
	get_tree().change_scene_to_file("res://UI/UISettings/SettingsSelect.tscn")


func _on_exit_button_pressed() -> void:
	get_tree().quit(0)
