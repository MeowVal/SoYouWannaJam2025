shader_type canvas_item;
uniform float xyangle;
uniform float zangle;
uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;

void fragment(){
	bool isinshade = false;
	float dist;
	float height;
	float lookupheight;
	float traceheight;
	vec4 col;

	
	//first I calculate the height of the current pixel
	col = texture(SCREEN_TEXTURE, SCREEN_UV);
	height = col.r;
	if(height < 1.0){height = 0.0;}
	height = round(height);

	//then I look at some pixels in a given direction
	for(int i = 0; i < 100; i ++){
		//distance of the pixel I'm looking at
		dist = SCREEN_PIXEL_SIZE.x * float(i)*2000.0;
		//UV of that pixel on screen
		float lookupx =SCREEN_UV.x + (0.001) * (-4.0) * dist ;
		float lookupy =SCREEN_UV.y + (0.001) * 0.0 * dist ;
		//Height of that pixel
		lookupheight = texture(SCREEN_TEXTURE, vec2(lookupx,  lookupy)).r;
		
		if(lookupheight < 1.0){lookupheight = 0.0;}
		lookupheight = round(lookupheight);
		
		//If that pixel's height is greater than the height of my current pixel
		if(lookupheight > height){
			//I calculate the height that that pixel should be if it was to draw a shadow
			//on my current pixel
			traceheight = dist * tan(zangle) + height;
			if(traceheight < 1.0){traceheight = 0.0;}
			traceheight = round(traceheight);
			
			//If the height of that pixel and the height it needs to be are close enough
			//then my current pixel needs to be shaded
			if(abs(traceheight-lookupheight) < 4.0){
				isinshade = true;
			}
		}
	}
	
	//Now I'm juste resetting the pixel's color to it's real value
	if(col.r > 1.0 || col.g > 1.0 || col.b > 1.0){
			col.r = mod(col.r, 1.0);
			col.g = mod(col.g, 1.0);
			col.b = mod(col.b, 1.0);
			COLOR = col;
			isinshade = false;
		}

	if(isinshade){
		COLOR = col - vec4(0.2, 0.15, 0.15, 0.0);
	} else {
		COLOR = col;
	}
}