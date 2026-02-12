class_name HealthBars
extends Control
#variables that are used by the PlayerHud script
#We want to modify the nodes in Healthbars so we can
#pass them as variables
@onready var main_health_bar: PlayerHealthBar = %PlayerHealthBar
@onready var clone_health_bar: PlayerHealthBar = %CloneHealthBar
