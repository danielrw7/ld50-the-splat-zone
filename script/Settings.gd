extends VBoxContainer

func _on_MenuButton_window_selected(value):
	OS.window_fullscreen = false
	OS.window_borderless = false
	OS.window_maximized = false
	match value:
		"Fullscreen":
			OS.window_fullscreen = true
		"Borderless Fullscreen":
			OS.window_borderless = true
			OS.window_maximized = true

func _on_VSyncToggle_toggled(button_pressed):
	OS.vsync_enabled = button_pressed
