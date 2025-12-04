class_name EnemyBase extends CharacterBody2D

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

signal onDeath()

func _ready():
	currentHealth = maxHealth
	return

func take_damage(amount : int) -> void:
	currentHealth -= amount
	if currentHealth <= 0:
		die()

func die() -> void:
	onDeath.emit()
	queue_free()

func move_behavior(_delta : float) -> void:
	# Placeholder for movement logic
	pass
