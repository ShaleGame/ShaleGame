shader_type canvas_item;

uniform vec4 ghost_color : source_color = vec4(0.921569, 0.203922, 0.607843, 1.0);

void fragment() {
	vec4 tex = texture(TEXTURE, UV);
	float inherited_alpha = COLOR.a;
	COLOR = vec4(ghost_color.rgb, tex.a * ghost_color.a * inherited_alpha);
}
