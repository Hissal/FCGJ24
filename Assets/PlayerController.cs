using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour, IDamageable
{
    [Header("Movement")]
    [SerializeField] private float softMaxVelocity = 5f;
    [SerializeField] private float hardMaxVelocity = 12f;
    [SerializeField] private float swimSpeed = 10f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Sprinting")]
   // public Image sprintBar;
   // public float sprintDrainAmount = 1f;
    [SerializeField] private float breathLossSprintingMultiplier = 4f;
   // public float regenSprintAfterSeconds = 2;
   // public float sprintRegenAmount = 5f;
   // private float sprintLeft = 100;
    private bool sprinting;
   // private bool regeningSprint;
   // private Timer sprintRegenTimer;

    [Header("Breath")]
    [SerializeField] private Image breathBar;
    [SerializeField] private float maxBreath;
    [SerializeField] private float breathLostPerSecond = 1f;
    private float currentBreath;

    private Vector2 movementInput;
    private Vector2 smoothedMovementInput;
    private Vector2 movementInputSmoothVelocity;

    private float inputX;
    private float inputY;

    private Rigidbody2D rb;
    private AudioSource audioSource;
    private SpriteRenderer rend;

    private bool stunned;
    private Timer stunTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        rend = GetComponentInChildren<SpriteRenderer>();

       // sprintRegenTimer = new Timer(this, (float totalTime) => StartCoroutine(RegenSprint()));
        stunTimer = new Timer(this, (float totalTime) => stunned = false);

        currentBreath = maxBreath;

        if (breathBar == null) breathBar = GameManager.Instance.BreathBar;
    }

   // private IEnumerator RegenSprint()
   // {
   //    // regeningSprint = true;
   //
   //    // while (sprintLeft != 100f && !sprinting)
   //    // {
   //    //     sprintLeft += sprintRegenAmount * Time.deltaTime;
   //    //     if (sprintLeft > 100f) sprintLeft = 100f;
   //    //     yield return null;
   //    // }
   //    //
   //    // regeningSprint = false;
   // }

    private void Update()
    {
        if (stunned) return;

        inputX = Input.GetAxis("Horizontal");
        inputY = Input.GetAxis("Vertical");
        movementInput = new Vector2(inputX, inputY);

        if (movementInput == Vector2.zero) audioSource.volume = 0f;
        else audioSource.volume = 1f;

        CheckSprint();
        //print(sprintLeft);
    }

    private void CheckSprint()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift)) sprinting = true;
        if (Input.GetKeyUp(KeyCode.LeftShift)) sprinting = false;
       // if (sprintLeft <= 0f) sprinting = false;
       //
       // if (sprintLeft < 100f && !sprintRegenTimer.active && !sprinting && !regeningSprint)
       // {
       //     sprintRegenTimer.StartTimer(regenSprintAfterSeconds);
       //     if (sprintLeft < 0f) sprintLeft = 0;
       // }
       // else if (sprinting && sprintRegenTimer.active)
       // {
       //     sprintRegenTimer.KillTimer();
       // }
       //
       // if (sprintBar != null) sprintBar.fillAmount = sprintLeft / 100f;
    }

    private void FixedUpdate()
    {
        float breathLoss = breathLostPerSecond * Time.fixedDeltaTime;
        if (sprinting) breathLoss = breathLostPerSecond * breathLossSprintingMultiplier * Time.fixedDeltaTime;
        currentBreath -= breathLoss;
        breathBar.fillAmount = currentBreath / maxBreath;

        if (currentBreath <= 0)
        {
            // Die
            print("Died");
            return;
        }

        if (stunned) return;

        smoothedMovementInput = Vector2.SmoothDamp(smoothedMovementInput, movementInput, ref movementInputSmoothVelocity, 0.1f);
        if (movementInput != Vector2.zero) Move();
        if (movementInput != Vector2.zero) RotateInDirectionOfInput();
    }

    private void Move()
    {
        Vector2 moveDirection = smoothedMovementInput.normalized;

        float speed = swimSpeed;
        float softMaxVelocity = this.softMaxVelocity;

        if (sprinting)
        {
            speed *= 2;
            softMaxVelocity *= 2;
           // sprintLeft -= sprintDrainAmount;
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

    public void TakeDamage(float damageAmount, Vector2 knockBackForce)
    {
        StartCoroutine(FlashRed());

        stunned = true;
        stunTimer.StartTimer(0.2f);

        rb.velocity = Vector2.zero;
        rb.AddForce(knockBackForce, ForceMode2D.Impulse);

        currentBreath -= damageAmount;

        print("Took " + damageAmount + " Damage");

        IEnumerator FlashRed()
        {
            rend.color = Color.blue;
            yield return new WaitForSeconds(0.1f);
            rend.color = Color.white;
        }
    }
}
