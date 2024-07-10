using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FireWizard : MonoBehaviourPunCallbacks
{
    [SerializeField] Vector3[]    PointLimited;
    [SerializeField] float[]      LimiedPointX;
    [SerializeField] float[]      LimiedPointY;
    [SerializeField] GameObject   ProjectilePrefab;
    [SerializeField] float        CoolDownAttack;

    List<GameObject>          players = new List<GameObject>();
    private GameObject        focusPlayer;
    private float             minDistance;

    private Rigidbody2D       rb2D;
    private SpriteRenderer    spriterenderer;
    private Animator          animator;
    private BoxCollider2D     boxCollider;
    private PhotonView        view;

    private float             Speed;
    private float             Damage;
    private float             pointX;
    private float             pointY;

    enum NorState { Idle, Move}
    NorState stateEnemy = NorState.Idle;

    private Vector2           Direction;
    private bool              isDeath;

    private Vector3           pointFocus;
    private bool              NorAttack;
    private bool              SkillAttack;
    private float             LastTimeDamaged;
    private bool              StateSkillAttack;
    private bool              CanBeAction;
    private float             TimeBeAttacked;
    private int               IndexPointLimited;

    private void Awake()
    {
        view = GetComponent<PhotonView>();
    }
    private void Start()
    {
        if(view.IsMine)
        {
            rb2D = GetComponent<Rigidbody2D>();
            spriterenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            boxCollider = GetComponent<BoxCollider2D>();
        }

        Direction = Vector2.right;
        Speed = transform.GetComponent<BaseObject>().Speed;
        Damage = transform.GetComponent<BaseObject>().Damage;

        focusPlayer = null;
        minDistance = 0;
        IndexPointLimited = 0;
        pointFocus = PointLimited[IndexPointLimited];
        LastTimeDamaged = 0.0f;
        CanBeAction = true;
    }

    private void Update()
    {
        if (view.IsMine)
        {
            if (transform.GetComponent<BaseObject>().currenHealth == 0)
            {
                isDeath = true;
            }

            if (isDeath)
            {
                animator.SetTrigger("Death");
            }

            if (GameObject.FindGameObjectWithTag("Player"))
            {
                StartCoroutine(GetPlayerInRoom());
                FindPlayerHasMinDistance();
            }
            else
            {
                StopCoroutine(GetPlayerInRoom());
            }

            if (!isDeath)
            {
                LastTimeDamaged += Time.deltaTime;
                if (Time.time - TimeBeAttacked >= 1.2f) CanBeAction = true;
                if(focusPlayer != null)
                {
                    if (Vector2.Distance(transform.position, focusPlayer.transform.position) >= 10f ) focusPlayer = null;

                }
                if (CanBeAction)
                {
                    if (focusPlayer != null)
                    {
                        Direction.x = focusPlayer.transform.position.x - transform.position.x;
                        MoveToAttackPlayer();
                    }
                    else
                    {
                        MoveNormal();
                    }
                }
                if (transform.GetComponent<BaseObject>().isHurt)
                {
                    CanBeAction = false;
                    TimeBeAttacked = Time.time;
                    animator.SetTrigger("Hurt");
                    transform.GetComponent<BaseObject>().isHurt = false;
                }
                UpdateAnimation();
            }
            else
            {
                MapSystem.instance.BossIsDie();
                Destroy(gameObject);
            }
        }

    }

    IEnumerator GetPlayerInRoom()
    {
        while(true)
        {
            players.Clear();
            GameObject[] li_Player = GameObject.FindGameObjectsWithTag("Player");
            foreach(GameObject player in  li_Player)
            {
                if(Vector2.Distance(transform.position, player.transform.position) <= 10f)
                {
                    if (focusPlayer == null)
                    {
                        focusPlayer = player;
                        minDistance = Vector2.Distance(transform.position, player.transform.position);
                    }
                    else
                    {
                        if (minDistance > Vector2.Distance(transform.position, player.transform.position))
                        {
                            focusPlayer = player;
                            minDistance = Vector2.Distance(transform.position, player.transform.position);
                        }
                    }
                }
                players.Add(player);
            }
            yield return new WaitForSeconds(3f);
        }
    }


    private void FindPlayerHasMinDistance()
    {
        foreach (GameObject player in players)
        {
            if (Vector2.Distance(transform.position, player.transform.position) > 5f) continue;
            if (minDistance > Vector2.Distance(transform.position, player.transform.position))
            {
                focusPlayer = player;
                minDistance = Vector2.Distance(transform.position, player.transform.position);
            }
        }
    }

    private void UpdateAnimation()
    {
        
        if(view.IsMine)
        {
            if (Direction.x != 0f)
            {
                if (Direction.x > .1f) spriterenderer.flipX = false;
                else spriterenderer.flipX = true;

                stateEnemy = NorState.Move;
            }
            else
            {
                stateEnemy = NorState.Idle;
            }

            if (transform.GetComponent<BaseObject>().isHurt)
            {
                animator.SetTrigger("Hurt");
            }

            photonView.RPC("UpdateSpriteRenderer", RpcTarget.OthersBuffered, spriterenderer.flipX);
            animator.SetInteger("NorState", (int)stateEnemy);
        }

    }

    [PunRPC]
    public void UpdateSpriteRenderer(bool state)
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        sprite.flipX = state;
    }

    private void MoveToAttackPlayer()
    {
        if (view.IsMine)
        {
            if (IsGround())
            {
                if (Random.Range(0, 50) % 2 == 0 && !NorAttack && !SkillAttack) NorAttack = true;
                else if ((Random.Range(0, 50) % 2 != 0 && !NorAttack && !SkillAttack)) SkillAttack = true;
                if (NorAttack)
                {
                    MoveNorToPlayer();
                }
                if (SkillAttack && !StateSkillAttack)
                {
                    TeleportSkill();
                }
            }
        }
    }

    private void MoveNormal()
    {
        if(Vector2.Distance(transform.position, pointFocus) < 0.2f)
        {
            IndexPointLimited++;
            if (IndexPointLimited > 1) IndexPointLimited = 0;
            pointFocus = PointLimited[IndexPointLimited];
        }
        Direction.x = pointFocus.x - transform.position.x;
        transform.position = Vector2.MoveTowards(transform.position, pointFocus, Time.deltaTime * Speed);
    }


    private void MoveNorToPlayer()
    {
        //move enemy to player        
        if(focusPlayer != null)
        {
            Vector3 look = focusPlayer.transform.position - transform.position;
            Direction.x = look.normalized.x;
            if (Vector3.Distance(focusPlayer.transform.position, transform.position) >= 0.7f)
            {
                rb2D.velocity = new Vector2(Direction.x * Speed, rb2D.velocity.y);
            }
        }

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Shield"))
        {
            rb2D.velocity = Vector2.zero;
            
            if(LastTimeDamaged >= CoolDownAttack)
            {
                LastTimeDamaged = 0.0f;
                if (collision.collider.CompareTag("Shield")) return;
                if (collision.collider.CompareTag("Player"))
                {
                    animator.SetTrigger("Attack");
                    LastTimeDamaged = 0.0f;
                    BaseObject obj = collision.gameObject.GetComponent<BaseObject>();
                    if (obj != null)
                    {
                        obj.OnBeAttacked(Damage);
                        PhotonView targetPhotonView = collision.gameObject.GetComponent<PhotonView>();
                        if (targetPhotonView != null)
                        {
                            targetPhotonView.RPC("SendViewIdBeAttacked", RpcTarget.Others, Damage);
                        }
                    }
                    NorAttack = false;
                    
                }
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Shield"))
        {
            rb2D.velocity = Vector2.zero;
            LastTimeDamaged += Time.deltaTime;
            
            if(LastTimeDamaged >= CoolDownAttack)
            {
                animator.SetTrigger("Attack");
                LastTimeDamaged = 0.0f;
                BaseObject obj = collision.gameObject.GetComponent<BaseObject>();
                if (obj != null)
                {
                    obj.OnBeAttacked(Damage);
                    PhotonView targetPhotonView = collision.gameObject.GetComponent<PhotonView>();
                    if (targetPhotonView != null)
                    {
                        targetPhotonView.RPC("SendViewIdBeAttacked", RpcTarget.Others, Damage);
                    }
                }
                NorAttack = false;
                if(Random.Range(0, 5) % 2 == 0 )
                {
                    TeleportSkill();
                }
            }

        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (view.IsMine)
        {
            if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Shield"))
            {
                MoveToAttackPlayer();
            }
        }
    }

    private bool IsGround()
    {
        return Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, .0f, Vector2.down, .1f, LayerMask.GetMask("Ground"));
    }

    [PunRPC]
    public void SendViewIdBeAttacked(float damage)
    {
        BaseObject obj = GetComponent<BaseObject>();
        obj.OnBeAttacked(damage);
    }

    private void TeleportSkill()
    {
        Direction.x = focusPlayer.transform.position.x - transform.position.x;
        StateSkillAttack = true;
        pointX = Random.Range(LimiedPointX[0], LimiedPointX[1]);
        pointY = Random.Range(LimiedPointY[0], LimiedPointY[1]);
        Vector3 point = new Vector3(pointX, pointY, 0);
        StartCoroutine(Teleport(point));
        
    }

    private void CloneProjetile(GameObject projectile ,int SumofProjectile, Vector3 startPoint, float speed)
    {
        LastTimeDamaged = 0.0f;
        for(int i = 0; i < SumofProjectile; i++)
        {
            float angle = i * (360 / SumofProjectile);
            float radian = angle * Mathf.Deg2Rad;

            Vector3 direction_projectile = new Vector3(Mathf.Cos(radian), Mathf.Sin(radian), 0);

            GameObject clone_projectile = PhotonNetwork.Instantiate(projectile.name, startPoint, Quaternion.Euler(0, 0, 90));

            Rigidbody2D rb = clone_projectile.GetComponent<Rigidbody2D>();
            if(rb != null )
            {
                rb.velocity = direction_projectile * speed * 0.5f;
            }
        }
        StateSkillAttack = false;
        SkillAttack = false;
    }

    IEnumerator Teleport(Vector3 point)
    {
        animator.SetTrigger("Hide");

        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        spriterenderer.enabled = false;
        boxCollider.enabled = false;
        gameObject.GetComponentInChildren<Canvas>().enabled = false;
        transform.position = point;
        yield return new WaitForSeconds(0.5f);
        spriterenderer.enabled = true;
        boxCollider.enabled = true;
        gameObject.GetComponentInChildren<Canvas>().enabled = true;
        animator.SetTrigger("Show");
        CloneProjetile(ProjectilePrefab, Random.Range(6, 12), transform.position, Speed * 2.1f);

        yield return new WaitForSeconds(0f);
    }

}
