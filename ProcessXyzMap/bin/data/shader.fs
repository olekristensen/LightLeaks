 #version 120
 #define PI (3.1415926536)
 #define TWO_PI (6.2831853072)

 uniform sampler2DRect xyzMap;
 uniform float elapsedTime;
 uniform sampler2DRect texture;

 varying vec3 normal;
 varying float randomOffset;

 const vec4 on = vec4(1.);
 const vec4 off = vec4(vec3(0.), 1.);

 const vec3 center = vec3(0.324913 , 0.5, 0.087108); 

 const float waves = 19.;



 uniform float blackbody_color[273] = float[273](
 	1.0000, 0.0425, 0.0000, /* 1000K */
 	1.0000, 0.0668, 0.0000, /* 1100K */
 	1.0000, 0.0911, 0.0000, /* 1200K */
 	1.0000, 0.1149, 0.0000, /* ... */
 	1.0000, 0.1380, 0.0000,
 	1.0000, 0.1604, 0.0000,
 	1.0000, 0.1819, 0.0000,
 	1.0000, 0.2024, 0.0000,
 	1.0000, 0.2220, 0.0000,
 	1.0000, 0.2406, 0.0000,
 	1.0000, 0.2630, 0.0062,
 	1.0000, 0.2868, 0.0155,
 	1.0000, 0.3102, 0.0261,
 	1.0000, 0.3334, 0.0379,
 	1.0000, 0.3562, 0.0508,
 	1.0000, 0.3787, 0.0650,
 	1.0000, 0.4008, 0.0802,
 	1.0000, 0.4227, 0.0964,
 	1.0000, 0.4442, 0.1136,
 	1.0000, 0.4652, 0.1316,
 	1.0000, 0.4859, 0.1505,
 	1.0000, 0.5062, 0.1702,
 	1.0000, 0.5262, 0.1907,
 	1.0000, 0.5458, 0.2118,
 	1.0000, 0.5650, 0.2335,
 	1.0000, 0.5839, 0.2558,
 	1.0000, 0.6023, 0.2786,
 	1.0000, 0.6204, 0.3018,
 	1.0000, 0.6382, 0.3255,
 	1.0000, 0.6557, 0.3495,
 	1.0000, 0.6727, 0.3739,
 	1.0000, 0.6894, 0.3986,
 	1.0000, 0.7058, 0.4234,
 	1.0000, 0.7218, 0.4485,
 	1.0000, 0.7375, 0.4738,
 	1.0000, 0.7529, 0.4992,
 	1.0000, 0.7679, 0.5247,
 	1.0000, 0.7826, 0.5503,
 	1.0000, 0.7970, 0.5760,
 	1.0000, 0.8111, 0.6016,
 	1.0000, 0.8250, 0.6272,
 	1.0000, 0.8384, 0.6529,
 	1.0000, 0.8517, 0.6785,
 	1.0000, 0.8647, 0.7040,
 	1.0000, 0.8773, 0.7294,
 	1.0000, 0.8897, 0.7548,
 	1.0000, 0.9019, 0.7801,
 	1.0000, 0.9137, 0.8051,
 	1.0000, 0.9254, 0.8301,
 	1.0000, 0.9367, 0.8550,
 	1.0000, 0.9478, 0.8795,
 	1.0000, 0.9587, 0.9040,
 	1.0000, 0.9694, 0.9283,
 	1.0000, 0.9798, 0.9524,
 	1.0000, 0.9900, 0.9763,
 	1.0000, 1.0000, 1.0000, /* 6500K */
 	0.9771, 0.9867, 1.0000,
 	0.9554, 0.9740, 1.0000,
 	0.9349, 0.9618, 1.0000,
 	0.9154, 0.9500, 1.0000,
 	0.8968, 0.9389, 1.0000,
 	0.8792, 0.9282, 1.0000,
 	0.8624, 0.9179, 1.0000,
 	0.8465, 0.9080, 1.0000,
 	0.8313, 0.8986, 1.0000,
 	0.8167, 0.8895, 1.0000,
 	0.8029, 0.8808, 1.0000,
 	0.7896, 0.8724, 1.0000,
 	0.7769, 0.8643, 1.0000,
 	0.7648, 0.8565, 1.0000,
 	0.7532, 0.8490, 1.0000,
 	0.7420, 0.8418, 1.0000,
 	0.7314, 0.8348, 1.0000,
 	0.7212, 0.8281, 1.0000,
 	0.7113, 0.8216, 1.0000,
 	0.7018, 0.8153, 1.0000,
 	0.6927, 0.8092, 1.0000,
 	0.6839, 0.8032, 1.0000,
 	0.6755, 0.7975, 1.0000,
 	0.6674, 0.7921, 1.0000,
 	0.6595, 0.7867, 1.0000,
 	0.6520, 0.7816, 1.0000,
 	0.6447, 0.7765, 1.0000,
 	0.6376, 0.7717, 1.0000,
 	0.6308, 0.7670, 1.0000,
 	0.6242, 0.7623, 1.0000,
 	0.6179, 0.7579, 1.0000,
 	0.6117, 0.7536, 1.0000,
 	0.6058, 0.7493, 1.0000,
 	0.6000, 0.7453, 1.0000,
 	0.5944, 0.7414, 1.0000 /* 10000K */
 	);


 vec3 interpolate_color(float a, int c1, int c2)
 {
 	vec3 c;
 	c.x = (1.0-a) * blackbody_color[c1] + a * blackbody_color[c2];
 	c.y = (1.0-a) * blackbody_color[c1+1] + a * blackbody_color[c2+1];
 	c.z = (1.0-a) * blackbody_color[c1+2] + a * blackbody_color[c2+2];
 	return c;
/*
    vec3 c;
    c.x = blackbody_color[c1];
 	c.y = blackbody_color[c1+1];
    c.z = blackbody_color[c1+2];

    return c;
    */     
}

