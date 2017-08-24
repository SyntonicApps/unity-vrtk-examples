using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRTK;

public class OptimizedColorWheel : MonoBehaviour {
	private GameObject blackWheel;
	private Image blackWheelImage;
	private OptimizedWhiteboardPen pen;
	private GameObject penTip;	
	private float hue, saturation, value = 1f;
	private VRTK_InteractableObject interactable;
	private VRTK_ControllerEvents events;
	private ControllerInteractionEventHandler eventHandler;

	// Use this for initialization
	void Start () {
		interactable = GetComponent<VRTK_InteractableObject>();
        if (interactable == null) {
            Debug.LogError("The gameobject using this script needs the VRTK_InteractableObject script attached to it");
            return;
        }

		pen = GetComponent<OptimizedWhiteboardPen>();
		if (pen == null) {
			Debug.LogError("You need to have a OptimizedWhitebaordPen component attached to the same gameobject as this script");
            return;
		}

		if (penTip == null) {
			penTip = transform.Find("Tip").gameObject;
		}

		blackWheel = transform.Find ("CanvasHolder/Canvas/BlackWheel").gameObject;
		blackWheelImage = blackWheel.GetComponent<Image> ();

		interactable.InteractableObjectGrabbed += new InteractableObjectEventHandler(ObjectGrabbed);
		interactable.InteractableObjectUngrabbed += new InteractableObjectEventHandler(ObjectReleased);
	}
	private void ObjectGrabbed(object sender, InteractableObjectEventArgs e) {
		events = e.interactingObject.GetComponent<VRTK_ControllerEvents>();
		if (events == null) {
			Debug.LogError("ColorWheel is required to be attached to a Controller that has the VRTK_ControllerEvents script attached to it");
			return;
		}
		eventHandler = new ControllerInteractionEventHandler(DoTouchpadAxisChanged);
		events.TouchpadAxisChanged += eventHandler;
	}

	private void ObjectReleased(object sender, InteractableObjectEventArgs e) {
		events.TouchpadAxisChanged -= eventHandler;
	}

	private void DoTouchpadAxisChanged(object sender, ControllerInteractionEventArgs e) {
		if (events.triggerPressed) {
			ChangedValue (e.touchpadAxis);
		} else {
			ChangedHueSaturation (e.touchpadAxis, e.touchpadAngle);
		}
	}

	private void ChangedValue(Vector2 touchpadAxis) {
		this.value = (touchpadAxis.y + 1) / 2;
		Color currColor = blackWheelImage.color;
		currColor.a = 1 - this.value;
		blackWheelImage.color = currColor;

		UpdateColor ();
	}

	private void ChangedHueSaturation(Vector2 touchpadAxis, float touchpadAngle) {
		float normalAngle = touchpadAngle - 90;
		if (normalAngle < 0)
			normalAngle = 360 + normalAngle;

		float rads = normalAngle * Mathf.PI / 180;
		float maxX = Mathf.Cos (rads);
		float maxY = Mathf.Sin (rads);

		float currX = touchpadAxis.x;
		float currY = touchpadAxis.y;

		float percentX = Mathf.Abs (currX / maxX);
		float percentY = Mathf.Abs (currY / maxY);

		this.hue = normalAngle / 360.0f;
		this.saturation = (percentX + percentY) / 2;

		UpdateColor ();
	}

	private void UpdateColor() {
		Color32 color = Color.HSVToRGB(this.hue, this.saturation, this.value);
		penTip.GetComponent<Renderer>().material.color = color;
		pen.SetBrushColor(color);
	}
}
