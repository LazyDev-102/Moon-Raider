How to create an explodable sprite:


1. Add a sprite to your scene.

2. Add a 2D collider and a 2D rigidbody.

3. Add the sprite exploder script.

4. Assign the "Sprite Exploder" script's pixel prefab. (We recommend the pixel prefab provided)

5. Press the "Generate Pixels" button.

6. Adjust settings to achieve desired effect.

----- Script Variables -----

Pixel Prefab
	- A prefab of a single pixel that will replace each pixel of your sprite to achieve an explosion.
	  This prefab should have a collider and rigidbody on it.

Explode From Center
	- A boolean that decides if your sprite explodes from its center.
	  If it's false and "Explode On Click" is true the sprite will explode from the mouse's postion 
	  when clicked. If it's false and "Explode On Collision" is true the sprite will explode from 
	  the point of collision.

Explode On Click 
	- A boolean that decides if when your sprite is clicked it explodes or not.

Explode On Collision
	- A boolean that decides if when your sprite collides with an object it explodes or not.

Minimum Explosion Velocity
	- A float for the minimum velocity the sprite has to have when it collides with an object to explode.
	  This is only visable when "Explode On Collision" is true.

Explosion Force
	- A float for how strong the explosion is.

Explosion Randomness
	- A float to represent how random the explosion is, e.g. higher randomness results in more varied 
	  explosions.
	  The range of floats used is the "Explosion Force" +/- "Explosion Randomness".
	  This float has to be within the range of 0 and "Explosion Force".

Pixel Lifespan
	- A float for how long each pixels life is in seconds.

Pixel Lifespan Randomness
	- A float to represent how random each pixels life is, e.g. higher randomness results in more varied
	  lifespans. 
	  The range of floats used is the "Pixel Lifespan" +/- "Pixel Lifespan Randomness".
	  This float has to be within the range of 0 and "Pixel Lifespan Randomness".

----- Recomendations -----

It is best to use sprites with no more the 100 visible pixels. 
Larger sprites can be used but can cause lag in some cases.

We suggest pixel lifespan is kept between 1 and 2.