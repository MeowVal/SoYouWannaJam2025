shader_type canvas_item;

uniform vec4 shadow_color : source_color = vec4(0.108,0.108,0.108,0.9);
uniform float shadow_angle : hint_range(0.0, 360) = 1.0;
uniform float shadow_length : hint_range(0.0, 1000) = 1.0;

void fragment() {

	float shadow_angle_radius = shadow_angle * 3.1416 / 360.0;
	vec2 direction = vec2(sin(shadow_angle_radius), cos(shadow_angle_radius));
	vec2 shadow_position = screen_uv_to_sdf(SCREEN_UV);


	float ray_distance_travelled = 0.0;
	while(ray_distance_travelled < shadow_length){
		float distance = texture_sdf(shadow_position);
		ray_distance_travelled += distance;
		if(distance < 0.01){
			break;
		}
		shadow_position += distance * direction;
	}
	float shadow_transparency = 1.0 - min(1.0, ray_distance_travelled / shadow_length);
	shadow_transparency = ceil(shadow_transparency);
	COLOR = vec4(shadow_color.rgb, shadow_transparency * shadow_color.a);

}
/*
void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
	float shadow_angle_rad = shadow_angle * 3.1416 / 180.0;
	vec2 direction = vec2(sin(shadow_angle_rad), cos(shadow_angle_rad));

	vec2 shadow_position = screen_uv_to_sdf(SCREEN_UV);

	float ray_distance_travelled = 0.0;

	while (ray_distance_travelled < shadow_length) {
		float distance = texture_sdf(shadow_position);
		ray_distance_travelled += distance;

		if (distance < 0.01) {
			break;
		}
		shadow_position += distance * direction;
	}

	float shadow_factor = 1.0 - min(1.0, ray_distance_travelled / shadow_length);
	shadow_factor = ceil(shadow_factor);

	// Final pixel color combining light and shadow
	vec3 light_rgb = LIGHT.rgb;
	vec4 lit_color = vec4(light_rgb, shadow_color.a);
	vec4 final_color = mix(shadow_color, lit_color, shadow_factor);
	LIGHT = final_color;

}*/
