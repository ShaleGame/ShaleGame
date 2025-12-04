class_name Turret extends EnemyBase

@onready var turretAxis = $TurretAxis
@onready var bulletSpawnPoint = $TurretAxis/Turret/BulletSpawn

var reachPlayer : bool = false
var axisAngle

var player

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	player = get_tree().get_first_node_in_group("Player")

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta: float) -> void:
	# Get players angle relative to turret axis
	axisAngle = self.get_angle_to(player.global_position) - deg_to_rad(90)
	
	if axisAngle > deg_to_rad(60) or axisAngle < deg_to_rad(-60):
		reachPlayer = false
	else:
		reachPlayer = true
	
	# Constrain the rotation of the axis
	axisAngle = clamp(axisAngle, deg_to_rad(-60), deg_to_rad(60))
	
	# Set rotation
	turretAxis.set_rotation(axisAngle)


func _on_bullet_timer_timeout() -> void:
	if reachPlayer:
		attackPattern.execute_attack(bulletSpawnPoint.global_position, player.global_position, axisAngle)


func _on_hitbox_area_entered(area: Area2D) -> void:
	print("Damaged")
	pass
