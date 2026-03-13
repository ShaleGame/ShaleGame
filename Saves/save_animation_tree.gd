extends AnimationTree


func _on_interactable_interacted() -> void:
	var state_machine = get("parameters/playback")
	state_machine.travel("use_save")

