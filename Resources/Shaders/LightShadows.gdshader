shader_type canvas_item;


uniform float shadow_limit : hint_range(0.0, 1.0) = 0.5;

void fragment() {
    // Distance from center of light
    float dist = length(UV - vec2(0.5));

    // Clamp visibility based on shadow_limit
    float factor = smoothstep(shadow_limit, shadow_limit + 0.05, 1.0 - dist);

    COLOR = texture(TEXTURE, UV) * factor * COLOR;
}
