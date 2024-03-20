using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fish : MonoBehaviour
{
    enum FishState { Idle, Chasing }
    private FishState state;

    [SerializeField] protected float maxSwimSpeed = 4f;
    [SerializeField] protected float startingSwimSpeed = 0.5f;
    [SerializeField] protected float acceleration = 0.034f;
    protected float swimSpeed;
    [SerializeField] private Transform detectionCircleCenter;
    [SerializeField] protected float detectionRange = 7.5f;
    [SerializeField] protected float chasingRange = 10f;
    [SerializeField] protected float exhaustInSeconds = 7.5f;
    [SerializeField] protected float exhaustedForSeconds = 4f;
    [SerializeField] protected float exhaustMultiplier = 0.99f;
    [SerializeField] protected float minSpeedWhileExhausted = 2f;
    protected float currentExhaustAmount = 1;

    [SerializeField] private float pointToMoveToDistFromWalls = 2f;

    protected float groundDetectionRange;

    [SerializeField] LayerMask playerLayer;
    [SerializeField] LayerMask groundLayer;

    protected Rigidbody2D rb;
    protected Transform player;

    protected Timer exhaustTimer;
    protected Timer resetExhaustTimer;
    protected bool exhausted;

    protected Vector2 pointToMoveTo;
    protected bool movingTowardPlayer;

    protected Vector2 playerPosSnapshot;
    protected Vector2 playerLastSeenAtPos;
    private Timer playerPosSnapshotTImer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameManager.Instance.player;

        exhaustTimer = new Timer(this, (float totalTime) => Exhaust());
        resetExhaustTimer = new Timer(this, (float totalTime) => ResetExhaust());
        playerPosSnapshotTImer = new Timer(this, (float totalTime) => SnapshotPlayerPos());

        currentExhaustAmount = 1f;
        swimSpeed = startingSwimSpeed;
    }

    protected virtual void Update()
    {
        if (Physics2D.OverlapCircle(detectionCircleCenter.position, detectionRange, playerLayer) && state != FishState.Chasing && !GroundBetweenPlayer(detectionRange))
        {
            StartChasingPlayer();
        }

        if (state == FishState.Chasing && !Physics2D.OverlapCircle(transform.position, chasingRange, playerLayer))
        {
            StopChasingPlayer();
        }
    }

    protected virtual void FixedUpdate()
    {
        if (state == FishState.Chasing)
        {
            if (swimSpeed < maxSwimSpeed) swimSpeed = Mathf.Clamp(swimSpeed *= 1 + acceleration, startingSwimSpeed, maxSwimSpeed);
            ChasePlayer();
        }

        if (exhausted)
        {
            currentExhaustAmount *= exhaustMultiplier;
            currentExhaustAmount = Mathf.Clamp(currentExhaustAmount, 0f, 1f);
        }
    }

    /// <summary>
    /// Called every frame after player enters detection range until player is no longer in chasing range
    /// </summary>
    protected virtual void ChasePlayer()
    {
        // TODO
        // Detect Player From eyes not center
        // // Flip Sprite so no upside down

        if (!GroundBetweenPlayer(chasingRange))
        {
            playerLastSeenAtPos = player.position;
            pointToMoveTo = playerLastSeenAtPos;
            //if (!playerPosSnapshotTImer.active) playerPosSnapshotTImer.StartTimer(1f);
        }
        else
        {
            //if (Vector2.Distance(transform.position, playerPosSnapshot) <= 0.25f) pointToMoveTo = playerLastSeenAtPos;
            //else pointToMoveTo = playerPosSnapshot;
            MovePointToMoveToAwayFromWalls();
        }

        float speed = swimSpeed * currentExhaustAmount;
        if (exhausted) speed = Mathf.Clamp(speed, minSpeedWhileExhausted, maxSwimSpeed);

        Vector2 moveDirection = (pointToMoveTo - (Vector2)transform.position).normalized;
        rb.velocity = moveDirection * speed;
        RotateInDirectionOfMovement(moveDirection);
    }

    private void MovePointToMoveToAwayFromWalls()
    {
        // Up Right
        RaycastHit2D hitUpRight = Physics2D.Raycast(pointToMoveTo, (Vector2.up + Vector2.right).normalized, 1f, groundLayer);
        if (hitUpRight.collider != null)
        {
            pointToMoveTo += (Vector2.down + Vector2.left).normalized * pointToMoveToDistFromWalls;
            return;
        }

        // Up Left
        RaycastHit2D hitUpLeft = Physics2D.Raycast(pointToMoveTo, (Vector2.up + Vector2.left).normalized, 1f, groundLayer);
        if (hitUpRight.collider != null)
        {
            pointToMoveTo += (Vector2.down + Vector2.right).normalized * pointToMoveToDistFromWalls;
            return;
        }

        // Down Right
        RaycastHit2D hitDownRight = Physics2D.Raycast(pointToMoveTo, (Vector2.down + Vector2.right).normalized, 1f, groundLayer);
        if (hitUpRight.collider != null)
        {
            pointToMoveTo += (Vector2.up + Vector2.left).normalized * pointToMoveToDistFromWalls;
            return;
        }

        // Down Left
        RaycastHit2D hitDownLeft = Physics2D.Raycast(pointToMoveTo, (Vector2.down + Vector2.left).normalized, 1f, groundLayer);
        if (hitUpRight.collider != null)
        {
            pointToMoveTo += (Vector2.up + Vector2.right).normalized * pointToMoveToDistFromWalls;
            return;
        }

        // Up
        RaycastHit2D hitUp = Physics2D.Raycast(pointToMoveTo, Vector2.up, 1f, groundLayer);
        if (hitUp.collider != null)
        {
            pointToMoveTo += Vector2.down * pointToMoveToDistFromWalls;
            return;
        }

        // Down
        RaycastHit2D hitDown = Physics2D.Raycast(pointToMoveTo, Vector2.down, 1f, groundLayer);
        if (hitDown.collider != null)
        {
            pointToMoveTo += Vector2.up * pointToMoveToDistFromWalls;
            return;
        }

        // Right
        RaycastHit2D hitRight = Physics2D.Raycast(pointToMoveTo, Vector2.right, 1f, groundLayer);
        if (hitDown.collider != null)
        {
            pointToMoveTo += Vector2.left * pointToMoveToDistFromWalls;
            return;
        }

        // Left
        RaycastHit2D hitLeft = Physics2D.Raycast(pointToMoveTo, Vector2.left, 1f, groundLayer);
        if (hitDown.collider != null)
        {
            pointToMoveTo += Vector2.right * pointToMoveToDistFromWalls;
            return;
        }
    }

    /// <summary>
    /// Called once when player enters detection range (Sets state to Chasing and starts exhaustTimer)
    /// </summary>
    protected virtual void StartChasingPlayer()
    {
        swimSpeed = startingSwimSpeed;
        state = FishState.Chasing;
        exhaustTimer.StartTimer(exhaustInSeconds);
    }
    /// <summary>
    /// Called once when player leaves chasing range (Sets state to Idle and Kills exhaustTimer and finishes resetExhaustTimer)
    /// </summary>
    protected virtual void StopChasingPlayer()
    {
        state = FishState.Idle;
        exhaustTimer.KillTimer();
        resetExhaustTimer.FinishTimer();
    }

    protected virtual void Exhaust()
    {
        exhausted = true;
        resetExhaustTimer.StartTimer(exhaustedForSeconds);
    }
    protected virtual void ResetExhaust()
    {
        swimSpeed = startingSwimSpeed;
        exhausted = false;
        currentExhaustAmount = 1f;
        if (state == FishState.Chasing) exhaustTimer.StartTimer(exhaustInSeconds);
    }

    private void RotateInDirectionOfMovement(Vector2 direction)
    {
        Quaternion targetRotation = Quaternion.LookRotation(transform.forward, direction);
        Quaternion rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 5f);

        rb.MoveRotation(rotation);
    }

    protected bool GroundBetweenPlayer(float range)
    {
        RaycastHit2D hitPlayer = Physics2D.Raycast(transform.position, (player.position - transform.position).normalized, range, playerLayer);
        RaycastHit2D hitGround = Physics2D.Raycast(transform.position, (player.position - transform.position).normalized, range, groundLayer);

        if (hitGround.collider != null) return true;
        else if (hitPlayer.collider != null) return false;
        return true;
    }

    protected void SnapshotPlayerPos()
    {
        playerPosSnapshot = player.position;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(pointToMoveTo, 0.5f);
        Gizmos.color = Color.grey;
        Gizmos.DrawWireSphere(playerPosSnapshot, 0.5f);
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(playerLastSeenAtPos, 0.5f);

        if (state == FishState.Chasing) Gizmos.color = Color.red;
        else Gizmos.color = Color.magenta;
        if (exhausted) Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(detectionCircleCenter.position, detectionRange);

        if (state == FishState.Chasing) Gizmos.color = Color.red;
        else Gizmos.color = Color.green;
        if (exhausted) Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, chasingRange);
    }
}
