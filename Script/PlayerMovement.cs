using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerMovement : MonoBehaviour
{
    [Header("Player Atribute")]

    [SerializeField] private int Health = 10;

    [SerializeField] private int Dmg = 10;

    [SerializeField] private bool is_alive = true;
    
    [SerializeField] private float Speed;

    [HideInInspector] public float movementX = 0;

    [HideInInspector] public float movementY = 0;

    [SerializeField] private ParticleSystem Dust;

    [SerializeField] private ParticleSystem DustWall;


    [Header("Attack Control")]

    [HideInInspector] public bool is_attacking = false;

    [SerializeField]  private float Attack_Range;

    [SerializeField]  private Vector3 Hit_Position;


    [Header("Jump Control")]

    public float jumpforce;

    public float timeonair;

    [SerializeField] private bool in_air;

    [SerializeField] private float GravityScale = 10;

    [SerializeField] private float FallingGravityScale = 10;

    [SerializeField] private  float CoyoteTime = 0.5f;

    [SerializeField] private float CoyoteTimeCounter;

    RaycastHit2D jumplRight;

    RaycastHit2D JumpLeft;



    [Header("Ground Raycast")]

    [SerializeField] private bool On_Ground = false;

    [SerializeField] private float GroundLenght = 0.7f;

    [SerializeField] private LayerMask GroundLayer;

    [SerializeField] private LayerMask EnemyLayer;


    [Header("Wall Raycast")]

    [SerializeField] private float WallJumpTime = 0.2f;

    [SerializeField] private float TimeInWall = 10f;

    [SerializeField] private float WallSlideSpeed = 0.3f;

    [SerializeField] private float WallDistance = 0.5f;

    [SerializeField] private float Xwallforce;

    [SerializeField] private float Ywallforce;

    [HideInInspector] public bool Is_Wall_Sliding;

    [SerializeField] private bool  Wall_jumping = false;

    [SerializeField]  private float jumpTime;

    RaycastHit2D wallcheckHit;
  

    /*Component*/

    private Rigidbody2D Rb2d;

    private Animator Anim;
    public static PlayerMovement player_script { get; private set; }
    int anim_walk = Animator.StringToHash("Walk");
    int anim_jump = Animator.StringToHash("Jump");
    int anim_ground = Animator.StringToHash("Ground");
    int anim_wallslide = Animator.StringToHash("WallSlide");


    void Start()
    {
        player_script = this;
        
        Rb2d = GetComponent<Rigidbody2D>();
        Anim = GetComponent<Animator>();

        jumpforce = Mathf.Sqrt(jumpforce * -2 * (Physics2D.gravity.y * Rb2d.gravityScale)); //Velocidad de salto en el aire
    }

    void FixedUpdate()
    {
        if (is_alive == true)
        {

        /* ------------------------[Walk Physic]-----------------------*/

        Walk();

        /* ------------------------[Animations Set-Up]-----------------------*/

        Anim.SetFloat(anim_walk, Mathf.Abs(movementX));

        Anim.SetBool(anim_ground, On_Ground);

        Anim.SetBool(anim_wallslide, Is_Wall_Sliding);

        /* ------------------------[Player Rotation]-----------------------*/

        if (!Mathf.Approximately(0, movementX))
        {
            transform.rotation = movementX < 0 ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;
        }

        /* ------------------------[Coyote Time]-----------------------*/

            timeonair = On_Ground || wallcheckHit ? timeonair = 0 : timeonair += Time.deltaTime; //Calculo de tiempo en el aire.
        
            CoyoteTimeCounter = On_Ground ? CoyoteTimeCounter = CoyoteTime : CoyoteTimeCounter -= Time.deltaTime; //Calculo de tiempo en el aire antes de saltar.

        /* ------------------------[Ground RayCast]-----------------------*/

            On_Ground = Physics2D.Raycast(transform.position, Vector2.down, GroundLenght, GroundLayer);
     
        /* ------------------------[Slide Wall RayCast]-----------------------*/

             wallcheckHit = transform.rotation.y == 0 ? wallcheckHit = Physics2D.Raycast(transform.position, new Vector2(WallDistance, 0), WallDistance, GroundLayer) : wallcheckHit = Physics2D.Raycast(transform.position, new Vector2(-WallDistance, 0), WallDistance, GroundLayer);

        /* ------------------------[Jump RayCasat]-----------------------*/

        JumpLeft = Physics2D.Raycast(new Vector3(this.transform.position.x - 0.3f, this.transform.position.y, this.transform.position.z), Vector2.up, GroundLenght, GroundLayer);

        jumplRight = Physics2D.Raycast(new Vector3(this.transform.position.x + 0.3f, this.transform.position.y, this.transform.position.z), Vector2.up, GroundLenght, GroundLayer);

        /* ------------------------[Wall Slide Logic]-----------------------*/

        WallSlide();

        /* ------------------------[Anti Gravity Apex]-----------------------*/

        AntiGravityApex();

        /* ------------------------[Head colision]-----------------------*/

        BumpedHead();
        /* ------------------------[Fall Animation]-----------------------*/
            
        Fall();
        }
    }

    public void Walk()
    {
        transform.position += new Vector3(movementX,0, 0) * Time.deltaTime * Speed;
   
    }

    public void Jump()
    {
        if (On_Ground)
        {
            CreateDust();
            Rb2d.AddForce(new Vector2(0, jumpforce), ForceMode2D.Impulse);
            Anim.SetTrigger(anim_jump);
        }

        else if (!On_Ground)
        {
            //Tiempo de tolerancia para realizar el CoyoteTime
            if (CoyoteTimeCounter > 0f && Rb2d.velocity.y <=0)
            {
                CoyoteTimeCounter = 0;
                CreateDust();
                Rb2d.AddForce(new Vector2(0, jumpforce), ForceMode2D.Impulse);

                Anim.SetTrigger(anim_jump);

            }

        }

        if (Is_Wall_Sliding || wallcheckHit  && movementY > 1)
        {
            Wall_jumping = true;
            Invoke("SetWallJumpinFalse", WallJumpTime);

        }if (Wall_jumping)
        {
            CreateDust();
            Rb2d.velocity = new Vector2(Xwallforce * -1, Ywallforce);
            Anim.SetTrigger(anim_jump);
        }

    }

    public void WallSlide()
    {

       jumpTime = wallcheckHit && !On_Ground ? jumpTime += Time.deltaTime + WallJumpTime : jumpTime = 0; //Tiempo del jugador en la pared

        Is_Wall_Sliding = wallcheckHit && !On_Ground ? Is_Wall_Sliding = true : Is_Wall_Sliding = false; // Compruebo si el jugador se esta deslizando


        if (jumpTime > TimeInWall)
        {
            Is_Wall_Sliding = false;
        

        }
        if (Is_Wall_Sliding)
        {
            CreteDustWall();
           Rb2d.velocity = new Vector2(Rb2d.velocity.x, Mathf.Clamp(Rb2d.velocity.y, WallSlideSpeed, float.MaxValue)); //Velocidad del jugador al deslizarse 
        }


    }

    void SetWallJumpinFalse() => Wall_jumping = false;

    void Fall()
    {
        if (timeonair > 0.7f)
        
            Anim.Play("Fall");
      
    }
    public void AntiGravityApex()
    {
        Rb2d.gravityScale = Rb2d.velocity.y >= 0 ? (Rb2d.gravityScale = GravityScale) : (Rb2d.gravityScale = FallingGravityScale);
        
    }

    public void BumpedHead()
    {
        if (JumpLeft && !jumplRight && !Is_Wall_Sliding)
        {
            transform.position += new Vector3(0.2f, 0);

        }else if (!JumpLeft && jumplRight && !Is_Wall_Sliding)
        {
            transform.position -= new Vector3(0.2f, 0);
        }
    }

    public void TakeDamage(int dmg)
    {
        Health -= dmg;

        if (Health < 0)
        {
            Anim.SetTrigger("Death");
            is_alive = false;
        }
    }
    public void Attack_Dmg()
    {

        Collider2D[] HitEnemy = Physics2D.OverlapCircleAll(this.transform.position, Attack_Range, EnemyLayer);

        foreach (Collider2D enemy in HitEnemy)
        {
            if (enemy.GetComponent<Enemy>() != null)
            {
                enemy.GetComponent<Enemy>().TakeDamage(Dmg);
            }
        }
    }



    void CreateDust()
    {
         Instantiate(Dust, transform.position, Quaternion.identity);
       
    }
    void CreteDustWall()
    {
        DustWall.Play();

    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(this.transform.position, transform.position + Vector3.down * GroundLenght);
        Debug.DrawRay(transform.position, new Vector2(WallDistance, 0), Color.blue);
        Debug.DrawRay(transform.position, new Vector2(-WallDistance, 0), Color.blue);
        Debug.DrawRay(new Vector3(this.transform.position.x -0.3f, this.transform.position.y,this.transform.position.z),  Vector3.up * GroundLenght, Color.green);
        Debug.DrawRay(new Vector3(this.transform.position.x + 0.3f, this.transform.position.y, this.transform.position.z), Vector3.up * GroundLenght, Color.green);

    }
 

  
}


