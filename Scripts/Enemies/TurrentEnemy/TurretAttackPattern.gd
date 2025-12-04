class_name TurretAttackPattern extends AttackPatternBase

# Better than using an export var for now
@onready var bullet = preload("res://Entities/Enemies/Debug/Bullet.tscn")

func _perform_single_attack(_origin : Vector2, _target : Vector2, angle : float) -> void:
	var bulletChild = bullet.instantiate()
		
	bulletChild.set_rotation(angle)
		
	add_child(bulletChild)
		
	bulletChild.global_position = _origin

