extends ReferenceRect

func set_color(color):
	if not (color is Color):
		$ColorRect.visible = false
		return
	$ColorRect.visible = true
	$ColorRect.color = color

#func set_size(size: Vector2, keep_margins = false):
#	.set_size(size, keep_margins)
#	
#	$ColorRect.set_size(size - Vector2.ONE * 2)
