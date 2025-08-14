shader_type canvas_item;

uniform vec4 color : source_color = vec4(0.639, 0.643, 0.435,0.459);
uniform float angle = -0.5;
uniform float position = -0.5;
uniform float starting_point = 5.0;
uniform float movement_speed = 1.0;
uniform float ray_separation : hint_range(0.0, 1.0) = 1.0;
uniform float ray_spread_horizontal : hint_range(-1.0, 1.0) = -1.0;
uniform float ray_spread_vertical : hint_range(0.0, 1.0) = 1.0;
uniform float ray_1_density = 20;
uniform float ray_2_density = 80;
uniform float ray_1_intensity : hint_range(0.0, 1.0) = 0.5;
uniform float ray_2_intensity : hint_range(0.0, 1.0) = 0.5;
uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;
// Ray angle calculations
mat2 angle_rays(float _angle){
	return mat2(vec2(cos(_angle), -sin(_angle)), vec2(sin(_angle), cos(_angle)));
}
// Ray pattern calculations
float random(vec2 _uv){
	return fract(sin(dot(_uv.xy, vec2(12.9898, 78.233)))* 43758.5453123);
}
float noise(in vec2 uv){
	vec2 i = floor(uv);
	vec2 f = fract(uv);
	float a = random(i);
	float b = random(i + vec2(1.0, 0.0));
	float c = random(i + vec2(0.0, 1.0));
	float d = random(i + vec2(1.0, 1.0));
	vec2 u = f * f * (3.0 - 2.0 * f);
	return mix(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d -b) * u.x * u.y;
}

void fragment() {
	// Ray rotation and position
	vec2 ray_positioning = (angle_rays(angle) * (UV - position)) / ((UV.y + ray_separation) - (UV.y * ray_separation));

	// Ray animation
	vec2 ray_1 = vec2(ray_positioning.x * ray_1_density + sin(TIME * 0.1 * movement_speed) * (ray_1_density * 0.2)+ starting_point, 1.0 );
	vec2 ray_2 = vec2(ray_positioning.x * ray_2_density + sin(TIME * 0.1 * movement_speed) * (ray_2_density * 0.2)+ starting_point, 1.0 );

	// Clamp bounds
	float bounds = step(ray_spread_horizontal, ray_positioning.x) * step(ray_spread_horizontal, 1.0 - ray_positioning.x);
	ray_1 *= bounds;
	ray_2 *= bounds;

	// Clamp intensity
	float rays;
	rays = clamp(noise(ray_1) * ray_1_intensity + (noise(ray_2) * ray_2_intensity), 0.0, 1.0);

	// Smoothen
	rays *= smoothstep(0.0, ray_spread_vertical, (1.0 - UV.y));
	rays *= smoothstep(0.0 + ray_spread_vertical, ray_spread_horizontal, ray_positioning.x);
	rays *= smoothstep(0.0 + ray_spread_horizontal, ray_spread_horizontal, 1.0 - ray_positioning.x);

	// Color
	vec3 ray_color = vec3(rays) * color.rgb;
	COLOR = vec4(ray_color, rays * color.a);

}
