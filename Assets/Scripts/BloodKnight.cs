using System.Collections;
using System.Collections.Generic;
using TMPro;
using TreeEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BloodKnight : MonoBehaviour
{
    [SerializeField] float     CoolDownAttack;

    private Rigidbody2D        rb2D;
    private Animator           animator;
    private SpriteRenderer     spriteRenderer;
    private BoxCollider2D      boxCollider;
    private Camera             cam;

    [SerializeField] Button    btnRight;
    [SerializeField] Button    btnLeft;
    [SerializeField] Button    btnAttack;
    [SerializeField] Button    btnJump;
    [SerializeField] Button    btnBlock;
    [SerializeField] GameObject Shield;
    [SerializeField] TMP_Text  Username;

    enum NorState { Idel, Run, Jump, Fall, Roll}
    NorState                   statePlayer = NorState.Idel;

    private float              Speed;
    private float              Damage;
    private float              Roll_Speed;

    private bool               isDeath;
    private bool               isRoll = false;
    private bool               isChangeBoxSize = false;

    private Vector2            Direction;


    private UiButtonHanlder    rightHanlder;
    private UiButtonHanlder    leftHanlder;
    private UiButtonHanlder    attackHanlder;
    private UiButtonHanlder    jumpHanlder;
    private UiButtonHanlder    blockHanlder;

    private float              LastTimeAttack;
    private float              TimeBeAttacked;
    private bool               CanBeAction;

    private void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        cam = Camera.main;
        cam.transform.SetParent(transform);
        cam.transform.position = new Vector3(0, 1, -10);

        rightHanlder = btnRight.GetComponent<UiButtonHanlder>();
        leftHanlder = btnLeft.GetComponent<UiButtonHanlder>();
        attackHanlder = btnAttack.GetComponent<UiButtonHanlder>();
        jumpHanlder = btnJump.GetComponent<UiButtonHanlder>();
        blockHanlder = btnBlock.GetComponent<UiButtonHanlder>();

        Speed = transform.GetComponent<BaseObject>().Speed;
        Damage = transform.GetComponent<BaseObject>().Damage;
        Roll_Speed = transform.GetComponent<BaseObject>().Roll_Speed ;
        Direction = Vector2.right;

        LastTimeAttack = Time.time;

        Shield.GetComponent<BoxCollider2D>().enabled = false;
        if (PlayerPrefs.HasKey("username"))
        {
            Username.text = PlayerPrefs.GetString("username");
        }

        CanBeAction = true;
    }

    private void Update()
    {
        if(transform.GetComponent<BaseObject>().currenHealth == 0)
        {
            isDeath = true;
        }
        if(!isDeath)
        {
            if (Time.time - TimeBeAttacked > 0.7f) CanBeAction = true;
            if(CanBeAction)
            {
                OnClickLeftBtn();
                OnClickRightBtn();
                OnClickAttackBtn();
                OnClickBlockBtn();
                OnClickJumpBtn();

            }
            UpdateAnimation();
        }
        else
        {
            animator.SetTrigger("Death");
            StartCoroutine(ReturnMenuGame());
            SceneManager.LoadScene("MenuGame");
        }
    }

    IEnumerator ReturnMenuGame()
    {
        yield return new WaitForSeconds(1.5f);
    }
    void UpdateAnimation()
    {
        if (rb2D.velocity.x == 0) statePlayer = NorState.Idel;
        if (rb2D.velocity.y > .1f)
        {
            statePlayer = NorState.Jump;
            if (attackHanlder.isButtonHeld) animator.SetTrigger("Jump_Attack");
        }
        
        if (rb2D.velocity.y < -.1f) statePlayer = NorState.Fall;
        if (transform.GetComponent<BaseObject>().isHurt)
        {
            TimeBeAttacked = Time.time;
            CanBeAction = false;
            animator.SetTrigger("Hurt");
            transform.GetComponent<BaseObject>().isHurt = false;
        }

        if (isRoll)
        {
            if (statePlayer == NorState.Roll)
            {
                if (!isChangeBoxSize)
                {
                    boxCollider.offset = new Vector2(boxCollider.offset.x, boxCollider.offset.y * 2);
                    boxCollider.size = new Vector2(boxCollider.size.x, boxCollider.size.y / 2);
                    isChangeBoxSize = true;
                }
            }
            else
            {
                if (isChangeBoxSize)
                {
                    boxCollider.offset = new Vector2(boxCollider.offset.x, boxCollider.offset.y / 2);
                    boxCollider.size = new Vector2(boxCollider.size.x, boxCollider.size.y * 2);
                    isRoll = false;
                    isChangeBoxSize = false;
                }

            }
        }
        animator.SetInteger("NorState", (int) statePlayer);
    }

    private void OnClickRightBtn()
    {
        if (rightHanlder.isButtonHeld)
        {

            Shield.transform.position = transform.position + new Vector3(0.8f, -0.5f, 0);
            statePlayer = NorState.Run;
            spriteRenderer.flipX = false;
            Direction = Vector2.right;
            
            rb2D.velocity = new Vector2(Direction.x * Speed, rb2D.velocity.y);
        }

        if(rightHanlder.isDoubleClick)
        {
            isRoll = true;
            statePlayer = NorState.Roll;
            spriteRenderer.flipX = false;
            Direction = Vector2.right;
            rb2D.velocity = new Vector2(Direction.x * Roll_Speed, rb2D.velocity.y);

            rightHanlder.isDoubleClick = false;
        }
    }

    private void OnClickLeftBtn()
    {
        if (leftHanlder.isButtonHeld)
        {
            Shield.transform.position = transform.position + new Vector3(-0.05f, -0.5f, 0);
            statePlayer = NorState.Run;
            spriteRenderer.flipX = true;
            Direction = Vector2.left;

            rb2D.velocity = new Vector2(Direction.x * Speed, rb2D.velocity.y);
        }

        if (leftHanlder.isDoubleClick)
        {
            isRoll = true;
            statePlayer = NorState.Roll;
            spriteRenderer.flipX = true;
            Direction = Vector2.left;
            rb2D.velocity = new Vector2(Direction.x * Roll_Speed, rb2D.velocity.y);
            leftHanlder.isDoubleClick=false;
        }
    }

    private void OnClickAttackBtn()
    {
        if (attackHanlder.isButtonHeld && isGround())
        {
            if(Time.time - LastTimeAttack > CoolDownAttack)
            {
                LastTimeAttack = Time.time;
                animator.SetTrigger("Attack");
                RaycastHit2D ray = Physics2D.Raycast(transform.position + new Vector3(0.42f * Direction.x, -0.25f, 0), Direction, 0.5f, LayerMask.GetMask("Player", "Enemy", "Shield"));
                if(ray.collider != null)
                {
                    if (ray.collider.CompareTag("Shield")) return;
                    BaseObject obj = ray.collider.GetComponent<BaseObject>();
                    obj.OnBeAttacked(Damage);
                }
            }
        }
    }

    private void OnClickJumpBtn()
    {
        if (jumpHanlder.isButtonHeld && isGround())
        {
            rb2D.velocity = new Vector2(rb2D.velocity.x, Speed * 1.2f);
        }
    }

    private void OnClickBlockBtn()
    {
        if (blockHanlder.isButtonHeld && isGround())
        {
            animator.SetBool("Block", true);
            Shield.GetComponent<BoxCollider2D>().enabled = true;
            rb2D.bodyType = RigidbodyType2D.Static;
        }
        if (!blockHanlder.isButtonHeld)
        {
            animator.SetBool("Block", false);
            Shield.GetComponent<BoxCollider2D>().enabled = false;
            rb2D.bodyType = RigidbodyType2D.Dynamic;
        }
    }

    private bool isGround()
    {
        return Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.size, 0f, Vector2.down, LayerMask.GetMask("Ground"));
    }

}
