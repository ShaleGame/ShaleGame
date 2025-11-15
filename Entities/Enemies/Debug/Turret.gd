class_name Turret extends Node2D

@onready var turretAxis = $TurretAxis
<<<<<<< HEAD
@onready var bullet = preload("res://Entities/Enemies/Debug/Bullet.tscn")
@onready var bulletSpawnPoint = $TurretAxis/Turret/BulletSpawn

var reachPlayer : bool = false
var axisAngle

=======

>>>>>>> a505d8e (Turret rotates to player position, also player is in global player group)
var player

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	player = get_tree().get_first_node_in_group("Player")

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta: float) -> void:
	# Get players angle relative to turret axis
<<<<<<< HEAD
	axisAngle = self.get_angle_to(player.global_position) - deg_to_rad(90)
	
	if axisAngle > deg_to_rad(60) or axisAngle < deg_to_rad(-60):
		reachPlayer = false
	else:
		reachPlayer = true
=======
	var axisAngle = self.get_angle_to(player.global_position) - deg_to_rad(90)
>>>>>>> a505d8e (Turret rotates to player position, also player is in global player group)
	
	# Constrain the rotation of the axis
	axisAngle = clamp(axisAngle, deg_to_rad(-60), deg_to_rad(60))
	
	# Set rotation
	turretAxis.set_rotation(axisAngle)
<<<<<<< HEAD


func _on_bullet_timer_timeout() -> void:
	if reachPlayer:
		var bulletChild = bullet.instantiate()
		
		bulletChild.set_rotation(axisAngle)
		
		add_child(bulletChild)
		
		bulletChild.global_position = bulletSpawnPoint.global_position
=======
>>>>>>> a505d8e (Turret rotates to player position, also player is in global player group)
