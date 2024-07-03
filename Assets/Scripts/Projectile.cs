using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviourPun
{
    public float Damage;
    public float TimeLife;

    private PhotonView view;

    private void Start()
    {
        view = GetComponent<PhotonView>();
    }

    private void Update()
    {
        if(view.IsMine)
        {
            TimeLife -= Time.deltaTime;
            if(TimeLife <= 0)
            {
                Destroy(gameObject);
            }
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null && collision.gameObject.CompareTag("Player"))
        {
            if (collision.CompareTag("Shield"))
            {
                Destroy(gameObject);
                return;
            }
            BaseObject obj = collision.GetComponent<BaseObject>();
            if (obj != null)
            {
                obj.OnBeAttacked(Damage);
                obj.isHurt = false;
                PhotonView targetPhotonView = obj.GetComponent<PhotonView>();
                if (targetPhotonView != null)
                {
                    targetPhotonView.RPC("SendViewIdBeAttacked", RpcTarget.Others, Damage);
                }
                Destroy(gameObject);
            }
        }
    }

    [PunRPC]
    public void SendViewIdBeAttacked(float damage)
    {
        BaseObject obj = GetComponent<BaseObject>();
        obj.OnBeAttacked(damage);
    }
}
