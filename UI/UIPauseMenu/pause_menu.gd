extends Control
@export var character: Character
@onready var scene_manager: SceneManager
@onready var settings_menu : SettingsSelector
@onready var save_manager: SaveManager
var settings_manager = null
@onready var pause_panel: PanelContainer = $PanelContainer
@onready var pause_blur: ColorRect = $ColorRect
var clone: Character
func _ready():
	$AnimationPlayer.play("RESET")
	hide()
	character.Cloneable.connect(&"CharacterSplitPost", _on_character_split)
	settings_manager = get_node_or_null("/root/SettingsManager")
	scene_manager = get_node("/root/SceneManager")
	settings_menu = $SettingSelectMenu
	save_manager = get_node("/root/SaveManager")
	scene_manager = get_node("/root/SceneManager")
#pause menu functions
func pauseLevel():
	show()
	get_tree().paused = true
	if _is_visual_effects_enabled():
		$AnimationPlayer.play("blur")
	else:
		pause_panel.modulate = Color(1, 1, 1, 1)
		var blur_material := pause_blur.material as ShaderMaterial
		if blur_material != null:
			blur_material.set_shader_parameter("lod", 0.0)

func restartLevel():

	var save: SaveFile = save_manager.ReloadCurrentSave()

	scene_manager.LoadSceneFromSave(save, true)

func resume():
	get_tree().paused = false
	if _is_visual_effects_enabled():
		$AnimationPlayer.play_backwards("blur")
	else:
		pause_panel.modulate = Color(1, 1, 1, 0)
		var blur_material := pause_blur.material as ShaderMaterial
		if blur_material != null:
			blur_material.set_shader_parameter("lod", 0.0)
	hide()
func _on_character_split(_orig_character, clone_character: Character):
	clone = clone_character
	if settings_menu != null:
		settings_menu.close_settings()
	clone_character.get_node("%PauseMenu").queue_free()

func openSettings(): #not implemented
	settings_menu.open_settings()

func quitToHome():
	scene_manager.LoadSceneSync("res://UI/UIMainMenu/MainMenu.tscn",true)
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

func _is_visual_effects_enabled() -> bool:
	if settings_manager == null:
		settings_manager = get_node_or_null("/root/SettingsManager")
	if settings_manager == null:
		return true
	if settings_manager.Current == null:
		return true
	return settings_manager.Current.VisualEffectsEnabled
