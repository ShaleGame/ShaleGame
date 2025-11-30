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