extends State

var direction := -1

var floor_check: RayCast2D
var wall_check: RayCast2D
var sprite: AnimatedSprite2D
var controller

func enter(_previous_state) -> Node:
    _cache_nodes()
    return null

func process(_delta: float) -> Node:
    if context == null:
        return null

    if floor_check == null or wall_check == null:
        _cache_nodes()

    if wall_check and wall_check.is_colliding():
        _flip()

    if floor_check and not floor_check.is_colliding():
        _flip()

    if controller and controller.has_method("SetMovementInput"):
        controller.SetMovementInput(Vector2(direction, 0))

    if sprite and sprite.animation != "Walk":
        sprite.play("Walk")

    return null

func _cache_nodes() -> void:
    floor_check = context.get_node_or_null("FloorCheck")
    wall_check = context.get_node_or_null("WallCheck")
    sprite = context.get_node_or_null("WalkingSprite")
    controller = context.Controller

func _flip() -> void:
    direction *= -1

    if sprite:
        sprite.scale.x *= -1
        sprite.play("Walk")

    if floor_check:
        floor_check.position.x *= -1

    if wall_check:
        wall_check.position.x *= -1
        wall_check.target_position.x *= -1
