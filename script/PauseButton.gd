extends Button

func _on_TileMap_pause_play(is_paused):
	if is_paused:
		text = "Resume"
	else:
		text = "Pause"
