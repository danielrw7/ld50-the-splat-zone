extends MenuButton

func _ready():
	get_popup().connect("id_pressed", self, "selected")

signal window_selected(value)
func selected(id):
	emit_signal("window_selected", get_popup().get_item_text(id))
	
