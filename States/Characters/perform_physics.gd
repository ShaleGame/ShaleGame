# this is an example of how you can prototype states with GDScript or make
# simple, small modular states without needing to create entire C# classes for
# them.

class_name PerformPhysics
extends State

var character: CharacterBody2D:
    get:
        return context

func physics_process(_delta: float) -> Node:
    character.move_and_slide()
    return null
