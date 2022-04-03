extends CheckBox

func _ready():
	if OS.get_name() == "HTML5":
		queue_free()
