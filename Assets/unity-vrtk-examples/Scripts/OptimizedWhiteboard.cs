using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class OptimizedWhiteboard : MonoBehaviour {
	public Texture2D penTexture;
	public Texture2D texture;
	private RenderTexture rt;
	private int textureSize = 2048;
	private int penSize = 5;
	private Color32[] color;
	private Color32 newColor;
	private bool touching, touchingLast;
	private float posX, posY;
	private float lastX, lastY;
	private bool changingColor = false;
	private float throttler = 0f;
	private float throttleTime = 0.15f;

	// Use this for initialization
	void Start () {
		// Get/create whiteboard texture
		if (texture == null) {
			this.texture = new Texture2D(textureSize, textureSize);
		}

		// Have whiteboard use rendertexture
		this.rt = new RenderTexture(textureSize, textureSize, 32);
		GetComponent<Renderer>().material.SetTexture("_MainTex", rt);
		Graphics.Blit(texture, rt);   
	}
	
	// Update is called once per frame
	void Update () {
		// Throttle the color changer
		if (changingColor) {
			if (throttler > throttleTime) {
				ThrottledSetColor();
			} else {
				throttler += Time.deltaTime;
			}
		}
		// Don't run if touching isn't happening
		if (!touchingLast && !touching) return;

		// Transform textureCoords into "pixel" values
		int x = (int) (posX * textureSize);
		int y = (int) (posY * textureSize);

		// Only set the pixels if we were touching last frame
		if (touchingLast) {
			//Draw on my RenderTexture positioned by posX and posY.
			Draw(x, y);

			// Interpolate pixels from previous touch
			for (float t = 0.01f; t < 1.00f; t += 0.1f) {
				int lerpX = (int) Mathf.Lerp (lastX, (float) x, t);
				int lerpY = (int) Mathf.Lerp (lastY, (float) y, t);
				Draw(lerpX, lerpY);
			}
		}
			
		this.lastX = (float) x;
		this.lastY = (float) y;

		this.touchingLast = this.touching;
	}

	private void Draw(int x, int y) {
		// Setting the active rendertexture lets Graphics.DrawTexture draw to it.
		RenderTexture.active = rt; 
		GL.PushMatrix ();

		// create a matrix as large as the rendertexture to change its pixels
		GL.LoadPixelMatrix (0, rt.width, rt.height, 0); 

		// create a rectangle that will tell where to draw our pen's texture on the rendertexture
		Rect texRect = new Rect (
			x - (penTexture.width / 2),
			(rt.height - y) - penTexture.height / 2, 
			penTexture.width, 
			penTexture.height
		);

		// Apply it to the rendertexture
		Graphics.DrawTexture (texRect, penTexture);
		GL.PopMatrix ();
		RenderTexture.active = null; 
	}

	public void ToggleTouch(bool touching) {
		this.touching = touching;
	}

	public void SetTouchPosition(float x, float y) {
		this.posX = x;
		this.posY = y;
	}

	public void SetColor(Color32 color) {
		this.newColor = color;
		changingColor = true;
	}

	private void ThrottledSetColor() {
		int halfSize = penSize/2;
		int texHalfSize = penTexture.height / 2;
		this.color = Enumerable.Repeat<Color32>(new Color32(0,0,0,0), penTexture.width * penTexture.height).ToArray();

		for (int row = texHalfSize - halfSize - 1; row < texHalfSize - halfSize + 1; row++) {
			for (int cell = texHalfSize - halfSize - 1; cell < texHalfSize + halfSize + 1 ; cell++) {
				this.color[(penTexture.width * row) + cell] = newColor;
			}
		}

		penTexture.SetPixels32(color);
		penTexture.Apply();
		throttler = 0f;
		changingColor = false;
	}

}
