using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {

    private GameObject targetPosition;
    [Header("Camera Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] [Range(30, 90)] private int rotationAmount;
    [SerializeField] private float rotationTime;
    private float targetRotationY = 0;
    private float currentRotationY = 0;
    
    [Header("Camera Interpolation")]
    [SerializeField] private float movementPercentagePerSecond;
    [SerializeField] private float minMovementSpeed;

    private Vector3 prevMoveSpeed = Vector3.zero;
    private Vector3 prevPosition = Vector3.zero;

    // Use this for initialization
    void Start() {
        targetPosition = new GameObject("Camera Target");
        targetPosition.transform.position = transform.position;
    }

    // Update is called once per frame
    void Update() {
        registerInput();
        moveTotargetPosition();
        moveToTargetRotation();
    }

    // Lerps to get to the target position
    private void moveTotargetPosition() {
        prevPosition = transform.position;
        if (prevMoveSpeed.magnitude > 0.02f && prevMoveSpeed.magnitude < minMovementSpeed) {
            transform.position += prevMoveSpeed.normalized * minMovementSpeed * Time.deltaTime;
            if (Vector3.Dot(prevMoveSpeed, targetPosition.transform.position - transform.position) < 0) { //Adjusts in case of over-shooting
                transform.position = targetPosition.transform.position;
            }
        }
        else {
            transform.position = Vector3.Lerp(transform.position, targetPosition.transform.position, movementPercentagePerSecond / 100f);
        }
        prevMoveSpeed = (transform.position - prevPosition) / Time.deltaTime;
    }

    // Lerps to get to the target rotation
    private void moveToTargetRotation() {
        if (currentRotationY == targetRotationY)
            return;

        //float nextRotationY = transform.rotation.eulerAngles.y;
        float changeInYRotation = targetRotationY - currentRotationY;
        currentRotationY += (changeInYRotation > 0 ? rotationAmount : -rotationAmount) * Time.deltaTime / rotationTime;
        if (Mathf.Abs(currentRotationY - targetRotationY) < rotationAmount * Time.deltaTime / rotationTime) {
            targetRotationY %= 360;
            currentRotationY = targetRotationY;
        }
        Vector3 newRotation = transform.rotation.eulerAngles;
        newRotation.y = currentRotationY;
        transform.rotation = Quaternion.Euler(newRotation);
    }

    // Register user input to alter where the target camera position is
    private void registerInput() {
        registerMovementInput();
        registerRotationInput();
    }

    private void registerMovementInput() {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Get movement in local space
        Vector3 movementVector = new Vector3(horizontal, 0, vertical);
        movementVector = transform.TransformDirection(movementVector);
        movementVector = movementVector.normalized * moveSpeed * Time.deltaTime;

        Vector3 newPosition = targetPosition.transform.position += movementVector;

        targetPosition.transform.position = newPosition;
    }

    private void registerRotationInput() {
        // Rotation can't be over twice the rotation amount
        if (Mathf.Abs(targetRotationY - currentRotationY) < rotationAmount * 2) {
            if (Input.GetKeyDown(UserInput.RotateRight)) {
                targetRotationY -= rotationAmount;
            }
            else if (Input.GetKeyDown(UserInput.RotateLeft)) {
                targetRotationY += rotationAmount;
            }
        }
    }

}
