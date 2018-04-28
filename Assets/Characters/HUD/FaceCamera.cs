using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class FaceCamera : MonoBehaviour {

    [SerializeField] Vector3 offset;
    private Camera cam;

	// Use this for initialization
	void Start () {
        cam = Camera.main;
	}
	
	// Update is called once per frame
	void Update () {
        rotateXTowardsCamera();
        moveToOffset();
	}

    private void rotateXTowardsCamera() {
        Quaternion newRotation = Quaternion.LookRotation(cam.transform.position - transform.position, Vector3.up);
        newRotation = Quaternion.Euler(newRotation.eulerAngles.x, 180, 180);

        transform.rotation = newRotation;
    }

    //TODO Consider taking this out, or putting this outside of player object, or into a camera overlay type of thing
    private void moveToOffset() {
        transform.position = transform.parent.position + offset;
    }
}
