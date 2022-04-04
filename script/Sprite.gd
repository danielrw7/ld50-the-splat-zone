extends Sprite

export var move_speed = 6.5;
var next_pos = Vector2.ZERO;

func parent_speed(_speed):
	move_speed = max(4.5 / _speed, 5)

func _process(delta):
	if (dead or position == next_pos or next_pos == Vector2.ZERO):
		return
	position += (next_pos - position) * clamp(delta * move_speed, 0, 1)
	var diff = next_pos - position
	diff = Vector2(abs(diff.x), abs(diff.y))
	if (diff.x < 0.05 and diff.y < 0.05):
		position = next_pos

func set_next_pos(new_pos: Vector2):
	var old_pos = Vector2(next_pos)
	next_pos = new_pos
	if position != old_pos and old_pos != Vector2.ZERO:
		position = old_pos

export var dead = false
func splat(color = null):
	dead = true
	if color == 2:
		$AnimationPlayer.play("SplatBlue")
	if color == 4:
		$AnimationPlayer.play("SplatRed")
	if color == 9:
		$AnimationPlayer.play("SplatGreen")

func score():
	rotation_degrees = 180
	set_next_pos(next_pos + Vector2.UP)
	$AnimationPlayer.play("Score")
