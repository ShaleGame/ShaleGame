class_name ApplyGravity
extends State

var character: CharacterBody2D:
    get:
        return context

func physics_process(delta: float) -> Node:
    var gravity: Vector2 = ProjectSettings \
        .get_setting("physics/2d/default_gravity_vector") * \
        ProjectSettings.get_setting("physics/2d/default_gravity")

    character.velocity += gravity * delta
    return null
