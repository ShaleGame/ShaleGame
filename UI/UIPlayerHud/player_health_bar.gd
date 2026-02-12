class_name PlayerHealthBar
extends Control

@onready var progress_bar: ProgressBar = $ProgressBar
@onready var icon: TextureRect = $CharacterIcon
var _half_length = 50
var _full_length = 100

func update_health(value: int) -> void:
	progress_bar.value = value
	
func set_health_bar_half() -> void:
	progress_bar.size.x = _half_length

func set_health_bar_full() -> void:
	progress_bar.size.x = _full_length
	
func hide_health_bar() -> void:
	visible = false;
func show_health_bar() -> void:
	visible = true;
