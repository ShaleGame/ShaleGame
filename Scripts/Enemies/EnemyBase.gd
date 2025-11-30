class_name EnemyBase extends Node2D

@export var ID : int = 0
@export var enemyName : String = "Enemy"
@export var maxHealth : int = 100
@export var currentHealth : int = 100
@export var movementSpeed : float = 100.0

enum enemyClass {
	MELEE,
	RANGED,
	SUPPORT,
	BOSS
}

@export var eClass : enemyClass = enemyClass.MELEE

@export var attackPattern : AttackPatternBase

@export var aggroRange : float = 300.0