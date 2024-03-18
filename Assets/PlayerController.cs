using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float softMaxVelocity;
    [SerializeField] private float hardMaxVelocity;
    [SerializeField] private float swimSpeed;
    [SerializeField] private float rotationSpeed;

    private Vector2 movementInput;
    private Vector2 smoothedMovementInput;
    private Vector2 movementInputSmoothVelocity;

    private float inputX;
    private float inputY;
    private bool sprinting;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        inputX = Input.GetAxis("Horizontal");
        inputY = Input.GetAxis("Vertical");

        movementInput = new Vector2(inputX, inputY);

        sprinting = Input.GetKey(KeyCode.LeftShift);
    }

    private void FixedUpdate()
    {
        smoothedMovementInput = Vector2.SmoothDamp(smoothedMovementInput, movementInput, ref movementInputSmoothVelocity, 0.1f);
        if (movementInput != Vector2.zero) Move();
        if (movementInput != Vector2.zero) RotateInDirectionOfInput();
    }

    private void Move()
    {
        Vector2 moveDirection = movementInput.normalized;

        float speed = swimSpeed;
        float softMaxVelocity = this.softMaxVelocity;

        if (sprinting)
        {
            speed *= 2;
            softMaxVelocity *= 2;
        }

        if (rb.velocity.magnitude >= softMaxVelocity) speed *= 0.25f;

        rb.AddForce(movementInput * speed);
        rb.velocity = Vector2.ClampMagnitude(rb.velocity, hardMaxVelocity);

        print(rb.velocity.magnitude + " " + speed);
    }

    private void RotateInDirectionOfInput()
    {
        Quaternion targetRotation = Quaternion.LookRotation(transform.forward, smoothedMovementInput);
        Quaternion rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed);

        rb.MoveRotation(rotation);
    }
}
