using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class credit : MonoBehaviour
{
    [SerializeField] private GameObject credits;

    public void Credit()
    {
        credits.SetActive(!credits.activeInHierarchy);
    }

}
