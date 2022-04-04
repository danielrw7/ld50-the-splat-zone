extends VBoxContainer

func _on_TileMap_pause_play(_is_paused):
	visible = true

signal select(color)
signal select_mode(painting)

func _on_SelectBlue_pressed():
	emit_signal("select", 2)
func _on_SelectRed_pressed():
	emit_signal("select", 4)
func _on_SelectGreen_pressed():
	emit_signal("select", 9)

func _on_SelectBrush_pressed():
	emit_signal("select_mode", true)
func _on_SelectRect_pressed():
	emit_signal("select_mode", false)
