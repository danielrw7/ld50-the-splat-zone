extends Control


# Declare member variables here. Examples:
# var a = 2
# var b = "text"


# Called when the node enters the scene tree for the first time.
func _ready():
	_on_FPSToggle_toggled(false)
	get_node("../../TileMapWrapper/Control/TileMap").connect("pause_play", self, "pause_play")

# Called every frame. 'delta' is the elapsed time since the previous frame.
#func _process(delta):
#	pass

signal start(reset)
func _on_StartButton_pressed():
	emit_signal("start", false)
	$MarginContainer/Main/StartButton.text = "Resume"
	$MarginContainer/Main/StartButton.visible = true
	$MarginContainer/Main/RestartButton.visible = true

func show_settings(show = true):
	$MarginContainer/Main.visible = !show
	$MarginContainer/Settings.visible = show

func _on_Settings_Button_pressed():
	show_settings(true)

func _on_BackButton_pressed():
	show_settings(false)

signal ShowFPS(value)
func _on_FPSToggle_toggled(button_pressed):
	emit_signal("ShowFPS", button_pressed)

func pause_play(is_paused):
	visible = is_paused

signal HideFPS()
signal WriteFPS(fps)
func _on_FPS_WriteFPS(fps):
	emit_signal("WriteFPS", fps)
func _on_FPS_HideFPS():
	emit_signal("HideFPS")

func _on_TileMapWrapper_game_over():
	$MarginContainer/Main/StartButton.visible = false

func _on_RestartButton_pressed():
	emit_signal("start", true)
	$MarginContainer/Main/StartButton.visible = true
