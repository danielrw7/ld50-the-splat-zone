extends Label

var score = 0
func _on_TileMapWrapper_score():
	score += 1
	text = "Score: " + str(score)

func _on_MainMenu_start(reset):
	visible = true
	if reset:
		score = -1
		_on_TileMapWrapper_score()
