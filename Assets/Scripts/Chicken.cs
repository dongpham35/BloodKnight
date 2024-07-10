using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chicken : MonoBehaviourPunCallbacks
{
    [SerializeField] Vector2      SpawnPoint;

    private List<GameObject>      Players = new List<GameObject>();
    private GameObject            focusPlayer;

    private Rigidbody2D           rb2D;
    private Animator              animator;
    private SpriteRenderer        spriteRenderer;
    private BoxCollider2D         boxcollider2d;
    private PhotonView            view;

    private float                 Speed;
    private float                 Damage;
    private float                 MinDistance;

    private bool                  isDeath;
    private bool                  isReadyNorMove;
    private bool                  isSawPlayer;
    private bool                  isMoveToSpawnPoint;

    enum                          NorState { Idel, Run}
    NorState                      stateChicken = NorState.Idel;

    private Vector2               Direction;
    private float                 LastTimeMove;
    private float                 LimitedTimeToChangeState;
    private float                 LastTimeAttack;
    private float                 CoolDownAttack;

    private void Awake()
    {
        view = GetComponent<PhotonView>();
    }
    private void Start()
    {
       if(view.IsMine)
        {
            rb2D = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            boxcollider2d = GetComponent<BoxCollider2D>();
            Speed = GetComponent<BaseObject>().Speed;
            Damage = GetComponent<BaseObject>().Damage;
        }
        

        Direction = Vector2.right;


        LastTimeMove = 0.0f;
        LimitedTimeToChangeState = Random.Range(4, 9) / 1.5f;

        if(Random.Range(1, 10) % 2 == 0) isReadyNorMove = true;
        else isReadyNorMove = false;

        isMoveToSpawnPoint = false;

        focusPlayer = null;
        MinDistance = 0.0f;

        CoolDownAttack = Random.Range(4, 5) / 2.5f;
        LastTimeAttack = 0.0f;
    }

    private void Update()
    {

        if (view.IsMine)
        {
            if (GetComponent<BaseObject>().currenHealth == 0) isDeath = true;

            if (isDeath) Destroy(gameObject);

            if (!isDeath)
            {
                LastTimeAttack += Time.deltaTime;
                StartCoroutine(GetPlayersInRoom());
                if (focusPlayer != null) MinDistance = Vector2.Distance(transform.position, focusPlayer.transform.position);
                if (focusPlayer != null && MinDistance <= 5.5f && !isMoveToSpawnPoint) isSawPlayer = true;
                else isSawPlayer = false;
                //Move to Spawn point when enemy are distancing spawn point 15f
                if (Vector2.Distance(transform.position, SpawnPoint) >= 15f && !isMoveToSpawnPoint)
                {
                    Vector2 look = SpawnPoint - new Vector2(transform.position.x, transform.position.y);
                    Direction.x = look.normalized.x;
                    isReadyNorMove = true;
                    isMoveToSpawnPoint = true;
                    isSawPlayer = false;
                    focusPlayer = null;
                    MinDistance = 0.0f;
                }
                if (isMoveToSpawnPoint && Vector2.Distance(transform.position, SpawnPoint) <= 1.5f)
                {
                    isMoveToSpawnPoint = false;
                }

                if (rb2D.velocity.x != 0)
                {
                    RaycastHit2D ray = Physics2D.Raycast(transform.position, Direction, 0.5f, LayerMask.GetMask("Ground"));
                    if (ray.collider != null)
                    {
                        Direction *= -1;
                        rb2D.velocity = rb2D.velocity.x * Direction;
                    }
                }
                LastTimeMove += Time.deltaTime;
                if (LastTimeMove >= LimitedTimeToChangeState)
                {
                    LastTimeMove = 0.0f;
                    LimitedTimeToChangeState = Random.Range(4, 9) / 1.5f;
                    isReadyNorMove = !isReadyNorMove;
                }
                if (isReadyNorMove && !isSawPlayer) NorMove();

                if (isSawPlayer)
                {
                    MoveToPlayer();
                    HitPlayer();
                }
                UpdateAnimation();
            }
        }


    }

    private void UpdateAnimation()
    {
        if(rb2D.velocity.x > .1f)
        {
            spriteRenderer.flipX = true;
            stateChicken = NorState.Run;
            Direction = Vector2.right;
        }else if(rb2D.velocity.x < -.1f)
        {
            spriteRenderer.flipX=false;
            stateChicken = NorState.Run;
            Direction = Vector2.left;
        }
        else
        {
            stateChicken = NorState.Idel;
        }

        photonView.RPC("UpdateSpriteRenderer", RpcTarget.OthersBuffered, spriteRenderer.flipX);
        animator.SetInteger("NorState", (int) stateChicken);
    }

    [PunRPC]
    public void UpdateSpriteRenderer(bool state)
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        sprite.flipX = state;
    }
    private void NorMove()
    {
        rb2D.velocity = new Vector2(Direction.x * Speed, rb2D.velocity.y);
    }
    private void MoveToPlayer()
    {
        Vector3 look = focusPlayer.transform.position - transform.position;
        Direction.x = look.normalized.x;
        if (Vector3.Distance(focusPlayer.transform.position, transform.position) >= 1.2f)
        {
            rb2D.velocity = new Vector2(Direction.x * Speed, rb2D.velocity.y);
        }
        RaycastHit2D ray = Physics2D.Raycast(transform.position, Direction, 0.5f, LayerMask.GetMask("Ground"));
        if (ray.collider != null && IsGround())
        {
            rb2D.velocity = new Vector2(rb2D.velocity.x, Speed * 1.9f);
        }
    }
    IEnumerator GetPlayersInRoom()
    {
        while (true)
        {
            if (Players.Count != 0) Players.Clear();
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            if (players.Length > 0)
            {
                foreach (GameObject gameObject in players)
                {
                    Players.Add(gameObject);
                    if (focusPlayer == null)
                    {
                        focusPlayer = gameObject;
                        MinDistance = Vector2.Distance(transform.position, focusPlayer.transform.position);
                    }
                    else
                    {
                        if (Vector2.Distance(transform.position, gameObject.transform.position) < MinDistance)
                        {
                            focusPlayer = gameObject;
                            MinDistance = Vector2.Distance(transform.position, focusPlayer.transform.position);
                        }
                    }
                }
            }
            yield return new WaitForSeconds(2.5f);
        }
    }


    private void HitPlayer()
    {
        if(LastTimeAttack >= CoolDownAttack)
        {
            RaycastHit2D ray = Physics2D.Raycast(transform.position, Direction, 0.7f, LayerMask.GetMask("Player", "Shield"));
            if (ray.collider != null)
            {
                LastTimeAttack = 0.0f;
                if (ray.collider.CompareTag("Shield")) return;
                BaseObject obj = ray.collider.GetComponent<BaseObject>();
                if (obj != null)
                {
                    obj.OnBeAttacked(Damage);
                    PhotonView targetPhotonView = obj.GetComponent<PhotonView>();
                    if (targetPhotonView != null)
                    {
                        targetPhotonView.RPC("SendViewIdBeAttacked", RpcTarget.Others, Damage);
                    }
                }
            }
        }
        
    }

    [PunRPC]
    public void SendViewIdBeAttacked( float damage)
    {
        BaseObject obj = GetComponent<BaseObject>();
        obj.OnBeAttacked(damage);
    }

    private bool IsGround()
    {
        return Physics2D.BoxCast(boxcollider2d.bounds.center, boxcollider2d.bounds.size, .0f, Vector2.down, .1f, LayerMask.GetMask("Ground"));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (view.IsMine)
        {
            if (collision.collider != null)
            {
                if (collision.collider.CompareTag("Player") || collision.collider.CompareTag("Shield")) rb2D.velocity = Vector2.zero;
                if (collision.collider.CompareTag("Shield") && LastTimeAttack >= CoolDownAttack)
                {
                    LastTimeAttack = 0.0f;
                    return;
                }
                if (collision.collider.CompareTag("Player") && LastTimeAttack >= CoolDownAttack)
                {
                    LastTimeAttack = 0.0f;
                    BaseObject obj = collision.gameObject.GetComponent<BaseObject>();
                    if (obj != null)
                    {
                        obj.OnBeAttacked(Damage);
                        PhotonView targetPhotonView = obj.GetComponent<PhotonView>();
                        if (targetPhotonView != null)
                        {
                            targetPhotonView.RPC("SendViewIdBeAttacked", RpcTarget.Others, Damage);
                        }
                    }
                }
            }
        }
    }


    private void OnCollisionExit2D(Collision2D collision)
    {
        if (view.IsMine) MoveToPlayer();
    }
}
