using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngleToCamera : MonoBehaviour {
	public float angleHorizontalToCamera;
	public float angleVerticalToCamera;
	public Camera whichCamera;

	private void Awake() {
		if (null == whichCamera) {
			whichCamera = Camera.main;
		}
	}

	private void Update() {
		if (null != whichCamera) {
			Vector3 vCamToObj = transform.position - whichCamera.transform.position;
			angleHorizontalToCamera = Vector3.Angle(whichCamera.transform.forward, Vector3.ProjectOnPlane(vCamToObj, whichCamera.transform.up));
			angleVerticalToCamera = Vector3.Angle(whichCamera.transform.forward, Vector3.ProjectOnPlane(vCamToObj, whichCamera.transform.right));
		}
	}
}