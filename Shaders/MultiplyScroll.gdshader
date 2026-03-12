shader_type canvas_item;

// Scrolling overlay texture used for multiply blending.
uniform sampler2D overlay_texture;
uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;
// Tint applied to the overlay texture.
uniform vec4 overlay_color : source_color = vec4(1.0, 1.0, 1.0, 1.0);

// Controls for scrolling/tiling the overlay.
uniform vec2 overlay_offset = vec2(0.0, 0.0);
uniform vec2 overlay_speed = vec2(0.0, 0.0);
uniform vec2 overlay_scale = vec2(1.0, 1.0);

// How strongly the multiply effect is blended in (0 = original screen, 1 = full screen*overlay).
uniform float blend : hint_range(0.0, 1.0) = 0.5;
// Optional intensity to amplify the overlay color before multiplying.
uniform float intensity : hint_range(0.0, 2.0) = 1.0;

void fragment() {
    // Sample the current screen color (what's already been rendered behind this canvas item)
    vec2 suv = SCREEN_UV;
    vec4 screen_col = texture(SCREEN_TEXTURE, suv);
    vec3 screen_rgb = screen_col.rgb;

    // Compute overlay UV with tiling, offset and time-based scrolling
    vec2 o_uv = UV * overlay_scale + overlay_offset + overlay_speed * TIME;
    vec4 o_tex = texture(overlay_texture, o_uv) * overlay_color;
    vec3 overlay = o_tex.rgb * intensity;

    // Multiply screen by overlay, then linearly blend with original screen color
    vec3 multiplied = screen_rgb * overlay;
    vec3 out_rgb = mix(screen_rgb, multiplied, blend);

    // Preserve screen alpha
    COLOR = vec4(out_rgb, screen_col.a);
}
