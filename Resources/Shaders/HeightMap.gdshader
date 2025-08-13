shader_type canvas_item;
uniform sampler2D heightmap;

void fragment(){
	vec4 heightmap_tex = texture(heightmap, UV);
	if(heightmap_tex.a > 0.9){
		// I'am adding 1.0 * h to to that pixel's color, where h
        // is the height of that pixel on the heightmap
		COLOR = texture(TEXTURE, UV) + vec4(1.0, 1.0, 1.0, 0.0) * (heightmap_tex.r * 255.0 + 1.0);
	} else {
		COLOR = texture(TEXTURE, UV);
	}
}