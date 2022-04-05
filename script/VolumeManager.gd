extends HBoxContainer

func _on_MainMenu_volume_changed(music, effects):
	$TileMapWrapper/MusicPlayer.volume_db = music
	$TileMapWrapper/DeathPlayer.volume_db = music
	$TileMapWrapper/ScorePlayer.volume_db = linear2db(db2linear(effects) * 0.333 * 2)
	$TileMapWrapper/Control/Swatter/Hit.volume_db = effects
	$TileMapWrapper/Control/Swatter/Miss.volume_db = effects
