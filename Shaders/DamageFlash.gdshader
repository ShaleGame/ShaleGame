shader_type canvas_item;

instance uniform float flash_intensity : hint_range(0.0, 1.0) = 0.0;

void fragment() {
	vec4 texture_color = texture(TEXTURE, UV);
	vec3 flashed_color = mix(texture_color.rgb, vec3(1.0, 1.0, 1.0), flash_intensity);
	COLOR = vec4(flashed_color, texture_color.a);
}
