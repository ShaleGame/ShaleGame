extends Control
@onready var settings_menu : SettingsSelector = %SettingSelectMenu
@onready var save_loader : SaveLoader = %SaveLoader
@onready var menu_particles: GPUParticles2D = $Node2D/GPUParticles2D

func _ready() -> void:
	var settings_manager: SettingsManager = $/root/SettingsManager
	if settings_manager.Current != null:
		menu_particles.emitting = settings_manager.Current.VisualEffectsEnabled
	else:
		menu_particles.emitting = true

func _on_start_buitton_pressed() -> void:
	save_loader.OpenSaveLoader()


func _on_settings_button_pressed() -> void:
	settings_menu.open_settings()


func _on_exit_button_pressed() -> void:
	get_tree().quit(0)
