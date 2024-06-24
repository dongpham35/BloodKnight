using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FireWizard : MonoBehaviour
{
    [SerializeField] GameObject[] LimitedPointCanMove;
    [SerializeField] GameObject[] PointsTeleport;

    List<GameObject>          players = new List<GameObject>();
    private GameObject        focusPlayer;
    private float             minDistance;

    private Rigidbody2D       rb2D;
    private SpriteRenderer    spriterenderer;
    private Animator          animator;
    private BoxCollider2D     boxCollider;

    private float             Speed;
    private float             Damage;

    enum NorState { Idle, Move}
    NorState stateEnemy = NorState.Idle;

    private Vector2           Direction;
    private bool              isDeath;
    private bool              isShow;

    private GameObject        pointFocus;

    private void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        spriterenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();

        Direction = Vector2.right;
        Speed = transform.GetComponent<BaseObject>().Speed;
        Damage = transform.GetComponent<BaseObject>().Damage;

        focusPlayer = null;
        minDistance = 0;
    }

    private void Update()
    {
        if (transform.GetComponent<BaseObject>().currenHealth == 0)
        {
            isDeath = true;
        }

        if (isDeath)
        {
            animator.SetTrigger("Death");
            StartCoroutine(MoveToMenu());
            SceneManager.LoadScene("MenuGame");
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

        if(!isDeath)
        {

            UpdateAnimation();
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
                if(Vector2.Distance(transform.position, player.transform.position) <= 25f)
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
            yield return new WaitForSeconds(10f);
        }
    }

    IEnumerator MoveToMenu()
    {
        yield return new WaitForSeconds(1.5f);
    }

    private void FindPlayerHasMinDistance()
    {
        foreach (GameObject player in players)
        {
            if (Vector2.Distance(transform.position, player.transform.position) > 25f) continue;
            if (minDistance > Vector2.Distance(transform.position, player.transform.position))
            {
                focusPlayer = player;
                minDistance = Vector2.Distance(transform.position, player.transform.position);
            }
        }
    }

    private void UpdateAnimation()
    {
        if(Direction.x != 0f)
        {
            if (Direction.x > .1f) spriterenderer.flipX = false;
            else spriterenderer.flipY = true;

            stateEnemy = NorState.Move;
        }
        else
        {
            stateEnemy = NorState.Idle ;
        }

        if(transform.GetComponent<BaseObject>().isHurt)
        {
            animator.SetTrigger("Hurt");
        }


        animator.SetInteger("NorState", (int)stateEnemy);

    }

    private void Move()
    {
        if(focusPlayer != null)
        {
            StopCoroutine(ChangePoint());
            
        }
        else
        {
            StartCoroutine(ChangePoint());
            if(pointFocus != null)
            {
                if(Vector2.Distance(transform.position, pointFocus.transform.position) <= 0.2f)
                {
                    Direction = Vector2.zero;
                }
                else
                {
                    Vector2.MoveTowards(transform.position, pointFocus.transform.position, Speed);
                    stateEnemy = NorState.Move;
                }
            }
        }
    }

    IEnumerator ChangePoint()
    {
        while (true)
        {
            int indexPoint = Random.Range(0, 4);
            pointFocus = LimitedPointCanMove[indexPoint];
            yield return new WaitForSeconds(6.5f);
        }
    }

    private void MoveNorToPlayer()
    {
        while(Vector2.Distance(transform.position, focusPlayer.transform.position) > 0.7f)
        {
            Vector2.MoveTowards(transform.position, focusPlayer.transform.position, Speed * Time.deltaTime * 1.5f);
        }
        Direction.x = (focusPlayer.transform.position - transform.position).x;
        RaycastHit2D ray = Physics2D.Raycast(transform.position, Direction, 0.5f, LayerMask.GetMask("Player"));
        if(ray.collider != null)
        {
            if (ray.collider.gameObject.CompareTag("Shield")) return;
            BaseObject obj = ray.collider.GetComponent<BaseObject>();
            obj.OnBeAttacked(Damage);
        }
    }

    private void Teleport()
    {
        int index = Random.Range(0, 3);
        GameObject point = PointsTeleport[index];
        animator.SetTrigger("Hide");
        isShow = true;
        spriterenderer.enabled = false;
        boxCollider.enabled = false;
        transform.position = point.transform.position;
        StartCoroutine(WaitSecond(1f));

        animator.SetBool("Show", isShow);
        StartCoroutine(WaitSecond(0.5f));
        isShow = false;
        spriterenderer.enabled = true;
        boxCollider.enabled = true;
        animator.SetBool("Show", isShow);
    }

    IEnumerator WaitSecond(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }

}
