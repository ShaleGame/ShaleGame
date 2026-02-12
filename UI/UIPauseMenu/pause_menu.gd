extends Control
@export var character: Character

var clone: Character

func _ready():
	$AnimationPlayer.play("RESET")
	hide()
	character.Cloneable.connect(&"CharacterSplitPost", _on_character_split)
	
#pause menu functions
func pauseLevel():
	show()
	get_tree().paused = true
	$AnimationPlayer.play("blur")

func restartLevel():
	get_tree().reload_current_scene()
	
func resume():
	get_tree().paused = false
	$AnimationPlayer.play_backwards("blur")
	hide()
func _on_character_split(_orig_character, clone_character: Character):
	clone = clone_character
	clone_character.get_node("%PauseMenu").queue_free()
	
func openSettings(): #not implemented
	return

func quitToHome():
	get_tree().change_scene_to_file("res://Scenes/MainMenu.tscn")
	
#test for keys
func testEsc():
	if Input.is_action_just_pressed("escape") and get_tree().paused == false:
		pauseLevel()
	elif Input.is_action_just_pressed("escape") and get_tree().paused == true:
		resume()
		
func _process(_delta):
	testEsc()
#button signal function overrides
func _on_resume_level_button_pressed():
	resume()
	
func _on_settings_button_pressed():
	openSettings()

func _on_quit_to_home_button_pressed():
	resume()
	quitToHome()

func _on_restart_pressed():
	resume()
	restartLevel()
