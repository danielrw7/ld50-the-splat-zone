extends RichTextLabel

signal change_speed(dir)
export(NodePath) var wrapper_path
onready var wrapper = get_node(wrapper_path)
onready var default_speed = wrapper.TimePerTick

func _on_RichTextLabel_meta_clicked(meta):
	if meta == "Default":
		wrapper.TimePerTick = default_speed
		return
	var dir = 1 if meta == "+" else -1
	emit_signal("change_speed", dir)
	wrapper.TimePerTick = clamp(wrapper.TimePerTick - dir * 0.1, 0.05, 2.05)
