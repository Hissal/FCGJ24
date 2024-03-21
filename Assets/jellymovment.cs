using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class jellymovment : MonoBehaviour
{
    [Header("Hits")]
    [SerializeField] float damageAmount = 2f;
    [SerializeField] float knockBackForceAmount = 4f;

    [Header("Bobbing")]
    [SerializeField] private float bobCooldown = 0.25f;
    [SerializeField] float bobForceAmount = 4f;

    private Vector2 startPos;
    private Rigidbody2D rb;

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
            rb.AddForce(Vector2.up * bobForceAmount, ForceMode2D.Impulse);
            yield return new WaitWhile(() => transform.position.y > startPos.y);
            yield return new WaitForSeconds(Random.Range(bobCooldown - bobCooldown * 0.5f, bobCooldown * 2));
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<IDamageable>() == null) return;

        Vector2 knockBackDirection = (collision.transform.position - transform.position).normalized;
        collision.GetComponent<IDamageable>().TakeDamage(damageAmount, knockBackDirection * knockBackForceAmount);
    }
}
