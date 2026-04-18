extends Line2D

# Maximum number of points kept in the trail
@export var max_points: int = 20

# How many points to add per second. The trail will add a new point every
# (1 / frequency) seconds.
@export var frequency: int = 10

var is_dead: bool = false

var _current_trail_sleep_time: float = 0.0
var _trail_sleep_time: float = 0.0
var _parent: Node2D = null

func _ready() -> void:
	# Avoid divide-by-zero if frequency hasn't been set in the editor.
	if frequency > 0:
		_trail_sleep_time = 1.0 / frequency
	else:
		_trail_sleep_time = 0.0

	_parent = get_parent() as Node2D

func _process(delta: float) -> void:
	# If marked dead, shrink the trail until empty and stop updating.
	if is_dead and points.size() > 0:
		remove_point(0)
		return

	var parent_pos: Vector2 = _parent.global_position

	_current_trail_sleep_time -= delta
	if _current_trail_sleep_time > 0.0:
		if points.size() > 0:
			# Update the last point to follow the parent smoothly between samples.
			set_point_position(points.size() - 1, parent_pos)
		return

	_current_trail_sleep_time = _trail_sleep_time

	add_point(parent_pos)

	# Trim oldest points if we exceeded the maximum.
	while points.size() > max_points:
		remove_point(0)
