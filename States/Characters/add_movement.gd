class_name AddMovement
extends GdState

@export var idle_state: Node

var character: CharacterBody2D:
	get:
		return context

func physics_process(_delta: float) -> Node:
	var movement_input: Vector2 = %ControllerComponent.MovementInput
	if movement_input.is_zero_approx():
		return idle_state
	character.velocity.x = movement_input.x * 40
	return null
