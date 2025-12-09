extends Node

func _on_health_component_health_changed(oldHealth: int):
	if 	get_parent().CurrentHealth <= 0:
		get_tree().change_scene_to_file("res://Scenes/DeathScreen.tscn")
		
