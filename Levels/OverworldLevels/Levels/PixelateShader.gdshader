//shader_type canvas_item;
//
//uniform sampler2D SCREEN_TEXTURE: hint_screen_texture, filter_linear_mipmap;
//
//uniform int pixel_size : hint_range(1, 64) = 4; // Pixel size
//uniform vec2 screen_size = vec2(1920.0, 1080.0); // Screen size (set manually)
//
//void fragment() {
//    //Pixel coordinates in screen space
//    vec2 pixel_coords = floor(FRAGCOORD.xy / float(pixel_size)) * float(pixel_size);
//    // Convert pixel coordinates to UVs for screen texture
//    vec2 uv = pixel_coords / screen_size;
//    // Get color from texture screen
//    COLOR = texture(SCREEN_TEXTURE, uv);
//}

shader_type canvas_item;

// --- Uniforms --- //
uniform float resolution: hint_range(0.0, 128.0, 1.0) = 32.0;

// --- Functions --- //
void fragment() {
	COLOR = texture(TEXTURE, round(UV * (resolution + 1.0) - (UV - 0.5)) / (resolution + 1.0));
}
