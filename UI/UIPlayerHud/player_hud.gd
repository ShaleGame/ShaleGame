extends Node
#creates health_bar variable of type HealthBar and assigns it the value
#of the HealthBars Node when the PlayerHud Node gets added to the scean tree. 
@onready var health_bars: HealthBars = $HealthBars
#creates characer varable of type Character, which is a class. @export 
# allows godot to define the object instance of character at runtime
@export var character: Character 
var clone: Character
#Summary
#when PlayerHud is loaded into the main scean tree
#_on_main_health_changed and _on_character_split
# are connected to signals emmited by HealthComponet
# and the ClonableComponent in the Character Node
#
func _ready() -> void:
	#varName.attribte.connect(&"signalName", functionToBeCalledOnSignalEmmission)
	character.Health.connect(&"HealthChanged", _on_main_health_changed)
	character.Cloneable.connect(&"CharacterSplitPost", _on_character_split)
	character.Cloneable.connect(&"CharacterMerged", _on_character_merge)
#_on_main_health_changed as parameter _old_health to catch
# the emmited parameter of the HealthChanged signal 
# new new_health is typed to int since the value
# its reading is also typed to int
#
#summary
#updatest the main MainPlayerHealth scene to display the current health of the player
func _on_main_health_changed(_old_health: int):
	var new_health: int = character.Health.CurrentHealth
	health_bars.main_health_bar.update_health(new_health)

#summary
#Updates the CloneHealthBar Scene to display to current health of the clone
func _on_clone_health_changed(_old_health: int):
	var new_health: int = clone.Health.CurrentHealth
	health_bars.clone_health_bar.update_health(new_health)
#Summary
#Is called when the CharacterSplitPost signal is emmited
#assigns the clone var to the instance of the clone that is made duing runtime
#removes the PlayerHud node from the clone's scene tree. 
#connects _on_clone_health_changed with the clone's HealthChanged signal
func _on_character_split(_orig_character, clone_character: Character):
	clone = clone_character
	clone_character.get_node("%PlayerHud").queue_free()
	clone_character.Health.connect(&"HealthChanged", _on_clone_health_changed)
	health_bars.main_health_bar.set_health_bar_half(character.Health.MaxHealth)
	health_bars.clone_health_bar.set_health_bar_half(clone.Health.MaxHealth)
	_on_clone_health_changed(1) #set clone health to current health
	_on_main_health_changed(1) #set player health to current health
	health_bars.clone_health_bar.show_health_bar()
	


func _on_character_merge(_orig_character: Character):
	#hide the clonehealth bar
	health_bars.main_health_bar.set_health_bar_full(character.Health.MaxHealth)
	health_bars.clone_health_bar.hide_health_bar()
	_on_main_health_changed(1) #ensures the most upto date health value is shown. 
