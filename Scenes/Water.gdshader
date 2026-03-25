shader_type canvas_item;

// Distortion
uniform sampler2D noise_texture : hint_default_white, repeat_enable, filter_linear;
uniform vec2 noise_scroll = vec2(0.1, 0.05);
uniform vec2 noise_scale = vec2(2.0, 2.0);
uniform float refraction_strength : hint_range(0.0, 0.05) = 0.015;
uniform float reflection_distortion : hint_range(0.0, 0.05) = 0.01;

// Reflection
uniform float reflection_strength : hint_range(0.0, 1.0) = 0.4;
uniform float reflection_fade : hint_range(0.0, 2.0) = 1.0;

// Tint strength — color comes from the node's Modulate property
uniform float tint_strength : hint_range(0.0, 1.0) = 0.0;

// Fade
uniform sampler2D fade_texture : hint_default_white, repeat_disable, filter_linear;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_linear;

void fragment() {
    vec2 uv = UV;
    float t = TIME;

    vec2 noise_uv = uv * noise_scale + noise_scroll * t;
    float noise_x = texture(noise_texture, noise_uv).r;
    float noise_y = texture(noise_texture, noise_uv + vec2(0.4, 0.7)).r;
    vec2 distortion = (vec2(noise_x, noise_y) - 0.5) * 2.0;

    vec4 refracted = texture(screen_texture, SCREEN_UV + distortion * refraction_strength);

    vec2 reflect_uv = vec2(SCREEN_UV.x, SCREEN_UV.y - uv.y * 2.0);
    vec4 reflected = texture(screen_texture, reflect_uv + distortion * reflection_distortion);

    float reflect_alpha = (1.0 - uv.y) * reflection_strength * exp(-uv.y * reflection_fade);

    vec4 water = mix(refracted, reflected, reflect_alpha);

    // Mix toward modulate color by tint_strength, preserve alpha
    water.rgb = mix(water.rgb, COLOR.rgb, tint_strength * COLOR.a);

    // Fade + modulate alpha
    water.a *= texture(fade_texture, uv).r * COLOR.a;

    COLOR = water;
}