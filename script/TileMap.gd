extends TileMap

export var tilemap_size = Vector2(21, 21)
export var tile_offset = Vector2.ONE
#export var tilemap_start_pos = Vector2.ZERO
export var tilemap_scale = Vector2(24, 24)

export(NodePath) var select_path
export(NodePath) var wrapper_path
onready var select: ReferenceRect = get_node(select_path)
onready var wrapper: Control = get_node(wrapper_path)
onready var AttackMap: TileMap = get_node("../TileMapAttack")
onready var Chars: Control = get_node("../../Chars")

signal pause_play(is_paused)
signal reset()
func _on_MainMenu_start(reset):
	paused = false
	if reset:
		emit_signal("reset")
	emit_signal("pause_play", false)

func _on_TileMapWrapper_game_over():
	paused = true
	emit_signal("pause_play", true)
	select.visible = false

var painting = true

func _ready():
	set_select_color()
	set_sizes()
	set_scale(tilemap_scale)

func set_scale(new_scale):
	# cell_size = new_scale
	tilemap_scale = new_scale
	scale = tilemap_scale / Vector2(24, 24)
	AttackMap.scale = scale
	Chars.rect_scale = tilemap_scale
	set_sizes()

func set_sizes():
	wrapper.rect_min_size = (tilemap_size + tile_offset * 2) * tilemap_scale
	select.rect_size = tilemap_scale
	select.border_width = max(tilemap_scale.x / 6, 3)
	var mouse_tile = tile_at_mouse()
	if not paused:
		place_select(mouse_tile)

func tilemap_start_pos():
	# var global_pos = get_global_transform()
	var global_pos = global_position
	return Vector2(global_pos.x, global_pos.y)

func tile_at_global_pos(pos):
	var res = (pos - tilemap_start_pos()) / tilemap_scale - tile_offset
	res.x = floor(res.x)
	res.y = floor(res.y)
	if res.x < -1 or res.y < -1 or res.x > 1 + tilemap_size.x - tile_offset.x or res.y > 1 + tilemap_size.y - tile_offset.y:
		return null
	res.x = clamp(floor(res.x), 0, tilemap_size.x - tile_offset.x)
	res.y = clamp(floor(res.y), 0, tilemap_size.y - tile_offset.y)
	return res

func tile_to_global_pos(tile):
	return tilemap_start_pos() + (tile + tile_offset) * tilemap_scale

func tile_at_mouse():
	return tile_at_global_pos(get_global_mouse_position())

func place(tile: Vector2, color = null):
	if color == null:
		color = selected_color
	set_cellv(tile + tile_offset, color)
	wrapper.PlaceTile(int(tile.x), int(tile.y), color)

var selecting = null
var selected_color = 2
var last_selected_color = 2
var selected_color_options = [1, 2, 4, 9]

var colors = [
	Color("#66420f"),
	Color("#ffffff"),
	Color("#0000ff"),
	Color("#000000"),
	Color("#FF0000"),
	Color("#000000"),
	Color("#000000"),
	Color("#000000"),
	Color("#000000"),
	Color("#0bf21e"),
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
	if hideMouse:
		Input.set_mouse_mode(Input.MOUSE_MODE_HIDDEN)
	select.visible = true
	if not is_selecting:
		if not mouse_in_bounds:
			select.visible = false
			if hideMouse:
				Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)
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

export var paused = true

func _input(event):
	if Input.is_action_just_pressed("ui_pause"):
		paused = !paused
		if select.visible and paused:
			select.visible = false
		emit_signal("pause_play", paused)
	if paused:
		return
	if event is InputEventMouseButton and event.pressed and (event.button_index == BUTTON_WHEEL_UP || event.button_index == BUTTON_WHEEL_DOWN):
		var dir = 1
		if event.button_index == BUTTON_WHEEL_DOWN:
			dir = -1
		var index = (selected_color_options.find(selected_color) + dir + selected_color_options.size()) % selected_color_options.size()
		selected_color = selected_color_options[index]
		if selected_color == 1:
			index = (selected_color_options.find(selected_color) + dir + selected_color_options.size()) % selected_color_options.size()
			selected_color = selected_color_options[index]
		set_select_color()
		return
	if painting:
		input_paint(event)
	else:
		input_rect(event)
	if Input.is_action_just_released("toggle_mode"):
		painting = !painting
		selecting = false
		set_select_color()
		place_select(tile_at_mouse())

var hideMouse = false
var holdingRight = false
var holdingLeft = false
func input_paint(event):
	if event is InputEventMouseButton and event.button_index == BUTTON_RIGHT:
		holdingRight = event.pressed
	if event is InputEventMouseButton and event.button_index == BUTTON_LEFT:
		holdingLeft = event.pressed
	if event is InputEventMouse:
		var mouse_tile = tile_at_mouse()
		place_select(mouse_tile)
		if mouse_tile != null and holdingRight:
			place(mouse_tile, 1)
		if mouse_tile != null and holdingLeft:
			place(mouse_tile, selected_color)
	if event is InputEventMouseButton and event.pressed and event.button_index == BUTTON_MIDDLE:
		var mouse_tile = tile_at_mouse()
		if mouse_tile != null:
			var color = get_cellv(mouse_tile + tile_offset)
			if color and color != 1:
				selected_color = color
				set_select_color()

func input_rect(event):
	if event is InputEventMouseButton and event.pressed and event.button_index == BUTTON_RIGHT:
		if selecting:
			selecting = null
			if selected_color == 1 and selected_color != last_selected_color:
				selected_color = last_selected_color
			set_select_color()
			place_select(tile_at_mouse())
			select.set_size(tilemap_scale)
			return
		if selected_color != 1:
			last_selected_color = selected_color
			selected_color = 1
	if event is InputEventMouseButton and event.pressed:
		if not select.visible:
			return
		var is_selecting = selecting is Vector2
		var mouse_tile = tile_at_mouse()
		if is_selecting:
			place_all(mouse_tile)
			if selected_color == 1 and selected_color != last_selected_color:
				selected_color = last_selected_color
				set_select_color()
		else:
			selecting = mouse_tile
			set_select_color()
		place_select(mouse_tile)
		return
	if event is InputEventMouse:
		var mouse_tile = tile_at_mouse()
		place_select(mouse_tile)
