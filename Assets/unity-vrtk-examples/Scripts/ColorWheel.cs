using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class ColorWheel : MonoBehaviour {

	public GameObject colorCube;

	// Use this for initialization
	void Start () {
		if (GetComponent<VRTK_ControllerEvents>() == null) {
			Debug.LogError("ColorWheel is required to be attached to a Controller that has the VRTK_ControllerEvents script attached to it");
			return;
		}

		GetComponent<VRTK_ControllerEvents>().TouchpadAxisChanged += new ControllerInteractionEventHandler(DoTouchpadAxisChanged);
	}

	private void DoTouchpadAxisChanged(object sender, ControllerInteractionEventArgs e) {
		ChangeColor (e.touchpadAxis, e.touchpadAngle);
	}

	private void ChangeColor(Vector2 touchpadAxis, float touchpadAngle) {
		float normalAngle = touchpadAngle - 90;
		if (normalAngle < 0)
			normalAngle = 360 + normalAngle;
		
		Debug.Log ("Trackpad axis at: " + touchpadAxis + " (" + normalAngle + " degrees)");

		float rads = touchpadAngle * Mathf.PI / 180;
		float maxX = Mathf.Sin (rads);
		float maxY = Mathf.Cos (rads);

		float currX = touchpadAxis.x;
		float currY = touchpadAxis.y;

		float percentX = Mathf.Abs (currX / maxX);
		float percentY = Mathf.Abs (currY / maxY);

		float saturation = (percentX + percentY) / 2;

		Color color = Color.HSVToRGB(normalAngle / 360.0f, saturation, 1.0f);

		colorCube.GetComponent<Renderer>().material.color = color;
	}
}
