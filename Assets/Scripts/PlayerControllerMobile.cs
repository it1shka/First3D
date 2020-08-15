using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerControllerMobile : MonoBehaviour
{
    #region Public Vars
    [Space, Space]
    public Joystick joystick;
    public TouchArea touchArea;
    public Transform cameraTf;
    public float touchAreaSensitivity;

    [Space, Space]
    public float speed = 10f;
    public float gravity = -9.81f;
    public float jumpVelocity = 5f;

    [Space, Space]
    public Transform legsPoint;
    public float checkDistance = .5f;
    public LayerMask checkMask;

    [Space, Space]
    public Transform stars;
    #endregion

    #region Private Vars
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float myLockedRotationY;
    private float cameraLockedRotationX;
    #endregion

    private void Awake()
    {
        Application.targetFrameRate = 60;
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        #region Turn Camera 
        if (!touchArea.isPressed) {
            myLockedRotationY = transform.eulerAngles.y;
            cameraLockedRotationX = cameraTf.eulerAngles.x;

        }
        else
        {
            var rotation = touchArea.delta * touchAreaSensitivity;
            transform.rotation = Quaternion.Euler(0f, -rotation.x + myLockedRotationY, 0f);
            var cameraRot = rotation.y + cameraLockedRotationX;
            cameraTf.localRotation = Quaternion.Euler(cameraRot, 0f, 0f);

        }
        #endregion

        #region Player Movement
        isGrounded = Physics.CheckSphere(legsPoint.position, checkDistance, checkMask);
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float
            x = joystick.Horizontal,
            z = joystick.Vertical;

        var move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        stars.position = transform.position;

        #endregion
    }
    public void Jump()
    {
        if (!isGrounded)
            return;
        velocity.y = jumpVelocity;
    }
}
