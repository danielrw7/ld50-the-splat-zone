extends VBoxContainer

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
