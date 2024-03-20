using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    private float length, height, middlePointX, middlePointY;
    [SerializeField] private Transform cam;
    [SerializeField] private float parallaxEffectX;
    [SerializeField] private float parallaxEffectY;

    private void Start()
    {
        middlePointX = transform.position.x;
        middlePointY = transform.position.y;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
        height = GetComponent<SpriteRenderer>().bounds.size.y;
    }

    private void LateUpdate()
    {
        float tempX = (cam.position.x * (1 - parallaxEffectX));
        float tempY = (cam.position.y * (1 - parallaxEffectY));
        float distX = (cam.position.x * parallaxEffectX);
        float distY = (cam.position.y * parallaxEffectY);

        transform.position = new Vector3(middlePointX + distX, middlePointY + distY, transform.position.z);

        if (tempX > middlePointX + length) middlePointX += length;
        else if (tempX < middlePointX - length) middlePointX -= length;

        if (tempY > middlePointY + height) middlePointY += height;
        else if (tempY < middlePointY - height) middlePointY -= height;
    }
}
