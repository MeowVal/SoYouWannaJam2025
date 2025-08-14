shader_type canvas_item;

uniform sampler2D height_map;
uniform float xy_angle;         // Angle in radians
uniform float z_angle;          // Vertical angle
uniform int max_shadow_steps = 100;
uniform float tex_step = 0.002; // Step size in UV space
uniform sampler2D indoor_mask;
uniform vec2 tile_size;   // e.g., (32, 16)
uniform vec2 map_offset;  // position of tilemap in screen
uniform vec2 map_pixel_size;

vec2 extrude(vec2 coord, float angle, float _length) {
	float x = _length * cos(angle) ;
	float y = _length * sin(angle);
    return  vec2(coord.x + x, coord.y + y);
}

float get_height_at(vec2 tex_coord, float xyAngle, float _distance, sampler2D heightMap) {
    vec2 new_coord = extrude(tex_coord, xyAngle, _distance);
    return texture(heightMap, new_coord).r;
}

float get_trace_height(float height, float zAngle, float _distance) {
    return _distance * tan(zAngle) + height;
}

bool is_in_shadow(float xyAngle, float zAngle, sampler2D heightMap, vec2 texCoord,  float _step, int max_steps) {
	float _distance;
  	float height;
  	float otherHeight;
  	float traceHeight;
	
    height  = texture(heightMap, texCoord).r;

    for (int i = 0; i < max_shadow_steps; i++) {
        float _distance = _step * float(i);
        otherHeight = get_height_at(texCoord, xyAngle, _distance, heightMap);

        if (otherHeight > height) {
            traceHeight = get_trace_height(height, zAngle, _distance);
            if (traceHeight <= otherHeight) {
                return true;
            }
        }
    }
    return false;
}
vec2 screen_to_tile_uv(vec2 screen_uv, vec2 screen_size) {
    // Convert UV to screen coords
    vec2 screen_pos = screen_uv * screen_size;

    // Offset relative to tilemap
    vec2 local = screen_pos - map_offset;

    float tile_x = (local.x / tile_size.x + local.y / tile_size.y) * 0.5;
    float tile_y = (local.y / tile_size.y - local.x / tile_size.x) * 0.5;

    // Normalize for heightmap sampling
    // Assuming the height_map is sized to the tilemap in tile coords
    // You may need to divide by number of tiles
    vec2 tile_uv = vec2(tile_x, tile_y);

    return tile_uv;
}
void fragment() {
	vec2 tile_uv = screen_to_tile_uv(UV, vec2(textureSize(height_map, 0)));
    float alpha = 0.0;
	float indoor = texture(indoor_mask, tile_uv).r;
	if(indoor > 0.5){
		if (is_in_shadow(xy_angle, z_angle, height_map, tile_uv,  tex_step, max_shadow_steps)) {
        alpha = 0.5;
    	}
	}
    
    COLOR = vec4(0.0, 0.0, 0.0, alpha);
}
