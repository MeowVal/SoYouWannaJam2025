extends SubViewportContainer

@export var view_height: int = 270

func _physics_process(delta: float) -> void:
	var screen_size := Vector2(get_window().size)
	var pixel_scale: int = round(screen_size.y / view_height)
	var game_size := Vector2i(round(screen_size.x/pixel_scale), round(screen_size.y/pixel_scale))
	
	size = game_size+Vector2i(2,2)
	scale = Vector2(pixel_scale, pixel_scale)
