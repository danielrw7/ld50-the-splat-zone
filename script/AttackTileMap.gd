extends TileMap

var flashing = false

func set_flashing(should = true):
	if flashing and not should:
		get_node("../AnimationPlayer").stop(true)
	elif not flashing and should:
		get_node("../AnimationPlayer").play("Flash")
		get_node("../AnimationPlayer").seek(0)

func _ready():
	#place_future_attack(Vector2(10, 10))
	set_flashing(true)

var attackCount = 0

func attack_count_inc():
	if attackCount == 0:
		set_flashing(true)
	attackCount += 1
func attack_count_dec():
	attackCount -= 1
	if attackCount == 0:
		set_flashing(false)
