extends RichTextLabel

func _on_Label_meta_clicked(meta):
	OS.shell_open(meta)
