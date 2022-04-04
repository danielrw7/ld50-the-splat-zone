extends RichTextLabel

func render():
	bbcode_text = ("Score: " + str(score) + "\n" + "Alive:\n" +
		"[color=#f1cd1c]/[/color]" + str(red) +
		"[color=#d93434]/[/color]" + str(blue) +
		"[color=#136a1e]/[/color]" + str(green))

var score = 0
func _on_TileMapWrapper_score():
	score += 1
	render()

func _on_MainMenu_start(reset):
	visible = true
	if reset:
		score = -1
		_on_TileMapWrapper_score()

var red = 0
var blue = 0
var green = 0
func _on_TileMapWrapper_alive_count(_red, _blue, _green):
	red = _red
	blue = _blue
	green = _green
	render()
