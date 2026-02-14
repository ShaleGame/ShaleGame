extends Node

func _on_health_component_health_changed(oldHealth: int):
	if 	get_parent().CurrentHealth <= 0:
		var scene_file := &"res://Scenes/DeathScreen.tscn"

		# call deferred since this may occur during a physics callback
		get_tree().call_deferred("change_scene_to_file", scene_file)

