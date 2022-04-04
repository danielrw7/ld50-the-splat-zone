extends Button

export(String) var url

func _on_Button_pressed():
	OS.shell_open(url)
