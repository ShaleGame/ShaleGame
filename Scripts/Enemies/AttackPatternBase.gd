class_name AttackPatternBase extends Node

@export var patternID : int = 0

enum patternType {
	SINGLE,
	BURST,
	SPREAD,
	CONE,
	SNIPER
}

@export var pType : patternType = patternType.SINGLE

@export var homing : bool = false

@export var speed : float = 300.0

func execute_attack(_origin : Vector2, _target : Vector2) -> void:
	# Placeholder for attack execution logic
	pass