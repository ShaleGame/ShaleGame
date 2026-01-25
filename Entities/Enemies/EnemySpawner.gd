class_name EnemySpawner extends Node2D

var nearPlayer = false

var enemySpawned = false

@export var spawnRange : int = 1000
@export var enemyScene : PackedScene

@onready var respawnTimer = $Timer

var player
var enemy

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
    player = get_tree().get_first_node_in_group("Player")

func _on_timer_timeout() -> void:
    enemySpawned = false

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
    var distance = global_position.distance_to(player.global_position)

    if distance < spawnRange:
        nearPlayer = true
    else:
        nearPlayer = false

    if nearPlayer and !enemySpawned:
        enemy = enemyScene.instantiate()

        self.get_parent().get_parent().add_child(enemy)

        enemy.global_position = global_position

        #enemy.onDeath.connect(enemyRespawn)

        enemySpawned = true
    elif !nearPlayer:
        if enemy and enemy is EnemyBase:
            #enemy.disconnect("onDeath", enemyRespawn)
            enemySpawned = false
            enemy.queue_free()
    else:
        pass

func enemyRespawn() -> void:
    respawnTimer.start()
