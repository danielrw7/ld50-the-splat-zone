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

func _ready():
	if OS.get_name() == "HTML5":
		queue_free()

func _on_VSyncToggle_toggled(button_pressed):
	OS.vsync_enabled = button_pressed

func _on_Fullscreen_pressed():
	_on_MenuButton_window_selected("Fullscreen")

func _on_Windowed_Fullscreen_pressed():
	_on_MenuButton_window_selected("Borderless Fullscreen")

func _on_Windowed_pressed():
	_on_MenuButton_window_selected("Windowed")
