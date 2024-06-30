using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HeroKnight : MonoBehaviour
{
    [SerializeField] float  CoolDownAttack = 0.4f;
    [SerializeField] float  RefreshAttack = 1f;

    [SerializeField] Button btnRight;
    [SerializeField] Button btnLeft;
    [SerializeField] Button btnAttack;
    [SerializeField] Button btnJump;
    [SerializeField] Button btnBlock;
    [SerializeField] GameObject Shield;
    [SerializeField] TMP_Text Username;

    private Rigidbody2D     rb2D;
    private Animator        animator2D;
    private SpriteRenderer  spriterenderer2D;
    private BoxCollider2D   boxcollider2D;
    private Camera          mainCam;

    enum                     NorState { Idle, Run, Jump, Fall, Roll}
    NorState                 statePlayer = NorState.Idle;

    private Vector2          Direction;
    private bool             isBlock;
    private bool             isIdle_Block;
    private bool             isDeath;
    private bool             isRoll;
    private bool             isChangeBoxSize;

    private UiButtonHanlder Right_Hanlder;
    private UiButtonHanlder Left_Hanlder;
    private UiButtonHanlder Attack_Hanlder;
    private UiButtonHanlder Jump_Hanlder;
    private UiButtonHanlder Block_Hanlder;

    private float           TimeSinceAttack = 0.0f;
    private int             comboAttack = 0;
    private float           TimeBeAttacked;
    private bool            CanBeAction;

    private float           Speed;
    private float           Roll_Speed;
    private float           Damage;

    private void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        animator2D = GetComponent<Animator>();
        spriterenderer2D = GetComponent<SpriteRenderer>();
        boxcollider2D = GetComponent<BoxCollider2D>();
        Direction = Vector2.right;

        Right_Hanlder = btnRight.GetComponent<UiButtonHanlder>();
        Left_Hanlder = btnLeft.GetComponent<UiButtonHanlder>();
        Attack_Hanlder =btnAttack.GetComponent<UiButtonHanlder>();
        Jump_Hanlder = btnJump.GetComponent<UiButtonHanlder>();
        Block_Hanlder = btnBlock.GetComponent<UiButtonHanlder>();

        Speed = transform.GetComponent<BaseObject>().Speed;
        Roll_Speed = transform.GetComponent<BaseObject>().Roll_Speed;
        Damage = transform.GetComponent<BaseObject>().Damage;

        mainCam = Camera.main;
        mainCam.transform.SetParent(transform);
        mainCam.transform.position = new Vector3(0, 1, -10);

        Shield.GetComponent<BoxCollider2D>().enabled = false;

        if (PlayerPrefs.HasKey("username"))
        {
            Username.text = PlayerPrefs.GetString("username");
        }

        CanBeAction = true;
    }


    private void Update()
    {
        TimeSinceAttack += Time.deltaTime;
        if(transform.GetComponent<BaseObject>().currenHealth == 0)
        {
            isDeath = true;
        }
        if (!isDeath)
        {
            if (Time.time - TimeBeAttacked > 0.5f) CanBeAction = true;
            if (CanBeAction)
            {
                RightBtn();
                LeftBtn();
                AttackBtn();
                JumpBtn();
                BlockBtn();
            }

            UpdateAnimation();
        }
        else
        {
            animator2D.SetTrigger("Death");
            StartCoroutine(ReturnMenuGame());
            SceneManager.LoadScene("MenuGame");
        }
        
    }

    IEnumerator ReturnMenuGame()
    {
        yield return new WaitForSeconds(1.5f);
    }

    private void UpdateAnimation()
    {
        if (rb2D.velocity.x == 0) statePlayer = NorState.Idle;
        if (rb2D.velocity.y > .1f) statePlayer = NorState.Jump;
        if (rb2D.velocity.y < -.1f) statePlayer = NorState.Fall;
        if (transform.GetComponent<BaseObject>().isHurt)
        {
            TimeBeAttacked = Time.time;
            CanBeAction = false;
            animator2D.SetTrigger("Hurt");
            transform.GetComponent<BaseObject>().isHurt = false;
        }
        if (isRoll)
        {
            if(statePlayer == NorState.Roll)
            {
                if(!isChangeBoxSize)
                {
                    boxcollider2D.offset = new Vector2(boxcollider2D.offset.x, boxcollider2D.offset.y / 2);
                    boxcollider2D.size = new Vector2(boxcollider2D.size.x, boxcollider2D.size.y / 2);
                    isChangeBoxSize = true;
                }
            }
            else
            {
                if(isChangeBoxSize)
                {
                    boxcollider2D.offset = new Vector2(boxcollider2D.offset.x, boxcollider2D.offset.y * 2);
                    boxcollider2D.size = new Vector2(boxcollider2D.size.x, boxcollider2D.size.y * 2);
                    isRoll = false;
                    isChangeBoxSize = false;
                }
                
            }
        }

        animator2D.SetInteger("NorState", (int) statePlayer);
    }

    private void RightBtn()
    {
        if(Right_Hanlder.isButtonHeld)
        {
            statePlayer = NorState.Run;
            Shield.transform.position = transform.position + new Vector3(0, 0.7f, 0);
            spriterenderer2D.flipX = false;
            Direction = Vector2.right;
            rb2D.velocity = new Vector2(Direction.x * Speed, rb2D.velocity.y);

        }

        if(Right_Hanlder.isDoubleClick)
        {
            isRoll = true;
            Direction = Vector2.right;
            spriterenderer2D.flipX = false;
            statePlayer = NorState.Roll;

            rb2D.velocity = new Vector2(Direction.x * Roll_Speed, rb2D.velocity.y);

            Right_Hanlder.isDoubleClick = false;
        }
    }

    private void LeftBtn()
    {
        if (Left_Hanlder.isButtonHeld)
        {
            statePlayer = NorState.Run;
            Shield.transform.position = transform.position + new Vector3(-0.75f, 0.7f, 0);
            spriterenderer2D.flipX = true;
            Direction = Vector2.left;
            rb2D.velocity = new Vector2(Direction.x * Speed, rb2D.velocity.y);

        }
        if (Left_Hanlder.isDoubleClick)
        {
            isRoll = true;
            Direction = Vector2.left;
            statePlayer = NorState.Roll;
            spriterenderer2D.flipX=true;

            rb2D.velocity = new Vector2(Direction.x * Roll_Speed, rb2D.velocity.y);

            Left_Hanlder.isDoubleClick = false;
        }
    }

    private void AttackBtn()
    {
        if(Attack_Hanlder.isButtonHeld && TimeSinceAttack > CoolDownAttack && isGround())
        {
            comboAttack++;

            if(comboAttack > 3)
            {
                comboAttack = 1;
            }

            if(TimeSinceAttack > RefreshAttack)
            {
                comboAttack = 1;
            }
            RaycastHit2D ray = Physics2D.Raycast(transform.position + new Vector3(0.35f * Direction.x, 0.6f, 0), Direction, 0.6f, LayerMask.GetMask("Player", "Enemy", "Shield"));
            if(ray.collider != null)
            {
                if (ray.collider.CompareTag("Shield")) return;
                BaseObject obj = ray.collider.GetComponent<BaseObject>();
                obj.OnBeAttacked(Damage);
            }
            animator2D.SetTrigger("Attack" + comboAttack);
            TimeSinceAttack = 0f;
        }
    }

    private void JumpBtn()
    {
        if (Jump_Hanlder.isButtonHeld && isGround())
        {
            rb2D.velocity = new Vector2(rb2D.velocity.x, Speed * 1.3f);
        }

    }

    private void BlockBtn()
    {
        if (Block_Hanlder.isButtonHeld && !isBlock && isGround())
        {
            isBlock = true;
            animator2D.SetTrigger("Block");
            Shield.GetComponent<BoxCollider2D>().enabled = true;
            rb2D.bodyType = RigidbodyType2D.Static;
        }
        if(Block_Hanlder.isButtonHeld && isBlock && isGround())
        {
            isIdle_Block = true;
            animator2D.SetBool("Idle_Block", isIdle_Block);
            Shield.GetComponent<BoxCollider2D>().enabled = true;
            rb2D.bodyType = RigidbodyType2D.Static;
        }

        if (!Block_Hanlder.isButtonHeld)
        {
            isIdle_Block = false;
            isBlock = false;
            animator2D.SetBool("Idle_Block", isIdle_Block) ;
            Shield.GetComponent<BoxCollider2D>().enabled = false;
            rb2D.bodyType = RigidbodyType2D.Dynamic;
        }
    }

    private bool isGround()
    {
        return Physics2D.BoxCast(boxcollider2D.bounds.center, boxcollider2D.bounds.size, 0f, Vector2.down, .1f, LayerMask.GetMask("Ground"));
    }


}
