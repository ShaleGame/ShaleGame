extends Area2D

class_name GiveItemArea

@export var item_scene: PackedScene

func _ready() -> void:
	# connect here instead of in the editor because this is a hard dependency
	connect("body_entered", Callable(self, "_on_body_entered"))

func _on_body_entered(body: Node) -> void:
	if body.is_in_group("Player"):
		print("Player collided with item area")
		if item_scene:
			print("Giving item to player")
			var item_instance = item_scene.instantiate()
			item_instance.OwnerCharacter = body
			body.add_child(item_instance)
			queue_free()
