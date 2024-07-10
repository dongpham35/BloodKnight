using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HeroKnight : MonoBehaviourPunCallbacks
{
    [SerializeField] float  CoolDownAttack = 0.4f;
    [SerializeField] float  RefreshAttack = 1f;
    [SerializeField] GameObject canvasAction;
    [SerializeField] GameObject canvasInfor;

    [SerializeField] Button btnRight;
    [SerializeField] Button btnLeft;
    [SerializeField] Button btnAttack;
    [SerializeField] Button btnJump;
    [SerializeField] Button btnBlock;
    [SerializeField] GameObject Shield;
    [SerializeField] TMP_Text Username;

    [SerializeField] AudioSource hitAudio;
    [SerializeField] AudioSource attackAudio;
    [SerializeField] AudioSource blockAudio;
    [SerializeField] AudioSource jumpAudio;
    [SerializeField] AudioSource collectItemAudio;
    [SerializeField] AudioSource moveAudio;

    private Rigidbody2D     rb2D;
    private Animator        animator2D;
    private SpriteRenderer  spriterenderer2D;
    private BoxCollider2D   boxcollider2D;
    private PhotonView      view;
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
    private int             level;
    private int             levelup;

    private float           Speed;
    private float           Roll_Speed;
    private float           Damage;


    float[] AddBloodWhenUp_level = new float[] {0.1f,0.3f,0.5f,0.7f,0.9f,01.1f,1.3f,1.5f , 1.7f, 1.9f};
    float[] AddAmorWhenUp_level = new float[] {0.1f,0.4f,0.7f,1.0f,1.2f,1.1f,0.9f,0.7f, 0.8f, 1.0f };
    float[] AddDamageWhenUp_level = new float[] {0.1f,0.2f,0.3f,0.4f,0.5f,0.6f,0.7f,0.8f,0.9f,1.0f };
    float[] AddSpeedWhenUp_level = new float[] {0.1f, 0.5f, 0.5f, 0.7f, 0.7f, 0.9f, 0.9f, 1.1f, 1.3f,1.3f };


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
            canvasAction.SetActive (true);
            canvasInfor.SetActive (true);

            rb2D = GetComponent<Rigidbody2D>();
            animator2D = GetComponent<Animator>();
            spriterenderer2D = GetComponent<SpriteRenderer>();
            boxcollider2D = GetComponent<BoxCollider2D>();
            Direction = Vector2.right;

            Right_Hanlder = btnRight.GetComponent<UiButtonHanlder>();
            Left_Hanlder = btnLeft.GetComponent<UiButtonHanlder>();
            Attack_Hanlder = btnAttack.GetComponent<UiButtonHanlder>();
            Jump_Hanlder = btnJump.GetComponent<UiButtonHanlder>();
            Block_Hanlder = btnBlock.GetComponent<UiButtonHanlder>();

            Speed = transform.GetComponent<BaseObject>().Speed;
            Roll_Speed = transform.GetComponent<BaseObject>().Roll_Speed;
            Damage = transform.GetComponent<BaseObject>().Damage;


            Shield.GetComponent<BoxCollider2D>().enabled = false;

            if (PlayerPrefs.HasKey("Username"))
            {
                Username.text = $"LV:{level}-" + PlayerPrefs.GetString("Username");
            }

            CanBeAction = true;

            photonView.RPC("DataSynchronization", RpcTarget.OthersBuffered, Username.text);
        }
        else
        {
            canvasInfor.SetActive(true);
        }

    }


    private void Update()
    {
        if(view.IsMine)
        {
            TimeSinceAttack += Time.deltaTime;
            if (GetComponent<BaseObject>().currenHealth == 0)
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
                if (level < levelup)
                {
                    for (int i = level; i < levelup; i++)
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
                        photonView.RPC("UpdateHealthBar", RpcTarget.Others, GetComponent<BaseObject>().Blood, GetComponent<BaseObject>().currenHealth);
                        level++;
                    }
                    Username.text = $"LV:{level}-" + PlayerPrefs.GetString("Username");
                }
            }
            else
            {
                animator2D.SetTrigger("Death");
                MapSystem.instance.PlayerDied();
            }
        }
        
    }

    [PunRPC]
    public void DataSynchronization(string username)
    {
        Username.text = username;
    }


    private void UpdateAnimation()
    {
        if (view.IsMine)
        {
            if (rb2D.velocity.x == 0) statePlayer = NorState.Idle;
            if (rb2D.velocity.y > .1f) statePlayer = NorState.Jump;
            if (rb2D.velocity.y < -.1f) statePlayer = NorState.Fall;
            if (GetComponent<BaseObject>().isHurt)
            {
                TimeBeAttacked = Time.time;
                CanBeAction = false;
                animator2D.SetTrigger("Hurt");
                GetComponent<BaseObject>().isHurt = false;
            }
            if (isRoll)
            {
                if (statePlayer == NorState.Roll)
                {
                    if (!isChangeBoxSize)
                    {
                        boxcollider2D.offset = new Vector2(boxcollider2D.offset.x, boxcollider2D.offset.y / 2);
                        boxcollider2D.size = new Vector2(boxcollider2D.size.x, boxcollider2D.size.y / 2);
                        isChangeBoxSize = true;
                    }
                }
                else
                {
                    if (isChangeBoxSize)
                    {
                        boxcollider2D.offset = new Vector2(boxcollider2D.offset.x, boxcollider2D.offset.y * 2);
                        boxcollider2D.size = new Vector2(boxcollider2D.size.x, boxcollider2D.size.y * 2);
                        isRoll = false;
                        isChangeBoxSize = false;
                    }

                }
            }
            photonView.RPC("UpdateSpriteRenderer", RpcTarget.OthersBuffered, spriterenderer2D.flipX);
            animator2D.SetInteger("NorState", (int)statePlayer);
        }
    }

    private void RightBtn()
    {
        if (Right_Hanlder.isButtonHeld)
        {
            moveAudio.Play();
            statePlayer = NorState.Run;
            Shield.transform.position = transform.position + new Vector3(0, 0.7f, 0);    
            spriterenderer2D.flipX = false;
            Direction = Vector2.right;
            rb2D.velocity = new Vector2(Direction.x * Speed * 0.5f, rb2D.velocity.y);

        }        

        if (Right_Hanlder.isDoubleClick)
        {
            moveAudio.Play();
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
            moveAudio.Play();
            statePlayer = NorState.Run;
            Shield.transform.position = transform.position + new Vector3(-0.75f, 0.7f, 0);
            spriterenderer2D.flipX = true;
            Direction = Vector2.left;
            rb2D.velocity = new Vector2(Direction.x * Speed * 0.5f, rb2D.velocity.y);

        }
        if (Left_Hanlder.isDoubleClick)
        {
            moveAudio.Play();
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
            attackAudio.Play();
            comboAttack++;

            if(comboAttack > 3)
            {
                comboAttack = 1;
            }

            if(TimeSinceAttack > RefreshAttack)
            {
                comboAttack = 1;
            }
            RaycastHit2D ray = Physics2D.Raycast(transform.position + new Vector3(0.35f * Direction.x, 0.6f, 0), Direction, 0.7f, LayerMask.GetMask("Player", "Enemy", "Shield"));
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
                        levelup = (int)(Mathf.Log(GetComponent<BaseObject>().EXP,2));

                    }
                    obj.isHurt = false;
                    GetComponent<BaseObject>().currenHealth = Mathf.Clamp(GetComponent<BaseObject>().currenHealth + Damage * 0.03f, 0, GetComponent<BaseObject>().Blood);
                    GetComponent<BaseObject>().healthBar.value = GetComponent<BaseObject>().currenHealth;
                    photonView.RPC("UpdateHealthBar", RpcTarget.Others, GetComponent<BaseObject>().currenHealth);
                    PhotonView targetPhotonView = obj.GetComponent<PhotonView>();
                    if (targetPhotonView != null)
                    {
                        targetPhotonView.RPC("SendViewIdBeAttacked", RpcTarget.Others, Damage);
                    }
                }
            }
            animator2D.SetTrigger("Attack" + comboAttack);
            TimeSinceAttack = 0f;
        }
    }


    [PunRPC]
    public void SendViewIdBeAttacked(float damage)
    {
        BaseObject obj = GetComponent<BaseObject>();
        obj.OnBeAttacked(damage);
    }

    private void JumpBtn()
    {
        if (Jump_Hanlder.isButtonHeld && isGround())
        {
            jumpAudio.Play();
            rb2D.velocity = new Vector2(rb2D.velocity.x, Speed * 1.3f);
        }

    }

    private void BlockBtn()
    {
        if (!view.IsMine) return;
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

    [PunRPC]
    public void UpdateSpriteRenderer(bool isFlipX)
    {
        SpriteRenderer sprire = GetComponent<SpriteRenderer>();
        sprire.flipX = isFlipX;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision != null && collision.gameObject.CompareTag("Fruit"))
        {
            collectItemAudio.Play();
            Fruit f = collision.gameObject.GetComponent<Fruit>();
            GetComponent<BaseObject>().currenHealth = Mathf.Clamp(GetComponent<BaseObject>().currenHealth + f.health, 0, GetComponent<BaseObject>().Blood);
            GetComponent<BaseObject>().healthBar.value = GetComponent<BaseObject>().currenHealth;
            photonView.RPC("UpdateHealthBar", RpcTarget.Others,GetComponent<BaseObject>().currenHealth);
            Destroy(collision.gameObject);
        }
    }

    [PunRPC]
    public void UpdateHealthBar(float currentHealth)
    {
        BaseObject obj = GetComponent<BaseObject>();
        obj.healthBar.value = currentHealth;
    }

}
