using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BloodKnight : MonoBehaviourPunCallbacks
{
    [SerializeField] float     CoolDownAttack;
    [SerializeField] GameObject canvasAction;
    [SerializeField] GameObject canvasInfor;

    private Rigidbody2D        rb2D;
    private Animator           animator;
    private SpriteRenderer     spriteRenderer;
    private BoxCollider2D      boxCollider;
    private PhotonView         view;

    [SerializeField] Button    btnRight;
    [SerializeField] Button    btnLeft;
    [SerializeField] Button    btnAttack;
    [SerializeField] Button    btnJump;
    [SerializeField] Button    btnBlock;
    [SerializeField] GameObject Shield;
    [SerializeField] TMP_Text  Username;

    [SerializeField] AudioSource hitAudio;
    [SerializeField] AudioSource attackAudio;
    [SerializeField] AudioSource blockAudio;
    [SerializeField] AudioSource jumpAudio;
    [SerializeField] AudioSource collectItemAudio;
    [SerializeField] AudioSource moveAudio;


    enum NorState { Idel, Run, Jump, Fall, Roll}
    NorState                   statePlayer = NorState.Idel;

    private float              Speed;
    private float              Damage;
    private float              Roll_Speed;
    private int                level;
    private int                levelup;

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


    float[] AddBloodWhenUp_level = new float[] { 0.2f, 0.4f, 0.6f, 0.8f, 1.0f, 1.2f ,1.4f ,1.6f, 1.8f, 2.0f};
    float[] AddAmorWhenUp_level = new float[] {0.2f, 0.3f, 0.5f, 0.6f, 0.8f, 0.9f, 1.1f, 1.2f, 1.3f,1.4f };
    float[] AddDamageWhenUp_level = new float[] {0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1.0f, 1.2f };
    float[] AddSpeedWhenUp_level = new float[] {0.2f, 0.3f, 0.5f, 0.7f, 0.8f, 0.9f, 1.2f, 1.4f , 1.5f, 1.6f};

    private void Awake()
    {
        view = GetComponent<PhotonView>();
        GetComponentInChildren<Camera>().enabled = false;
        GetComponentInChildren<AudioListener>().enabled = false;
        canvasAction.SetActive(false);
        canvasInfor.SetActive(false);

    }
    private void Start()
    {
        if (view.IsMine)
        {
            GetComponentInChildren<Camera>().enabled = true;
            GetComponentInChildren<AudioListener>().enabled = true;
            canvasAction.SetActive(true);
            canvasInfor.SetActive(true);
            rb2D = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            boxCollider = GetComponent<BoxCollider2D>();

            rightHanlder = btnRight.GetComponent<UiButtonHanlder>();
            leftHanlder = btnLeft.GetComponent<UiButtonHanlder>();
            attackHanlder = btnAttack.GetComponent<UiButtonHanlder>();
            jumpHanlder = btnJump.GetComponent<UiButtonHanlder>();
            blockHanlder = btnBlock.GetComponent<UiButtonHanlder>();

            Speed = transform.GetComponent<BaseObject>().Speed;
            Damage = transform.GetComponent<BaseObject>().Damage;
            Roll_Speed = transform.GetComponent<BaseObject>().Roll_Speed;

            Direction = Vector2.right;

            LastTimeAttack = Time.time;

            Shield.GetComponent<BoxCollider2D>().enabled = false;
            if (PlayerPrefs.HasKey("Username"))
            {
                Username.text = $"LV:{level}-" + PlayerPrefs.GetString("Username");
            }

            CanBeAction = true;
            level = 0;
            photonView.RPC("DataSynchronization", RpcTarget.OthersBuffered, Username.text);
        }
        else
        {
            canvasInfor.SetActive(true);
        }

    }

    private void Update()
    {
        if (view.IsMine)
        {
            
            if (transform.GetComponent<BaseObject>().currenHealth == 0)
            {
                isDeath = true;
            }
            if (!isDeath)
            {
                if (Time.time - TimeBeAttacked > 0.7f) CanBeAction = true;
                if (CanBeAction)
                {
                    OnClickLeftBtn();
                    OnClickRightBtn();
                    OnClickAttackBtn();
                    OnClickBlockBtn();
                    OnClickJumpBtn();

                }
                UpdateAnimation();
                if(level < levelup)
                {
                    for(int i = level; i<levelup; i++)
                    {
                        GetComponent<BaseObject>().Blood += AddBloodWhenUp_level[i];
                        GetComponent<BaseObject>().Amor += AddAmorWhenUp_level[i];
                        GetComponent<BaseObject>().Damage += AddDamageWhenUp_level[i];
                        GetComponent<BaseObject>().Speed += AddSpeedWhenUp_level[i];
                        GetComponent<BaseObject>().currenHealth = Mathf.Clamp(GetComponent<BaseObject>().currenHealth + AddBloodWhenUp_level[i], 0, GetComponent<BaseObject>().Blood);
                        GetComponent<BaseObject>().healthBar.maxValue = GetComponent<BaseObject>().Blood;
                        GetComponent<BaseObject>().healthBar.value = GetComponent<BaseObject>().currenHealth;
                        Speed = GetComponent<BaseObject>().Speed;
                        Damage = GetComponent<BaseObject>().Damage;
                        level++;
                    }
                    Username.text = $"LV:{level}-" + PlayerPrefs.GetString("Username");
                }
            }
            else
            {
                animator.SetTrigger("Death");
                MapSystem.instance.PlayerDied();

            }
        }
        
    }


    [PunRPC]
    public void DataSynchronization(string username)
    {
        Username.text = username;
    }


    void UpdateAnimation()
    {
        if(view.IsMine)
        {
            if (rb2D.velocity.x == 0) statePlayer = NorState.Idel;
            if (rb2D.velocity.y > .1f)
            {
                statePlayer = NorState.Jump;
                if (attackHanlder.isButtonHeld) animator.SetTrigger("Jump_Attack");
            }

            if (rb2D.velocity.y < -.1f) statePlayer = NorState.Fall;
            if (GetComponent<BaseObject>().isHurt)
            {
                TimeBeAttacked = Time.time;
                CanBeAction = false;
                animator.SetTrigger("Hurt");
                GetComponent<BaseObject>().isHurt = false;
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
            photonView.RPC("UpdateSpriteRenderer", RpcTarget.OthersBuffered, spriteRenderer.flipX);
            animator.SetInteger("NorState", (int)statePlayer);
        }
    }

    private void OnClickRightBtn()
    {
        if (rightHanlder.isButtonHeld)
        {
            moveAudio.Play();
            Shield.transform.position = transform.position + new Vector3(0.8f, -0.5f, 0);
            statePlayer = NorState.Run;
            spriteRenderer.flipX = false;
            Direction = Vector2.right;
            
            rb2D.velocity = new Vector2(Direction.x * Speed * 0.5f, rb2D.velocity.y);
        }

        if(rightHanlder.isDoubleClick)
        {
            moveAudio.Play();
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
            moveAudio.Play();
            Shield.transform.position = transform.position + new Vector3(-0.05f, -0.5f, 0);
            statePlayer = NorState.Run;
            spriteRenderer.flipX = true;
            Direction = Vector2.left;

            rb2D.velocity = new Vector2(Direction.x * Speed * 0.5f, rb2D.velocity.y);
        }

        if (leftHanlder.isDoubleClick)
        {
            moveAudio.Play();
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
                attackAudio.Play();
                LastTimeAttack = Time.time;
                animator.SetTrigger("Attack");
                RaycastHit2D ray = Physics2D.Raycast(transform.position + new Vector3(0.42f * Direction.x, -0.25f, 0), Direction, 0.7f, LayerMask.GetMask("Player", "Enemy", "Shield"));
                if(ray.collider != null)
                {
                    if (ray.collider.CompareTag("Shield"))
                    {
                        blockAudio.Play();
                        return;
                    }
                    BaseObject obj = ray.collider.GetComponent<BaseObject>();
                    if (obj != null)
                    {
                        hitAudio.Play();
                        obj.OnBeAttacked(Damage);
                        if (obj.currenHealth == 0)
                        {
                            GetComponent<BaseObject>().EXP += obj.EXP;
                            levelup = (int)(Mathf.Log(GetComponent<BaseObject>().EXP, 2));
                        }
                        obj.isHurt = false;
                        GetComponent<BaseObject>().currenHealth = Mathf.Clamp(GetComponent<BaseObject>().currenHealth + Damage*0.05f, 0, GetComponent<BaseObject>().Blood);
                        GetComponent<BaseObject>().healthBar.value = GetComponent<BaseObject>().currenHealth;
                        photonView.RPC("UpdateHealthBar", RpcTarget.Others, GetComponent<BaseObject>().currenHealth);
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


    [PunRPC]
    public void SendViewIdBeAttacked(float damage)
    {
        BaseObject obj = GetComponent<BaseObject>();
        obj.OnBeAttacked(damage);
    }
    private void OnClickJumpBtn()
    {
        if (jumpHanlder.isButtonHeld && isGround())
        {
            jumpAudio.Play();
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
        return Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0f, Vector2.down, .1f, LayerMask.GetMask("Ground"));
    }

    [PunRPC]
    public void UpdateSpriteRenderer(bool isFlipX)
    {
        SpriteRenderer spri = GetComponent<SpriteRenderer>();
        spri.flipX = isFlipX;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision != null && collision.gameObject.CompareTag("Fruit"))
        {
            collectItemAudio.Play();
            Fruit f = collision.gameObject.GetComponent<Fruit>();
            GetComponent<BaseObject>().currenHealth = Mathf.Clamp(GetComponent<BaseObject>().currenHealth + f.health, 0, GetComponent<BaseObject>().Blood);
            GetComponent<BaseObject>().healthBar.value = GetComponent<BaseObject>().currenHealth;
            photonView.RPC("UpdateHealthBar", RpcTarget.Others, GetComponent<BaseObject>().currenHealth);
            Destroy(collision.gameObject);
        }
    }

    [PunRPC]
    public void UpdateHealthBar( float currentHealth)
    {
        BaseObject obj = GetComponent<BaseObject>();
        obj.healthBar.value = currentHealth;
    }
}
