class_name SettingsSelector
extends Control

@onready var menu_particles: GPUParticles2D = $Node2D/GPUParticles2D
var settings_manager = null

#create submenus
var setting_audio
var settings_video
var settings_keybinds
var canvas_layer
var submenus = []
var settings_is_open = false

const SettingsAudioScene := preload("res://UI/UISettings/SettingsAudio.tscn")
const SettingsVideoScene := preload("res://UI/UISettings/SettingsVideo.tscn")
const SettingsKeybindsScene := preload("res://UI/UISettings/SettingsKeybinds.tscn")

func _ready() -> void:
	settings_manager = get_node_or_null("/root/SettingsManager")
	if settings_manager != null and settings_manager.Current != null:
		menu_particles.emitting = settings_manager.Current.VisualEffectsEnabled
	else:
		menu_particles.emitting = true

func open_settings() -> void:
	setting_audio = SettingsAudioScene.instantiate()
	settings_video = SettingsVideoScene.instantiate()
	settings_keybinds = SettingsKeybindsScene.instantiate()
	canvas_layer = CanvasLayer.new()
	submenus = [setting_audio, settings_video, settings_keybinds]
	
	canvas_layer.layer = 10
	for submenu in submenus:
		submenu.hide()
		canvas_layer.add_child(submenu)
	add_child(canvas_layer)
	settings_is_open = true
	show()

func _on_audio_button_pressed() -> void:
	if settings_is_open:
		setting_audio.show()
	
func _on_video_button_pressed() -> void:
	if settings_is_open:
		settings_video.show()
func _on_keybinds_button_pressed() -> void:
	if settings_is_open:
		settings_keybinds.show()

func _on_back_button_pressed() -> void:
	close_settings()
	
func close_settings() -> void:
	if settings_is_open:
		for submenu in submenus:
			submenu.queue_free()
		canvas_layer.queue_free()
		settings_is_open = false
		hide()
		
