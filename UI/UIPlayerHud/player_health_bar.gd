class_name PlayerHealthBar
extends Control

@onready var progress_bar: TextureProgressBar = $TextureProgressBar
@onready var icon: TextureRect = $CharacterIcon
var _full_length = 100

func update_health(value: int) -> void:
	progress_bar.value = value
	
func set_health_bar_half(max_health: int) -> void:
	progress_bar.size.x = _full_length
	progress_bar.max_value = max_health

func set_health_bar_full(max_health: int) -> void:
	progress_bar.size.x = _full_length
	progress_bar.max_value = max_health
	
func hide_health_bar() -> void:
	visible = false;
func show_health_bar() -> void:
	visible = true;
