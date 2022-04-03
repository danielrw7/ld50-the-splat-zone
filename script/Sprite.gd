extends Sprite

export var speed = 7;
var next_pos = Vector2.ZERO;

func _process(delta):
	if (dead or position == next_pos or next_pos == Vector2.ZERO):
		return
	position += (next_pos - position) * delta * speed
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
