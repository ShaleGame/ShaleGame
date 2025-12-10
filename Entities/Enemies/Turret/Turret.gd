class_name Turret extends EnemyBase

@onready var turretAxis = $TurretAxis
@onready var bulletSpawnPoint = $TurretAxis/Turret/BulletSpawn

var reachPlayer : bool = false
var axisAngle

var player
var raycast : RayCast2D

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	player = get_tree().get_first_node_in_group("Player")
	raycast = RayCast2D.new()
	raycast.collision_mask = (1 << 0) | (1 << 1) # Assuming Player is on layer 1 and Walls on layer 0
	add_child(raycast)

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta: float) -> void:
	# Get players angle relative to turret axis
	axisAngle = self.get_angle_to(player.global_position) - deg_to_rad(90)
	
	if axisAngle > deg_to_rad(60) or axisAngle < deg_to_rad(-60):
		reachPlayer = false
	else:
		reachPlayer = true
	
	# Cast raycast to see if it can reach the player
	
	raycast.global_position = turretAxis.global_position
	var dir = (player.global_position - turretAxis.global_position).normalized()
	raycast.target_position = dir * aggroRange
	raycast.enabled = true
	raycast.force_raycast_update()

	if raycast.is_colliding():
		var collider = raycast.get_collider()
		if collider.is_in_group("Player"):
			reachPlayer = true
			#print("Player in sight")
		else:
			reachPlayer = false
			#print("Player not in sight")
	else:
		reachPlayer = false
		#print("Player not in sight")
	
	# Constrain the rotation of the axis
	axisAngle = clamp(axisAngle, deg_to_rad(-60), deg_to_rad(60))
	
	# Set rotation
	turretAxis.set_rotation(axisAngle)


func _on_bullet_timer_timeout() -> void:
	if reachPlayer:
		attackPattern.execute_attack(bulletSpawnPoint.global_position, player.global_position, axisAngle)


func _on_hitbox_area_entered(area: Area2D) -> void:
	var parent = area.get_parent()

	if parent.is_in_group("Bullet"):
		var damageComponent = parent.get_node("DamageComponent")

		print("Bullet!")

		if damageComponent != null:
			take_damage(damageComponent.DamageAmount)
