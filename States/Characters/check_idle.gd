class_name MoveStateCheckIdle
extends State

@export var idle_state: Node

var character: CharacterBody2D:
    get:
        return context

func process(_delta: float) -> Node:
    var movement_input: Vector2 = %ControllerComponent.MovementInput

    if movement_input.is_zero_approx():
        return idle_state

    return null
