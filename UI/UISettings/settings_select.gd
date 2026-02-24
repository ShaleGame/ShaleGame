extends Control


func _on_audio_buitton_pressed() -> void:
	get_tree().change_scene_to_file("res://UI/UISettings/SettingsAudio.tscn")


func _on_video_button_pressed() -> void:
	get_tree().change_scene_to_file("res://UI/UISettings/SettingsVideo.tscn")

func _on_keybinds_button_pressed() -> void:
	get_tree().change_scene_to_file("res://UI/UISettings/SettingsKeybinds.tscn")

func _on_back_button_pressed() -> void:
	get_tree().change_scene_to_file("res://Scenes/MainMenu.tscn")
