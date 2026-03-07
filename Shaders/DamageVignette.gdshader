shader_type canvas_item;

/// How strong the vignette is (0 = invisible, 1 = full).
uniform float strength : hint_range(0.0, 1.0) = 0.0;

/// Color of the vignette (alpha is ignored; strength controls opacity).
uniform vec4 vignette_color : source_color = vec4(0.8, 0.0, 0.0, 1.0);

/// Controls how far in from the edges the vignette starts (higher = smaller clear centre).
uniform float radius : hint_range(0.0, 2.0) = 1.2;

/// Controls how soft/hard the vignette falloff is (higher = softer).
uniform float softness : hint_range(0.01, 4.0) = 1.8;

void fragment() {
	// Distance from centre of screen in [0, ~1.4] range.
	vec2 uv = SCREEN_UV * 2.0 - 1.0;
	float dist = length(uv);

	// Smooth vignette mask: 0 in the centre, 1 at the edge.
	float vignette = smoothstep(radius - softness, radius, dist);

	COLOR = vec4(vignette_color.rgb, vignette * strength);
}
