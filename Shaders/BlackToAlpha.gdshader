shader_type canvas_item;

uniform float threshold : hint_range(0.0, 1.0) = 0.1;

void fragment() {
    vec4 c = texture(TEXTURE, UV);
    float brightness = dot(c.rgb, vec3(0.299, 0.587, 0.114));
    c.a *= smoothstep(0.0, threshold, brightness);
    COLOR = c;
}