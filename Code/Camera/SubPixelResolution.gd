extends SubViewportContainer

@export var view_height: int = 270

func _process(_delta: float) -> void:
	var screen_size := Vector2(get_window().size)
	var pixel_scale := screen_size.y / view_height
	var game_size := Vector2i(round(screen_size.x/pixel_scale), view_height)+Vector2i(4,4)
	
	size = game_size+Vector2i(2,2)
	scale = Vector2(pixel_scale, pixel_scale)
