using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class jellymovment : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] float forceAmount = 4f;

    private Vector2 startPos;

    [SerializeField] private float bobCooldown = 0.25f;

    private void Start()
    {
        startPos = transform.position;
        rb = GetComponent<Rigidbody2D>();
        StartCoroutine(AddUpForce());
    }

    private IEnumerator AddUpForce()
    {
        while (true)
        {
            print("Added force");
            rb.AddForce(Vector2.up * forceAmount, ForceMode2D.Impulse);
            yield return new WaitWhile(() => transform.position.y > startPos.y);
            yield return new WaitForSeconds(bobCooldown);
        }
    }
}
