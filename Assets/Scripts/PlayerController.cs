using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
#region Public Vars
    public float mouseSensitivity = 100f;
    public Transform cameraTf;

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
    private float xRotation = 0f;
    private Vector3 velocity;
    private CharacterController controller;
    private bool isGrounded;
#endregion
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
#region Camera rotations
        var mouse = new Vector2(
            Input.GetAxis("Mouse X"),
            Input.GetAxis("Mouse Y")
        ) * mouseSensitivity * Time.deltaTime;

        xRotation -= mouse.y;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.Rotate(Vector3.up * mouse.x);
        cameraTf.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
#endregion

#region Player movement
        isGrounded = Physics.CheckSphere(legsPoint.position, checkDistance, checkMask);
        if(isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float
            x = Input.GetAxis("Horizontal"),
            z = Input.GetAxis("Vertical");

        var move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if(Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            velocity.y = jumpVelocity;
        }

        stars.position = transform.position;
#endregion
    }
}
