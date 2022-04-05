extends Control

func _ready():
	_on_Speed_change_speed()

func swat(tile):
	var dists = [
		tile.x,
		tile.y,
		22 - tile.y - 5,
	]
	var smallest_i = 0
	for i in range(1, 3):
		if dists[i] < dists[smallest_i]:
			smallest_i = i

	if smallest_i == 0:
		$Sprite.rotation_degrees = 180
	if smallest_i == 1:
		$Sprite.rotation_degrees = -90
	if smallest_i == 2:
		$Sprite.rotation_degrees = 90

	rect_position = tile * 24 * rect_scale
	# visible = true
	$AnimationPlayer.play("Swat")

func _on_Speed_change_speed(_dir = null):
	yield(get_tree().create_timer(0.001), "timeout")
	$AnimationPlayer.playback_speed = min(4, 1 / get_parent().get_parent().get("TimePerTick"))
