extends Control

func _ready():
	_on_FPSToggle_toggled(false)
	get_node("../../TileMapWrapper/Control/TileMap").connect("pause_play", self, "pause_play")

signal start(reset)
signal pause()

func _on_StartButton_pressed():
	emit_signal("start", false)
	$MarginContainer/Main/StartButton.visible = false
	$MarginContainer/Main/RestartButton.visible = true
	$MarginContainer/Main/CreditsButton.visible = false
	$MarginContainer/Main/ExitButton.visible = false
	$MarginContainer/Main/MainMenuButton.visible = true
	print("main menu visible")

func show_settings(show = true):
	$MarginContainer/Settings.visible = show
	
func show_credits(show = true):
	$MarginContainer/Credits.visible = show
	
func show_controls(show = true):
	$MarginContainer/Controls.visible = show

func _on_Settings_Button_pressed():
	$MarginContainer/Main.visible = false
	show_settings(true)
	show_controls(false)
	show_credits(false)

func _on_CreditsButton_pressed():
	$MarginContainer/Main.visible = false
	show_settings(false)
	show_controls(false)
	show_credits(true)
	
func _on_ControlsButton_pressed():
	$MarginContainer/Main.visible = false
	show_settings(false)
	show_controls(true)
	show_credits(false)

func _on_BackButton_pressed():
	$MarginContainer/Main.visible = true
	show_settings(false)
	show_credits(false)
	show_controls(false)

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

func _on_MainMenuButton_pressed():
	emit_signal("start", true)
	$MarginContainer/Main/StartButton.visible = true
	$MarginContainer/Main/RestartButton.visible = false
	$MarginContainer/Main/CreditsButton.visible = true
	$MarginContainer/Main/MainMenuButton.visible = false
	$MarginContainer/Main/ExitButton.visible = OS.get_name() != "HTML5"
	print("main menu invisible")
	emit_signal("pause")

signal can_hide_mouse(val)
func _on_HideMouse_toggled(button_pressed):
	emit_signal("can_hide_mouse", button_pressed)