vec3 colorTemp(float temp, float intensity){
    float temp_index = (temp-1000.)/100.0;//((temp - 1000.) / 100.);
    //temp_index = 0.;
    float alpha = mod(temp , 100.) / 100.0;
  //	float alpha = intensity;

  vec3 color = intensity * interpolate_color(alpha, int(temp_index)*3, int(temp_index)*3+3);
/*
*/

 //   cout<<temp_index<<"   "<<blackbody_color[temp_index]*intensity<<endl;
  //  color.set(blackbody_color[temp_index]*intensity, blackbody_color[temp_index+1]*intensity, blackbody_color[temp_index+2]*intensity);
  return color;
}




// triangle wave from 0 to 1
float wrap(float n) {
	return abs(mod(n, 2.)-1.)*-1. + 1.;
}

// creates a cosine wave in the plane at a given angle
float wave(float angle, vec2 point) {
	float cth = cos(angle);
	float sth = sin(angle);
	return (cos (cth*point.x + sth*point.y) + 1.) / 2.;
}

// sum cosine waves at various interfering angles
// wrap values when they exceed 1
float quasi(float interferenceAngle, vec2 point) {
	float sum = 0.;
	for (float i = 0.; i < waves; i++) {
		sum += wave(3.1416*i*interferenceAngle, point);
	}
	return wrap(sum);
}

float animate00(float stage){
	float a = mod(stage, 1);
	
	//Range 0...1

	a -= 0.5;
	a *= 2.;

	//Range -1...1

	a = pow(a,2.);

	//Range -1...1

	a += 1.;
	a *= 0.5;

	//Range 0 ... 0.5 ... 0 
	a = 1-a;
	return a;
}


float animate01(float stage){
	float a = mod(stage, 1);
	
	//Range 0...1

	a -= 0.5;
	a *= 2.;

	//Range -1...1

	a = pow(a,3.);

	//Range -1...1

	a += 1.;
	a *= 0.5;

	//Range 0...1

	return a;
}

