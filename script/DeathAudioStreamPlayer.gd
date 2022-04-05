extends AudioStreamPlayer

func _on_TileMapWrapper_game_over():
	yield(get_tree().create_timer(0.1), "timeout")
	play(0)
