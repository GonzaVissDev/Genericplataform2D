using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    //Variables de las estadisticas del Enemigo.
    public float Vision_Radius;
    public float Attack_Radius;
    public float AtkSpeed;
    public float speed;
    bool IsAttackig;
    int Dmg = 100;
    int Health = 30;
    bool is_alive = true;

    [SerializeField] private LayerMask layertarget;


    // Variable para guardar El Objetos que utilizaremos.

    GameObject Player;
    Animator anim;
    SpriteRenderer sp;

    Rigidbody2D rb2d;

    bool encontreenemigo;
    RaycastHit2D PlayerPosition;
    //Variable para guardar la posicion inicial
    Vector3 StartPosition, target;


    void Start()
    {

        sp = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        // Guardo la posicion Actual.
        StartPosition = transform.position;

        rb2d = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Vector3 Target = StartPosition;

        //Comprobamos en un RayCast la distancia del enemigo hacia el jugador.

        PlayerPosition = Physics2D.Raycast(transform.position, Vector2.left, 10f, layertarget);

        if (is_alive == true)
        {

            //Si el RayCast encuentra al jugador lo ponemos dentro del Target
            if (PlayerPosition.collider != null)
            {
                if (PlayerPosition.collider.tag == "Player")
                {
                    Player = GameObject.FindGameObjectWithTag("Player");

                    Target = Player.transform.position;

                    Vector3 Forward = transform.TransformDirection(Player.transform.position - transform.position);

                }
            }


            // Ahora Calculo la distancia que hay entre la direccion actual hasta el Target
            float Distance = Vector3.Distance(Target, transform.position);

            Vector3 Dir = (Target - transform.position).normalized;

            sp.flipX = Dir.x < 0 ? sp.flipX = true : sp.flipX = false;


            //Si es el enemigo y Esta en rango de Ataque, se queda quieto para Disparar
            if (Target != StartPosition && Distance < Attack_Radius)
            {
                anim.SetBool("Walk", false);

                if (!IsAttackig) StartCoroutine(Attack(AtkSpeed));
            }
            else
            {
                anim.SetBool("Walk", true);

                rb2d.MovePosition(rb2d.transform.position + Dir * (speed * Time.deltaTime));
            }

            //Comprobacion para evitar Bugs.

            if (Target == StartPosition && Distance < 0.05f && IsAttackig == false)
            {
                transform.position = StartPosition;

                anim.SetBool("Walk", false);

            }


            Debug.DrawLine(transform.position, Target, Color.green);

        }

    }

    //Metodo para dibujar los Gizmos.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, Vision_Radius);
        Gizmos.DrawWireSphere(transform.position, Attack_Radius);
        Gizmos.DrawLine(this.transform.position, transform.position + Vector3.left * 10f);
    }

    
    // Enumerator que nos sirvira para saber cada cuanto tiene que atacar el enemigo.
    IEnumerator Attack(float speed)
    {
        IsAttackig = true;

        anim.Play("Attack");

        yield return new WaitForSecondsRealtime(speed);

        IsAttackig = false;
    }


    public void Attack_Dmg()
    {

        Collider2D[] HitEnemy = Physics2D.OverlapCircleAll(this.transform.position, Attack_Radius, layertarget);

        foreach (Collider2D enemy in HitEnemy)
        {
            if (enemy.GetComponent<PlayerMovement>() != null)
            {
                enemy.GetComponent<PlayerMovement>().TakeDamage(Dmg);
            }

        }
    }

    public void TakeDamage(int dmg)
    {
        anim.Play("Hit");
        Health -= dmg;

        if (Health < 0)
        {
            is_alive = false;
           
            anim.Play("Death");

            Destroy(this.gameObject, 0.5f);
        }
    }
}

