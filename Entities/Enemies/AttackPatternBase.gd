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

# Use an onready function in the derived class to preload the projectile scene

func execute_attack(_origin : Vector2, _target : Vector2, angle : float) -> void:
	
	if pType == patternType.SINGLE:
		_perform_single_attack(_origin, _target, angle)
	elif pType == patternType.BURST:
		_perform_burst_attack(_origin, _target, angle)
	elif pType == patternType.SPREAD:
		_perform_spread_attack(_origin, _target, angle)
	elif pType == patternType.CONE:
		_perform_cone_attack(_origin, _target, angle)
	elif pType == patternType.SNIPER:
		_perform_sniper_attack(_origin, _target, angle)

func _perform_single_attack(_origin : Vector2, _target : Vector2, angle : float) -> void:
	# Placeholder for single attack logic
	pass

func _perform_burst_attack(_origin : Vector2, _target : Vector2, angle : float) -> void:	
	# Placeholder for burst attack logic
	pass

func _perform_spread_attack(_origin : Vector2, _target : Vector2, angle : float) -> void:
	# Placeholder for spread attack logic
	pass

func _perform_cone_attack(_origin : Vector2, _target : Vector2, angle : float) -> void:
	# Placeholder for cone attack logic
	pass

func _perform_sniper_attack(_origin : Vector2, _target : Vector2, angle : float) -> void:
	# Placeholder for sniper attack logic
	pass