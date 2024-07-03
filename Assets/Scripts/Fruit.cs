using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fruit : MonoBehaviour
{
    public float speed;
    public float health;

    private PhotonView view;
    private Rigidbody2D rb;

    private Vector2 Direction;

    private void Start()
    {
        view = GetComponent<PhotonView>();
        if (view.IsMine)
        {
            rb = GetComponent<Rigidbody2D>();
            Direction = Vector2.right;
            rb.velocity = Direction * speed;
        }
    }

    private void Update()
    {
        if(rb.velocity.x == 0)
        {
            Direction *= -1;
            rb.velocity = Direction * speed;
        }
    }

}