void main() {
	vec4 curSample = texture2DRect(xyzMap, gl_TexCoord[0].st);
	vec3 position = curSample.xyz;
	float present = curSample.a;


	gl_FragColor = vec4(0);
	if(position.z > 0){

		float stages = 1.;
		float stage = 14;//+mod(elapsedTime * .1, stages);

		if(stage <= 1.) {

			// diagonal stripes
			float speed = .01;
			const float scale = .1 ;
			//float a = animate01(stage)*0.1;
			float a = scale/2.;

			gl_FragColor = (mod((position.x + position.y + position.z) + (elapsedTime * speed), scale) > a) ?
			on : off;



		} else if(stage <= 2.) {
			// crazy color bounce
			float a = animate00(stage);
			gl_FragColor = vec4(mod(elapsedTime+ a*10. + position, 1.0) *a , 1.);
		} else if(stage <= 3.) {
			// fast rising stripes
			//if(normal.z == 0.) {
			float a = animate01(stage);
			float speed = .01;
			float scale = 0.1;
			gl_FragColor = 
			(mod((-position.z) + (speed), scale) < (a*scale / 1.)) ?
			on : off;

			/*} else {
				gl_FragColor = off;
				}*/
		} else if(stage <= 5.) {
			// crazy triangles, grid lines
			float speed = .2;
			float scale = .5;
			float cutoff = .9;
			vec3 cur = mod(position + speed * elapsedTime, scale) / scale;
			cur *= 1. - abs(normal);
			if(stage < 4.) {
				gl_FragColor = ((cur.x + cur.y + cur.z) < cutoff) ? off : on;
			} else {
				gl_FragColor = (max(max(cur.x, cur.y), cur.z) < cutoff) ? off : on;
			}
		} else if(stage <= 6.) {
			// moving outlines 
			const float speed = 10.;
			const float scale = 6.;
			float localTime = 5. * randomOffset + elapsedTime;
			gl_FragColor = 
			(mod((-position.x - position.y + position.z) + (localTime * speed), scale) > scale / 2.) ?
			on : off;
		} else if(stage <= 7.) {
			// spinning (outline or face) 
			vec2 divider = vec2(cos(elapsedTime*2.), sin(elapsedTime*2.));
			float side = (position.x * divider.y) - (position.y * divider.x);

			gl_FragColor = abs(side) < .01 + 0.01 * sin(elapsedTime * 1.) ? on : off;
		} else if(stage <= 8.){
				vec2 normPosition = (position.xz + position.yx) / 100.;
				float b = 0.3*(sin(elapsedTime*3.0+30.0*position.y)+1.);
				//gl_FragColor = vec4(vec3(b), 1.);

				//Color
				vec3 color = colorTemp(4000. + (1-b+0.3) * 5000.,1.);

				gl_FragColor = vec4(color*b, 1.);
		} else if(stage <= 9.){
				float t = sin(elapsedTime)*.2;
				vec2 fromCenter = center.yz - position.yz;
				vec2 rot = vec2(sin(t), cos(t));

				float b = sin(100.*dot(rot, fromCenter));
			/*float angle = mod(elapsedTime, TWO_PI);

			float b = mod( angle - atan(fromCenter.x / fromCenter.y)  , TWO_PI);
			if(b > TWO_PI){
				b -= TWO_PI;
			}
			float c = abs(b / TWO_PI );
			*/
			//float c = 0.5*(sin(b+elapsedTime)+1.);
				gl_FragColor = vec4(vec3(b), 1.);		
		} else if(stage <= 10.){
				float t = elapsedTime;
				vec2 fromCenter = center.yx - position.yx;
				vec2 rot = vec2(sin(t), cos(t));

				float r = (1.+sin(4.*dot(rot, fromCenter)))*0.5;
				
				vec3 color = colorTemp(4000. + r * 5000.,1.);

				gl_FragColor = vec4(color, 1.);		

		} else if(stage <= 11.){
				float t = elapsedTime;
				vec2 fromCenter = center.yx - position.yx;
				vec2 rot = vec2(sin(t), cos(t));

				float r = sin(2.*dot(rot, fromCenter));
				float g = sin(4.*dot(rot, fromCenter));
				float b = sin(6.*dot(rot, fromCenter));


				gl_FragColor = vec4(r,g,b, 1.);		
		} else if(stage <= 12.){
				vec3 fromCenter = center.xyz - position.xyz;
				float b = sin(20.*mod(length(fromCenter)+(0.2*sin(elapsedTime*5.)), 10.));

				gl_FragColor = vec4(vec3(b), 1.);

		} else if(stage <= 13.) {
				vec2 normPosition = (position.xz + position.yx) / 100.;
				float b = quasi(elapsedTime*0.04, (normPosition)*200.);
				gl_FragColor = vec4(vec3(b), 1.);
		} else if(stage <= 14){ 
			// Text on end wall
			if(position.y == center.y * 2.){
				vec2 samplePos = position.xz;
				samplePos.x -= sin(elapsedTime)* 0.02;
				samplePos.y -= 0.05;
				samplePos *= 2.;
				samplePos *= 1024.;
				vec4 curSample = texture2DRect(texture, samplePos);

				gl_FragColor = curSample;
			}

		}
		else if(stage <= 15){ 
			// Text on ceilling
			if(position.y == center.y * 2.){
				vec2 samplePos = position.xz;
				samplePos.x -= sin(elapsedTime)* 0.02;
				samplePos.y -= 0.05;
				samplePos *= 2.;
				samplePos *= 1024.;
				vec4 curSample = texture2DRect(texture, samplePos);

				gl_FragColor = curSample;
			}

		}
	}
}

