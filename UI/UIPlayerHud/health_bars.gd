class_name HealthBars
extends Control
#variables that are used by the PlayerHud script
#We want to modify the nodes in Healthbars so we can
#pass them as variables
@onready var main_health_bar: PlayerHealthBar = %PlayerHealthBar
@onready var clone_health_bar: PlayerHealthBar = %CloneHealthBar
@onready var heal_pool_bar: TextureProgressBar = %HealPoolBar
@onready var character_icon: TextureRect = %CharacterIcon
@onready var margin_container_2: MarginContainer = %MarginContainer2

func set_icon_texture(texture: Texture2D) -> void:
	character_icon.texture = texture

func show_margin_container_2() -> void:
	margin_container_2.visible = true

func hide_margin_container_2() -> void:
	margin_container_2.visible = false
