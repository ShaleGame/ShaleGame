extends Button

func _pressed() -> void:
	var save_manager: SaveManager = get_node("/root/SaveManager")
	var scene_manager: SceneManager = get_node("/root/SceneManager")

	var save: SaveFile = save_manager.ReloadCurrentSave()

	scene_manager.LoadSceneFromSave(save)
