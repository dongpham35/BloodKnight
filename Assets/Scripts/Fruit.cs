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

    private void Awake()
    {
        view = GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (view.IsMine)
        {
            rb = GetComponent<Rigidbody2D>();
            Direction = Vector2.right;
            rb.velocity = Direction * speed;
        }
    }

    private void Update()
    {
        if(view.IsMine)
        {
            if (rb.velocity.x == 0)
            {
                Direction *= -1;
                rb.velocity = Direction * speed;
            }
        }
    }

}
