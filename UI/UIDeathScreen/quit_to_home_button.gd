extends Button
@onready var scene_manager : SceneManager 
func _pressed():
	scene_manager = get_node("/root/SceneManager")
	scene_manager.LoadSceneSync("res://UI/UIMainMenu/MainMenu.tscn",true)
