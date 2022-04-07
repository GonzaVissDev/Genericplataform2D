using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement))]
public class InputManager : MonoBehaviour
{
    [SerializeField] private InputPlayer Control;

    private float Playeraxis;
    private float PlayeraxisY;

    [HideInInspector]
    public bool segundoataque = false;

    private Queue<InputControl> inputbuffer;

    public static InputManager input_manager { get; set; }


    private void Awake()
    {
        Control = new InputPlayer();
        inputbuffer = new Queue<InputControl>();
        input_manager = this;

        Control.Player.Jump.performed += _ => Jump(_);
        Control.Player.Movement.performed += _ => Move(_.ReadValue<Vector2>(), _);
        Control.Player.Attack.performed += _ => Attack(_);

        Control.Player.Attack.canceled += _ => CancelarCola();
        Control.Player.Jump.canceled += _ => CancelarCola();
        Control.Player.Movement.canceled += _ => CancelarCola();

    }

    void Move(Vector2 direction, InputAction.CallbackContext contx)
    {
        inputbuffer.Enqueue(contx.control);
        Playeraxis = direction.x;
        PlayeraxisY = direction.y;


    }
    void Jump(InputAction.CallbackContext contx)
    {
        if (inputbuffer.Count == 0)
        {
            PlayerMovement.player_script.Jump();
            inputbuffer.Enqueue(contx.control);

        }
        else if (inputbuffer.Count > 0)
        {
            //Comparo si el ultimo boton en la cola de "inputbuffer" es el mismo que el del salto.(uso es script "Chekinputbuffer" para verificarlo)
            if (CheckInputBuffer.CompareInputControler(contx.control, inputbuffer.Peek()))
            {
                
                inputbuffer.Enqueue(contx.control);
                PlayerMovement.player_script.Jump();

            }
            else if((CheckInputBuffer.CompareInputControler(contx.control, inputbuffer.Peek()) == false))
            {
                //si el ultimo boton es el mismo que el del salto, entonces significa que esta saltando
                Debug.Log("Ya esta saltando");
            }

          
         
        }

    }
    void Attack(InputAction.CallbackContext contx)
    {
        if (inputbuffer.Count == 0)
        {
            PlayerMovement.player_script.is_attacking = true;
            segundoataque = true;
            inputbuffer.Enqueue(contx.control);
          
        } 
        else if (inputbuffer.Count > 0)
        {
            if (CheckInputBuffer.CompareInputControler(contx.control, inputbuffer.Peek()))
            {
             
                PlayerMovement.player_script.is_attacking = true;
                segundoataque = true;
                inputbuffer.Enqueue(contx.control);
            }else
            {
                PlayerMovement.player_script.is_attacking = true;
                inputbuffer.Enqueue(contx.control);
                segundoataque = false;
            }
        }
    }

    private void Update()
    {
        //le envia la informacion del teclado en tiempo real a player movement
        PlayerMovement.player_script.movementX = Control.Player.Movement.IsPressed() ? PlayerMovement.player_script.movementX = Playeraxis : PlayerMovement.player_script.movementX = 0;
        PlayerMovement.player_script.movementY = Control.Player.Movement.IsPressed() ? PlayerMovement.player_script.movementY = PlayeraxisY : PlayerMovement.player_script.movementY = 0;


    }

    void CancelarCola() => Invoke("RemoveQueue", 0.12f); //Llamo cancelarCola para remover el boton que se unio en el inputbuffer.
    private void OnEnable() => Control.Player.Enable(); 
    private void OnDisable() => Control.Player.Disable();

    public void RemoveQueue()
    {
        if (inputbuffer.Count > 0)
        {
         
            inputbuffer.Dequeue();
            
        }

    }


}
