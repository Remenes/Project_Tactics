using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

    [SerializeField] private Transform target;
    //[SerializeField] private float moveSpeed;

    public Transform GetTarget() { return target; }
    public void SetNewTarget(Transform newTarget) { target = newTarget; }

    [SerializeField] private float movementPercentagePerSecond;
    [SerializeField] private float minMovementSpeed;

    private Vector3 prevMoveSpeed = Vector3.zero;
    private Vector3 prevPosition = Vector3.zero;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        moveToTarget();
	}

    private void moveToTarget() {
        prevPosition = transform.position;
        if (prevMoveSpeed.magnitude > 0.02f && prevMoveSpeed.magnitude < minMovementSpeed) {
            transform.position += prevMoveSpeed.normalized * minMovementSpeed * Time.deltaTime;
            if (Vector3.Dot(prevMoveSpeed, target.position - transform.position) < 0) { //Adjusts in case of over-shooting
                transform.position = target.position;
            }
        }
        else {
            transform.position = Vector3.Lerp(transform.position, target.position, movementPercentagePerSecond * Time.deltaTime);
        }
        prevMoveSpeed = (transform.position - prevPosition) / Time.deltaTime;
    }
}
