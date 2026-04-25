shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_linear;
uniform float min_alpha : hint_range(0.0, 1.0) = 0.0;
uniform float max_alpha : hint_range(0.0, 1.0) = 1.0;
uniform float contrast : hint_range(0.1, 5.0) = 1.0;

void fragment() {
    vec4 c = texture(TEXTURE, UV);
    
    // Sample brightness of the lighting at this pixel
    float brightness = dot(texture(screen_texture, SCREEN_UV).rgb, vec3(0.299, 0.587, 0.114));
    
    // Remap brightness to your desired alpha range
    float alpha = pow(brightness, contrast);
    alpha = mix(min_alpha, max_alpha, alpha);
    
    COLOR = vec4(c.rgb, c.a * alpha);
}