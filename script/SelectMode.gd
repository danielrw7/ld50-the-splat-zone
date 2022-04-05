extends VBoxContainer

export(Texture) var brushRed = null
export(Texture) var brushGreen = null
export(Texture) var brushYellow = null
export(Texture) var rectRed = null
export(Texture) var rectGreen = null
export(Texture) var rectYellow = null

func _ready():
	$SelectBrush.select(true)

func _on_SelectBrush_pressed():
	$SelectRect.select(false)

func _on_SelectRect_pressed():
	$SelectBrush.select(false)

func _on_TileMap_select_mode(is_painting):
	if is_painting:
		$SelectBrush.select(true)
		$SelectRect.select(false)
	else:
		$SelectBrush.select(false)
		$SelectRect.select(true)

func _on_TileMap_select_color(color_index):
	var brush = null
	var rect = null
	if color_index == 1:
		brush = brushYellow
		rect = rectYellow
	elif color_index == 2:
		brush = brushRed
		rect = rectRed
	elif color_index == 3:
		brush = brushGreen
		rect = rectGreen
	else:
		return
	$SelectBrush.set_texture(brush)
	$SelectRect.set_texture(rect)
