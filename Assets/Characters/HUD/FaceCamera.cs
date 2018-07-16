using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class FaceCamera : MonoBehaviour {
    
    private Camera cam;

	// Use this for initialization
	void Start () {
        cam = Camera.main;
	}
	
	// Update is called once per frame
	void Update () {
        rotateChildrenXTowardsCamera();
        rotateYTowardsCamera();
    }

    // Rotates each children's X coordinates so it doesn't take in the pivotpoint of the arm
    private void rotateChildrenXTowardsCamera() {
        //Quaternion newRotation = Quaternion.LookRotation(cam.transform.position - transform.position, Vector3.up);

        foreach (Transform child in transform) {
            //child.transform.rotation = newRotation;
        }
    }

    // Rotates this pivot containing the children towards the camera to give it a circular pivot motion
    private void rotateYTowardsCamera() {
        // HUD faces in the same (but opposite) direction as the camera's forward without the x and z rotations
        float cameraYRot = cam.transform.rotation.eulerAngles.y;
        float newRotationY = cameraYRot + 180;
        Vector3 newRotation = transform.rotation.eulerAngles;
        newRotation.y = newRotationY;

        transform.rotation = Quaternion.Euler(newRotation);
    }

}
