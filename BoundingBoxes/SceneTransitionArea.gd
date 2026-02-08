extends Area2D

class_name SceneTransitionArea

@export_file("*.tscn") var target_scene_path: String

func _ready() -> void:
    # connect here instead of in the editor because this is a hard dependency
    connect("body_entered", Callable(self, "_on_body_entered"))

func _on_body_entered(body: Node) -> void:
    if body.is_in_group("Player"):
        get_tree().change_scene_to_file(target_scene_path)
