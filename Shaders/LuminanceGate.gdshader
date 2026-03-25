shader_type canvas_item;

uniform vec3 key_color : source_color = vec3(0.0, 0.0, 0.0);
uniform float transparent_ceiling : hint_range(0.0, 1.0) = 0.2;
uniform float opaque_floor : hint_range(0.0, 1.0) = 0.4;

void fragment() {
    vec4 c = texture(TEXTURE, UV);
    
    // How similar is this pixel to the key color?
    float key_dist = distance(c.rgb, key_color);
    
    // Pixels close to key = transparent, far from key = opaque
    // transparent_ceiling = distance at which transparency starts lifting
    // opaque_floor = distance at which pixel is fully opaque
    float alpha = smoothstep(transparent_ceiling, opaque_floor, key_dist);
    
    COLOR = vec4(c.rgb, c.a * alpha);
}