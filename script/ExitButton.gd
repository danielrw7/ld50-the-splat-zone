extends Button

func _on_ExitButton_pressed():
	get_tree().quit()

func _ready():
	if OS.get_name() == "HTML5":
		queue_free()
