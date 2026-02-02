extends GdState

@export var axis_node: Node2D
@export var angle_offset_deg := -90.0

func process(_delta: float) -> Node:
	var character = context as Character

	if character.Controller.Target != Vector2.ZERO:
		var target: Vector2 = character.Controller.Target
		var axis_angle: float = target.angle()
		axis_angle += deg_to_rad(angle_offset_deg)
		axis_node.rotation = axis_angle

	return null
