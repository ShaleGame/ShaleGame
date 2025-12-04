class_name WalkingEnemy extends EnemyBase

var direction : int = -1  # 1 for right, -1 for left

@onready var floor_check : RayCast2D = $FloorCheck
@onready var wall_check : RayCast2D = $WallCheck

@onready var sprite : AnimatedSprite2D = $WalkingSprite

func _process(delta: float) -> void:
	move_behavior(delta)

	if sprite.animation != "Walk":
		sprite.play("Walk")

func move_behavior(_delta : float) -> void:
	velocity.x = direction * movementSpeed

	if wall_check.is_colliding():
		_flip()
	
	if not floor_check.is_colliding():
		_flip()
	
	move_and_slide()

func _flip() -> void:
	direction *= -1

	# Flip the sprite horizontally
	$WalkingSprite.scale.x *= -1
	sprite.play("Walk")

	# Flip raycasts horizontally
	floor_check.position.x *= -1
	wall_check.position.x *= -1
	wall_check.target_position.x *= -1

func _on_hitbox_area_entered(area: Area2D) -> void:
	var parent = area.get_parent()

	if parent.is_in_group("Bullet"):
		var damageComponent = parent.get_node("DamageComponent")

		print("Bullet!")

		if damageComponent != null:
			take_damage(damageComponent.DamageAmount)
