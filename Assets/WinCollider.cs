using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinCollider : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerController>() != null)
        {
            // Win Code Goes Here
            print("WIN");
            UnityEngine.SceneManagement.SceneManager.LoadScene(2);
        }
    }
}
