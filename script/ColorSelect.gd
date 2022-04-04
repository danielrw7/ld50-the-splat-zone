extends Control

func select(is_selected):
	$ReferenceRect.editor_only = !is_selected

export(Texture) var texture = null

func _ready():
	select(false)
	if texture:
		$ReferenceRect/Sprite.texture = texture

signal pressed()
func _on_Button_pressed():
	select(true)
	emit_signal("pressed")
