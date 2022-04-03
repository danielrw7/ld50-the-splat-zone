extends Label

func _on_MainMenu_HideFPS():
	visible = false
func _on_MainMenu_WriteFPS(fps):
	visible = true
	text = "FPS: " + str(fps)
