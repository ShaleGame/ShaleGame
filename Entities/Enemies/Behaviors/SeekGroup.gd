extends GdState

@export var found_state: Node
@export var not_found_state: Node
@export var enemy_component: EnemyComponent
@export var raycast: RayCast2D
@export var target_group := "Player"
@export var detection_range := 200.0

func physics_process(_delta: float) -> Node:
	var targets = get_tree().get_nodes_in_group(target_group)

	# first do distance check
	for target in targets:
		if target is Node2D:
			var cur_pos: Vector2 = context.global_position
			var target_pos: Vector2 = target.global_position
			if cur_pos.distance_to(target_pos) <= detection_range:
				# do raycast check
				var direction = cur_pos.direction_to(target_pos)
				raycast.target_position = direction * detection_range
				raycast.force_raycast_update()

				if raycast.is_colliding():
					var collider = raycast.get_collider()
					if collider == target:
						# found target
						enemy_component.HasTarget = true
						enemy_component.TargetPosition = direction
						return found_state

	# if not found, clear target
	enemy_component.HasTarget = false
	enemy_component.TargetPosition = Vector2.ZERO
	return not_found_state
