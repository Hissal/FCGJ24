using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float softMaxVelocity = 5f;
    [SerializeField] private float hardMaxVelocity = 12f;
    [SerializeField] private float swimSpeed = 10f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Sprinting")]
    public Image sprintBar;
    public float sprintDrainAmount = 1f;
    public float regenSprintAfterSeconds = 2;
    public float sprintRegenAmount = 5f;
    private float sprintLeft = 100;
    private bool sprinting;
    private bool regeningSprint;
    private Timer sprintRegenTimer;

    private Vector2 movementInput;
    private Vector2 smoothedMovementInput;
    private Vector2 movementInputSmoothVelocity;

    private float inputX;
    private float inputY;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sprintRegenTimer = new Timer(this, (float totalTime) => StartCoroutine(RegenSprint()));
    }

    private IEnumerator RegenSprint()
    {
        regeningSprint = true;

        while (sprintLeft != 100f && !sprinting)
        {
            sprintLeft += sprintRegenAmount * Time.deltaTime;
            if (sprintLeft > 100f) sprintLeft = 100f;
            yield return null;
        }

        regeningSprint = false;
    }

    private void Update()
    {
        inputX = Input.GetAxis("Horizontal");
        inputY = Input.GetAxis("Vertical");
        movementInput = new Vector2(inputX, inputY);

        CheckSprint();
        //print(sprintLeft);
    }

    private void CheckSprint()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift)) sprinting = true;
        if (Input.GetKeyUp(KeyCode.LeftShift)) sprinting = false;
        if (sprintLeft <= 0f) sprinting = false;

        if (sprintLeft < 100f && !sprintRegenTimer.active && !sprinting && !regeningSprint)
        {
            sprintRegenTimer.StartTimer(regenSprintAfterSeconds);
            if (sprintLeft < 0f) sprintLeft = 0;
        }
        else if (sprinting && sprintRegenTimer.active)
        {
            sprintRegenTimer.KillTimer();
        }

        if (sprintBar != null) sprintBar.fillAmount = sprintLeft / 100f;
    }

    private void FixedUpdate()
    {
        smoothedMovementInput = Vector2.SmoothDamp(smoothedMovementInput, movementInput, ref movementInputSmoothVelocity, 0.1f);
        if (movementInput != Vector2.zero) Move();
        if (movementInput != Vector2.zero) RotateInDirectionOfInput();
    }

    private void Move()
    {
        Vector2 moveDirection = smoothedMovementInput.normalized;

        float speed = swimSpeed;
        float softMaxVelocity = this.softMaxVelocity;

        if (sprinting && sprintLeft > 0f)
        {
            speed *= 2;
            softMaxVelocity *= 2;
            sprintLeft -= sprintDrainAmount;
        }

        if (rb.velocity.magnitude >= softMaxVelocity) speed *= 0.25f;

        rb.AddForce(moveDirection * speed);
        rb.velocity = Vector2.ClampMagnitude(rb.velocity, hardMaxVelocity);
    }

    private void RotateInDirectionOfInput()
    {
        Quaternion targetRotation = Quaternion.LookRotation(transform.forward, smoothedMovementInput);
        Quaternion rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed);

        rb.MoveRotation(rotation);
    }
}
