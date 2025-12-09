class_name TurretBullet extends Node2D

@export var damage : int = 10
@export var speed : int = 100

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	
	# Get forward direction vector and rotate it
	var forwardDirection = Vector2(1,0).rotated(rotation + deg_to_rad(90))
	
	# Calculate the movement amount
	var movement = forwardDirection * speed * delta
	
	# Apply
	global_position += movement

func _on_lifespan_timeout() -> void:
	self.queue_free()
