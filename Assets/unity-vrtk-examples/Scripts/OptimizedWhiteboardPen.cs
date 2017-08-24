using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class OptimizedWhiteboardPen : VRTK_InteractableObject {

	public OptimizedWhiteboard whiteboard;
	public Color brushColor = new Color(0,0,0,1); // Our starting color
	private Color newColor;
	private RaycastHit touch;
	private Quaternion lastAngle;
	private bool lastTouch;
	private bool held = false;
	private VRTK_ControllerReference controllerReference;
	private GameObject penTip;
	private GameObject tipPoint;

	// Use this for initialization
	void Start () {
		// cache pen tip and its touchpoint
		if (penTip == null) {
			penTip = transform.Find("Tip").gameObject;
		}
		newColor = penTip.GetComponent<MeshRenderer>().material.color;
		tipPoint = penTip.transform.Find("TouchPoint").gameObject;

		// Only run script while this object is grabbed
		this.InteractableObjectGrabbed += new InteractableObjectEventHandler(ObjectGrabbed);
		this.InteractableObjectUngrabbed += new InteractableObjectEventHandler(ObjectReleased);
	}

	private void ObjectGrabbed(object sender, InteractableObjectEventArgs e) {
		controllerReference = VRTK_ControllerReference.GetControllerReference(e.interactingObject.GetComponent<VRTK_ControllerEvents>().gameObject);
		held = true;
	}
	private void ObjectReleased(object sender, InteractableObjectEventArgs e) {
		held = false;
	}

	// Update is called once per frame
	void Update () {
		if (!held) return;
		float tipHeight = penTip.transform.localScale.y;
		Vector3 tip = tipPoint.transform.position;

		if (lastTouch) {
			tipHeight *= 1.1f;
		}

		// Check for a Raycast from the tip of the pen
		if (Physics.Raycast (tip, transform.up, out touch, tipHeight)) {
			// Only get whiteboard component if we haven't cached it yet
			whiteboard = (whiteboard != null) ? whiteboard : touch.collider.GetComponent<OptimizedWhiteboard>();
			if (whiteboard == null) return;

			// Give haptic feedback when touching the whiteboard
			VRTK_ControllerHaptics.TriggerHapticPulse ( controllerReference, 0.05f);

			if (newColor != null) {
				if (newColor != brushColor) {
					brushColor = newColor;
					whiteboard.SetColor (brushColor);
				}
			}


			// Set whiteboard parameters
			whiteboard.SetTouchPosition (touch.textureCoord.x, touch.textureCoord.y);
			whiteboard.ToggleTouch (true);

			// If we started touching, get the current angle of the pen
			if (lastTouch == false) {
				lastTouch = true;
				lastAngle = transform.rotation;
			}
		} else {
			if (whiteboard == null) return;
			whiteboard.ToggleTouch (false);
			lastTouch = false;
		}

		// Lock the rotation of the pen if "touching"
		if (lastTouch) {
			transform.rotation = lastAngle;
		}
	}

	public void SetBrushColor(Color color) {
		newColor = color;
	}

}
