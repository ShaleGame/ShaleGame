extends Area2D

class_name GiveItemArea

@export var item_scene: PackedScene

func _ready() -> void:
    # connect here instead of in the editor because this is a hard dependency
    connect("body_entered", Callable(self, "_on_body_entered"))

func _on_body_entered(body: Node) -> void:
    if body.is_in_group("Player"):
        if body.Cloneable.IsClone:
            return
        print("Player collided with item area")
        if item_scene:
            var inventory: InventoryComponent = body.Inventory
            if inventory.HasWeaponFromScene(item_scene):
                queue_free()
                return
            print("Giving item to player")
            var item_instance = item_scene.instantiate()
            var hud = body.get_node("%PlayerHud")
            var item_get = hud.get_node("GetItem")
            item_get.connect("CloseWeaponUI", _on_close_weapon_ui)
            item_get.Item = item_instance.ItemData
            item_get.StartItemGet()
            inventory.add_child(item_instance)
            inventory.EquipWeapon(item_instance, true)

func _on_close_weapon_ui() -> void:
    print("Close signal received")
    queue_free()
