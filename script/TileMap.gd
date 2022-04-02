extends TileMap

export var tilemap_size = Vector2(21, 21)
export var tile_offset = Vector2.ONE
export var tilemap_start_pos = Vector2.ZERO
export var tilemap_scale = Vector2(24, 24)

onready var select: ReferenceRect = get_node("../Select")

func _ready():
	set_select_color()

func tile_at_global_pos(pos):
	var res = (pos - tilemap_start_pos) / tilemap_scale - tile_offset
	res.x = floor(res.x)
	res.y = floor(res.y)
	if res.x < -1 or res.y < -1 or res.x > 1 + tilemap_size.x - tile_offset.x or res.y > 1 + tilemap_size.y - tile_offset.y:
		return null
	res.x = clamp(floor(res.x), 0, tilemap_size.x - tile_offset.x)
	res.y = clamp(floor(res.y), 0, tilemap_size.y - tile_offset.y)
	return res

func tile_to_global_pos(tile):
	return tilemap_start_pos + (tile + tile_offset) * tilemap_scale

func tile_at_mouse():
	return tile_at_global_pos(get_global_mouse_position())

func place(tile: Vector2):
	set_cellv(tile + tile_offset, selected_color)

var selecting = null
var selected_color = 0

var colors = [
	Color("#66420f"),
]

func place_all(mouse_tile):
	if not (mouse_tile is Vector2):
		return
	var bounds_min = Vector2(min(selecting.x, mouse_tile.x), min(selecting.y, mouse_tile.y))
	var bounds_max = Vector2(max(selecting.x, mouse_tile.x), max(selecting.y, mouse_tile.y))
	for _x in range(bounds_min.x, bounds_max.x + 1):
		for _y in range(bounds_min.y, bounds_max.y + 1):
			place(Vector2(_x, _y))
	selecting = null
	set_select_color()
	select.set_size(tilemap_scale)
	
func place_select(mouse_tile):
	var is_selecting = selecting is Vector2
	var mouse_in_bounds = mouse_tile is Vector2
	select.visible = true
	if not is_selecting:
		if not mouse_in_bounds:
			select.visible = false
			return
		select.set_position(tile_to_global_pos(mouse_tile))
		return
	if not mouse_in_bounds:
		mouse_tile = selecting
	var bounds_min = Vector2(min(selecting.x, mouse_tile.x), min(selecting.y, mouse_tile.y))
	var bounds_max = Vector2(max(selecting.x, mouse_tile.x), max(selecting.y, mouse_tile.y))
	select.set_position(tile_to_global_pos(bounds_min))
	select.set_size(tilemap_scale * (bounds_max - bounds_min + Vector2.ONE))


func set_select_color():
	var color = Color(colors[selected_color])
	color.a = 0.5 if selecting else 0.25
	select.set_color(color)

func _input(event):
	var tile
	if event is InputEventMouseButton and event.pressed:
		if not select.visible:
			return
		var is_selecting = selecting is Vector2
		var mouse_tile = tile_at_mouse()
		if is_selecting:
			place_all(mouse_tile)
		else:
			selecting = mouse_tile
			set_select_color()
		place_select(mouse_tile)
		return
	elif event is InputEventMouse:
		var mouse_tile = tile_at_mouse()
		place_select(mouse_tile)
