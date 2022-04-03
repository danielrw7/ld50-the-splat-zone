extends Control


# Declare member variables here. Examples:
# var a = 2
# var b = "text"


# Called when the node enters the scene tree for the first time.
func _ready():
	_on_FPSToggle_toggled(true)

# Called every frame. 'delta' is the elapsed time since the previous frame.
#func _process(delta):
#	pass

signal start()
func _on_StartButton_pressed():
	emit_signal("start")

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
