extends GdState

func process(_delta: float) -> Node:
	var character: Character = context as Character
	var controller: EnemyController = character.Controller
	var enemy_component: EnemyComponent = character.get_node("EnemyComponent")

	if enemy_component.HasTarget:
		controller.SetTargetInput(enemy_component.TargetPosition)
		controller.SetPrimaryAttack(true)

	return null

func exit(_next_state: Node) -> void:
	var character: Character = context as Character
	var controller: EnemyController = character.Controller

	controller.SetPrimaryAttack(false)
	controller.SetTargetInput(Vector2.ZERO)
