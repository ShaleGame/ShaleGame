class_name SettingsAudio
extends Control

const BUS_MASTER: int = 0
const BUS_MUSIC: int = 1
const BUS_AMBIENT: int = 2
const BUS_SFX: int = 3

@onready var master_slider: HSlider = %MasterVolumeSlider
@onready var music_slider: HSlider = %MusicVolumeSlider
@onready var ambient_slider: HSlider = %AmbientVolumeSlider
@onready var sfx_slider: HSlider = %SFXSlider

func _ready() -> void:
	master_slider.value = db_to_linear(AudioServer.get_bus_volume_db(BUS_MASTER))
	music_slider.value = db_to_linear(AudioServer.get_bus_volume_db(BUS_MUSIC))
	ambient_slider.value = db_to_linear(AudioServer.get_bus_volume_db(BUS_AMBIENT))
	sfx_slider.value = db_to_linear(AudioServer.get_bus_volume_db(BUS_SFX))

func _on_back_button_pressed() -> void:
	hide()

func _on_master_volume_slider_value_changed(value: float) -> void:
	AudioServer.set_bus_volume_db(0, linear_to_db(master_slider.value))

func _on_music_volume_slider_value_changed(value: float) -> void:
	AudioServer.set_bus_volume_db(1, linear_to_db(music_slider.value))

func _on_ambient_volume_slider_value_changed(value: float) -> void:
	AudioServer.set_bus_volume_db(2, linear_to_db(ambient_slider.value))

func _on_sfx_slider_value_changed(value: float) -> void:
	AudioServer.set_bus_volume_db(3, linear_to_db(sfx_slider.value))
