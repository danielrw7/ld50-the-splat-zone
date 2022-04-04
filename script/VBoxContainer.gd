extends VBoxContainer

# https://godotforums.org/discussion/comment/29983/#Comment_29983
func _ready():
	get_tree().root.connect("size_changed", self, "_on_viewport_size_changed")
	_on_viewport_size_changed()
var resizing = false
func _on_viewport_size_changed():
	if resizing:
		return
	set_size(OS.get_window_size())
	var offsetted_size = OS.get_window_size() - Vector2.ONE
	var num_tiles = $HBoxContainer/TileMapWrapper/Control/TileMap.tilemap_size + $HBoxContainer/TileMapWrapper/Control/TileMap.tile_offset * 2
	var div = OS.get_window_size() / num_tiles * Vector2(0.75, 1)
	var new_scale = floor(max(1, min(div.x / 8, div.y / 8))) * 8
	$HBoxContainer/TileMapWrapper/Control/TileMap.set_scale(Vector2(new_scale, new_scale))
