extends Node

func _on_health_component_health_changed(_old_health: int):
	var health_component: HealthComponent = get_parent()
	if health_component.CurrentHealth <= 0:
		health_component.get_parent().queue_free()
